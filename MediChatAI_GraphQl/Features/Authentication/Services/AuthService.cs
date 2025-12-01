using Microsoft.IdentityModel.Tokens;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Admin.DTOs;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Features.Authentication.DTOs;
using MediChatAI_GraphQl.Features.Notifications.DTOs;
using Microsoft.AspNetCore.Identity;

namespace MediChatAI_GraphQl.Features.Authentication.Services;


public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IActivityLoggingService _activityLoggingService;
    private readonly ILogger<AuthService> _logger;
    private readonly ISystemSettingsService _systemSettingsService;
    private readonly INotificationService _notificationService;


    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IEmailService emailService,
        IActivityLoggingService activityLoggingService,
        ILogger<AuthService> logger,
        ISystemSettingsService systemSettingsService,
        INotificationService notificationService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _notificationService = notificationService;
        _configuration = configuration;
        _emailService = emailService;
        _activityLoggingService = activityLoggingService;
        _logger = logger;
        _systemSettingsService = systemSettingsService;
    }


    public async Task<AuthResult> RegisterAsync(RegisterUserInput input)
    {
        // Validation
        if (input.Password != input.ConfirmPassword)
        {
            return new AuthResult(false, null, null, null, new[] { "Passwords do not match" });
        }


        if (!IsValidRole(input.Role))
        {
            return new AuthResult(false, null, null, null, new[] { "Invalid role specified" });
        }


        var existingUser = await _userManager.FindByEmailAsync(input.Email);
        if (existingUser != null)
        {
            return new AuthResult(false, null, null, null, new[] { "User with this email already exists" });
        }


        var user = new ApplicationUser
        {
            UserName = input.Email,
            Email = input.Email,
            FirstName = input.FirstName,
            LastName = input.LastName,
            PasswordLastChangedAt = DateTime.UtcNow
        };


        var result = await _userManager.CreateAsync(user, input.Password);
        if (!result.Succeeded)
        {
            return new AuthResult(false, null, null, null, result.Errors.Select(e => e.Description));
        }


        // Add role
        await _userManager.AddToRoleAsync(user, input.Role);

        // Log registration activity
        await _activityLoggingService.LogUserActivityAsync(user.Id, ActivityTypes.Register,
            $"User registered with role {input.Role}", $"Email: {user.Email}");

        // Send email verification
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var emailConfirmationLink = $"http://localhost:5212/verify-email?userId={user.Id}&token={Uri.EscapeDataString(emailToken)}";

        await _emailService.SendEmailVerificationAsync(user.Email!, user.FirstName, emailConfirmationLink);

        // Send welcome notification
        await _notificationService.CreateNotificationAsync(
            user.Id,
            "Welcome to MediChatAI! 🎉",
            "Your account has been created successfully. Please verify your email to get started.",
            NotificationType.Success,
            NotificationCategory.System,
            NotificationPriority.Normal,
            "/verify-email",
            "Verify Email");

        var userInfo = new UserInfo(user.Id, user.Email!, user.FirstName, user.LastName, input.Role, false, user.TwoFactorEnabled);
        return new AuthResult(true, null, null, userInfo, Array.Empty<string>());
    }


    public async Task<AuthResult> LoginAsync(LoginUserInput input)
    {
        // Load security settings
        var settings = await _systemSettingsService.GetSystemSettingsAsync();

        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            return new AuthResult(false, null, null, null, new[] { "Invalid email or password" });
        }

        // Check account lockout
        if (user.AccountLockedUntil.HasValue && user.AccountLockedUntil.Value > DateTime.UtcNow)
        {
            var minutesRemaining = (int)(user.AccountLockedUntil.Value - DateTime.UtcNow).TotalMinutes;
            return new AuthResult(false, null, null, null, new[] { $"ACCOUNT_LOCKED|{minutesRemaining}" });
        }

        // Reset lockout if expired
        if (user.AccountLockedUntil.HasValue && user.AccountLockedUntil.Value <= DateTime.UtcNow)
        {
            user.AccountLockedUntil = null;
            user.FailedLoginAttempts = 0;
            await _userManager.UpdateAsync(user);
        }

        // Check email verification
        if (settings.RequireEmailVerification && !user.EmailConfirmed)
        {
            return new AuthResult(false, null, null, null, new[] { "EMAIL_NOT_VERIFIED" });
        }

        // Check password expiry
        if (settings.PasswordExpiryDays > 0 && user.PasswordLastChangedAt.HasValue)
        {
            var daysSinceChange = (DateTime.UtcNow - user.PasswordLastChangedAt.Value).TotalDays;
            if (daysSinceChange > settings.PasswordExpiryDays)
            {
                user.IsPasswordExpired = true;
                await _userManager.UpdateAsync(user);
                return new AuthResult(false, null, null, null, new[] { "PASSWORD_EXPIRED" });
            }
        }

        // Check password
        var result = await _signInManager.CheckPasswordSignInAsync(user, input.Password, false);
        if (!result.Succeeded)
        {
            // Increment failed login attempts
            user.FailedLoginAttempts++;

            // Check if account should be locked
            if (user.FailedLoginAttempts >= settings.MaxLoginAttempts)
            {
                user.AccountLockedUntil = DateTime.UtcNow.AddMinutes(settings.AccountLockoutMinutes);
                await _userManager.UpdateAsync(user);

                // Log lockout event
                await _activityLoggingService.LogUserActivityAsync(user.Id, ActivityTypes.Login,
                    "Account locked due to failed login attempts",
                    $"Failed attempts: {user.FailedLoginAttempts}, Locked until: {user.AccountLockedUntil}");

                return new AuthResult(false, null, null, null, new[] { $"ACCOUNT_LOCKED|{settings.AccountLockoutMinutes}" });
            }

            await _userManager.UpdateAsync(user);

            var attemptsRemaining = settings.MaxLoginAttempts - user.FailedLoginAttempts;
            return new AuthResult(false, null, null, null, new[] { $"Invalid email or password. {attemptsRemaining} attempt(s) remaining." });
        }

        // Reset failed attempts on successful login
        user.FailedLoginAttempts = 0;
        user.AccountLockedUntil = null;

        // Check if MFA is enabled (prioritize authenticator app over email OTP)
        if (settings.EnableTwoFactor)
        {
            // Check if user has authenticator app enabled (preferred method)
            if (user.IsAuthenticatorEnabled)
            {
                // Return response indicating authenticator is required
                return new AuthResult(false, null, null, null, new[] { "AUTHENTICATOR_REQUIRED" });
            }
            // Fallback to email OTP if user hasn't set up authenticator
            else if (user.TwoFactorEnabled)
            {
                // Generate OTP for email-based 2FA
                var otpCode = GenerateOtpCode();
                user.OtpCode = otpCode;
                user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(10);
                await _userManager.UpdateAsync(user);

                // Send OTP email
                await _emailService.SendOtpAsync(user.Email!, user.FirstName, otpCode);

                // Return special response indicating email 2FA is required
                return new AuthResult(false, null, null, null, new[] { "MFA_REQUIRED" });
            }
        }

        var token = await GenerateJwtTokenAsync(user, input.RememberMe);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = input.RememberMe
            ? DateTime.UtcNow.AddDays(30) // 30 days for remember me
            : DateTime.UtcNow.AddDays(7); // 7 days for normal login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = new UserInfo(user.Id, user.Email!, user.FirstName, user.LastName, roles.FirstOrDefault() ?? "Patient", user.EmailConfirmed, user.TwoFactorEnabled);

        // Log login activity
        var loginDetails = input.RememberMe ? "Remember Me enabled" : "Standard login";
        await _activityLoggingService.LogUserActivityAsync(user.Id, ActivityTypes.Login,
            "User logged in successfully", loginDetails);

        // Send login alert
        await _emailService.SendLoginAlertAsync(user.Email!, user.FirstName, DateTime.UtcNow);

        // Send login notification
        var loginTime = DateTime.UtcNow.ToString("MMM dd, yyyy 'at' h:mm tt");
        await _notificationService.CreateNotificationAsync(
            user.Id,
            "Login Successful ✓",
            $"You logged in successfully on {loginTime}. Welcome back, {user.FirstName}!",
            NotificationType.Success,
            NotificationCategory.Security,
            NotificationPriority.Normal,
            null,
            null);

        return new AuthResult(true, token, refreshToken, userInfo, Array.Empty<string>());
    }


    public async Task<AuthResult> RefreshTokenAsync(RefreshTokenInput input)
    {
        var principal = GetPrincipalFromExpiredToken(input.Token);
        if (principal == null)
        {
            return new AuthResult(false, null, null, null, new[] { "Invalid token" });
        }


        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId!);


        if (user == null || user.RefreshToken != input.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResult(false, null, null, null, new[] { "Invalid refresh token" });
        }


        var newJwtToken = await GenerateJwtTokenAsync(user);
        var newRefreshToken = GenerateRefreshToken();


        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);


        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = new UserInfo(user.Id, user.Email!, user.FirstName, user.LastName, roles.FirstOrDefault() ?? "Patient", user.EmailConfirmed, user.TwoFactorEnabled);


        return new AuthResult(true, newJwtToken, newRefreshToken, userInfo, Array.Empty<string>());
    }


    public async Task<bool> ForgotPasswordAsync(ForgotPasswordInput input)
    {
        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null || !user.EmailConfirmed)
        {
            return true; // Don't reveal whether user exists
        }


        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"http://localhost:5215/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        
        await _emailService.SendPasswordResetAsync(user.Email!, user.FirstName, resetLink);
        return true;
    }


    public async Task<bool> ResetPasswordAsync(ResetPasswordInput input)
    {
        if (input.Password != input.ConfirmPassword)
        {
            return false;
        }


        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            return false;
        }


        var result = await _userManager.ResetPasswordAsync(user, input.Token, input.Password);
        return result.Succeeded;
    }


    public async Task<bool> VerifyEmailAsync(VerifyEmailInput input)
    {
        var user = await _userManager.FindByIdAsync(input.UserId);
        if (user == null)
        {
            return false;
        }


        var result = await _userManager.ConfirmEmailAsync(user, input.Token);
        return result.Succeeded;
    }


    public async Task<UserInfo?> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;


        var roles = await _userManager.GetRolesAsync(user);
        return new UserInfo(user.Id, user.Email!, user.FirstName, user.LastName, roles.FirstOrDefault() ?? "Patient", user.EmailConfirmed, user.TwoFactorEnabled);
    }


    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<OtpResult> SendOtpAsync(SendOtpInput input)
    {
        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            return new OtpResult(false, new[] { "User not found" });
        }

        if (user.EmailConfirmed)
        {
            return new OtpResult(false, new[] { "Email is already verified" });
        }

        var otpCode = GenerateOtpCode();
        user.OtpCode = otpCode;
        user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(10);

        _logger.LogInformation("Generated OTP for user {Email}: {OtpCode} (expires at {ExpiryTime})",
            user.Email, otpCode, user.OtpExpiryTime);

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return new OtpResult(false, updateResult.Errors.Select(e => e.Description));
        }

        await _emailService.SendOtpAsync(user.Email!, user.FirstName, otpCode);
        return new OtpResult(true, Array.Empty<string>());
    }

    public async Task<AuthResult> VerifyOtpAsync(VerifyOtpInput input)
    {
        var user = await _userManager.FindByEmailAsync(input.Email);
        if (user == null)
        {
            return new AuthResult(false, null, null, null, new[] { "User not found" });
        }

        if (user.EmailConfirmed)
        {
            return new AuthResult(false, null, null, null, new[] { "Email is already verified" });
        }

        if (string.IsNullOrEmpty(user.OtpCode) || user.OtpExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResult(false, null, null, null, new[] { "OTP has expired or is invalid" });
        }

        if (user.OtpCode != input.OtpCode)
        {
            return new AuthResult(false, null, null, null, new[] { "Invalid OTP code" });
        }

        user.EmailConfirmed = true;
        user.OtpCode = null;
        user.OtpExpiryTime = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return new AuthResult(false, null, null, null, updateResult.Errors.Select(e => e.Description));
        }

        var token = await GenerateJwtTokenAsync(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = new UserInfo(user.Id, user.Email!, user.FirstName, user.LastName, roles.FirstOrDefault() ?? "Patient", user.EmailConfirmed);

        // Log email verification and login activity
        await _activityLoggingService.LogUserActivityAsync(user.Id, ActivityTypes.EmailVerification,
            "Email verified via OTP", $"OTP Code: {input.OtpCode}");
        await _activityLoggingService.LogUserActivityAsync(user.Id, ActivityTypes.Login,
            "User logged in after email verification", "OTP verification login");

        // Send email verification success notification
        await _notificationService.CreateNotificationAsync(
            user.Id,
            "Email Verified Successfully! ✓",
            "Your email has been verified. You now have full access to all features.",
            NotificationType.Success,
            NotificationCategory.Security,
            NotificationPriority.High,
            null,
            null);

        return new AuthResult(true, token, refreshToken, userInfo, Array.Empty<string>());
    }

    public async Task<UserProfile?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfile(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            roles.FirstOrDefault() ?? "Patient",
            user.EmailConfirmed,
            user.TwoFactorEnabled,
            user.ProfileImage,
            user.DateOfBirth,
            user.Gender,
            user.Address,
            user.City,
            user.State,
            user.ZipCode,
            user.Country,
            user.PhoneNumber,
            user.Specialization,
            user.LicenseNumber,
            user.Department,
            user.EmergencyContactName,
            user.EmergencyContactPhone,
            user.BloodType,
            user.Allergies,
            user.MedicalHistory,
            user.CreatedAt,
            user.LastLoginAt,
            user.LastProfileUpdate
        );
    }

    public async Task<ProfileResult> UpdateProfileAsync(string userId, UpdateProfileInput input)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new ProfileResult(false, null, new[] { "User not found" });
        }

        // Update basic info
        user.FirstName = input.FirstName;
        user.LastName = input.LastName;
        user.PhoneNumber = input.PhoneNumber;
        user.DateOfBirth = input.DateOfBirth;
        user.Gender = input.Gender;
        user.Address = input.Address;
        user.City = input.City;
        user.State = input.State;
        user.ZipCode = input.ZipCode;
        user.Country = input.Country;
        user.EmergencyContactName = input.EmergencyContactName;
        user.EmergencyContactPhone = input.EmergencyContactPhone;
        user.LastProfileUpdate = DateTime.UtcNow;

        // Role-specific fields
        var roles = await _userManager.GetRolesAsync(user);
        var userRole = roles.FirstOrDefault();

        if (userRole == "Doctor")
        {
            user.Specialization = input.Specialization;
            user.LicenseNumber = input.LicenseNumber;
            user.Department = input.Department;
        }
        else if (userRole == "Patient")
        {
            user.BloodType = input.BloodType;
            user.Allergies = input.Allergies;
            user.MedicalHistory = input.MedicalHistory;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return new ProfileResult(false, null, result.Errors.Select(e => e.Description));
        }

        // Log profile update activity
        await _activityLoggingService.LogUserActivityAsync(userId, ActivityTypes.ProfileUpdate,
            "User updated profile information", $"Role: {userRole}");

        // Send notification about profile update
        await _notificationService.CreateNotificationAsync(
            userId,
            "Profile Updated Successfully",
            "Your profile information has been updated. Changes may take a few moments to reflect across the platform.",
            NotificationType.Success,
            NotificationCategory.System,
            NotificationPriority.Normal,
            "/profile",
            "View Profile");

        var updatedProfile = await GetUserProfileAsync(userId);
        return new ProfileResult(true, updatedProfile, Array.Empty<string>());
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordInput input)
    {
        if (input.NewPassword != input.ConfirmNewPassword)
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ChangePasswordAsync(user, input.CurrentPassword, input.NewPassword);

        if (result.Succeeded)
        {
            // Update password change timestamp and reset expiry flag
            user.PasswordLastChangedAt = DateTime.UtcNow;
            user.IsPasswordExpired = false;
            await _userManager.UpdateAsync(user);

            // Log password change activity
            await _activityLoggingService.LogUserActivityAsync(userId, ActivityTypes.PasswordChange,
                "User changed password", "Password updated successfully");

            // Send notification about password change
            await _notificationService.CreateNotificationAsync(
                userId,
                "Password Changed Successfully 🔒",
                $"Your password was changed on {DateTime.UtcNow:MMM dd, yyyy 'at' h:mm tt}. If you didn't make this change, please contact support immediately.",
                NotificationType.Success,
                NotificationCategory.Security,
                NotificationPriority.High,
                null,
                null);
        }

        return result.Succeeded;
    }

    public async Task<string?> UploadProfileImageAsync(string userId, IFormFile file)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        if (file == null || file.Length == 0)
            return null;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Any(ext => ext == extension))
            return null;

        const int maxSizeInBytes = 5 * 1024 * 1024; // 5MB
        if (file.Length > maxSizeInBytes)
            return null;

        var uploadsPath = System.IO.Path.Combine("wwwroot", "uploads", "profiles");
        System.IO.Directory.CreateDirectory(uploadsPath);

        var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
        var filePath = System.IO.Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"/uploads/profiles/{fileName}";

        user.ProfileImage = imageUrl;
        await _userManager.UpdateAsync(user);

        return imageUrl;
    }


    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user, bool rememberMe = false)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

        // Load session timeout from database settings
        var settings = await _systemSettingsService.GetSystemSettingsAsync();

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };


        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Use session timeout from database settings
        var expiration = rememberMe
            ? DateTime.UtcNow.AddDays(30) // 30 days for remember me
            : DateTime.UtcNow.AddMinutes(settings.SessionTimeoutMinutes); // Use dynamic session timeout

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };


        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }


    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }


    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);


        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"]
        };


        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }


        return principal;
    }


    private static bool IsValidRole(string role)
    {
        return role is "Doctor" or "Patient";
    }

    public async Task<bool> EnableMfaAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.TwoFactorEnabled = true;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            await _activityLoggingService.LogUserActivityAsync(userId, ActivityTypes.ProfileUpdate,
                "MFA enabled", "Two-factor authentication enabled for account");

            // Send notification about MFA being enabled
            await _notificationService.CreateNotificationAsync(
                userId,
                "Two-Factor Authentication Enabled 🛡️",
                "Two-factor authentication has been enabled for your account. This adds an extra layer of security to protect your account.",
                NotificationType.Success,
                NotificationCategory.Security,
                NotificationPriority.High,
                "/profile",
                "View Settings");
        }

        return result.Succeeded;
    }

    public async Task<bool> DisableMfaAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.TwoFactorEnabled = false;
        user.IsAuthenticatorEnabled = false;
        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            await _activityLoggingService.LogUserActivityAsync(userId, ActivityTypes.ProfileUpdate,
                "MFA disabled", "Two-factor authentication disabled for account");

            // Send security alert email
            await _emailService.SendTwoFactorDisabledAsync(user.Email!, user.FirstName);

            // Send notification about MFA being disabled
            await _notificationService.CreateNotificationAsync(
                userId,
                "Two-Factor Authentication Disabled ⚠️",
                "Two-factor authentication has been disabled for your account. If you didn't make this change, please enable it immediately and contact support.",
                NotificationType.Warning,
                NotificationCategory.Security,
                NotificationPriority.Urgent,
                "/profile",
                "Review Security");
        }

        return result.Succeeded;
    }

    public async Task<AuthResult> VerifyMfaAsync(string email, string otpCode)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new AuthResult(false, null, null, null, new[] { "User not found" });
        }

        // Check if OTP is valid
        if (string.IsNullOrEmpty(user.OtpCode) || user.OtpExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResult(false, null, null, null, new[] { "OTP has expired or is invalid" });
        }

        if (user.OtpCode != otpCode)
        {
            return new AuthResult(false, null, null, null, new[] { "Invalid OTP code" });
        }

        // Clear OTP after successful verification
        user.OtpCode = null;
        user.OtpExpiryTime = DateTime.UtcNow;

        // Reset failed attempts since MFA is successful
        user.FailedLoginAttempts = 0;
        user.AccountLockedUntil = null;

        var token = await GenerateJwtTokenAsync(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = new UserInfo(user.Id, user.Email!, user.FirstName, user.LastName, roles.FirstOrDefault() ?? "Patient", user.EmailConfirmed);

        // Log MFA verification activity
        await _activityLoggingService.LogUserActivityAsync(user.Id, ActivityTypes.Login,
            "User logged in with MFA", "Two-factor authentication verified successfully");

        // Send login alert
        await _emailService.SendLoginAlertAsync(user.Email!, user.FirstName, DateTime.UtcNow);

        // Send login notification
        var loginTime = DateTime.UtcNow.ToString("MMM dd, yyyy 'at' h:mm tt");
        await _notificationService.CreateNotificationAsync(
            user.Id,
            "Login Successful ✓",
            $"You logged in successfully on {loginTime} with two-factor authentication. Welcome back, {user.FirstName}!",
            NotificationType.Success,
            NotificationCategory.Security,
            NotificationPriority.Normal,
            null,
            null);

        return new AuthResult(true, token, refreshToken, userInfo, Array.Empty<string>());
    }

    public async Task<AuthenticatorSetupResult> SetupAuthenticatorAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthenticatorSetupResult(false, null, null, null, new[] { "User not found" });
        }

        // Generate or retrieve authenticator key
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        // Store the key in our custom field for reference
        user.AuthenticatorKey = unformattedKey;
        await _userManager.UpdateAsync(user);

        // Format the key for display (groups of 4 characters)
        var formattedKey = FormatAuthenticatorKey(unformattedKey!);

        // Generate QR code URL in otpauth format
        var email = Uri.EscapeDataString(user.Email!);
        var qrCodeUrl = $"otpauth://totp/MediChatAI:{email}?secret={unformattedKey}&issuer=MediChatAI&digits=6";

        // Log activity
        await _activityLoggingService.LogUserActivityAsync(userId, ActivityTypes.ProfileUpdate,
            "Authenticator setup initiated", "User started authenticator app setup");

        return new AuthenticatorSetupResult(true, unformattedKey, formattedKey, qrCodeUrl, Array.Empty<string>());
    }

    public async Task<bool> VerifyAuthenticatorSetupAsync(string userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        // Strip spaces and hyphens from code
        var cleanCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);

        // Verify the code using ASP.NET Identity's built-in TOTP verification
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            cleanCode);

        if (isValid)
        {
            // Enable authenticator for this user
            user.IsAuthenticatorEnabled = true;
            await _userManager.UpdateAsync(user);

            // Log activity
            await _activityLoggingService.LogUserActivityAsync(userId, ActivityTypes.ProfileUpdate,
                "Authenticator enabled", "User successfully enabled authenticator app");

            // Send confirmation email
            await _emailService.SendTwoFactorEnabledAsync(user.Email!, user.FirstName);

            return true;
        }

        return false;
    }

    public async Task<AuthResult> VerifyAuthenticatorCodeAsync(string email, string code)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return new AuthResult(false, null, null, null, new[] { "User not found" });
        }

        if (!user.IsAuthenticatorEnabled)
        {
            return new AuthResult(false, null, null, null, new[] { "Authenticator not enabled for this account" });
        }

        // Strip spaces and hyphens from code
        var cleanCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);

        // Verify the TOTP code
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            cleanCode);

        if (!isValid)
        {
            return new AuthResult(false, null, null, null, new[] { "Invalid authenticator code" });
        }
        // Reset failed attempts since authenticator verification is successful
        user.FailedLoginAttempts = 0;
        user.AccountLockedUntil = null;

        var token = await GenerateJwtTokenAsync(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var userInfo = new UserInfo(user.Id, user.Email!, user.FirstName, user.LastName, roles.FirstOrDefault() ?? "Patient", user.EmailConfirmed);

        // Log authenticator verification activity
        await _activityLoggingService.LogUserActivityAsync(user.Id, ActivityTypes.Login,
            "User logged in with authenticator", "Two-factor authentication via authenticator app verified successfully");

        // Send login alert
        await _emailService.SendLoginAlertAsync(user.Email!, user.FirstName, DateTime.UtcNow);

        // Send login notification
        var loginTime = DateTime.UtcNow.ToString("MMM dd, yyyy 'at' h:mm tt");
        await _notificationService.CreateNotificationAsync(
            user.Id,
            "Login Successful ✓",
            $"You logged in successfully on {loginTime} using authenticator app. Welcome back, {user.FirstName}!",
            NotificationType.Success,
            NotificationCategory.Security,
            NotificationPriority.Normal,
            null,
            null);

        return new AuthResult(true, token, refreshToken, userInfo, Array.Empty<string>());
    }

    private static string FormatAuthenticatorKey(string unformattedKey)
    {
        var result = new System.Text.StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }
}

namespace MediChatAI_GraphQl.Features.Authentication.DTOs;

public record RegisterUserInput(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string Role);

public record LoginUserInput(
    string Email,
    string Password,
    bool RememberMe = false);

public record ForgotPasswordInput(
    string Email);

public record ResetPasswordInput(
    string Email,
    string Token,
    string Password,
    string ConfirmPassword);

public record VerifyEmailInput(
    string UserId,
    string Token);

public record AuthResult(
    bool Success,
    string? Token,
    string? RefreshToken,
    UserInfo? User,
    IEnumerable<string> Errors);

public record UserInfo(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool EmailConfirmed,
    bool TwoFactorEnabled = false);

public record RefreshTokenInput(
    string Token,
    string RefreshToken);

public record SendOtpInput(
    string Email);

public record VerifyOtpInput(
    string Email,
    string OtpCode);

public record OtpResult(
    bool Success,
    IEnumerable<string> Errors);

public record AuthenticatorSetupResult(
    bool Success,
    string? SharedKey,
    string? ManualEntryKey,
    string? QrCodeUrl,
    IEnumerable<string> Errors);

public record UserProfile(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    string? ProfileImage,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Country,
    string? PhoneNumber,
    string? Specialization,
    string? LicenseNumber,
    string? Department,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? BloodType,
    string? Allergies,
    string? MedicalHistory,
    DateTime CreatedAt,
    DateTime LastLoginAt,
    DateTime? LastProfileUpdate);

public record UpdateProfileInput(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Country,
    string? Specialization,
    string? LicenseNumber,
    string? Department,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? BloodType,
    string? Allergies,
    string? MedicalHistory);

public record ChangePasswordInput(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);

public record ProfileResult(
    bool Success,
    UserProfile? Profile,
    IEnumerable<string> Errors);
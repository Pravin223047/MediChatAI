namespace MediChatAI_BlazorWebAssembly.Features.Authentication.Models;

// Authentication Request Models
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class VerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

// Authentication Response Models
public record AuthResponse(
    bool Success,
    string? Token,
    string? RefreshToken,
    UserInfo? User,
    string[] Errors);

public record UserInfo(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool EmailConfirmed,
    bool TwoFactorEnabled = false);

// OTP Models
public class SendOtpRequest
{
    public string Email { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}

public record OtpResponse(bool Success, string[] Errors);

// GraphQL Response Wrappers for Authentication Operations
public record LoginResponse(AuthResponse LoginUser);
public record LoginUserResponse(AuthResponse LoginUser);

public record RegisterResponse(AuthResponse RegisterUser);
public record RegisterUserResponse(AuthResponse RegisterUser);

public record RefreshTokenResponse(AuthResponse RefreshToken);

public record SendOtpResponse(OtpResponse SendOtp);

public record VerifyOtpResponse(AuthResponse VerifyOtp);

public record VerifyAuthenticatorCodeResponse(AuthResponse VerifyAuthenticatorCode);

public record ChangePasswordResponse(bool ChangePassword);
using MediChatAI_GraphQl.Features.Authentication.DTOs;
using Microsoft.AspNetCore.Http;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterUserInput input);
    Task<AuthResult> LoginAsync(LoginUserInput input);
    Task<AuthResult> RefreshTokenAsync(RefreshTokenInput input);
    Task<bool> ForgotPasswordAsync(ForgotPasswordInput input);
    Task<bool> ResetPasswordAsync(ResetPasswordInput input);
    Task<bool> VerifyEmailAsync(VerifyEmailInput input);
    Task<UserInfo?> GetCurrentUserAsync(string userId);
    Task LogoutAsync(string userId);
    Task<OtpResult> SendOtpAsync(SendOtpInput input);
    Task<AuthResult> VerifyOtpAsync(VerifyOtpInput input);
    Task<UserProfile?> GetUserProfileAsync(string userId);
    Task<ProfileResult> UpdateProfileAsync(string userId, UpdateProfileInput input);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordInput input);
    Task<string?> UploadProfileImageAsync(string userId, IFormFile file);
    Task<bool> EnableMfaAsync(string userId);
    Task<bool> DisableMfaAsync(string userId);
    Task<AuthResult> VerifyMfaAsync(string email, string otpCode);
    Task<AuthenticatorSetupResult> SetupAuthenticatorAsync(string userId);
    Task<bool> VerifyAuthenticatorSetupAsync(string userId, string code);
    Task<AuthResult> VerifyAuthenticatorCodeAsync(string email, string code);
}
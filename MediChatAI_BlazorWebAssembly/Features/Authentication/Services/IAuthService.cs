using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Authentication.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request);
    Task<UserInfo?> GetCurrentUserAsync();
    Task<bool> LogoutAsync();
    Task<AuthResponse?> RefreshTokenAsync();
    Task<OtpResponse> SendOtpAsync(string email);
    Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<(string? email, string? password, bool rememberMe)> GetRememberedCredentialsAsync();
    Task ClearRememberedCredentialsAsync();
    Task<AuthResponse> VerifyAuthenticatorCodeAsync(string email, string code);
}
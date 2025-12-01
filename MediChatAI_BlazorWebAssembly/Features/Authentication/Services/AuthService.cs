using Blazored.LocalStorage;
using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Authentication.Services;

public class AuthService : IAuthService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILocalStorageService _localStorage;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(
        IGraphQLService graphQLService,
        ILocalStorageService localStorage,
        CustomAuthStateProvider authStateProvider)
    {
        _graphQLService = graphQLService;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var query = """
            mutation LoginUser($input: LoginUserInput!) {
              loginUser(input: $input) {
                success
                token
                refreshToken
                user {
                  id
                  email
                  firstName
                  lastName
                  role
                  emailConfirmed
                }
                errors
              }
            }
            """;

        var variables = new { input = request };
        var response = await _graphQLService.SendQueryAsync<LoginUserResponse>(query, variables);

        var authResponse = response?.LoginUser ?? new AuthResponse(false, null, null, null, new[] { "Network error occurred" });

        if (authResponse.Success && authResponse.Token != null)
        {
            await _localStorage.SetItemAsync("token", authResponse.Token);
            await _localStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);
            await _localStorage.SetItemAsync("user", authResponse.User);

            // Save credentials if remember me is checked
            if (request.RememberMe)
            {
                await _localStorage.SetItemAsync("rememberedEmail", request.Email);
                await _localStorage.SetItemAsync("rememberedPassword", request.Password);
                await _localStorage.SetItemAsync("rememberMeEnabled", true);
            }

            // Set session as active for this application instance
            await _authStateProvider.SetSessionActiveAsync();

            // Store current session version
            await StoreSessionVersionAsync();

            await _authStateProvider.MarkUserAsAuthenticated(authResponse.User!, authResponse.Token);
        }

        return authResponse;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var query = """
            mutation RegisterUser($input: RegisterUserInput!) {
              registerUser(input: $input) {
                success
                token
                refreshToken
                user {
                  id
                  email
                  firstName
                  lastName
                  role
                  emailConfirmed
                }
                errors
              }
            }
            """;

        var variables = new { input = request };
        var response = await _graphQLService.SendQueryAsync<RegisterUserResponse>(query, variables);

        return response?.RegisterUser ?? new AuthResponse(false, null, null, null, new[] { "Network error occurred" });
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var query = """
            mutation ForgotPassword($input: ForgotPasswordInput!) {
              forgotPassword(input: $input)
            }
            """;

        var variables = new { input = request };
        return await _graphQLService.SendQueryAsync<bool>(query, variables);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var query = """
            mutation ResetPassword($input: ResetPasswordInput!) {
              resetPassword(input: $input)
            }
            """;

        var variables = new { input = request };
        return await _graphQLService.SendQueryAsync<bool>(query, variables);
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var query = """
            mutation VerifyEmail($input: VerifyEmailInput!) {
              verifyEmail(input: $input)
            }
            """;

        var variables = new { input = request };
        return await _graphQLService.SendQueryAsync<bool>(query, variables);
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        var query = """
            query GetCurrentUser {
              currentUser {
                id
                email
                firstName
                lastName
                role
                emailConfirmed
              }
            }
            """;

        return await _graphQLService.SendQueryAsync<UserInfo?>(query);
    }

    public async Task<bool> LogoutAsync()
    {
        var query = """
            mutation Logout {
              logout
            }
            """;

        await _graphQLService.SendQueryAsync<bool>(query);

        await _localStorage.RemoveItemAsync("token");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _localStorage.RemoveItemAsync("user");
        await _localStorage.RemoveItemAsync("app_session_id");
        await _localStorage.RemoveItemAsync("sessionVersion");
        await _localStorage.RemoveItemAsync("session_warning_shown");

        // Clear remembered credentials on logout
        await _localStorage.RemoveItemAsync("rememberedEmail");
        await _localStorage.RemoveItemAsync("rememberedPassword");
        await _localStorage.RemoveItemAsync("rememberMeEnabled");

        await _authStateProvider.MarkUserAsLoggedOut();
        return true;
    }

    public async Task<AuthResponse?> RefreshTokenAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("token");
        var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            return null;

        var query = """
            mutation RefreshToken($input: RefreshTokenInput!) {
              refreshToken(input: $input) {
                success
                token
                refreshToken
                user {
                  id
                  email
                  firstName
                  lastName
                  role
                  emailConfirmed
                }
                errors
              }
            }
            """;

        var variables = new { input = new { token, refreshToken } };
        var response = await _graphQLService.SendQueryAsync<AuthResponse>(query, variables);

        if (response?.Success == true && response.Token != null)
        {
            await _localStorage.SetItemAsync("token", response.Token);
            await _localStorage.SetItemAsync("refreshToken", response.RefreshToken);
            await _localStorage.SetItemAsync("user", response.User);

            // Set session as active for this application instance
            await _authStateProvider.SetSessionActiveAsync();

            // Store current session version
            await StoreSessionVersionAsync();

            await _authStateProvider.MarkUserAsAuthenticated(response.User!, response.Token);
        }

        return response;
    }

    private async Task StoreSessionVersionAsync()
    {
        try
        {
            // Fetch current session version from server
            var query = """
                query GetSystemSettings {
                  systemSettings {
                    sessionVersion
                  }
                }
                """;

            var response = await _graphQLService.SendQueryAsync<SystemSettingsResponse>(query);
            if (response?.SystemSettings != null)
            {
                await _authStateProvider.SetSessionVersionAsync(response.SystemSettings.SessionVersion);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error storing session version: {ex.Message}");
        }
    }

    private class SystemSettingsResponse
    {
        public SessionVersionDto? SystemSettings { get; set; }
    }

    private class SessionVersionDto
    {
        public int SessionVersion { get; set; }
    }

    public async Task<OtpResponse> SendOtpAsync(string email)
    {
        var query = """
            mutation SendOtp($input: SendOtpInput!) {
              sendOtp(input: $input) {
                success
                errors
              }
            }
            """;

        var variables = new { input = new { email } };
        var response = await _graphQLService.SendQueryAsync<SendOtpResponse>(query, variables);

        return response?.SendOtp ?? new OtpResponse(false, new[] { "Network error occurred" });
    }

    public async Task<AuthResponse> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var query = """
            mutation VerifyOtp($input: VerifyOtpInput!) {
              verifyOtp(input: $input) {
                success
                token
                refreshToken
                user {
                  id
                  email
                  firstName
                  lastName
                  role
                  emailConfirmed
                }
                errors
              }
            }
            """;

        var variables = new { input = request };
        var response = await _graphQLService.SendQueryAsync<VerifyOtpResponse>(query, variables);

        var authResponse = response?.VerifyOtp ?? new AuthResponse(false, null, null, null, new[] { "Network error occurred" });

        if (authResponse.Success && authResponse.Token != null)
        {
            await _localStorage.SetItemAsync("token", authResponse.Token);
            await _localStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);
            await _localStorage.SetItemAsync("user", authResponse.User);

            // Set session as active for this application instance
            await _authStateProvider.SetSessionActiveAsync();

            // Store current session version
            await StoreSessionVersionAsync();

            await _authStateProvider.MarkUserAsAuthenticated(authResponse.User!, authResponse.Token);
        }

        return authResponse;
    }


    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var query = """
            mutation ChangePassword($input: ChangePasswordInput!) {
              changePassword(input: $input)
            }
            """;

        var variables = new { input = request };
        var response = await _graphQLService.SendQueryAsync<ChangePasswordResponse>(query, variables);

        return response?.ChangePassword ?? false;
    }

    public async Task<(string? email, string? password, bool rememberMe)> GetRememberedCredentialsAsync()
    {
        var rememberMeEnabled = await _localStorage.GetItemAsync<bool>("rememberMeEnabled");
        if (!rememberMeEnabled)
            return (null, null, false);

        var email = await _localStorage.GetItemAsync<string>("rememberedEmail");
        var password = await _localStorage.GetItemAsync<string>("rememberedPassword");

        return (email, password, true);
    }

    public async Task ClearRememberedCredentialsAsync()
    {
        await _localStorage.RemoveItemAsync("rememberedEmail");
        await _localStorage.RemoveItemAsync("rememberedPassword");
        await _localStorage.RemoveItemAsync("rememberMeEnabled");
    }

    public async Task<AuthResponse> VerifyAuthenticatorCodeAsync(string email, string code)
    {
        var query = """
            mutation VerifyAuthenticatorCode($email: String!, $code: String!) {
              verifyAuthenticatorCode(email: $email, code: $code) {
                success
                token
                refreshToken
                user {
                  id
                  email
                  firstName
                  lastName
                  role
                  emailConfirmed
                  twoFactorEnabled
                }
                errors
              }
            }
            """;

        var variables = new { email, code };
        var response = await _graphQLService.SendQueryAsync<VerifyAuthenticatorCodeResponse>(query, variables);

        var authResponse = response?.VerifyAuthenticatorCode ?? new AuthResponse(false, null, null, null, new[] { "Network error occurred" });

        if (authResponse.Success && authResponse.Token != null)
        {
            await _localStorage.SetItemAsync("token", authResponse.Token);
            await _localStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);
            await _localStorage.SetItemAsync("user", authResponse.User);

            // Set session as active for this application instance
            await _authStateProvider.SetSessionActiveAsync();

            // Store current session version
            await StoreSessionVersionAsync();

            await _authStateProvider.MarkUserAsAuthenticated(authResponse.User!, authResponse.Token);
        }

        return authResponse;
    }
}
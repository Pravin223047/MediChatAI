using Blazored.LocalStorage;
using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.State;
using MediChatAI_BlazorWebAssembly.Core.Services.Session;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Services;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace MediChatAI_BlazorWebAssembly.Features.Authentication.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITokenValidationService _tokenValidationService;
    private readonly SessionNotificationState _notificationState;
    private const string APP_SESSION_KEY = "app_session_id";
    private const string SESSION_VERSION_KEY = "sessionVersion";

    public CustomAuthStateProvider(
        ILocalStorageService localStorage,
        IServiceProvider serviceProvider,
        ITokenValidationService tokenValidationService,
        SessionNotificationState notificationState)
    {
        _localStorage = localStorage;
        _serviceProvider = serviceProvider;
        _tokenValidationService = tokenValidationService;
        _notificationState = notificationState;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Check if this is a new application session or if session is invalid
            var isSessionValid = await CheckAndValidateSessionAsync();

            if (!isSessionValid)
            {
                await ClearAllAuthDataAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var user = await _localStorage.GetItemAsync<UserInfo>("user");
            var token = await _localStorage.GetItemAsync<string>("token");

            if (user == null || string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Validate token expiration
            if (_tokenValidationService.IsTokenExpired(token))
            {
                Console.WriteLine("Token expired in GetAuthenticationStateAsync");
                await ClearAllAuthDataAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            return new AuthenticationState(claimsPrincipal);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAuthenticationStateAsync: {ex.Message}");
            await ClearAllAuthDataAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task MarkUserAsAuthenticated(UserInfo user, string token)
    {
        // Reset notification state for new login session
        _notificationState.ResetForNewSession();
        Console.WriteLine("CustomAuthStateProvider: Reset notification state for new login");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        var identity = new ClaimsIdentity(claims, "jwt");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));

        // Start SignalR connection for notifications
        try
        {
            var notificationService = _serviceProvider.GetService(typeof(INotificationService)) as INotificationService;
            if (notificationService != null)
            {
                await notificationService.StartAsync(token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting notification service: {ex.Message}");
        }
    }

    public async Task MarkUserAsLoggedOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));

        // Stop SignalR connection
        try
        {
            var notificationService = _serviceProvider.GetService(typeof(INotificationService)) as INotificationService;
            if (notificationService != null)
            {
                await notificationService.StopAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping notification service: {ex.Message}");
        }
    }

    public async Task SetSessionActiveAsync()
    {
        // Generate a unique session ID for this application instance
        var sessionId = Guid.NewGuid().ToString();
        await _localStorage.SetItemAsync(APP_SESSION_KEY, sessionId);
    }

    public async Task SetSessionVersionAsync(int version)
    {
        await _localStorage.SetItemAsync(SESSION_VERSION_KEY, version);
    }

    private async Task<bool> CheckAndValidateSessionAsync()
    {
        try
        {
            var storedSessionId = await _localStorage.GetItemAsync<string>(APP_SESSION_KEY);

            // If no session ID exists, this is a new session - clear all auth data
            if (string.IsNullOrEmpty(storedSessionId))
            {
                Console.WriteLine("No session ID found - clearing auth data");
                return false;
            }

            // Check token validity
            var token = await _localStorage.GetItemAsync<string>("token");
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("No token found");
                return false;
            }

            // Validate token expiration
            if (_tokenValidationService.IsTokenExpired(token))
            {
                Console.WriteLine("Token is expired");
                return false;
            }

            // Session version is validated on login and via 401 errors on API calls
            // No need to check on every auth state to avoid performance issues
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating session: {ex.Message}");
            return false;
        }
    }

    private async Task ClearAllAuthDataAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync("token");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _localStorage.RemoveItemAsync("user");
            await _localStorage.RemoveItemAsync(APP_SESSION_KEY);
            await _localStorage.RemoveItemAsync(SESSION_VERSION_KEY);

            // Only clear remembered credentials if not remember me enabled
            var rememberMe = await _localStorage.GetItemAsync<bool>("rememberMeEnabled");
            if (!rememberMe)
            {
                await _localStorage.RemoveItemAsync("rememberedEmail");
                await _localStorage.RemoveItemAsync("rememberedPassword");
                await _localStorage.RemoveItemAsync("rememberMeEnabled");
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }
}
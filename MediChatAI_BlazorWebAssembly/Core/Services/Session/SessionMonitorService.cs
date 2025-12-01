using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Services;
using MediChatAI_BlazorWebAssembly.Core.Services.State;

namespace MediChatAI_BlazorWebAssembly.Core.Services.Session;

public class SessionMonitorService : ISessionMonitorService, IDisposable
{
    private readonly ITokenValidationService _tokenValidationService;
    private readonly ILocalStorageService _localStorage;
    private readonly IAuthService _authService;
    private readonly NavigationManager _navigationManager;
    private readonly SessionNotificationState _notificationState;
    private Timer? _monitoringTimer;
    private bool _isMonitoring = false;
    private const int CHECK_INTERVAL_SECONDS = 10; // Check every 10 seconds for better precision
    private const int FINAL_WARNING_SECONDS = 60; // Warn at 1 minute (60 seconds) before expiry

    public event EventHandler<TimeSpan>? SessionStarted;
    public event EventHandler<TimeSpan>? SessionExpiringSoon;
    public event EventHandler? SessionExpired;

    public SessionMonitorService(
        ITokenValidationService tokenValidationService,
        ILocalStorageService localStorage,
        IAuthService authService,
        NavigationManager navigationManager,
        SessionNotificationState notificationState)
    {
        _tokenValidationService = tokenValidationService;
        _localStorage = localStorage;
        _authService = authService;
        _navigationManager = navigationManager;
        _notificationState = notificationState;
    }

    public async Task StartMonitoringAsync()
    {
        // Check if monitoring is already active using singleton state
        if (_notificationState.IsMonitoring)
        {
            Console.WriteLine("Session monitoring already running - skipping initialization");
            return;
        }

        _isMonitoring = true;
        _notificationState.IsMonitoring = true;

        // Get initial time remaining
        var timeRemaining = await GetTimeUntilExpirationAsync();

        if (timeRemaining.HasValue && timeRemaining.Value.TotalSeconds > 0)
        {
            // Check if this is a new session (time remaining > warning threshold)
            if (timeRemaining.Value.TotalSeconds > FINAL_WARNING_SECONDS)
            {
                // Check if we've already shown the session start notification (global state)
                if (!_notificationState.HasShownSessionStartNotification)
                {
                    // Fire SessionStarted event for initial notification (only once per session)
                    Console.WriteLine($"Session started with {timeRemaining.Value.TotalMinutes:F0} minutes remaining");
                    _notificationState.HasShownSessionStartNotification = true;
                    SessionStarted?.Invoke(this, timeRemaining.Value);
                }
                else
                {
                    Console.WriteLine($"Session start notification already shown - skipping");
                }
            }
            else
            {
                // Session is close to expiring, check if we need to show warning
                Console.WriteLine($"Session monitoring started with {timeRemaining.Value.TotalSeconds:F0} seconds remaining (already in warning period)");
            }
        }

        // Perform initial check
        await CheckSessionValidityAsync();

        // Start periodic monitoring
        _monitoringTimer = new Timer(async _ => await CheckSessionValidityAsync(),
            null,
            TimeSpan.FromSeconds(CHECK_INTERVAL_SECONDS),
            TimeSpan.FromSeconds(CHECK_INTERVAL_SECONDS));
    }

    public void StopMonitoring()
    {
        _isMonitoring = false;
        _notificationState.ResetMonitoring(); // Reset global monitoring state
        _monitoringTimer?.Dispose();
        _monitoringTimer = null;
    }

    public async Task<TimeSpan?> GetTimeUntilExpirationAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            return _tokenValidationService.GetTimeUntilExpiration(token);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsSessionValidAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            return _tokenValidationService.IsTokenValid(token);
        }
        catch
        {
            return false;
        }
    }

    private async Task CheckSessionValidityAsync()
    {
        if (!_isMonitoring)
            return;

        try
        {
            var token = await _localStorage.GetItemAsync<string>("token");

            if (string.IsNullOrEmpty(token))
            {
                StopMonitoring();
                return;
            }

            // Check if token is expired
            if (_tokenValidationService.IsTokenExpired(token))
            {
                Console.WriteLine("Session expired - logging out user");
                await HandleSessionExpiredAsync();
                return;
            }

            // Check if token is expiring soon - show warning only at 1 minute remaining
            var timeUntilExpiration = _tokenValidationService.GetTimeUntilExpiration(token);
            if (timeUntilExpiration.HasValue)
            {
                var totalSeconds = timeUntilExpiration.Value.TotalSeconds;

                // Show final warning when exactly at or below 60 seconds (1 minute)
                // Check global state to ensure we only show once per session
                if (totalSeconds <= FINAL_WARNING_SECONDS && totalSeconds > 0)
                {
                    if (!_notificationState.HasShownFinalWarning)
                    {
                        _notificationState.HasShownFinalWarning = true;
                        Console.WriteLine($"FINAL WARNING: Session expiring in 1 minute");
                        SessionExpiringSoon?.Invoke(this, timeUntilExpiration.Value);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking session validity: {ex.Message}");
        }
    }

    private async Task HandleSessionExpiredAsync()
    {
        StopMonitoring();
        SessionExpired?.Invoke(this, EventArgs.Empty);

        // Reset all notification flags for next login
        _notificationState.ResetForNewSession();

        // Logout the user
        await _authService.LogoutAsync();

        // Navigate to login page - clean redirect without query parameters
        // The toast notification already informs the user about session expiration
        _navigationManager.NavigateTo("/login", forceLoad: true);
    }

    public void Dispose()
    {
        StopMonitoring();
    }
}

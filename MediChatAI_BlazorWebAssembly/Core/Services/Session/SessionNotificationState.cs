namespace MediChatAI_BlazorWebAssembly.Core.Services.Session;

/// <summary>
/// Singleton state manager for session notifications
/// Tracks global flags to prevent duplicate notifications across app lifetime
/// Thread-safe, no dependencies on scoped services
/// </summary>
public class SessionNotificationState
{
    private readonly object _lock = new();
    private bool _hasShownSessionStartNotification = false;
    private bool _hasShownFinalWarning = false;
    private bool _isMonitoring = false;

    /// <summary>
    /// Gets or sets whether the session start notification has been shown
    /// </summary>
    public bool HasShownSessionStartNotification
    {
        get
        {
            lock (_lock)
            {
                return _hasShownSessionStartNotification;
            }
        }
        set
        {
            lock (_lock)
            {
                _hasShownSessionStartNotification = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the final warning (1 minute) has been shown
    /// </summary>
    public bool HasShownFinalWarning
    {
        get
        {
            lock (_lock)
            {
                return _hasShownFinalWarning;
            }
        }
        set
        {
            lock (_lock)
            {
                _hasShownFinalWarning = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets whether session monitoring is currently active
    /// </summary>
    public bool IsMonitoring
    {
        get
        {
            lock (_lock)
            {
                return _isMonitoring;
            }
        }
        set
        {
            lock (_lock)
            {
                _isMonitoring = value;
            }
        }
    }

    /// <summary>
    /// Resets all notification flags for a new session
    /// Call this when user logs in or session is refreshed
    /// </summary>
    public void ResetForNewSession()
    {
        lock (_lock)
        {
            _hasShownSessionStartNotification = false;
            _hasShownFinalWarning = false;
            _isMonitoring = false;
            Console.WriteLine("SessionNotificationState: Reset for new session");
        }
    }

    /// <summary>
    /// Resets monitoring state when user logs out
    /// Preserves notification history until next login
    /// </summary>
    public void ResetMonitoring()
    {
        lock (_lock)
        {
            _isMonitoring = false;
            Console.WriteLine("SessionNotificationState: Monitoring stopped");
        }
    }
}

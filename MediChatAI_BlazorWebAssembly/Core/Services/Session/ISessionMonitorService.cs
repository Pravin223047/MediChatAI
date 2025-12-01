namespace MediChatAI_BlazorWebAssembly.Core.Services.Session;

public interface ISessionMonitorService
{
    /// <summary>
    /// Event fired when session monitoring starts with the initial time remaining
    /// </summary>
    event EventHandler<TimeSpan>? SessionStarted;

    /// <summary>
    /// Event fired when the session is about to expire (1 minute warning)
    /// </summary>
    event EventHandler<TimeSpan>? SessionExpiringSoon;

    /// <summary>
    /// Event fired when the session has expired
    /// </summary>
    event EventHandler? SessionExpired;

    /// <summary>
    /// Starts monitoring the session
    /// </summary>
    Task StartMonitoringAsync();

    /// <summary>
    /// Stops monitoring the session
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// Gets the time remaining until session expiration
    /// </summary>
    Task<TimeSpan?> GetTimeUntilExpirationAsync();

    /// <summary>
    /// Checks if the current session is valid
    /// </summary>
    Task<bool> IsSessionValidAsync();
}

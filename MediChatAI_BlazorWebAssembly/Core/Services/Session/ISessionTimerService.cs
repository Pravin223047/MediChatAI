namespace MediChatAI_BlazorWebAssembly.Core.Services.Session;

public interface ISessionTimerService
{
    /// <summary>
    /// Event fired every second with the remaining time until session expiration
    /// </summary>
    event EventHandler<TimeSpan>? OnTimerTick;

    /// <summary>
    /// Event fired when the session timer starts
    /// </summary>
    event EventHandler? OnTimerStarted;

    /// <summary>
    /// Event fired when the session timer stops
    /// </summary>
    event EventHandler? OnTimerStopped;

    /// <summary>
    /// Starts the session timer
    /// </summary>
    Task StartTimerAsync();

    /// <summary>
    /// Stops the session timer
    /// </summary>
    void StopTimer();

    /// <summary>
    /// Gets the current time remaining until session expiration
    /// </summary>
    Task<TimeSpan?> GetTimeRemainingAsync();

    /// <summary>
    /// Checks if the timer is currently running
    /// </summary>
    bool IsRunning { get; }
}

namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Models;

/// <summary>
/// Defines the severity levels for error messages in the Emergency Chatbot
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Informational message - no action required
    /// </summary>
    Info,

    /// <summary>
    /// Warning message - user should be aware but can continue
    /// </summary>
    Warning,

    /// <summary>
    /// Error message - something went wrong, retry may help
    /// </summary>
    Error,

    /// <summary>
    /// Critical error - immediate action required (call 911)
    /// </summary>
    Critical
}

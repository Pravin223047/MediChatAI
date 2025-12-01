namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Models;

/// <summary>
/// Represents a user-friendly error message with severity and suggested actions
/// </summary>
public class ErrorMessage
{
    /// <summary>
    /// The user-friendly error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The severity level of the error
    /// </summary>
    public ErrorSeverity Severity { get; set; }

    /// <summary>
    /// Suggested actions the user can take
    /// </summary>
    public List<ErrorAction> SuggestedActions { get; set; } = new();

    /// <summary>
    /// Icon to display with the error (emoji or icon name)
    /// </summary>
    public string Icon { get; set; } = "⚠️";

    /// <summary>
    /// Whether this error should auto-dismiss after a timeout
    /// </summary>
    public bool AutoDismiss { get; set; } = false;

    /// <summary>
    /// Auto-dismiss timeout in seconds (if AutoDismiss is true)
    /// </summary>
    public int AutoDismissSeconds { get; set; } = 5;
}

/// <summary>
/// Represents an action the user can take in response to an error
/// </summary>
public class ErrorAction
{
    /// <summary>
    /// The label for the action button
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The action type
    /// </summary>
    public ErrorActionType ActionType { get; set; }

    /// <summary>
    /// Optional URL for phone/link actions
    /// </summary>
    public string? ActionData { get; set; }
}

/// <summary>
/// Types of actions that can be taken in response to an error
/// </summary>
public enum ErrorActionType
{
    Retry,
    Dismiss,
    Call911,
    ClearChat,
    Reload,
    SimplifyQuestion,
    ContactSupport
}

namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Models;

/// <summary>
/// Represents a message waiting in the queue to be sent
/// </summary>
public class PendingMessage
{
    /// <summary>
    /// Unique identifier for this pending message
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The message content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Whether this message came from voice input
    /// </summary>
    public bool IsVoiceInput { get; set; }

    /// <summary>
    /// Timestamp when the message was queued
    /// </summary>
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of retry attempts for this message
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Maximum number of retries allowed
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Whether this message has been cancelled
    /// </summary>
    public bool IsCancelled { get; set; } = false;
}

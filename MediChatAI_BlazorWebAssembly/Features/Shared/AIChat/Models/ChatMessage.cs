namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Models;

public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // "user" or "assistant"
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MessageType Type { get; set; } = MessageType.Text;
    public List<ChatAttachment>? Attachments { get; set; }
    public string? VoiceUrl { get; set; }
    public bool IsLoading { get; set; }
    public ChatMessageMetadata? Metadata { get; set; }
}

public enum MessageType
{
    Text,
    Image,
    Voice,
    SymptomAnalysis,
    ImageAnalysis,
    System
}

public class ChatAttachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class ChatMessageMetadata
{
    public SymptomAnalysisResult? SymptomAnalysis { get; set; }
    public ImageAnalysisResult? ImageAnalysis { get; set; }
    public List<string>? SuggestedActions { get; set; }
    public string? UrgencyLevel { get; set; } // "low", "medium", "high", "emergency"
    public Dictionary<string, string>? AdditionalData { get; set; }
}

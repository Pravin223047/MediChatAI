namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.DTOs;

// GraphQL Request DTOs
public class SendChatMessageInput
{
    public string? SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? VoiceUrl { get; set; }
    public string? MessageType { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}

public class AnalyzeSymptomsInput
{
    public List<string> Symptoms { get; set; } = new();
    public string? Duration { get; set; }
    public string? Severity { get; set; }
    public string? Age { get; set; }
    public string? Gender { get; set; }
    public Dictionary<string, string>? AdditionalInfo { get; set; }
}

public class AnalyzeImageInput
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? ImageType { get; set; }
    public string? Context { get; set; }
    public Dictionary<string, string>? AdditionalInfo { get; set; }
}

public class SaveConversationInput
{
    public string SessionId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
}

// GraphQL Response DTOs
public class ChatMessageResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public ChatMessageDto? ChatMessage { get; set; }
}

public class ChatMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? MessageType { get; set; }
    public List<AttachmentDto>? Attachments { get; set; }
    public MetadataDto? Metadata { get; set; }
}

public class AttachmentDto
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class MetadataDto
{
    public SymptomAnalysisDto? SymptomAnalysis { get; set; }
    public ImageAnalysisDto? ImageAnalysis { get; set; }
    public List<string>? SuggestedActions { get; set; }
    public string? UrgencyLevel { get; set; }
}

public class SymptomAnalysisDto
{
    public string Id { get; set; } = string.Empty;
    public List<string> Symptoms { get; set; } = new();
    public List<PossibleConditionDto> PossibleConditions { get; set; } = new();
    public string UrgencyLevel { get; set; } = string.Empty;
    public string UrgencyMessage { get; set; } = string.Empty;
    public List<string> RecommendedActions { get; set; } = new();
}

public class PossibleConditionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Probability { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public class ImageAnalysisDto
{
    public string Id { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public List<FindingDto> Findings { get; set; } = new();
    public string Confidence { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}

public class FindingDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class SavedConversationResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public SavedConversationDto? Conversation { get; set; }
}

public class SavedConversationDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int MessageCount { get; set; }
}

public class ConversationHistoryResponse
{
    public List<SavedConversationDto> Conversations { get; set; } = new();
}

public class ConversationMessagesResponse
{
    public List<ChatMessageDto> Messages { get; set; } = new();
}

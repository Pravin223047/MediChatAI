namespace MediChatAI_GraphQl.Features.AIChat.DTOs;

// ============================================
// INPUT DTOS
// ============================================

public record SendAIChatMessageInput(
    string? SessionId,
    string Message,
    string? ImageUrl,
    string? VoiceUrl,
    string? MessageType
);

public record AnalyzeSymptomsInput(
    List<string> Symptoms,
    string? Duration,
    string? Severity
);

public record AnalyzeImageInput(
    string ImageUrl,
    string? ImageType,
    string? Context
);

public record SaveConversationInput(
    string SessionId,
    string Title,
    string? Summary
);

// ============================================
// RESPONSE DTOS
// ============================================

public class AIChatMessageResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AIChatMessageDto? ChatMessage { get; set; }
}

public class AIChatMessageDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SessionId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? MessageType { get; set; }
    public List<AIChatAttachmentDto>? Attachments { get; set; }
    public AIChatMetadataDto? Metadata { get; set; }
}

public class AIChatAttachmentDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class AIChatMetadataDto
{
    public AIChatSymptomAnalysisDto? SymptomAnalysis { get; set; }
    public AIChatImageAnalysisDto? ImageAnalysis { get; set; }
    public List<string>? SuggestedActions { get; set; }
    public string? UrgencyLevel { get; set; }
}

// ============================================
// SYMPTOM ANALYSIS DTOS
// ============================================

public class AIChatSymptomAnalysisResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AIChatSymptomAnalysisDto? Analysis { get; set; }
}

public class AIChatSymptomAnalysisDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<string> Symptoms { get; set; } = new();
    public List<AIChatPossibleConditionDto> PossibleConditions { get; set; } = new();
    public string UrgencyLevel { get; set; } = string.Empty;
    public string UrgencyMessage { get; set; } = string.Empty;
    public List<string> RecommendedActions { get; set; } = new();
    public List<string> Questions { get; set; } = new();
    public string Disclaimer { get; set; } = "This is an AI-generated analysis for educational purposes only. Always consult a healthcare professional for accurate medical advice.";
}

public class AIChatPossibleConditionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Probability { get; set; }
    public string Severity { get; set; } = string.Empty;
    public List<string> CommonSymptoms { get; set; } = new();
    public string? WhenToSeekCare { get; set; }
}

// ============================================
// IMAGE ANALYSIS DTOS
// ============================================

public class AIChatImageAnalysisResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public AIChatImageAnalysisDto? Analysis { get; set; }
}

public class AIChatImageAnalysisDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ImageUrl { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public string Confidence { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public List<AIChatFindingDto> Findings { get; set; } = new();
}

public class AIChatFindingDto
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? Notes { get; set; }
}

// ============================================
// CONVERSATION MANAGEMENT DTOS
// ============================================

public class SaveConversationResponse
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

public class DeleteConversationResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

public class SuggestedQuestionsResponse
{
    public List<string> Questions { get; set; } = new();
}

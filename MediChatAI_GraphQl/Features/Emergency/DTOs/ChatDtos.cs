namespace MediChatAI_GraphQl.Features.Emergency.DTOs;

/// <summary>
/// Represents a chat message in the emergency chat system
/// </summary>
public class ChatMessageDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public bool IsUserMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? SessionId { get; set; }
    public ChatMessageMetadata? Metadata { get; set; }
}

/// <summary>
/// Metadata associated with a chat message
/// </summary>
public class ChatMessageMetadata
{
    public bool HasVoiceInput { get; set; }
    public string? UserRole { get; set; }
    public string? UserId { get; set; }
    public double? UserLatitude { get; set; }
    public double? UserLongitude { get; set; }
}

/// <summary>
/// Input for sending a chat message
/// </summary>
public class SendChatMessageInput
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public ChatContextDto? Context { get; set; }
    public bool IsVoiceInput { get; set; }
}

/// <summary>
/// Context information for the AI chat
/// </summary>
public class ChatContextDto
{
    public double? UserLatitude { get; set; }
    public double? UserLongitude { get; set; }
    public List<HospitalContextDto>? AvailableHospitals { get; set; }
    public string? EmergencyType { get; set; }
    public string? UserRole { get; set; }
    public string? UserId { get; set; }
}

/// <summary>
/// Simplified hospital data for chat context
/// </summary>
public class HospitalContextDto
{
    public string Name { get; set; } = string.Empty;
    public double? Rating { get; set; }
    public double? DistanceInKm { get; set; }
    public bool IsOpen { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Type { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool HasEmergencyDepartment { get; set; }
    public string? Specializations { get; set; }
}

/// <summary>
/// Response from the AI chat service
/// </summary>
public class ChatResponseDto
{
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<QuickActionDto>? SuggestedActions { get; set; }
    public List<HospitalRecommendationDto>? HospitalRecommendations { get; set; }
    public string? DetectedLanguage { get; set; }
    public ChatMetrics? Metrics { get; set; }
}

/// <summary>
/// Quick action suggestions from the AI
/// </summary>
public class QuickActionDto
{
    public string Label { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Hospital recommendation from AI analysis
/// </summary>
public class HospitalRecommendationDto
{
    public string HospitalName { get; set; } = string.Empty;
    public string ReasonForRecommendation { get; set; } = string.Empty;
    public double? DistanceInKm { get; set; }
    public double? Rating { get; set; }
    public int Priority { get; set; } // 1 = highest priority
    public string? PhoneNumber { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

/// <summary>
/// Metrics for chat performance
/// </summary>
public class ChatMetrics
{
    public int ResponseTimeMs { get; set; }
    public int ConfidenceScore { get; set; }
    public bool RequiresFollowUp { get; set; }
    public string? SentimentAnalysis { get; set; }
}

/// <summary>
/// Chat history request
/// </summary>
public class ChatHistoryRequest
{
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public int Limit { get; set; } = 50;
    public DateTime? Since { get; set; }
}

/// <summary>
/// Chat history response
/// </summary>
public class ChatHistoryResponse
{
    public List<ChatMessageDto> Messages { get; set; } = new();
    public string? SessionId { get; set; }
    public DateTime? OldestMessageTimestamp { get; set; }
    public DateTime? NewestMessageTimestamp { get; set; }
    public int TotalMessages { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

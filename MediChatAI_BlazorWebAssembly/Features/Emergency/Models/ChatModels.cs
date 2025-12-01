namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Models;

// Frontend DTOs matching backend structure

public class ChatMessageDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public bool IsUserMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? SessionId { get; set; }
}

public class SendChatMessageInput
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public ChatContextDto? Context { get; set; }
    public bool IsVoiceInput { get; set; }
}

public class ChatContextDto
{
    public double? UserLatitude { get; set; }
    public double? UserLongitude { get; set; }
    public List<HospitalContextDto>? AvailableHospitals { get; set; }
    public string? EmergencyType { get; set; }
    public string? UserRole { get; set; }
    public string? UserId { get; set; }
}

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

public class ChatResponseDto
{
    public string MessageId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<QuickActionDto>? SuggestedActions { get; set; }
    public List<HospitalRecommendationDto>? HospitalRecommendations { get; set; }
    public string? DetectedLanguage { get; set; }
}

public class QuickActionDto
{
    public string Label { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? PhoneNumber { get; set; }
}

public class HospitalRecommendationDto
{
    public string HospitalName { get; set; } = string.Empty;
    public string ReasonForRecommendation { get; set; } = string.Empty;
    public double? DistanceInKm { get; set; }
    public double? Rating { get; set; }
    public int Priority { get; set; }
    public string? PhoneNumber { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

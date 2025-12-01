namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Models;

public class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = "New Conversation";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    public List<ChatMessage> Messages { get; set; } = new();
    public bool IsSaved { get; set; }
    public string? Summary { get; set; }
    public Dictionary<string, object>? Context { get; set; }
    public SessionType Type { get; set; } = SessionType.General;
}

public enum SessionType
{
    General,
    SymptomCheck,
    ImageAnalysis,
    MedicationQuery,
    DoctorRecommendation,
    HealthEducation,
    ClinicalDecisionSupport, // For doctors
    PatientCaseAnalysis // For doctors
}

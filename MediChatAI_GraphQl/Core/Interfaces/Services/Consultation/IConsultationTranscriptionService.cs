namespace MediChatAI_GraphQl.Core.Interfaces.Services.Consultation;

public interface IConsultationTranscriptionService
{
    // Transcription
    Task<string?> TranscribeAudioAsync(string audioUrl);
    Task<string?> TranscribeRecordingAsync(int recordingId);

    // Summary Generation
    Task<string?> GenerateConsultationSummaryAsync(int sessionId);
    Task<string?> GenerateSummaryFromTranscriptAsync(string transcript, string? clinicalNotes = null);

    // Extract Clinical Data
    Task<ExtractedClinicalDataDto?> ExtractClinicalDataFromTranscriptAsync(string transcript);

    // Follow-up Recommendations
    Task<List<string>> GenerateFollowUpRecommendationsAsync(int sessionId);
}

public class ExtractedClinicalDataDto
{
    public string? ChiefComplaint { get; set; }
    public List<string> Symptoms { get; set; } = new();
    public string? Diagnosis { get; set; }
    public List<string> Medications { get; set; } = new();
    public string? TreatmentPlan { get; set; }
    public List<string> FollowUpInstructions { get; set; } = new();
    public string? NextSteps { get; set; }
}

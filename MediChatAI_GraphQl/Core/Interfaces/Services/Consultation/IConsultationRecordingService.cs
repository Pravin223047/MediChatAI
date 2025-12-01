using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Consultation;

public interface IConsultationRecordingService
{
    // Recording Management
    Task<ConsultationRecordingDto> StartRecordingAsync(int sessionId, string recordedByUserId);
    Task<ConsultationRecordingDto> StopRecordingAsync(int recordingId);
    Task<ConsultationRecordingDto?> GetRecordingByIdAsync(int recordingId);
    Task<List<ConsultationRecordingDto>> GetRecordingsBySessionAsync(int sessionId);
    Task<List<ConsultationRecordingDto>> GetRecordingsByPatientAsync(string patientId);

    // Upload & Processing
    Task<ConsultationRecordingDto> UploadRecordingAsync(int recordingId, Stream fileStream, string fileName);
    Task<bool> GenerateThumbnailAsync(int recordingId);
    Task<bool> UpdateRecordingMetadataAsync(int recordingId, long fileSizeBytes, int durationSeconds, string? format, string? resolution);

    // Transcription
    Task<bool> UpdateTranscriptAsync(int recordingId, string transcriptText, string? transcriptUrl = null);
    Task<bool> UpdateAISummaryAsync(int recordingId, string aiSummary);

    // Access Control
    Task<bool> UpdateAccessPermissionsAsync(int recordingId, bool patientAccess, bool doctorAccess);
    Task<bool> DeleteRecordingAsync(int recordingId);

    // Status
    Task<bool> UpdateRecordingStatusAsync(int recordingId, Core.Entities.RecordingStatus status, string? errorMessage = null);
}

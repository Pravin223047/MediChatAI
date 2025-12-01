using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Consultation;

public interface IConsultationSessionService
{
    // Session Management
    Task<ConsultationSessionDto> CreateConsultationSessionAsync(int appointmentId, string doctorId, string patientId);
    Task<ConsultationSessionDto> StartConsultationAsync(int sessionId);
    Task<ConsultationSessionDto> EndConsultationAsync(int sessionId);
    Task<ConsultationSessionDto?> GetConsultationSessionByIdAsync(int sessionId);
    Task<ConsultationSessionDto?> GetActiveConsultationByAppointmentAsync(int appointmentId);
    Task<ConsultationSessionDto?> GetConsultationByRoomIdAsync(string roomId);

    // History & Lists
    Task<List<ConsultationSessionDto>> GetConsultationsByDoctorAsync(string doctorId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<ConsultationSessionDto>> GetConsultationsByPatientAsync(string patientId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<ConsultationSessionDto>> GetSessionsByDoctorIdAsync(string doctorId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<ConsultationSessionDto>> GetSessionsByPatientIdAsync(string patientId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<ConsultationSessionDto>> GetActiveConsultationsAsync();

    // Clinical Notes
    Task<ConsultationSessionDto> UpdateConsultationNotesAsync(
        int sessionId,
        string? chiefComplaint = null,
        string? observations = null,
        string? diagnosis = null,
        string? treatmentPlan = null,
        string? followUpInstructions = null,
        DateTime? nextFollowUpDate = null);
    Task<bool> UpdateClinicalNotesAsync(int sessionId, string? chiefComplaint, string? observations, string? diagnosis, string? treatmentPlan);

    // Participants
    Task<ConsultationParticipantDto> AddParticipantAsync(int sessionId, CreateParticipantDto participant);
    Task<bool> RemoveParticipantAsync(int sessionId, int participantId, string? reason = null);
    Task<List<ConsultationParticipantDto>> GetParticipantsAsync(int sessionId);
    Task<List<ConsultationParticipantDto>> GetSessionParticipantsAsync(int sessionId);
    Task<ConsultationParticipantDto?> ValidateParticipantTokenAsync(string token);
    Task<ConsultationParticipantDto?> VerifyParticipantTokenAsync(string token);
    Task<ConsultationParticipantDto?> JoinWithTokenAsync(string token);
    Task<bool> UpdateParticipantOnlineStatusAsync(int participantId, bool isOnline);

    // Notes Management
    Task<ConsultationNoteDto> AddConsultationNoteAsync(int sessionId, string authorId, string content, Core.Entities.NoteType noteType, bool isMarkdown, bool isPrivate);
    Task<ConsultationNoteDto> UpdateConsultationNoteAsync(int noteId, string authorId, string content, bool isPrivate);
    Task<bool> DeleteConsultationNoteAsync(int noteId, string authorId);
    Task<List<ConsultationNoteDto>> GetSessionNotesAsync(int sessionId);

    // Recording Management
    Task<bool> UpdateRecordingStatusAsync(int sessionId, bool isRecording);

    // Session Status
    Task<ConsultationSessionDto> CancelConsultationAsync(int sessionId, string reason);
    Task<bool> RateConsultationAsync(int sessionId, int rating, string? feedback = null);

    // AI & Summary
    Task<bool> GenerateAISummaryAsync(int sessionId);
    Task<bool> UpdateAISummaryAsync(int sessionId, string summary);

    // Meeting Link
    Task<string> GenerateMeetingLinkAsync(int sessionId);
}

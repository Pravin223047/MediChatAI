using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Core.Services.Consultation;

/// <summary>
/// Service for managing real-time consultation room sessions
/// </summary>
public interface IConsultationRoomService
{
    #region Session Management

    /// <summary>
    /// Creates a new consultation session from an appointment
    /// </summary>
    Task<ConsultationSessionDto?> CreateConsultationSessionAsync(int appointmentId, string doctorId, string patientId);

    /// <summary>
    /// Starts an existing consultation session
    /// </summary>
    Task<ConsultationSessionDto?> StartConsultationAsync(int sessionId);

    /// <summary>
    /// Ends an active consultation session
    /// </summary>
    Task<ConsultationSessionDto?> EndConsultationAsync(int sessionId);

    /// <summary>
    /// Gets a consultation session by its ID
    /// </summary>
    Task<ConsultationSessionDto?> GetConsultationSessionByIdAsync(int sessionId);

    /// <summary>
    /// Gets an active consultation session for an appointment
    /// </summary>
    Task<ConsultationSessionDto?> GetActiveConsultationByAppointmentAsync(int appointmentId);

    /// <summary>
    /// Gets a consultation session by its room ID
    /// </summary>
    Task<ConsultationSessionDto?> GetConsultationByRoomIdAsync(string roomId);

    /// <summary>
    /// Cancels a consultation session with a reason
    /// </summary>
    Task<ConsultationSessionDto?> CancelConsultationAsync(int sessionId, string reason);

    /// <summary>
    /// Rates a completed consultation
    /// </summary>
    Task<bool> RateConsultationAsync(int sessionId, int rating, string? feedback = null);

    #endregion

    #region History & Lists

    /// <summary>
    /// Gets all consultations for a specific doctor
    /// </summary>
    Task<List<ConsultationSessionDto>> GetConsultationsByDoctorAsync(string doctorId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets all consultations for a specific patient
    /// </summary>
    Task<List<ConsultationSessionDto>> GetConsultationsByPatientAsync(string patientId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets all active (in-progress) consultations
    /// </summary>
    Task<List<ConsultationSessionDto>> GetActiveConsultationsAsync();

    /// <summary>
    /// Gets active consultations for a specific doctor
    /// </summary>
    Task<List<ConsultationSessionDto>> GetDoctorActiveConsultationsAsync(string doctorId);

    /// <summary>
    /// Gets recent completed consultations for a specific doctor
    /// </summary>
    Task<List<ConsultationSessionDto>> GetDoctorRecentConsultationsAsync(string doctorId, int count = 10);

    #endregion

    #region Clinical Notes

    /// <summary>
    /// Updates clinical notes for a consultation session
    /// </summary>
    Task<ConsultationSessionDto?> UpdateConsultationNotesAsync(
        int sessionId,
        string? chiefComplaint = null,
        string? observations = null,
        string? diagnosis = null,
        string? treatmentPlan = null,
        string? followUpInstructions = null,
        DateTime? nextFollowUpDate = null);

    /// <summary>
    /// Adds a note to the consultation session
    /// </summary>
    Task<ConsultationNoteDto?> AddConsultationNoteAsync(int sessionId, string content, string type = "General", bool isMarkdown = false, bool isPrivate = false);

    /// <summary>
    /// Updates an existing consultation note
    /// </summary>
    Task<ConsultationNoteDto?> UpdateConsultationNoteAsync(int noteId, string content, bool isPrivate);

    /// <summary>
    /// Deletes a consultation note
    /// </summary>
    Task<bool> DeleteConsultationNoteAsync(int noteId);

    /// <summary>
    /// Gets all notes for a consultation session
    /// </summary>
    Task<List<ConsultationNoteDto>> GetSessionNotesAsync(int sessionId);

    #endregion

    #region Participants

    /// <summary>
    /// Adds a participant to the consultation session
    /// </summary>
    Task<ConsultationParticipantDto?> AddParticipantAsync(int sessionId, CreateParticipantInput participant);

    /// <summary>
    /// Removes a participant from the consultation session
    /// </summary>
    Task<bool> RemoveParticipantAsync(int sessionId, int participantId, string? reason = null);

    /// <summary>
    /// Gets all participants in a consultation session
    /// </summary>
    Task<List<ConsultationParticipantDto>> GetParticipantsAsync(int sessionId);

    /// <summary>
    /// Validates a participant invitation token
    /// </summary>
    Task<ConsultationParticipantDto?> ValidateParticipantTokenAsync(string token);

    /// <summary>
    /// Joins a consultation using an invitation token
    /// </summary>
    Task<ConsultationParticipantDto?> JoinWithTokenAsync(string token);

    /// <summary>
    /// Updates participant online status
    /// </summary>
    Task<bool> UpdateParticipantOnlineStatusAsync(int participantId, bool isOnline);

    #endregion

    #region Recording Management

    /// <summary>
    /// Updates the recording status of a consultation session
    /// </summary>
    Task<bool> UpdateRecordingStatusAsync(int sessionId, bool isRecording);

    /// <summary>
    /// Gets all recordings for a consultation session
    /// </summary>
    Task<List<ConsultationRecordingDto>> GetRecordingsBySessionAsync(int sessionId);

    #endregion

    #region AI & Summary

    /// <summary>
    /// Generates an AI summary for a completed consultation
    /// </summary>
    Task<bool> GenerateAISummaryAsync(int sessionId);

    /// <summary>
    /// Updates the AI summary for a consultation
    /// </summary>
    Task<bool> UpdateAISummaryAsync(int sessionId, string summary);

    #endregion

    #region Meeting Link

    /// <summary>
    /// Generates a meeting link for the consultation session
    /// </summary>
    Task<string?> GenerateMeetingLinkAsync(int sessionId);

    #endregion
}

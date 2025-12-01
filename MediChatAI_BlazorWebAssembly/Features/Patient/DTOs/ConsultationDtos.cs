namespace MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

#region Historical Consultations

public class ConsultationHistoryDto
{
    public int Id { get; set; }
    public string DoctorId { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string Specialization { get; set; } = "";
    public string DoctorProfileImage { get; set; } = "";
    public DateTime ConsultationDate { get; set; }
    public string ChiefComplaint { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string Observations { get; set; } = "";
    public List<string> TreatmentPlan { get; set; } = new();
    public List<ConsultationPrescriptionDto> Prescriptions { get; set; } = new();
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public bool IsRated { get; set; }
    public int? Rating { get; set; }
    public string? PatientFeedback { get; set; }
    public string ConsultationType { get; set; } = "";
}

public class ConsultationPrescriptionDto
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = "";
    public string Dosage { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string Duration { get; set; } = "";
}

#endregion

#region Real-Time Consultation Session

public class ConsultationSessionDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorSpecialization { get; set; }
    public string? DoctorProfileImage { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? PatientProfileImage { get; set; }

    public string RoomId { get; set; } = string.Empty;
    public string? MeetingLink { get; set; }
    public string Status { get; set; } = "Scheduled";

    public DateTime ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public int PlannedDurationMinutes { get; set; }
    public int? ActualDurationMinutes { get; set; }

    public bool IsRecording { get; set; }
    public DateTime? RecordingStartTime { get; set; }

    public string? ChiefComplaint { get; set; }
    public string? DoctorObservations { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextFollowUpDate { get; set; }

    public string? AISummary { get; set; }
    public string? TranscriptUrl { get; set; }

    public string? VideoQuality { get; set; }
    public string? AudioQuality { get; set; }
    public string? ConnectionIssues { get; set; }

    public int? PatientRating { get; set; }
    public string? PatientFeedback { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    public List<ConsultationParticipantDto>? Participants { get; set; }
    public List<ConsultationRecordingDto>? Recordings { get; set; }
    public int RecordingCount { get; set; }
    public int ParticipantCount { get; set; }
    public int PrescriptionCount { get; set; }
}

public class ConsultationRecordingDto
{
    public int Id { get; set; }
    public int ConsultationSessionId { get; set; }
    public string RecordingUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string Type { get; set; } = "FullSession";
    public string Status { get; set; } = "Recording";

    public long FileSizeBytes { get; set; }
    public int DurationSeconds { get; set; }
    public string? Format { get; set; }
    public string? Resolution { get; set; }

    public string? TranscriptUrl { get; set; }
    public string? TranscriptText { get; set; }
    public bool IsTranscribed { get; set; }
    public DateTime? TranscribedAt { get; set; }

    public string? AISummary { get; set; }
    public bool IsSummaryGenerated { get; set; }

    public string RecordedByUserId { get; set; } = string.Empty;
    public string RecordedByUserName { get; set; } = string.Empty;

    public bool IsPatientAccessible { get; set; }
    public bool IsDoctorAccessible { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ConsultationParticipantDto
{
    public int Id { get; set; }
    public int ConsultationSessionId { get; set; }

    public string? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Relation { get; set; }
    public string Role { get; set; } = "Observer";

    public int Permissions { get; set; }

    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsOnline { get; set; }

    public string? InvitationToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }

    public string? InvitedByUserId { get; set; }
    public string? InvitedByUserName { get; set; }
    public DateTime? InvitedAt { get; set; }

    public bool IsRemoved { get; set; }
    public DateTime? RemovedAt { get; set; }
    public string? RemovalReason { get; set; }
}

public class ConsultationNoteDto
{
    public int Id { get; set; }
    public int ConsultationSessionId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "General";
    public string? Title { get; set; }
    public bool IsMarkdown { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsSharedWithPatient { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

#endregion

#region Input DTOs

public class CreateConsultationSessionInput
{
    public int AppointmentId { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public DateTime ScheduledStartTime { get; set; }
    public int PlannedDurationMinutes { get; set; } = 30;
}

public class UpdateConsultationNotesInput
{
    public int SessionId { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? DoctorObservations { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
}

public class CreateParticipantInput
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Relation { get; set; }
    public string Role { get; set; } = "Observer";
    public int Permissions { get; set; }
    public string? InvitedByUserId { get; set; }
    public string? InvitationMessage { get; set; }
}

public class RateConsultationInput
{
    public int ConsultationId { get; set; }
    public int Rating { get; set; }
    public string? Feedback { get; set; }
}

public class GetConsultationsInput
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DoctorId { get; set; }
    public int? Limit { get; set; } = 50;
    public int? Skip { get; set; } = 0;
}

public class AddConsultationNoteInput
{
    public int SessionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "General";
    public string? Title { get; set; }
    public bool IsMarkdown { get; set; }
    public bool IsPrivate { get; set; }
}

public class UpdateRecordingStatusInput
{
    public int SessionId { get; set; }
    public bool IsRecording { get; set; }
}

#endregion

#region GraphQL Response Models

public class GetPatientConsultationsResponse
{
    public List<ConsultationHistoryDto> GetPatientConsultations { get; set; } = new();
}

public class RateConsultationResponse
{
    public bool RateConsultation { get; set; }
}

public class ConsultationSessionResponse
{
    public ConsultationSessionDto? ConsultationSession { get; set; }
}

public class ConsultationSessionsResponse
{
    public List<ConsultationSessionDto> ConsultationSessions { get; set; } = new();
}

public class ConsultationParticipantsResponse
{
    public List<ConsultationParticipantDto> Participants { get; set; } = new();
}

public class ConsultationRecordingsResponse
{
    public List<ConsultationRecordingDto> Recordings { get; set; } = new();
}

public class ConsultationNotesResponse
{
    public List<ConsultationNoteDto> Notes { get; set; } = new();
}

public class BooleanResponse
{
    public bool Success { get; set; }
}

#endregion

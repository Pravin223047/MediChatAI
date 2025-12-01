using MediChatAI_GraphQl.Core.Entities;

namespace MediChatAI_GraphQl.Shared.DTOs;

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
    public ConsultationStatus Status { get; set; }

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

    // Navigation
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
    public RecordingType Type { get; set; }
    public RecordingStatus Status { get; set; }

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
    public ConsultationParticipantRole Role { get; set; }

    public ParticipantPermission Permissions { get; set; }

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
    public NoteType Type { get; set; }
    public string? Title { get; set; }
    public bool IsMarkdown { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsSharedWithPatient { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Input DTOs for creating/updating
public class CreateConsultationSessionDto
{
    public int AppointmentId { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public DateTime ScheduledStartTime { get; set; }
    public int PlannedDurationMinutes { get; set; } = 30;
}

public class UpdateConsultationNotesDto
{
    public int SessionId { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? DoctorObservations { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
}

public class CreateParticipantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Relation { get; set; }
    public ConsultationParticipantRole Role { get; set; }
    public ParticipantPermission Permissions { get; set; }
    public string? InvitedByUserId { get; set; }
    public string? InvitationMessage { get; set; }
}

public class RateConsultationDto
{
    public int SessionId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Feedback { get; set; }
}

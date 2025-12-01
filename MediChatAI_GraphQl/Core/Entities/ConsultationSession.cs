namespace MediChatAI_GraphQl.Core.Entities;

public enum ConsultationStatus
{
    Scheduled,
    WaitingForDoctor,
    Active,
    Completed,
    Cancelled,
    NoShow
}

public class ConsultationSession
{
    public int Id { get; set; }

    // Related appointment
    public int AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    // Participants
    public string DoctorId { get; set; } = string.Empty;
    public ApplicationUser? Doctor { get; set; }

    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser? Patient { get; set; }

    // Session details
    public string RoomId { get; set; } = string.Empty; // Unique room identifier for WebRTC
    public string? MeetingLink { get; set; }
    public ConsultationStatus Status { get; set; } = ConsultationStatus.Scheduled;

    // Timing
    public DateTime ScheduledStartTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public int PlannedDurationMinutes { get; set; } = 30;
    public int? ActualDurationMinutes { get; set; }

    // Recording status
    public bool IsRecording { get; set; } = false;
    public DateTime? RecordingStartTime { get; set; }

    // Clinical notes
    public string? ChiefComplaint { get; set; }
    public string? DoctorObservations { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextFollowUpDate { get; set; }

    // AI-generated content
    public string? AISummary { get; set; }
    public string? TranscriptUrl { get; set; }

    // Quality metrics
    public string? VideoQuality { get; set; } // Poor, Fair, Good, Excellent
    public string? AudioQuality { get; set; }
    public string? ConnectionIssues { get; set; } // JSON array of issue descriptions

    // Rating and feedback
    public int? PatientRating { get; set; } // 1-5 stars
    public string? PatientFeedback { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Navigation properties
    public ICollection<ConsultationParticipant> Participants { get; set; } = new List<ConsultationParticipant>();
    public ICollection<ConsultationNote> Notes { get; set; } = new List<ConsultationNote>();
    public ICollection<ConsultationRecording> Recordings { get; set; } = new List<ConsultationRecording>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}

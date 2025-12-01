namespace MediChatAI_GraphQl.Core.Entities;

public enum ConsultationParticipantRole
{
    Doctor,
    Patient,
    FamilyMember,
    Colleague,
    Specialist,
    MedicalStudent,
    Nurse,
    Other
}

[Flags]
public enum ParticipantPermission
{
    None = 0,
    ViewMedicalHistory = 1,
    ViewPrescriptions = 2,
    AccessChat = 4,
    ViewRecording = 8,
    ViewNotes = 16,
    All = ViewMedicalHistory | ViewPrescriptions | AccessChat | ViewRecording | ViewNotes
}

public class ConsultationParticipant
{
    public int Id { get; set; }

    // Related consultation
    public int ConsultationSessionId { get; set; }
    public ConsultationSession? ConsultationSession { get; set; }

    // Participant identity
    public string? UserId { get; set; } // Nullable for guest participants
    public ApplicationUser? User { get; set; }

    // Guest participant details (for non-registered users)
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Relation { get; set; } // e.g., "Spouse", "Child", "Parent"
    public ConsultationParticipantRole Role { get; set; } = ConsultationParticipantRole.Other;

    // Permissions (bit flags for multiple permissions)
    public ParticipantPermission Permissions { get; set; } = ParticipantPermission.None;

    // Session participation
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsOnline { get; set; } = false;

    // Access token for guest participants
    public string? InvitationToken { get; set; } // Unique token for joining
    public DateTime? TokenExpiresAt { get; set; }
    public bool TokenUsed { get; set; } = false;

    // Invitation details
    public string? InvitedByUserId { get; set; }
    public ApplicationUser? InvitedBy { get; set; }
    public DateTime? InvitedAt { get; set; }
    public string? InvitationMessage { get; set; }

    // Connection metadata
    public string? ConnectionId { get; set; } // SignalR connection ID
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsRemoved { get; set; } = false;
    public DateTime? RemovedAt { get; set; }
    public string? RemovalReason { get; set; }
}

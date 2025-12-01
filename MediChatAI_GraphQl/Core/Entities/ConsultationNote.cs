namespace MediChatAI_GraphQl.Core.Entities;

public enum NoteType
{
    ChiefComplaint,
    History,
    PhysicalExamination,
    Assessment,
    Plan,
    Prescription,
    FollowUp,
    General,
    PrivateNote // Only visible to doctor
}

public class ConsultationNote
{
    public int Id { get; set; }

    // Related consultation
    public int ConsultationSessionId { get; set; }
    public ConsultationSession? ConsultationSession { get; set; }

    // Author
    public string AuthorId { get; set; } = string.Empty;
    public ApplicationUser? Author { get; set; }

    // Note content
    public string Content { get; set; } = string.Empty;
    public NoteType Type { get; set; } = NoteType.General;
    public string? Title { get; set; }

    // Formatting
    public bool IsMarkdown { get; set; } = false;
    public string? FormattedContent { get; set; } // HTML version if markdown

    // Visibility
    public bool IsPrivate { get; set; } = false; // Only author and admins can see
    public bool IsSharedWithPatient { get; set; } = true;

    // Clinical tags
    public string? Tags { get; set; } // JSON array of tags like ["vital signs", "follow-up"]

    // Attachments
    public string? AttachmentUrls { get; set; } // JSON array of URLs

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Version control
    public int Version { get; set; } = 1;
    public string? PreviousVersionContent { get; set; }

    // Audit trail
    public string? EditReason { get; set; }
    public string? EditedByUserId { get; set; }
    public ApplicationUser? EditedBy { get; set; }
    public DateTime? LastEditedAt { get; set; }
}

namespace MediChatAI_GraphQl.Core.Entities;

public enum RecordingStatus
{
    Recording,
    Processing,
    Completed,
    Failed,
    Deleted
}

public enum RecordingType
{
    AudioOnly,
    VideoWithAudio,
    ScreenShare
}

public class ConsultationRecording
{
    public int Id { get; set; }

    // Related consultation
    public int ConsultationSessionId { get; set; }
    public ConsultationSession? ConsultationSession { get; set; }

    // Recording details
    public string RecordingUrl { get; set; } = string.Empty; // Cloudinary URL
    public string? ThumbnailUrl { get; set; }
    public RecordingType Type { get; set; } = RecordingType.VideoWithAudio;
    public RecordingStatus Status { get; set; } = RecordingStatus.Recording;

    // File metadata
    public long FileSizeBytes { get; set; } = 0;
    public int DurationSeconds { get; set; } = 0;
    public string? Format { get; set; } // webm, mp4, etc.
    public string? VideoCodec { get; set; }
    public string? AudioCodec { get; set; }
    public string? Resolution { get; set; } // e.g., "1920x1080"
    public int? Bitrate { get; set; }

    // Transcription
    public string? TranscriptUrl { get; set; } // URL to transcript file or JSON
    public string? TranscriptText { get; set; } // Full text transcript
    public bool IsTranscribed { get; set; } = false;
    public DateTime? TranscribedAt { get; set; }
    public string? TranscriptionLanguage { get; set; } // e.g., "en-US"

    // AI-generated summary
    public string? AISummary { get; set; }
    public bool IsSummaryGenerated { get; set; } = false;
    public DateTime? SummaryGeneratedAt { get; set; }

    // Who initiated the recording
    public string RecordedByUserId { get; set; } = string.Empty;
    public ApplicationUser? RecordedBy { get; set; }

    // Access control
    public bool IsPatientAccessible { get; set; } = true;
    public bool IsDoctorAccessible { get; set; } = true;
    public bool IsPublic { get; set; } = false;

    // Cloudinary metadata
    public string? CloudinaryPublicId { get; set; }
    public string? CloudinaryAssetId { get; set; }

    // Timestamps
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Processing errors
    public string? ErrorMessage { get; set; }
    public int ProcessingAttempts { get; set; } = 0;
}

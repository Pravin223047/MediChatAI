using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public enum MessageType
{
    Text,
    Image,
    File,
    Audio,
    Video,
    System
}

public enum MessageStatus
{
    Sent,
    Delivered,
    Read
}

public class DoctorMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string SenderId { get; set; } = string.Empty;

    public ApplicationUser? Sender { get; set; }

    [Required]
    [MaxLength(450)]
    public string ReceiverId { get; set; } = string.Empty;

    public ApplicationUser? Receiver { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public MessageType MessageType { get; set; } = MessageType.Text;

    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public bool IsEdited { get; set; } = false;

    public DateTime? EditedAt { get; set; }

    /// <summary>
    /// File attachment URL if message contains file
    /// </summary>
    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }

    /// <summary>
    /// File name for attachments
    /// </summary>
    [MaxLength(255)]
    public string? AttachmentFileName { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? AttachmentFileSize { get; set; }

    /// <summary>
    /// MIME type of attachment
    /// </summary>
    [MaxLength(100)]
    public string? AttachmentMimeType { get; set; }

    /// <summary>
    /// Reference to parent message if this is a reply
    /// </summary>
    public Guid? ReplyToMessageId { get; set; }

    public DoctorMessage? ReplyToMessage { get; set; }

    /// <summary>
    /// Additional metadata stored as JSON
    /// </summary>
    [MaxLength(1000)]
    public string? Metadata { get; set; }

    /// <summary>
    /// Conversation thread ID to group related messages
    /// </summary>
    public Guid? ConversationId { get; set; }
}

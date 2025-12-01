using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public class GroupConversation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public string CreatedBy { get; set; } = string.Empty; // User ID who created the group

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    [MaxLength(500)]
    public string? GroupAvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public virtual ICollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
}

public class GroupMember
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid GroupConversationId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty; // AspNetUsers ID

    public string Role { get; set; } = "Member"; // Admin, Member

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Notification preferences
    public bool MuteNotifications { get; set; } = false;

    // Navigation properties
    public virtual GroupConversation GroupConversation { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}

public class GroupMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid GroupConversationId { get; set; }

    [Required]
    public string SenderId { get; set; } = string.Empty; // AspNetUsers ID

    [Required]
    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public string MessageType { get; set; } = "Text"; // Text, File, Image, Voice, Video

    [MaxLength(1000)]
    public string? AttachmentUrl { get; set; }

    [MaxLength(200)]
    public string? AttachmentName { get; set; }

    public long? AttachmentSize { get; set; }

    // Reply/Thread support
    public Guid? ReplyToMessageId { get; set; }

    // Navigation properties
    public virtual GroupConversation GroupConversation { get; set; } = null!;
    public virtual ApplicationUser Sender { get; set; } = null!;
    public virtual GroupMessage? ReplyToMessage { get; set; }
    public virtual ICollection<GroupMessageReaction> Reactions { get; set; } = new List<GroupMessageReaction>();
    public virtual ICollection<GroupMessageReadStatus> ReadStatuses { get; set; } = new List<GroupMessageReadStatus>();
}

public class GroupMessageReaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid GroupMessageId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Emoji { get; set; } = string.Empty;

    public DateTime ReactedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual GroupMessage GroupMessage { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}

public class GroupMessageReadStatus
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid GroupMessageId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual GroupMessage GroupMessage { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}

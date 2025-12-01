using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    System
}

public enum NotificationCategory
{
    Appointment,
    Doctor,
    Admin,
    Security,
    System,
    Medical,
    General
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public class Notification
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; } = NotificationType.Info;

    public NotificationCategory Category { get; set; } = NotificationCategory.General;

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public bool IsRead { get; set; } = false;

    public DateTime? ReadAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExpiresAt { get; set; }

    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    [MaxLength(100)]
    public string? ActionText { get; set; }

    /// <summary>
    /// Group related notifications together (e.g., "5 new appointments")
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// Additional metadata stored as JSON
    /// </summary>
    [MaxLength(2000)]
    public string? Metadata { get; set; }

    /// <summary>
    /// Icon class or identifier for custom icons
    /// </summary>
    [MaxLength(100)]
    public string? Icon { get; set; }
}

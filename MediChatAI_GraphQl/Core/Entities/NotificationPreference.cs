using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public class NotificationPreference
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    // Master toggles
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool InAppNotificationsEnabled { get; set; } = true;
    public bool PushNotificationsEnabled { get; set; } = true;
    public bool SoundEnabled { get; set; } = true;

    // Category-specific preferences
    public bool AppointmentNotifications { get; set; } = true;
    public bool DoctorNotifications { get; set; } = true;
    public bool AdminNotifications { get; set; } = true;
    public bool SecurityNotifications { get; set; } = true;
    public bool SystemNotifications { get; set; } = true;
    public bool MedicalNotifications { get; set; } = true;

    // Quiet hours
    public bool QuietHoursEnabled { get; set; } = false;
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }

    [MaxLength(100)]
    public string? Timezone { get; set; } = "UTC";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

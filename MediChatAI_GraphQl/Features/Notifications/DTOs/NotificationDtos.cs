using MediChatAI_GraphQl.Core.Entities;

namespace MediChatAI_GraphQl.Features.Notifications.DTOs;

public record GetNotificationsInput(
    int? Skip = 0,
    int? Take = 20,
    bool? IsRead = null,
    NotificationCategory? Category = null,
    NotificationPriority? Priority = null);

public class NotificationsResult
{
    public List<Notification> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
}

public record MarkAsReadInput(Guid NotificationId);

public record DeleteNotificationInput(Guid NotificationId);

public record UpdateNotificationPreferencesInput(
    bool EmailNotificationsEnabled,
    bool InAppNotificationsEnabled,
    bool PushNotificationsEnabled,
    bool SoundEnabled,
    bool AppointmentNotifications,
    bool DoctorNotifications,
    bool AdminNotifications,
    bool SecurityNotifications,
    bool SystemNotifications,
    bool MedicalNotifications,
    bool QuietHoursEnabled,
    TimeSpan? QuietHoursStart,
    TimeSpan? QuietHoursEnd,
    string? Timezone);

// Admin-only queries
public record GetAllNotificationsInput(
    string? UserId = null,
    NotificationCategory? Category = null,
    NotificationType? Type = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? Skip = 0,
    int? Take = 50);

public class AdminNotificationView
{
    public Notification Notification { get; set; } = null!;
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
}

public class AllNotificationsResult
{
    public List<AdminNotificationView> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
}

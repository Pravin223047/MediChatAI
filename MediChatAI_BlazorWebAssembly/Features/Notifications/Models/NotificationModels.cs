using System.Text.Json.Serialization;
using MediChatAI_BlazorWebAssembly.Core.Utilities;

namespace MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

[JsonConverter(typeof(CaseInsensitiveEnumConverter<NotificationType>))]
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    System
}

[JsonConverter(typeof(CaseInsensitiveEnumConverter<NotificationCategory>))]
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

[JsonConverter(typeof(CaseInsensitiveEnumConverter<NotificationPriority>))]
public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationCategory Category { get; set; }
    public NotificationPriority Priority { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public Guid? GroupId { get; set; }
    public string? Metadata { get; set; }
    public string? Icon { get; set; }
}

public class NotificationPreferenceDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool InAppNotificationsEnabled { get; set; } = true;
    public bool PushNotificationsEnabled { get; set; } = true;
    public bool SoundEnabled { get; set; } = true;
    public bool AppointmentNotifications { get; set; } = true;
    public bool DoctorNotifications { get; set; } = true;
    public bool AdminNotifications { get; set; } = true;
    public bool SecurityNotifications { get; set; } = true;
    public bool SystemNotifications { get; set; } = true;
    public bool MedicalNotifications { get; set; } = true;
    public bool QuietHoursEnabled { get; set; } = false;
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
    public string? Timezone { get; set; } = "UTC";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public record GetNotificationsInput(
    int? Skip = 0,
    int? Take = 20,
    bool? IsRead = null,
    NotificationCategory? Category = null,
    NotificationPriority? Priority = null);

public class GetNotificationsResponse
{
    public List<NotificationDto> Notifications { get; set; } = new();
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

// Admin-only notification queries
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
    public NotificationDto Notification { get; set; } = new();
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
}

public class AllNotificationsResult
{
    public List<AdminNotificationView> Notifications { get; set; } = new();
    public int TotalCount { get; set; }
}

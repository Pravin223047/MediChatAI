using MediChatAI_GraphQl.Core.Entities;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Shared;

public interface INotificationService
{
    /// <summary>
    /// Create a new notification for a specific user
    /// </summary>
    Task<Notification> CreateNotificationAsync(
        string userId,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationCategory category = NotificationCategory.General,
        NotificationPriority priority = NotificationPriority.Normal,
        string? actionUrl = null,
        string? actionText = null,
        Guid? groupId = null,
        string? metadata = null,
        string? icon = null,
        DateTime? expiresAt = null);

    /// <summary>
    /// Send notification to multiple users
    /// </summary>
    Task<List<Notification>> SendBulkNotificationsAsync(
        List<string> userIds,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationCategory category = NotificationCategory.General,
        NotificationPriority priority = NotificationPriority.Normal,
        string? actionUrl = null,
        string? actionText = null);

    /// <summary>
    /// Send notification to all users with a specific role
    /// </summary>
    Task<List<Notification>> SendNotificationToRoleAsync(
        string role,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationCategory category = NotificationCategory.General,
        NotificationPriority priority = NotificationPriority.Normal,
        string? actionUrl = null,
        string? actionText = null);

    /// <summary>
    /// Get paginated notifications for a user
    /// </summary>
    Task<(List<Notification> notifications, int totalCount)> GetUserNotificationsAsync(
        string userId,
        int skip = 0,
        int take = 20,
        bool? isRead = null,
        NotificationCategory? category = null,
        NotificationPriority? priority = null);

    /// <summary>
    /// Get unread notification count for a user
    /// </summary>
    Task<int> GetUnreadCountAsync(string userId);

    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task<bool> MarkAsReadAsync(Guid notificationId, string userId);

    /// <summary>
    /// Mark all notifications as read for a user
    /// </summary>
    Task<int> MarkAllAsReadAsync(string userId);

    /// <summary>
    /// Delete a notification
    /// </summary>
    Task<bool> DeleteNotificationAsync(Guid notificationId, string userId);

    /// <summary>
    /// Delete all read notifications older than specified days
    /// </summary>
    Task<int> CleanupOldNotificationsAsync(int olderThanDays = 30);

    /// <summary>
    /// Get or create notification preferences for a user
    /// </summary>
    Task<NotificationPreference> GetOrCreatePreferencesAsync(string userId);

    /// <summary>
    /// Update notification preferences
    /// </summary>
    Task<NotificationPreference> UpdatePreferencesAsync(string userId, NotificationPreference preferences);

    /// <summary>
    /// Check if user should receive notification based on preferences and quiet hours
    /// </summary>
    Task<bool> ShouldSendNotificationAsync(string userId, NotificationCategory category);

    /// <summary>
    /// Get IQueryable for all notifications (Admin only)
    /// </summary>
    IQueryable<Notification> GetAllNotificationsQuery();
}

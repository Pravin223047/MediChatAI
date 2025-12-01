using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Features.Notifications.Hubs;
using System.Text.Json;

namespace MediChatAI_GraphQl.Features.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<NotificationService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<NotificationService> logger,
        IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<Notification> CreateNotificationAsync(
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
        DateTime? expiresAt = null)
    {
        try
        {
            // Check if user should receive this notification
            var shouldSend = await ShouldSendNotificationAsync(userId, category);
            if (!shouldSend)
            {
                _logger.LogInformation($"Notification blocked by user preferences for user {userId}");
                return null!;
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Category = category,
                Priority = priority,
                ActionUrl = actionUrl,
                ActionText = actionText,
                GroupId = groupId,
                Metadata = metadata,
                Icon = icon,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created notification {notification.Id} for user {userId}");

            // Send real-time notification via SignalR
            try
            {
                var notificationJson = JsonSerializer.Serialize(notification, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("ReceiveNotification", notificationJson);

                // Also update unread count
                var unreadCount = await GetUnreadCountAsync(userId);
                await _hubContext.Clients.Group($"user_{userId}")
                    .SendAsync("UnreadCountUpdated", unreadCount);

                _logger.LogInformation($"Sent SignalR notification to user {userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending SignalR notification to user {userId}");
                // Don't fail the whole operation if SignalR fails
            }

            return notification;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating notification for user {userId}");
            throw;
        }
    }

    public async Task<List<Notification>> SendBulkNotificationsAsync(
        List<string> userIds,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationCategory category = NotificationCategory.General,
        NotificationPriority priority = NotificationPriority.Normal,
        string? actionUrl = null,
        string? actionText = null)
    {
        var notifications = new List<Notification>();
        var groupId = Guid.NewGuid();

        // Pre-check all user preferences in a single query
        var userIdsWithPreferences = await _context.NotificationPreferences
            .AsNoTracking()
            .Where(p => userIds.Contains(p.UserId))
            .Select(p => p.UserId)
            .ToListAsync();

        // Add notifications in bulk for better performance
        foreach (var userId in userIds)
        {
            // Check if user should receive this notification
            var shouldSend = await ShouldSendNotificationAsync(userId, category);
            if (!shouldSend)
            {
                _logger.LogInformation($"Notification blocked by user preferences for user {userId}");
                continue;
            }

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Category = category,
                Priority = priority,
                ActionUrl = actionUrl,
                ActionText = actionText,
                GroupId = groupId,
                CreatedAt = DateTime.UtcNow
            };

            notifications.Add(notification);
        }

        // Bulk insert all notifications in single database call
        if (notifications.Any())
        {
            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Send real-time notifications via SignalR for all users
            foreach (var notification in notifications)
            {
                try
                {
                    var notificationJson = JsonSerializer.Serialize(notification, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    await _hubContext.Clients.Group($"user_{notification.UserId}")
                        .SendAsync("ReceiveNotification", notificationJson);

                    var unreadCount = await GetUnreadCountAsync(notification.UserId);
                    await _hubContext.Clients.Group($"user_{notification.UserId}")
                        .SendAsync("UnreadCountUpdated", unreadCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending SignalR notification to user {notification.UserId}");
                }
            }
        }

        return notifications;
    }

    public async Task<List<Notification>> SendNotificationToRoleAsync(
        string role,
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationCategory category = NotificationCategory.General,
        NotificationPriority priority = NotificationPriority.Normal,
        string? actionUrl = null,
        string? actionText = null)
    {
        // Optimize: Query only user IDs instead of full user objects
        var roleEntity = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == role);

        if (roleEntity == null)
            return new List<Notification>();

        var userIds = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.RoleId == roleEntity.Id)
            .Select(ur => ur.UserId)
            .ToListAsync();

        return await SendBulkNotificationsAsync(
            userIds, title, message, type, category, priority, actionUrl, actionText);
    }

    public async Task<(List<Notification> notifications, int totalCount)> GetUserNotificationsAsync(
        string userId,
        int skip = 0,
        int take = 20,
        bool? isRead = null,
        NotificationCategory? category = null,
        NotificationPriority? priority = null)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow);

        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);

        if (category.HasValue)
            query = query.Where(n => n.Category == category.Value);

        if (priority.HasValue)
            query = query.Where(n => n.Priority == priority.Value);

        var totalCount = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (notifications, totalCount);
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .Where(n => n.ExpiresAt == null || n.ExpiresAt > DateTime.UtcNow)
            .CountAsync();
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return notifications.Count;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> CleanupOldNotificationsAsync(int olderThanDays = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

        var oldNotifications = await _context.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.Notifications.RemoveRange(oldNotifications);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Cleaned up {oldNotifications.Count} old notifications");
        return oldNotifications.Count;
    }

    public async Task<NotificationPreference> GetOrCreatePreferencesAsync(string userId)
    {
        var preferences = await _context.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (preferences == null)
        {
            preferences = new NotificationPreference
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.NotificationPreferences.Add(preferences);
            await _context.SaveChangesAsync();
        }

        return preferences;
    }

    public async Task<NotificationPreference> UpdatePreferencesAsync(string userId, NotificationPreference preferences)
    {
        var existing = await GetOrCreatePreferencesAsync(userId);

        existing.EmailNotificationsEnabled = preferences.EmailNotificationsEnabled;
        existing.InAppNotificationsEnabled = preferences.InAppNotificationsEnabled;
        existing.PushNotificationsEnabled = preferences.PushNotificationsEnabled;
        existing.SoundEnabled = preferences.SoundEnabled;
        existing.AppointmentNotifications = preferences.AppointmentNotifications;
        existing.DoctorNotifications = preferences.DoctorNotifications;
        existing.AdminNotifications = preferences.AdminNotifications;
        existing.SecurityNotifications = preferences.SecurityNotifications;
        existing.SystemNotifications = preferences.SystemNotifications;
        existing.MedicalNotifications = preferences.MedicalNotifications;
        existing.QuietHoursEnabled = preferences.QuietHoursEnabled;
        existing.QuietHoursStart = preferences.QuietHoursStart;
        existing.QuietHoursEnd = preferences.QuietHoursEnd;
        existing.Timezone = preferences.Timezone;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> ShouldSendNotificationAsync(string userId, NotificationCategory category)
    {
        var preferences = await GetOrCreatePreferencesAsync(userId);

        // Check if in-app notifications are enabled
        if (!preferences.InAppNotificationsEnabled)
            return false;

        // Check category-specific preferences
        var categoryEnabled = category switch
        {
            NotificationCategory.Appointment => preferences.AppointmentNotifications,
            NotificationCategory.Doctor => preferences.DoctorNotifications,
            NotificationCategory.Admin => preferences.AdminNotifications,
            NotificationCategory.Security => preferences.SecurityNotifications,
            NotificationCategory.System => preferences.SystemNotifications,
            NotificationCategory.Medical => preferences.MedicalNotifications,
            _ => true
        };

        if (!categoryEnabled)
            return false;

        // Check quiet hours
        if (preferences.QuietHoursEnabled &&
            preferences.QuietHoursStart.HasValue &&
            preferences.QuietHoursEnd.HasValue)
        {
            var currentTime = DateTime.UtcNow.TimeOfDay;
            var start = preferences.QuietHoursStart.Value;
            var end = preferences.QuietHoursEnd.Value;

            // Handle quiet hours that span midnight
            if (start < end)
            {
                if (currentTime >= start && currentTime <= end)
                    return false;
            }
            else
            {
                if (currentTime >= start || currentTime <= end)
                    return false;
            }
        }

        return true;
    }

    public IQueryable<Notification> GetAllNotificationsQuery()
    {
        return _context.Notifications.AsNoTracking().AsQueryable();
    }
}

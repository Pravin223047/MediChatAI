using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Notifications.Services;

public interface INotificationService
{
    // General notification events
    event Action<NotificationDto>? OnNotificationReceived;
    event Action<int>? OnUnreadCountChanged;
    event Action? OnNotificationsUpdated;

    // Appointment-specific real-time events
    event Action<object>? OnNewAppointmentRequest;
    event Action<object>? OnAppointmentApproved;
    event Action<object>? OnAppointmentRejected;
    event Action<object>? OnAppointmentCancelled;
    event Action<object>? OnAppointmentUpdated;
    event Action<object>? OnAppointmentReminder;
    event Action<object>? OnNewPrescription;
    event Action<object>? OnDocumentUploaded;
    event Action<object>? OnVitalRecorded;

    // Connection management
    Task StartAsync(string accessToken);
    Task StopAsync();
    Task<bool> ReconnectAsync();
    bool IsConnected { get; }

    // Notification operations
    Task<GetNotificationsResponse?> GetNotificationsAsync(GetNotificationsInput input);
    Task<int> GetUnreadCountAsync();
    Task<bool> MarkAsReadAsync(Guid notificationId);
    Task<int> MarkAllAsReadAsync();
    Task<bool> DeleteNotificationAsync(Guid notificationId);

    // Admin-only operations
    Task<AllNotificationsResult?> GetAllNotificationsAsync(GetAllNotificationsInput input);

    // Preferences
    Task<NotificationPreferenceDto?> GetPreferencesAsync();
    Task<NotificationPreferenceDto?> UpdatePreferencesAsync(UpdateNotificationPreferencesInput input);
}

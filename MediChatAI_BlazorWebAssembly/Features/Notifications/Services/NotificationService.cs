using Microsoft.AspNetCore.SignalR.Client;
using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using Microsoft.JSInterop;
using System.Text.Json;
using Blazored.LocalStorage;

namespace MediChatAI_BlazorWebAssembly.Features.Notifications.Services;

public class NotificationService : INotificationService, IAsyncDisposable
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<NotificationService> _logger;
    private readonly IJSRuntime _jsRuntime;
    private readonly ILocalStorageService _localStorage;
    private HubConnection? _hubConnection;
    private bool _soundEnabled = true;

    // General notification events
    public event Action<NotificationDto>? OnNotificationReceived;
    public event Action<int>? OnUnreadCountChanged;
    public event Action? OnNotificationsUpdated;

    // Appointment-specific real-time events
    public event Action<object>? OnNewAppointmentRequest;
    public event Action<object>? OnAppointmentApproved;
    public event Action<object>? OnAppointmentRejected;
    public event Action<object>? OnAppointmentCancelled;
    public event Action<object>? OnAppointmentUpdated;
    public event Action<object>? OnAppointmentReminder;
    public event Action<object>? OnNewPrescription;
    public event Action<object>? OnDocumentUploaded;
    public event Action<object>? OnVitalRecorded;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public NotificationService(
        IGraphQLService graphQLService,
        ILogger<NotificationService> logger,
        IJSRuntime jsRuntime,
        ILocalStorageService localStorage)
    {
        _graphQLService = graphQLService;
        _logger = logger;
        _jsRuntime = jsRuntime;
        _localStorage = localStorage;
    }

    public async Task StartAsync(string accessToken)
    {
        try
        {
            if (_hubConnection != null)
            {
                await StopAsync();
            }

            _hubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5095/notificationHub", options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                        Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                .Build();

            // Subscribe to SignalR events
            _hubConnection.On<string>("ReceiveNotification", async (notificationJson) =>
            {
                try
                {
                    _logger.LogInformation($"Received notification via SignalR: {notificationJson}");

                    var notification = JsonSerializer.Deserialize<NotificationDto>(notificationJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (notification != null)
                    {
                        _logger.LogInformation($"Deserialized notification: {notification.Title}");

                        // Play notification sound
                        if (_soundEnabled)
                        {
                            _logger.LogInformation("Playing notification sound");
                            await PlayNotificationSoundAsync(notification.Type);
                        }

                        _logger.LogInformation("Invoking notification events");
                        OnNotificationReceived?.Invoke(notification);
                        OnNotificationsUpdated?.Invoke();

                        // Update unread count
                        var unreadCount = await GetUnreadCountAsync();
                        OnUnreadCountChanged?.Invoke(unreadCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing received notification");
                }
            });

            _hubConnection.On<int>("UnreadCountUpdated", (count) =>
            {
                OnUnreadCountChanged?.Invoke(count);
            });

            // Subscribe to appointment-specific SignalR events
            _hubConnection.On<object>("NewAppointmentRequest", (data) =>
            {
                _logger.LogInformation("Received NewAppointmentRequest event");
                OnNewAppointmentRequest?.Invoke(data);
            });

            _hubConnection.On<object>("AppointmentApproved", (data) =>
            {
                _logger.LogInformation("Received AppointmentApproved event");
                OnAppointmentApproved?.Invoke(data);
            });

            _hubConnection.On<object>("AppointmentRejected", (data) =>
            {
                _logger.LogInformation("Received AppointmentRejected event");
                OnAppointmentRejected?.Invoke(data);
            });

            _hubConnection.On<object>("AppointmentCancelled", (data) =>
            {
                _logger.LogInformation("Received AppointmentCancelled event");
                OnAppointmentCancelled?.Invoke(data);
            });

            _hubConnection.On<object>("AppointmentUpdated", (data) =>
            {
                _logger.LogInformation("Received AppointmentUpdated event");
                OnAppointmentUpdated?.Invoke(data);
            });

            _hubConnection.On<object>("AppointmentReminder", (data) =>
            {
                _logger.LogInformation("Received AppointmentReminder event");
                OnAppointmentReminder?.Invoke(data);
            });

            _hubConnection.On<object>("NewPrescription", (data) =>
            {
                _logger.LogInformation("Received NewPrescription event");
                OnNewPrescription?.Invoke(data);
            });

            _hubConnection.On<object>("DocumentUploaded", (data) =>
            {
                _logger.LogInformation("Received DocumentUploaded event");
                OnDocumentUploaded?.Invoke(data);
            });

            _hubConnection.On<object>("VitalRecorded", (data) =>
            {
                _logger.LogInformation("Received VitalRecorded event");
                OnVitalRecorded?.Invoke(data);
            });

            _hubConnection.Reconnected += (connectionId) =>
            {
                _logger.LogInformation($"Reconnected to notification hub: {connectionId}");
                // Refresh notifications after reconnection
                OnNotificationsUpdated?.Invoke();
                return Task.CompletedTask;
            };

            _hubConnection.Closed += async (error) =>
            {
                if (error != null)
                    _logger.LogWarning("Notification hub connection closed: {Message}", error.Message);

                await Task.Delay(Random.Shared.Next(0, 5) * 1000);
                try
                {
                    await _hubConnection.StartAsync();
                    _logger.LogInformation("Successfully reconnected to notification hub");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not reconnect to notification hub: {Message}", ex.Message);
                }
            };

            // Add timeout for connection attempt (10 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _hubConnection.StartAsync(cts.Token);
            _logger.LogInformation("Connected to notification hub");

            // Load initial unread count
            var initialCount = await GetUnreadCountAsync();
            OnUnreadCountChanged?.Invoke(initialCount);

            // Load existing notifications after connection
            _logger.LogInformation("Loading existing notifications after SignalR connect");
            OnNotificationsUpdated?.Invoke();
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("SignalR connection timed out after 10 seconds - server may not be running. Real-time notifications will be disabled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to connect to notification hub - real-time notifications will be disabled. Error: {Message}", ex.Message);
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            _logger.LogInformation("Disconnected from notification hub");
        }
    }

    public async Task<bool> ReconnectAsync()
    {
        try
        {
            _logger.LogInformation("Attempting to reconnect to notification hub...");

            // Get the current access token from localStorage
            var token = await _localStorage.GetItemAsync<string>("token");

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Cannot reconnect: No access token found in localStorage");
                return false;
            }

            // Stop existing connection if any
            if (_hubConnection != null)
            {
                await StopAsync();
            }

            // Start new connection
            await StartAsync(token);

            _logger.LogInformation("Successfully reconnected to notification hub");
            return IsConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reconnect to notification hub");
            return false;
        }
    }

    public async Task<GetNotificationsResponse?> GetNotificationsAsync(GetNotificationsInput input)
    {
        var query = $@"
        query {{
            notifications(input: {{
                skip: {input.Skip ?? 0},
                take: {input.Take ?? 20}
                {(input.IsRead.HasValue ? $", isRead: {input.IsRead.Value.ToString().ToLower()}" : "")}
                {(input.Category.HasValue ? $", category: {input.Category.Value.ToString().ToUpper()}" : "")}
                {(input.Priority.HasValue ? $", priority: {input.Priority.Value.ToString().ToUpper()}" : "")}
            }}) {{
                notifications {{
                    id
                    userId
                    title
                    message
                    type
                    category
                    priority
                    isRead
                    readAt
                    createdAt
                    expiresAt
                    actionUrl
                    actionText
                    groupId
                    metadata
                    icon
                }}
                totalCount
            }}
        }}";

        try
        {
            _logger.LogInformation("GetNotificationsAsync: Calling GraphQL...");
            var response = await _graphQLService.SendQueryAsync<GetNotificationsResponseWrapper>(query);
            _logger.LogInformation($"GetNotificationsAsync: Response = {(response != null ? "not null" : "null")}");

            if (response?.Notifications != null)
            {
                _logger.LogInformation($"GetNotificationsAsync: Got {response.Notifications.Notifications.Count} notifications, TotalCount = {response.Notifications.TotalCount}");
                return response.Notifications;
            }

            _logger.LogWarning("GetNotificationsAsync: Response or Notifications was null");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications");
            return null;
        }
    }

    public async Task<AllNotificationsResult?> GetAllNotificationsAsync(GetAllNotificationsInput input)
    {
        var query = $@"
        query {{
            allNotifications(input: {{
                skip: {input.Skip ?? 0},
                take: {input.Take ?? 50}
                {(!string.IsNullOrEmpty(input.UserId) ? $", userId: \"{input.UserId}\"" : "")}
                {(input.Category.HasValue ? $", category: {input.Category.Value.ToString().ToUpper()}" : "")}
                {(input.Type.HasValue ? $", type: {input.Type.Value.ToString().ToUpper()}" : "")}
                {(input.StartDate.HasValue ? $", startDate: \"{input.StartDate.Value:yyyy-MM-ddTHH:mm:ss}\"" : "")}
                {(input.EndDate.HasValue ? $", endDate: \"{input.EndDate.Value:yyyy-MM-ddTHH:mm:ss}\"" : "")}
            }}) {{
                notifications {{
                    notification {{
                        id
                        userId
                        title
                        message
                        type
                        category
                        priority
                        isRead
                        readAt
                        createdAt
                        expiresAt
                        actionUrl
                        actionText
                        groupId
                        metadata
                        icon
                    }}
                    userEmail
                    userName
                    userRole
                }}
                totalCount
            }}
        }}";

        try
        {
            _logger.LogInformation("GetAllNotificationsAsync: Calling GraphQL (Admin)...");
            var response = await _graphQLService.SendQueryAsync<AllNotificationsResponseWrapper>(query);
            _logger.LogInformation($"GetAllNotificationsAsync: Response = {(response != null ? "not null" : "null")}");

            if (response?.AllNotifications != null)
            {
                _logger.LogInformation($"GetAllNotificationsAsync: Got {response.AllNotifications.Notifications.Count} notifications, TotalCount = {response.AllNotifications.TotalCount}");
                return response.AllNotifications;
            }

            _logger.LogWarning("GetAllNotificationsAsync: Response or AllNotifications was null");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all notifications (admin)");
            return null;
        }
    }

    public async Task<int> GetUnreadCountAsync()
    {
        var query = @"
        query {
            unreadNotificationCount
        }";

        try
        {
            _logger.LogInformation("GetUnreadCountAsync: Calling GraphQL...");
            var response = await _graphQLService.SendQueryAsync<UnreadCountResponseWrapper>(query);
            _logger.LogInformation($"GetUnreadCountAsync: Response = {(response != null ? "not null" : "null")}");

            if (response != null)
            {
                var count = response.UnreadNotificationCount;
                _logger.LogInformation($"GetUnreadCountAsync: Returning count = {count}");
                return count;
            }

            _logger.LogWarning("GetUnreadCountAsync: Response was null, returning 0");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count");
            return 0;
        }
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId)
    {
        var mutation = $@"
        mutation {{
            markNotificationAsRead(input: {{ notificationId: ""{notificationId}"" }})
        }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<MarkAsReadResponseWrapper>(mutation);
            var success = response?.Data?.MarkNotificationAsRead ?? false;

            if (success)
            {
                OnNotificationsUpdated?.Invoke();
                var unreadCount = await GetUnreadCountAsync();
                OnUnreadCountChanged?.Invoke(unreadCount);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return false;
        }
    }

    public async Task<int> MarkAllAsReadAsync()
    {
        var mutation = @"
        mutation {
            markAllNotificationsAsRead
        }";

        try
        {
            var response = await _graphQLService.SendQueryAsync<MarkAllAsReadResponseWrapper>(mutation);
            var count = response?.Data?.MarkAllNotificationsAsRead ?? 0;

            if (count > 0)
            {
                OnNotificationsUpdated?.Invoke();
                OnUnreadCountChanged?.Invoke(0);
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return 0;
        }
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId)
    {
        var mutation = $@"
        mutation {{
            deleteNotification(input: {{ notificationId: ""{notificationId}"" }})
        }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<DeleteNotificationResponseWrapper>(mutation);
            var success = response?.Data?.DeleteNotification ?? false;

            if (success)
            {
                OnNotificationsUpdated?.Invoke();
                var unreadCount = await GetUnreadCountAsync();
                OnUnreadCountChanged?.Invoke(unreadCount);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification");
            return false;
        }
    }

    public async Task<NotificationPreferenceDto?> GetPreferencesAsync()
    {
        var query = @"
        query {
            notificationPreferences {
                id
                userId
                emailNotificationsEnabled
                inAppNotificationsEnabled
                pushNotificationsEnabled
                soundEnabled
                appointmentNotifications
                doctorNotifications
                adminNotifications
                securityNotifications
                systemNotifications
                medicalNotifications
                quietHoursEnabled
                quietHoursStart
                quietHoursEnd
                timezone
                createdAt
                updatedAt
            }
        }";

        try
        {
            var response = await _graphQLService.SendQueryAsync<PreferencesResponseWrapper>(query);
            return response?.Data?.NotificationPreferences;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification preferences");
            return null;
        }
    }

    public async Task<NotificationPreferenceDto?> UpdatePreferencesAsync(UpdateNotificationPreferencesInput input)
    {
        var mutation = $@"
        mutation {{
            updateUserNotificationPreferences(input: {{
                emailNotificationsEnabled: {input.EmailNotificationsEnabled.ToString().ToLower()},
                inAppNotificationsEnabled: {input.InAppNotificationsEnabled.ToString().ToLower()},
                pushNotificationsEnabled: {input.PushNotificationsEnabled.ToString().ToLower()},
                soundEnabled: {input.SoundEnabled.ToString().ToLower()},
                appointmentNotifications: {input.AppointmentNotifications.ToString().ToLower()},
                doctorNotifications: {input.DoctorNotifications.ToString().ToLower()},
                adminNotifications: {input.AdminNotifications.ToString().ToLower()},
                securityNotifications: {input.SecurityNotifications.ToString().ToLower()},
                systemNotifications: {input.SystemNotifications.ToString().ToLower()},
                medicalNotifications: {input.MedicalNotifications.ToString().ToLower()},
                quietHoursEnabled: {input.QuietHoursEnabled.ToString().ToLower()}
                {(input.QuietHoursStart.HasValue ? $", quietHoursStart: \"{input.QuietHoursStart.Value}\"" : "")}
                {(input.QuietHoursEnd.HasValue ? $", quietHoursEnd: \"{input.QuietHoursEnd.Value}\"" : "")}
                {(!string.IsNullOrEmpty(input.Timezone) ? $", timezone: \"{input.Timezone}\"" : "")}
            }}) {{
                id
                userId
                emailNotificationsEnabled
                inAppNotificationsEnabled
                pushNotificationsEnabled
                soundEnabled
                appointmentNotifications
                doctorNotifications
                adminNotifications
                securityNotifications
                systemNotifications
                medicalNotifications
                quietHoursEnabled
                quietHoursStart
                quietHoursEnd
                timezone
                createdAt
                updatedAt
            }}
        }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<UpdatePreferencesResponseWrapper>(mutation);
            return response?.Data?.UpdateUserNotificationPreferences;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification preferences");
            return null;
        }
    }

    private async Task PlayNotificationSoundAsync(NotificationType type)
    {
        try
        {
            var soundFile = type switch
            {
                NotificationType.Success => "success.mp3",
                NotificationType.Warning => "warning.mp3",
                NotificationType.Error => "error.mp3",
                _ => "notification.mp3"
            };

            await _jsRuntime.InvokeVoidAsync("playNotificationSound", soundFile);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error playing notification sound");
            // Don't fail if sound playback fails
        }
    }

    public void SetSoundEnabled(bool enabled)
    {
        _soundEnabled = enabled;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    // Response wrappers
    private class GetNotificationsResponseWrapper
    {
        public GetNotificationsResponse? Notifications { get; set; }
    }

    private class AllNotificationsResponseWrapper
    {
        public AllNotificationsResult? AllNotifications { get; set; }
    }

    private class UnreadCountResponseWrapper
    {
        public int UnreadNotificationCount { get; set; }
    }

    private class MarkAsReadResponseWrapper
    {
        public MarkAsReadResponseData? Data { get; set; }
    }

    private class MarkAsReadResponseData
    {
        public bool MarkNotificationAsRead { get; set; }
    }

    private class MarkAllAsReadResponseWrapper
    {
        public MarkAllAsReadResponseData? Data { get; set; }
    }

    private class MarkAllAsReadResponseData
    {
        public int MarkAllNotificationsAsRead { get; set; }
    }

    private class DeleteNotificationResponseWrapper
    {
        public DeleteNotificationResponseData? Data { get; set; }
    }

    private class DeleteNotificationResponseData
    {
        public bool DeleteNotification { get; set; }
    }

    private class PreferencesResponseWrapper
    {
        public PreferencesResponseData? Data { get; set; }
    }

    private class PreferencesResponseData
    {
        public NotificationPreferenceDto? NotificationPreferences { get; set; }
    }

    private class UpdatePreferencesResponseWrapper
    {
        public UpdatePreferencesResponseData? Data { get; set; }
    }

    private class UpdatePreferencesResponseData
    {
        public NotificationPreferenceDto? UpdateUserNotificationPreferences { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MediChatAI_GraphQl.Features.Notifications.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            // Add user to their personal notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation($"User {userId} connected to notifications (ConnectionId: {Context.ConnectionId})");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation($"User {userId} disconnected from notifications");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client confirms notification was received
    /// </summary>
    public async Task ConfirmNotificationReceived(string notificationId)
    {
        _logger.LogInformation($"Notification {notificationId} confirmed by connection {Context.ConnectionId}");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Get user's online status
    /// </summary>
    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }

    // ============================================
    // APPOINTMENT SYSTEM REAL-TIME NOTIFICATIONS
    // ============================================

    /// <summary>
    /// Notify doctor of a new appointment request
    /// </summary>
    public async Task NotifyNewAppointmentRequest(string doctorId, object requestData)
    {
        await Clients.Group($"user_{doctorId}").SendAsync("NewAppointmentRequest", requestData);
        _logger.LogInformation($"Sent new appointment request notification to doctor {doctorId}");
    }

    /// <summary>
    /// Notify patient that their appointment request was approved
    /// </summary>
    public async Task NotifyAppointmentApproved(string patientId, object appointmentData)
    {
        await Clients.Group($"user_{patientId}").SendAsync("AppointmentApproved", appointmentData);
        _logger.LogInformation($"Sent appointment approved notification to patient {patientId}");
    }

    /// <summary>
    /// Notify patient that their appointment request was rejected
    /// </summary>
    public async Task NotifyAppointmentRejected(string patientId, object rejectionData)
    {
        await Clients.Group($"user_{patientId}").SendAsync("AppointmentRejected", rejectionData);
        _logger.LogInformation($"Sent appointment rejected notification to patient {patientId}");
    }

    /// <summary>
    /// Notify both doctor and patient of an appointment cancellation
    /// </summary>
    public async Task NotifyAppointmentCancelled(string userId, object cancellationData)
    {
        await Clients.Group($"user_{userId}").SendAsync("AppointmentCancelled", cancellationData);
        _logger.LogInformation($"Sent appointment cancelled notification to user {userId}");
    }

    /// <summary>
    /// Notify patient of a new prescription
    /// </summary>
    public async Task NotifyNewPrescription(string patientId, object prescriptionData)
    {
        await Clients.Group($"user_{patientId}").SendAsync("NewPrescription", prescriptionData);
        _logger.LogInformation($"Sent new prescription notification to patient {patientId}");
    }

    /// <summary>
    /// Notify doctor that a patient uploaded a new document
    /// </summary>
    public async Task NotifyDocumentUploaded(string doctorId, object documentData)
    {
        await Clients.Group($"user_{doctorId}").SendAsync("DocumentUploaded", documentData);
        _logger.LogInformation($"Sent document uploaded notification to doctor {doctorId}");
    }

    /// <summary>
    /// Notify patient of newly recorded vitals
    /// </summary>
    public async Task NotifyVitalRecorded(string patientId, object vitalData)
    {
        await Clients.Group($"user_{patientId}").SendAsync("VitalRecorded", vitalData);
        _logger.LogInformation($"Sent vital recorded notification to patient {patientId}");
    }

    /// <summary>
    /// Notify user of appointment reminder (24h before, 1h before, etc.)
    /// </summary>
    public async Task NotifyAppointmentReminder(string userId, object reminderData)
    {
        await Clients.Group($"user_{userId}").SendAsync("AppointmentReminder", reminderData);
        _logger.LogInformation($"Sent appointment reminder to user {userId}");
    }

    /// <summary>
    /// Notify doctor of updated appointment (rescheduled, etc.)
    /// </summary>
    public async Task NotifyAppointmentUpdated(string userId, object appointmentData)
    {
        await Clients.Group($"user_{userId}").SendAsync("AppointmentUpdated", appointmentData);
        _logger.LogInformation($"Sent appointment updated notification to user {userId}");
    }

    // ============================================
    // SCHEDULED REPORT NOTIFICATIONS
    // ============================================

    /// <summary>
    /// Notify all admins when a scheduled report has been executed
    /// </summary>
    public async Task NotifyScheduledReportExecuted(object reportData)
    {
        // Broadcast to all connected admin users
        await Clients.Group("admins").SendAsync("ScheduledReportExecuted", reportData);
        _logger.LogInformation("Sent scheduled report execution notification to all admins");
    }

    /// <summary>
    /// Join the admins group for receiving admin-only notifications
    /// </summary>
    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        _logger.LogInformation($"Connection {Context.ConnectionId} joined admins group");
    }

    /// <summary>
    /// Leave the admins group
    /// </summary>
    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admins");
        _logger.LogInformation($"Connection {Context.ConnectionId} left admins group");
    }
}

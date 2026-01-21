using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Admin.DTOs;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Features.Authentication.DTOs;
using MediChatAI_GraphQl.Features.Notifications.DTOs;
using MediChatAI_GraphQl.Features.Patient.DTOs;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Infrastructure.Data;
using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MediChatAI_GraphQl.GraphQL.Mutations;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Appointment;
using MediChatAI_GraphQl.Core.Interfaces.Services.Document;
using MediChatAI_GraphQl.Core.Interfaces.Services.Prescription;
using MediChatAI_GraphQl.Core.Interfaces.Services.Patient;
using MediChatAI_GraphQl.Core.Interfaces.Services.Consultation;

public class Mutation
{
    /// <summary>
    /// Register a new user account
    /// </summary>
    public async Task<AuthResult> RegisterUser(
        RegisterUserInput input,
        [Service] IAuthService authService)
    {
        return await authService.RegisterAsync(input);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    public async Task<AuthResult> LoginUser(
        LoginUserInput input,
        [Service] IAuthService authService)
    {
        return await authService.LoginAsync(input);
    }

    /// <summary>
    /// Refresh JWT token using refresh token
    /// </summary>
    public async Task<AuthResult> RefreshToken(
        RefreshTokenInput input,
        [Service] IAuthService authService)
    {
        return await authService.RefreshTokenAsync(input);
    }

    /// <summary>
    /// Request password reset email
    /// </summary>
    public async Task<bool> ForgotPassword(
        ForgotPasswordInput input,
        [Service] IAuthService authService)
    {
        return await authService.ForgotPasswordAsync(input);
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    public async Task<bool> ResetPassword(
        ResetPasswordInput input,
        [Service] IAuthService authService)
    {
        return await authService.ResetPasswordAsync(input);
    }

    /// <summary>
    /// Verify email address with token
    /// </summary>
    public async Task<bool> VerifyEmail(
        VerifyEmailInput input,
        [Service] IAuthService authService)
    {
        return await authService.VerifyEmailAsync(input);
    }

    /// <summary>
    /// Logout current user (invalidate refresh token)
    /// </summary>
    [Authorize]
    public async Task<bool> Logout(
        ClaimsPrincipal claimsPrincipal,
        [Service] IAuthService authService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        await authService.LogoutAsync(userId);
        return true;
    }

    /// <summary>
    /// Send OTP verification code to email
    /// </summary>
    public async Task<OtpResult> SendOtp(
        SendOtpInput input,
        [Service] IAuthService authService)
    {
        return await authService.SendOtpAsync(input);
    }

    /// <summary>
    /// Verify OTP code and complete email verification
    /// </summary>
    public async Task<AuthResult> VerifyOtp(
        VerifyOtpInput input,
        [Service] IAuthService authService)
    {
        return await authService.VerifyOtpAsync(input);
    }

    /// <summary>
    /// Update user profile information
    /// </summary>
    [Authorize]
    public async Task<ProfileResult> UpdateProfile(
        UpdateProfileInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAuthService authService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return new ProfileResult(false, null, new[] { "User not authenticated" });

        return await authService.UpdateProfileAsync(userId, input);
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [Authorize]
    public async Task<bool> ChangePassword(
        ChangePasswordInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAuthService authService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await authService.ChangePasswordAsync(userId, input);
    }

    /// <summary>
    /// Enable Multi-Factor Authentication for current user
    /// </summary>
    [Authorize]
    public async Task<bool> EnableMfa(
        ClaimsPrincipal claimsPrincipal,
        [Service] IAuthService authService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await authService.EnableMfaAsync(userId);
    }

    /// <summary>
    /// Disable Multi-Factor Authentication for current user
    /// </summary>
    [Authorize]
    public async Task<bool> DisableMfa(
        ClaimsPrincipal claimsPrincipal,
        [Service] IAuthService authService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await authService.DisableMfaAsync(userId);
    }

    /// <summary>
    /// Verify MFA code during login (email OTP)
    /// </summary>
    public async Task<AuthResult> VerifyMfa(
        string email,
        string otpCode,
        [Service] IAuthService authService)
    {
        return await authService.VerifyMfaAsync(email, otpCode);
    }

    /// <summary>
    /// Setup authenticator app for current user - returns secret key and QR code URL
    /// </summary>
    [Authorize]
    public async Task<AuthenticatorSetupResult> SetupAuthenticator(
        ClaimsPrincipal claimsPrincipal,
        [Service] IAuthService authService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return new AuthenticatorSetupResult(false, null, null, null, new[] { "User not authenticated" });

        return await authService.SetupAuthenticatorAsync(userId);
    }

    /// <summary>
    /// Verify authenticator setup with a test code from the app
    /// </summary>
    [Authorize]
    public async Task<bool> VerifyAuthenticatorSetup(
        string code,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAuthService authService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await authService.VerifyAuthenticatorSetupAsync(userId, code);
    }

    /// <summary>
    /// Verify authenticator code during login
    /// </summary>
    public async Task<AuthResult> VerifyAuthenticatorCode(
        string email,
        string code,
        [Service] IAuthService authService)
    {
        return await authService.VerifyAuthenticatorCodeAsync(email, code);
    }

    // Admin-specific mutations
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<AdminOperationResult> UpdateUserStatus(
        UpdateUserStatusInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAdminService adminService)
    {
        var adminUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
            return new AdminOperationResult(false, new[] { "Admin not authenticated" });

        return await adminService.UpdateUserStatusAsync(adminUserId, input);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<AdminOperationResult> UpdateUserRole(
        UpdateUserRoleInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAdminService adminService)
    {
        var adminUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
            return new AdminOperationResult(false, new[] { "Admin not authenticated" });

        return await adminService.UpdateUserRoleAsync(adminUserId, input);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<AdminOperationResult> DeleteUser(
        string userId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAdminService adminService)
    {
        var adminUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
            return new AdminOperationResult(false, new[] { "Admin not authenticated" });

        return await adminService.DeleteUserAsync(adminUserId, userId);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<AdminOperationResult> SendSystemNotification(
        string title,
        string message,
        string? targetRole,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAdminService adminService)
    {
        var adminUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
            return new AdminOperationResult(false, new[] { "Admin not authenticated" });

        return await adminService.SendSystemNotificationAsync(adminUserId, title, message, targetRole);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<AdminOperationResult> ExportUserData(
        string? userId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAdminService adminService)
    {
        var adminUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
            return new AdminOperationResult(false, new[] { "Admin not authenticated" });

        return await adminService.ExportUserDataAsync(adminUserId, userId);
    }

    // Doctor onboarding mutations
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<DoctorProfileResult> CompleteDoctorProfile(
        DoctorProfileCompletionInput input,
        string aadhaarImagePath,
        string? medicalCertificateFilePath,
        ClaimsPrincipal claimsPrincipal,
        [Service] IDoctorOnboardingService doctorOnboardingService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return new DoctorProfileResult(false, "User not authenticated", new[] { "User not authenticated" });

        return await doctorOnboardingService.CompleteProfileAsync(userId, input, aadhaarImagePath, medicalCertificateFilePath);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<AdminApprovalResult> ApproveOrRejectDoctor(
        AdminApprovalInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IDoctorOnboardingService doctorOnboardingService)
    {
        var adminUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminUserId))
            return new AdminApprovalResult(false, "Admin not authenticated", new[] { "Admin not authenticated" });

        return await doctorOnboardingService.ApproveOrRejectDoctorAsync(adminUserId, input);
    }

    // System Settings mutations
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<SettingsUpdateResult> UpdateGeneralSettings(
        UpdateGeneralSettingsInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ISystemSettingsService systemSettingsService)
    {
        var adminUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return await systemSettingsService.UpdateGeneralSettingsAsync(adminUserId, input);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<SettingsUpdateResult> UpdateSecuritySettings(
        UpdateSecuritySettingsInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ISystemSettingsService systemSettingsService)
    {
        var adminUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return await systemSettingsService.UpdateSecuritySettingsAsync(adminUserId, input);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<SettingsUpdateResult> UpdateSmtpSettings(
        UpdateSmtpSettingsInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ISystemSettingsService settingsService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return await settingsService.UpdateSmtpSettingsAsync(userId, input);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<SettingsUpdateResult> UpdateEmailThemeSettings(
        UpdateEmailThemeInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ISystemSettingsService settingsService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return await settingsService.UpdateEmailThemeSettingsAsync(userId, input);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<SettingsUpdateResult> UpdateNotificationSettings(
        UpdateNotificationSettingsInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ISystemSettingsService settingsService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return await settingsService.UpdateNotificationSettingsAsync(userId, input);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<SettingsUpdateResult> UpdateAppearanceSettings(
        UpdateAppearanceSettingsInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ISystemSettingsService settingsService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return await settingsService.UpdateAppearanceSettingsAsync(userId, input);
    }

    [Authorize(Roles = new[] { "Admin" })]
    public async Task<SmtpTestResult> TestSmtpConnection(
        ClaimsPrincipal claimsPrincipal,
        [Service] ISystemSettingsService settingsService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        return await settingsService.TestSmtpConnectionAsync(userId);
    }

    [Authorize]
    public async Task<EmailTemplatePreviewResult> PreviewEmailTemplate(
        EmailTemplatePreviewInput input,
        [Service] ISystemSettingsService settingsService)
    {
        return await settingsService.PreviewEmailTemplateAsync(
            input.TemplateName,
            input.Theme,
            input.PrimaryColor,
            input.SecondaryColor
        );
    }

    // Notification mutations
    /// <summary>
    /// Mark a single notification as read
    /// </summary>
    [Authorize]
    public async Task<bool> MarkNotificationAsRead(
        Guid notificationId,
        ClaimsPrincipal claimsPrincipal,
        [Service] INotificationService notificationService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await notificationService.MarkAsReadAsync(notificationId, userId);
    }

    /// <summary>
    /// Mark all notifications as read for the current user
    /// </summary>
    [Authorize]
    public async Task<int> MarkAllNotificationsAsRead(
        ClaimsPrincipal claimsPrincipal,
        [Service] INotificationService notificationService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return 0;

        return await notificationService.MarkAllAsReadAsync(userId);
    }

    /// <summary>
    /// Delete a notification
    /// </summary>
    [Authorize]
    public async Task<bool> DeleteNotification(
        Guid notificationId,
        ClaimsPrincipal claimsPrincipal,
        [Service] INotificationService notificationService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await notificationService.DeleteNotificationAsync(notificationId, userId);
    }

    [Authorize]
    public async Task<NotificationPreference> UpdateUserNotificationPreferences(
        UpdateNotificationPreferencesInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] INotificationService notificationService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated");

        var preferences = new NotificationPreference
        {
            UserId = userId,
            EmailNotificationsEnabled = input.EmailNotificationsEnabled,
            InAppNotificationsEnabled = input.InAppNotificationsEnabled,
            PushNotificationsEnabled = input.PushNotificationsEnabled,
            SoundEnabled = input.SoundEnabled,
            AppointmentNotifications = input.AppointmentNotifications,
            DoctorNotifications = input.DoctorNotifications,
            AdminNotifications = input.AdminNotifications,
            SecurityNotifications = input.SecurityNotifications,
            SystemNotifications = input.SystemNotifications,
            MedicalNotifications = input.MedicalNotifications,
            QuietHoursEnabled = input.QuietHoursEnabled,
            QuietHoursStart = input.QuietHoursStart,
            QuietHoursEnd = input.QuietHoursEnd,
            Timezone = input.Timezone
        };

        return await notificationService.UpdatePreferencesAsync(userId, preferences);
    }

    // Emergency Chat Mutations
    /// <summary>
    /// Send a chat message to the emergency AI assistant
    /// </summary>
    [AllowAnonymous]
    public async Task<MediChatAI_GraphQl.Features.Emergency.DTOs.ChatResponseDto> SendChatMessage(
        MediChatAI_GraphQl.Features.Emergency.DTOs.SendChatMessageInput input,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Emergency.IGeminiEmergencyChatService chatService)
    {
        return await chatService.SendMessageAsync(input);
    }

    /// <summary>
    /// Clear chat history for a session
    /// </summary>
    [AllowAnonymous]
    public async Task<bool> ClearChatHistory(
        string sessionId,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Emergency.IGeminiEmergencyChatService chatService)
    {
        return await chatService.ClearChatHistoryAsync(sessionId);
    }

    // Doctor Dashboard Mutations
    /// <summary>
    /// Update doctor preferences (theme, dashboard settings, etc.)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<DoctorPreference> UpdateDoctorPreferencesAsync(
        UpdateDoctorPreferencesInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IDoctorPreferencesService preferencesService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await preferencesService.UpdatePreferencesAsync(doctorId, input);
    }

    /// <summary>
    /// Record a new patient vital sign
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<PatientVital> RecordPatientVitalAsync(
        string patientId,
        VitalType vitalType,
        string value,
        string? unit,
        string? notes,
        int? systolicValue,
        int? diastolicValue,
        ClaimsPrincipal claimsPrincipal,
        [Service] IPatientVitalsService vitalsService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        var input = new RecordVitalsInput(
            PatientId: patientId,
            RecordedByDoctorId: doctorId,
            VitalType: vitalType,
            Value: value,
            Unit: unit,
            SystolicValue: systolicValue,
            DiastolicValue: diastolicValue,
            Notes: notes
        );

        return await vitalsService.RecordVitalsAsync(input);
    }

    /// <summary>
    /// Acknowledge an emergency alert
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> AcknowledgeEmergencyAlertAsync(
        Guid alertId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IEmergencyAlertService alertService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await alertService.AcknowledgeAlertAsync(alertId, doctorId);
    }

    /// <summary>
    /// Resolve an emergency alert
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> ResolveEmergencyAlertAsync(
        Guid alertId,
        string? resolutionNotes,
        ClaimsPrincipal claimsPrincipal,
        [Service] IEmergencyAlertService alertService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await alertService.ResolveAlertAsync(alertId, doctorId, resolutionNotes);
    }

    /// <summary>
    /// Dismiss an emergency alert
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> DismissEmergencyAlertAsync(
        Guid alertId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IEmergencyAlertService alertService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await alertService.DismissAlertAsync(alertId, doctorId);
    }

    /// <summary>
    /// Send a message to another user (doctor/patient)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<DoctorMessage> SendDoctorMessageAsync(
        string receiverId,
        string content,
        MessageType messageType,
        string? attachmentUrl,
        string? attachmentFileName,
        Guid? conversationId,
        Guid? replyToMessageId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IDoctorChatService chatService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        var input = new Features.Doctor.DTOs.SendMessageInput(
            SenderId: doctorId,
            ReceiverId: receiverId,
            Content: content,
            MessageType: messageType,
            AttachmentUrl: attachmentUrl,
            AttachmentFileName: attachmentFileName,
            ReplyToMessageId: replyToMessageId,
            ConversationId: conversationId
        );

        return await chatService.SendMessageAsync(input);
    }

    /// <summary>
    /// Mark a message as read
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> MarkDoctorMessageAsReadAsync(
        Guid messageId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IDoctorChatService chatService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            return false;

        return await chatService.MarkAsReadAsync(messageId);
    }

    /// <summary>
    /// Create a new emergency alert
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<EmergencyAlert> CreateEmergencyAlertAsync(
        string patientId,
        string title,
        string description,
        AlertSeverity severity,
        AlertCategory category,
        Guid? relatedVitalId,
        string? patientLocation,
        string? recommendedAction,
        ClaimsPrincipal claimsPrincipal,
        [Service] IEmergencyAlertService alertService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        var input = new CreateEmergencyAlertInput(
            PatientId: patientId,
            DoctorId: doctorId,
            Title: title,
            Description: description,
            Severity: severity,
            Category: category,
            RelatedVitalId: relatedVitalId,
            PatientLocation: patientLocation,
            RecommendedAction: recommendedAction
        );

        return await alertService.CreateAlertAsync(input);
    }

    // ============================================
    // APPOINTMENT SYSTEM MUTATIONS
    // ============================================

    /// <summary>
    /// Create a new appointment request (for patients)
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public async Task<AppointmentRequestDto> CreateAppointmentRequestAsync(
        CreateAppointmentRequestDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAppointmentService appointmentService,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            input.PatientId = patientId; // Ensure the patient ID is set
            logger.LogInformation("Creating appointment request for patient {PatientId}", patientId);

            var result = await appointmentService.CreateAppointmentRequestAsync(input);

            logger.LogInformation("Appointment request created successfully with ID {RequestId}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating appointment request: {Message}", ex.Message);
            throw new GraphQLException($"Failed to create appointment request: {ex.Message}");
        }
    }

    /// <summary>
    /// Approve an appointment request (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<AppointmentDto> ApproveAppointmentRequestAsync(
        ReviewAppointmentRequestDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAppointmentService appointmentService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        input.ReviewedByDoctorId = doctorId;
        return await appointmentService.ApproveAppointmentRequestAsync(input);
    }

    /// <summary>
    /// Reject an appointment request (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> RejectAppointmentRequestAsync(
        int requestId,
        string reason,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAppointmentService appointmentService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await appointmentService.RejectAppointmentRequestAsync(requestId, doctorId, reason);
    }

    /// <summary>
    /// Cancel an appointment request (for patients)
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public async Task<bool> CancelAppointmentRequestAsync(
        int requestId,
        string reason,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAppointmentService appointmentService)
    {
        var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            throw new UnauthorizedAccessException("Patient not authenticated");

        return await appointmentService.CancelAppointmentRequestAsync(requestId, patientId, reason);
    }

    /// <summary>
    /// Create a direct appointment (for doctors/admin)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor", "Admin" })]
    public async Task<AppointmentDto> CreateAppointmentAsync(
        CreateAppointmentDto input,
        [Service] IAppointmentService appointmentService)
    {
        return await appointmentService.CreateAppointmentAsync(input);
    }

    /// <summary>
    /// Update an existing appointment
    /// </summary>
    [Authorize(Roles = new[] { "Doctor", "Admin" })]
    public async Task<AppointmentDto> UpdateAppointmentAsync(
        UpdateAppointmentDto input,
        [Service] IAppointmentService appointmentService)
    {
        return await appointmentService.UpdateAppointmentAsync(input);
    }

    /// <summary>
    /// Cancel an appointment
    /// </summary>
    [Authorize]
    public async Task<bool> CancelAppointmentAsync(
        int appointmentId,
        string reason,
        [Service] IAppointmentService appointmentService)
    {
        return await appointmentService.CancelAppointmentAsync(appointmentId, reason);
    }

    /// <summary>
    /// Mark an appointment as completed (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> CompleteAppointmentAsync(
        int appointmentId,
        string? doctorNotes,
        [Service] IAppointmentService appointmentService)
    {
        return await appointmentService.CompleteAppointmentAsync(appointmentId, doctorNotes);
    }

    /// <summary>
    /// Reschedule an existing appointment to a new date and time (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<AppointmentDto> RescheduleAppointmentAsync(
        int appointmentId,
        DateTime newAppointmentDateTime,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAppointmentService appointmentService,
        [Service] ITimeBlockService timeBlockService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        // Get the appointment to verify ownership
        var appointment = await appointmentService.GetAppointmentByIdAsync(appointmentId);
        if (appointment == null || appointment.DoctorId != doctorId)
            throw new UnauthorizedAccessException("Appointment not found or access denied");

        // Check for time block conflicts (exclude the appointment being rescheduled)
        var hasConflict = await timeBlockService.HasConflictAsync(
            doctorId,
            newAppointmentDateTime.Date,
            newAppointmentDateTime.TimeOfDay,
            newAppointmentDateTime.TimeOfDay.Add(TimeSpan.FromMinutes(appointment.DurationMinutes)),
            excludeTimeBlockId: null,
            excludeAppointmentId: appointmentId);

        if (hasConflict)
            throw new InvalidOperationException("The new time slot conflicts with an existing time block or appointment");

        // Update the appointment with new date and time
        var updateDto = new UpdateAppointmentDto
        {
            Id = appointmentId,
            AppointmentDateTime = newAppointmentDateTime
        };

        return await appointmentService.UpdateAppointmentAsync(updateDto);
    }

    // ============================================
    // TIMEBLOCK MUTATIONS
    // ============================================

    /// <summary>
    /// Create a single time block for the authenticated doctor
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<TimeBlockDto> CreateTimeBlockAsync(
        CreateTimeBlockDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ITimeBlockService timeBlockService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        input.DoctorId = doctorId;
        return await timeBlockService.CreateTimeBlockAsync(input);
    }

    /// <summary>
    /// Create recurring time blocks for the authenticated doctor
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<List<TimeBlockDto>> CreateRecurringTimeBlocksAsync(
        CreateTimeBlockDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ITimeBlockService timeBlockService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        input.DoctorId = doctorId;
        return await timeBlockService.CreateRecurringTimeBlocksAsync(input);
    }

    /// <summary>
    /// Update an existing time block
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<TimeBlockDto> UpdateTimeBlockAsync(
        UpdateTimeBlockDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ITimeBlockService timeBlockService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        var existingBlock = await timeBlockService.GetTimeBlockByIdAsync(input.Id);
        if (existingBlock == null || existingBlock.DoctorId != doctorId)
            throw new UnauthorizedAccessException("Time block not found or access denied");

        return await timeBlockService.UpdateTimeBlockAsync(input);
    }

    /// <summary>
    /// Delete a time block
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> DeleteTimeBlockAsync(
        Guid timeBlockId,
        ClaimsPrincipal claimsPrincipal,
        [Service] ITimeBlockService timeBlockService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await timeBlockService.DeleteTimeBlockAsync(timeBlockId, doctorId);
    }

    /// <summary>
    /// Deactivate a time block without deleting it
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> DeactivateTimeBlockAsync(
        Guid timeBlockId,
        ClaimsPrincipal claimsPrincipal,
        [Service] ITimeBlockService timeBlockService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await timeBlockService.DeactivateTimeBlockAsync(timeBlockId, doctorId);
    }

    // ============================================
    // PATIENT DOCUMENT MUTATIONS
    // ============================================

    /// <summary>
    /// Upload a patient document
    /// </summary>
    [Authorize]
    public async Task<PatientDocumentDto> UploadPatientDocumentAsync(
        UploadPatientDocumentDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IPatientDocumentService documentService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated");

        input.UploadedById = userId;

        // If PatientId is not provided, use the current user's ID (for patients uploading their own documents)
        if (string.IsNullOrEmpty(input.PatientId))
            input.PatientId = userId;

        return await documentService.UploadDocumentAsync(input);
    }

    /// <summary>
    /// Delete a patient document
    /// </summary>
    [Authorize]
    public async Task<bool> DeletePatientDocumentAsync(
        int documentId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IPatientDocumentService documentService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await documentService.DeleteDocumentAsync(documentId, userId);
    }

    /// <summary>
    /// Verify a patient document (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> VerifyPatientDocumentAsync(
        int documentId,
        string? notes,
        ClaimsPrincipal claimsPrincipal,
        [Service] IPatientDocumentService documentService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            return false;

        return await documentService.VerifyDocumentAsync(documentId, doctorId, notes);
    }

    // ============================================
    // PRESCRIPTION MUTATIONS
    // ============================================

    /// <summary>
    /// Create a new prescription (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<PrescriptionDto> CreatePrescriptionAsync(
        CreatePrescriptionDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IPrescriptionService prescriptionService,
        [Service] ApplicationDbContext context)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        // Validate doctor-patient relationship
        var hasRelationship = await context.Appointments
            .AnyAsync(a => a.DoctorId == doctorId && a.PatientId == input.PatientId);

        if (!hasRelationship)
        {
            var hasConsultation = await context.ConsultationSessions
                .AnyAsync(c => c.DoctorId == doctorId && c.PatientId == input.PatientId);

            if (!hasConsultation)
                throw new UnauthorizedAccessException("You can only prescribe to patients you have consulted with");
        }

        // Validate patient allergies against prescribed medications
        var patient = await context.Users.FindAsync(input.PatientId);
        if (patient != null && !string.IsNullOrEmpty(patient.Allergies))
        {
            var allergies = patient.Allergies.ToLower().Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim()).ToList();

            foreach (var item in input.Items)
            {
                var medicationName = item.MedicationName.ToLower();
                var genericName = item.GenericName?.ToLower() ?? "";

                if (allergies.Any(a => medicationName.Contains(a) || genericName.Contains(a)))
                {
                    throw new InvalidOperationException($"Patient is allergic to {item.MedicationName}. Please review patient allergies before prescribing.");
                }
            }
        }

        input.DoctorId = doctorId;
        return await prescriptionService.CreatePrescriptionAsync(input);
    }

    /// <summary>
    /// Cancel a prescription (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> CancelPrescriptionAsync(
        int prescriptionId,
        string reason,
        [Service] IPrescriptionService prescriptionService)
    {
        return await prescriptionService.CancelPrescriptionAsync(prescriptionId, reason);
    }

    /// <summary>
    /// Refill a prescription
    /// </summary>
    [Authorize]
    public async Task<bool> RefillPrescriptionAsync(
        int prescriptionId,
        [Service] IPrescriptionService prescriptionService)
    {
        return await prescriptionService.RefillPrescriptionAsync(prescriptionId);
    }

    // ============================================
    // PATIENT MODULE MUTATIONS
    // ============================================

    /// <summary>
    /// Record a health metric (patient self-reporting vitals)
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public async Task<HealthMetricDto> RecordHealthMetricAsync(
        RecordHealthMetricInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IPatientVitalsService vitalsService)
    {
        var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            throw new UnauthorizedAccessException("Patient not authenticated");

        var vitalInput = new RecordVitalsInput(
            patientId,
            null, // RecordedByDoctorId - null for patient self-recording
            Enum.Parse<VitalType>(input.MetricType),
            input.Value.ToString(),
            input.Unit,
            input.SystolicValue,
            input.DiastolicValue,
            input.Notes
        );

        var vital = await vitalsService.RecordVitalsAsync(vitalInput);

        return new HealthMetricDto
        {
            Id = vital.Id,
            PatientId = vital.PatientId,
            MetricType = vital.VitalType.ToString(),
            Value = double.TryParse(vital.Value, out var val) ? val : 0,
            Unit = vital.Unit ?? "",
            SystolicValue = vital.SystolicValue,
            DiastolicValue = vital.DiastolicValue,
            RecordedDate = vital.RecordedAt,
            RecordedBy = patientId,
            Notes = vital.Notes,
            Status = vital.Severity.ToString()
        };
    }

    /// <summary>
    /// Rate a completed consultation
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public Task<bool> RateConsultationAsync(
        RateConsultationInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] Microsoft.EntityFrameworkCore.DbContext context)
    {
        var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return Task.FromResult(false);

        // TODO: Implement consultation rating entity and save to database
        // For now, returning true
        return Task.FromResult(true);
    }

    /// <summary>
    /// Request prescription refill
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public async Task<bool> RequestPrescriptionRefillAsync(
        int prescriptionId,
        string? notes,
        ClaimsPrincipal claimsPrincipal,
        [Service] IPrescriptionService prescriptionService)
    {
        var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return false;

        // TODO: Create refill request notification to doctor
        return await prescriptionService.RefillPrescriptionAsync(prescriptionId);
    }

    /// <summary>
    /// Update medication adherence (mark medication as taken/missed)
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public Task<bool> UpdateMedicationAdherenceAsync(
        UpdateMedicationAdherenceInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] Microsoft.EntityFrameworkCore.DbContext context)
    {
        var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return Task.FromResult(false);

        // TODO: Implement MedicationAdherence entity and save to database
        // For now, returning true
        return Task.FromResult(true);
    }

    /// <summary>
    /// Create a medication reminder
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public Task<MedicationReminderDto> CreateMedicationReminderAsync(
        CreateMedicationReminderInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] Microsoft.EntityFrameworkCore.DbContext context)
    {
        var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            throw new UnauthorizedAccessException("Patient not authenticated");

        // TODO: Implement MedicationReminder entity and save to database
        // For now, returning a dummy response
        return Task.FromResult(new MedicationReminderDto
        {
            Id = 1,
            PatientId = patientId,
            PrescriptionId = input.PrescriptionId,
            MedicationName = "Medication Name", // TODO: Get from prescription
            Dosage = "Dosage", // TODO: Get from prescription
            ReminderTimes = input.ReminderTimes,
            IsActive = true,
            StartDate = input.StartDate,
            EndDate = input.EndDate,
            AdherenceLogs = new List<MedicationAdherenceLog>(),
            TotalDoses = 0,
            DosesTaken = 0,
            DosesMissed = 0,
            AdherencePercentage = 0,
            RefillAlertEnabled = input.RefillAlertEnabled,
            RefillAlertDaysBefore = input.RefillAlertDaysBefore
        });
    }

    /// <summary>
    /// Upload lab result document
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public Task<LabResultDto> UploadLabResultAsync(
        UploadLabResultInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] Microsoft.EntityFrameworkCore.DbContext context)
    {
        var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            throw new UnauthorizedAccessException("Patient not authenticated");

        // TODO: Implement LabResult entity and save to database
        // For now, returning a dummy response
        return Task.FromResult(new LabResultDto
        {
            Id = 1,
            PatientId = patientId,
            TestName = input.TestName,
            TestCategory = input.TestCategory,
            TestDate = input.TestDate,
            ResultDate = DateTime.UtcNow,
            Status = "Completed",
            OrderedByDoctorId = null,
            OrderedByDoctorName = null,
            Parameters = input.Parameters ?? new List<LabTestParameter>(),
            DoctorComments = null,
            DocumentUrl = input.DocumentUrl,
            IsAbnormal = false
        });
    }

    /// <summary>
    /// Send a message to another user
    /// </summary>
    [Authorize]
    public async Task<MessageDto> SendMessageAsync(
        Features.Patient.DTOs.SendMessageInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IMessagingService messagingService)
    {
        var senderId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(senderId))
            throw new UnauthorizedAccessException("User not authenticated");

        var message = await messagingService.SendMessageAsync(senderId, input.ReceiverId, input.Content, input.AttachmentUrl);

        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Content = message.Content,
            SentAt = message.SentAt,
            Status = message.Status.ToString(),
            MessageType = message.MessageType.ToString(),
            AttachmentUrl = message.AttachmentUrl,
            AttachmentFileName = message.AttachmentFileName,
            IsSender = true
        };
    }

    /// <summary>
    /// Mark messages as read
    /// </summary>
    [Authorize]
    public async Task<bool> MarkMessagesAsReadAsync(
        Guid conversationId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IMessagingService messagingService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await messagingService.MarkMessagesAsReadAsync(conversationId, userId);
    }

    /// <summary>
    /// Upload a message attachment (image, video, audio, or file)
    /// </summary>
    [Authorize]
    public async Task<Features.Patient.DTOs.UploadAttachmentResponse> UploadMessageAttachmentAsync(
        string fileBase64,
        string fileName,
        string mimeType,
        ClaimsPrincipal claimsPrincipal,
        [Service] ICloudinaryService cloudinaryService,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            // Validate file size (max 50MB)
            var fileBytes = Convert.FromBase64String(fileBase64);
            if (fileBytes.Length > 50 * 1024 * 1024)
                throw new InvalidOperationException("File size exceeds 50MB limit");

            string? uploadedUrl = null;
            using (var stream = new MemoryStream(fileBytes))
            {
                // Determine file type and upload accordingly
                if (mimeType.StartsWith("image/"))
                {
                    uploadedUrl = await cloudinaryService.UploadImageAsync(stream, fileName, "chat-attachments");
                }
                else if (mimeType.StartsWith("video/"))
                {
                    uploadedUrl = await cloudinaryService.UploadVideoAsync(stream, fileName, "chat-attachments");
                }
                else if (mimeType.StartsWith("audio/"))
                {
                    uploadedUrl = await cloudinaryService.UploadAudioAsync(stream, fileName, "chat-attachments");
                }
                else
                {
                    // For other file types (PDF, DOC, etc.)
                    uploadedUrl = await cloudinaryService.UploadDocumentAsync(stream, fileName, "chat-attachments");
                }
            }

            if (string.IsNullOrEmpty(uploadedUrl))
                throw new InvalidOperationException("File upload failed");

            return new Features.Patient.DTOs.UploadAttachmentResponse
            {
                Url = uploadedUrl,
                FileName = fileName,
                FileSize = fileBytes.Length,
                MimeType = mimeType,
                Success = true
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading message attachment");
            return new Features.Patient.DTOs.UploadAttachmentResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Add or update a reaction to a message
    /// </summary>
    [Authorize]
    public async Task<MessageReactionResponse> AddMessageReactionAsync(
        Guid messageId,
        string emoji,
        ClaimsPrincipal claimsPrincipal,
        [Service] ApplicationDbContext context,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            // Check if message exists
            var message = await context.DoctorMessages.FindAsync(messageId);
            if (message == null)
                throw new InvalidOperationException("Message not found");

            // Check if user already reacted to this message
            var existingReaction = await context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);

            if (existingReaction != null)
            {
                // Update existing reaction
                existingReaction.Emoji = emoji;
                existingReaction.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new reaction
                var reaction = new MessageReaction
                {
                    Id = Guid.NewGuid(),
                    MessageId = messageId,
                    UserId = userId,
                    Emoji = emoji,
                    CreatedAt = DateTime.UtcNow
                };
                context.MessageReactions.Add(reaction);
            }

            await context.SaveChangesAsync();

            // Get reaction counts for this message
            var reactions = await context.MessageReactions
                .Where(r => r.MessageId == messageId)
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionCount { Emoji = g.Key, Count = g.Count() })
                .ToListAsync();

            logger.LogInformation("User {UserId} added reaction {Emoji} to message {MessageId}", userId, emoji, messageId);

            return new MessageReactionResponse
            {
                Success = true,
                MessageId = messageId,
                Reactions = reactions
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding message reaction");
            return new MessageReactionResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Remove a reaction from a message
    /// </summary>
    [Authorize]
    public async Task<MessageReactionResponse> RemoveMessageReactionAsync(
        Guid messageId,
        ClaimsPrincipal claimsPrincipal,
        [Service] ApplicationDbContext context,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var reaction = await context.MessageReactions
                .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);

            if (reaction != null)
            {
                context.MessageReactions.Remove(reaction);
                await context.SaveChangesAsync();
                logger.LogInformation("User {UserId} removed reaction from message {MessageId}", userId, messageId);
            }

            // Get updated reaction counts for this message
            var reactions = await context.MessageReactions
                .Where(r => r.MessageId == messageId)
                .GroupBy(r => r.Emoji)
                .Select(g => new ReactionCount { Emoji = g.Key, Count = g.Count() })
                .ToListAsync();

            return new MessageReactionResponse
            {
                Success = true,
                MessageId = messageId,
                Reactions = reactions
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing message reaction");
            return new MessageReactionResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Edit a message content
    /// </summary>
    [Authorize]
    public async Task<EditMessageResponse> EditMessageAsync(
        Guid messageId,
        string newContent,
        ClaimsPrincipal claimsPrincipal,
        [Service] ApplicationDbContext context,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var message = await context.DoctorMessages.FindAsync(messageId);

            if (message == null)
                throw new InvalidOperationException("Message not found");

            if (message.SenderId != userId)
                throw new UnauthorizedAccessException("You can only edit your own messages");

            if (message.IsDeleted)
                throw new InvalidOperationException("Cannot edit a deleted message");

            // Update message
            message.Content = newContent;
            message.IsEdited = true;
            message.EditedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            logger.LogInformation("User {UserId} edited message {MessageId}", userId, messageId);

            return new EditMessageResponse
            {
                Success = true,
                MessageId = messageId,
                NewContent = newContent,
                EditedAt = message.EditedAt.Value
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error editing message {MessageId}", messageId);
            return new EditMessageResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Delete a message (soft delete)
    /// </summary>
    [Authorize]
    public async Task<DeleteMessageResponse> DeleteMessageAsync(
        Guid messageId,
        ClaimsPrincipal claimsPrincipal,
        [Service] ApplicationDbContext context,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var message = await context.DoctorMessages.FindAsync(messageId);

            if (message == null)
                throw new InvalidOperationException("Message not found");

            if (message.SenderId != userId)
                throw new UnauthorizedAccessException("You can only delete your own messages");

            if (message.IsDeleted)
                throw new InvalidOperationException("Message is already deleted");

            // Soft delete
            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
            message.Content = "This message was deleted";

            await context.SaveChangesAsync();

            logger.LogInformation("User {UserId} deleted message {MessageId}", userId, messageId);

            return new DeleteMessageResponse
            {
                Success = true,
                MessageId = messageId,
                DeletedAt = message.DeletedAt.Value
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return new DeleteMessageResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    // Group Chat Mutations
    [Authorize]
    public async Task<GroupChatResponse> CreateGroupConversation(
        CreateGroupConversationInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ApplicationDbContext context,
        [Service] ILogger<Mutation> logger)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return new GroupChatResponse { Success = false, Message = "User not authenticated" };

        var group = new GroupConversation
        {
            Name = input.Name,
            Description = input.Description,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        context.GroupConversations.Add(group);

        // Add creator as admin
        var creatorMember = new GroupMember
        {
            GroupConversationId = group.Id,
            UserId = userId,
            Role = "Admin",
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };
        context.GroupMembers.Add(creatorMember);

        // Add other members
        foreach (var memberId in input.MemberUserIds.Where(id => id != userId))
        {
            context.GroupMembers.Add(new GroupMember
            {
                GroupConversationId = group.Id,
                UserId = memberId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await context.SaveChangesAsync();

        return new GroupChatResponse
        {
            Success = true,
            Message = "Group created successfully",
            Group = new GroupConversationDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                CreatedBy = group.CreatedBy,
                CreatedAt = group.CreatedAt
            }
        };
    }

    [Authorize]
    public async Task<GroupChatResponse> SendGroupMessage(
        SendGroupMessageInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] ApplicationDbContext context,
        [Service] ILogger<Mutation> logger)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return new GroupChatResponse { Success = false, Message = "User not authenticated" };

        // Verify user is a member
        var isMember = await context.GroupMembers
            .AnyAsync(gm => gm.GroupConversationId == input.GroupConversationId && gm.UserId == userId && gm.IsActive);

        if (!isMember)
            return new GroupChatResponse { Success = false, Message = "You are not a member of this group" };

        var message = new GroupMessage
        {
            GroupConversationId = input.GroupConversationId,
            SenderId = userId,
            Content = input.Content,
            MessageType = input.MessageType,
            AttachmentUrl = input.AttachmentUrl,
            AttachmentName = input.AttachmentName,
            AttachmentSize = input.AttachmentSize,
            ReplyToMessageId = input.ReplyToMessageId,
            SentAt = DateTime.UtcNow
        };

        context.GroupMessages.Add(message);
        await context.SaveChangesAsync();

        var sender = await context.Users.FindAsync(userId);

        return new GroupChatResponse
        {
            Success = true,
            Message = "Message sent successfully",
            GroupMessage = new GroupMessageDto
            {
                Id = message.Id,
                GroupConversationId = message.GroupConversationId,
                SenderId = userId,
                SenderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "Unknown",
                Content = message.Content,
                SentAt = message.SentAt,
                MessageType = message.MessageType,
                AttachmentUrl = message.AttachmentUrl,
                AttachmentName = message.AttachmentName
            }
        };
    }

    public class EditMessageResponse
    {
        public bool Success { get; set; }
        public Guid MessageId { get; set; }
        public string NewContent { get; set; } = string.Empty;
        public DateTime EditedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class DeleteMessageResponse
    {
        public bool Success { get; set; }
        public Guid MessageId { get; set; }
        public DateTime DeletedAt { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class MessageReactionResponse
    {
        public bool Success { get; set; }
        public Guid MessageId { get; set; }
        public List<ReactionCount> Reactions { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class ReactionCount
    {
        public string Emoji { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>
    /// Initialize conversations for existing appointments (Admin only)
    /// </summary>
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<int> InitializeConversationsForExistingAppointmentsAsync(
        [Service] MediChatAI_GraphQl.Shared.Services.ConversationInitializationService conversationService)
    {
        return await conversationService.InitializeConversationsForExistingAppointmentsAsync();
    }

    // ============================================
    // CONSULTATION SYSTEM MUTATIONS
    // ============================================

    /// <summary>
    /// Create a new consultation session from an appointment (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<ConsultationSessionDto> CreateConsultationSessionAsync(
        int appointmentId,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService,
        [Service] ApplicationDbContext context)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        // Get appointment and verify doctor
        var appointment = await context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            throw new ArgumentException("Appointment not found");

        if (appointment.DoctorId != doctorId)
            throw new UnauthorizedAccessException("You can only create consultations for your own appointments");

        return await consultationService.CreateConsultationSessionAsync(appointmentId, doctorId, appointment.PatientId);
    }

    /// <summary>
    /// Start a consultation session (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<ConsultationSessionDto> StartConsultationAsync(
        int sessionId,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await consultationService.StartConsultationAsync(sessionId);
    }

    /// <summary>
    /// End a consultation session (for doctors)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<ConsultationSessionDto> EndConsultationAsync(
        int sessionId,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await consultationService.EndConsultationAsync(sessionId);
    }

    /// <summary>
    /// Update consultation clinical notes (for doctors during consultation)
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> UpdateConsultationNotesAsync(
        UpdateConsultationNotesDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await consultationService.UpdateClinicalNotesAsync(
            input.SessionId,
            input.ChiefComplaint,
            input.DoctorObservations,
            input.Diagnosis,
            input.TreatmentPlan);
    }

    /// <summary>
    /// Add a participant to a consultation session
    /// </summary>
    [Authorize(Roles = new[] { "Doctor", "Patient" })]
    public async Task<ConsultationParticipantDto> AddConsultationParticipantAsync(
        int sessionId,
        CreateParticipantDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated");

        return await consultationService.AddParticipantAsync(sessionId, input);
    }

    /// <summary>
    /// Remove a participant from a consultation session
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<bool> RemoveConsultationParticipantAsync(
        int sessionId,
        int participantId,
        string? reason,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await consultationService.RemoveParticipantAsync(sessionId, participantId, reason);
    }

    /// <summary>
    /// Add a consultation note
    /// </summary>
    [Authorize]
    public async Task<ConsultationNoteDto> AddConsultationNoteAsync(
        int sessionId,
        string content,
        NoteType noteType,
        bool isMarkdown,
        bool isPrivate,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated");

        return await consultationService.AddConsultationNoteAsync(sessionId, userId, content, noteType, isMarkdown, isPrivate);
    }

    /// <summary>
    /// Update a consultation note
    /// </summary>
    [Authorize]
    public async Task<ConsultationNoteDto> UpdateConsultationNoteAsync(
        int noteId,
        string content,
        bool isPrivate,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated");

        return await consultationService.UpdateConsultationNoteAsync(noteId, userId, content, isPrivate);
    }

    /// <summary>
    /// Delete a consultation note
    /// </summary>
    [Authorize]
    public async Task<bool> DeleteConsultationNoteAsync(
        int noteId,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return false;

        return await consultationService.DeleteConsultationNoteAsync(noteId, userId);
    }

    /// <summary>
    /// Start recording a consultation (for patients)
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public async Task<ConsultationRecordingDto> StartConsultationRecordingAsync(
        int sessionId,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationRecordingService recordingService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated");

        return await recordingService.StartRecordingAsync(sessionId, userId);
    }

    /// <summary>
    /// Stop recording a consultation
    /// </summary>
    [Authorize]
    public async Task<ConsultationRecordingDto> StopConsultationRecordingAsync(
        int recordingId,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationRecordingService recordingService)
    {
        return await recordingService.StopRecordingAsync(recordingId);
    }

    /// <summary>
    /// Upload a consultation recording
    /// </summary>
    [Authorize]
    public async Task<ConsultationRecordingDto> UploadConsultationRecordingAsync(
        int recordingId,
        string fileBase64,
        string fileName,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationRecordingService recordingService,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var fileBytes = Convert.FromBase64String(fileBase64);

            if (fileBytes.Length > 500 * 1024 * 1024) // 500MB limit
                throw new InvalidOperationException("Recording file size exceeds 500MB limit");

            using var stream = new MemoryStream(fileBytes);
            return await recordingService.UploadRecordingAsync(recordingId, stream, fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading consultation recording {RecordingId}", recordingId);
            throw new GraphQLException($"Failed to upload recording: {ex.Message}");
        }
    }

    /// <summary>
    /// Update recording access permissions
    /// </summary>
    [Authorize(Roles = new[] { "Doctor", "Patient" })]
    public async Task<bool> UpdateRecordingAccessPermissionsAsync(
        int recordingId,
        bool patientAccess,
        bool doctorAccess,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationRecordingService recordingService)
    {
        return await recordingService.UpdateAccessPermissionsAsync(recordingId, patientAccess, doctorAccess);
    }

    /// <summary>
    /// Delete a consultation recording
    /// </summary>
    [Authorize]
    public async Task<bool> DeleteConsultationRecordingAsync(
        int recordingId,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationRecordingService recordingService)
    {
        return await recordingService.DeleteRecordingAsync(recordingId);
    }

    /// <summary>
    /// Generate AI summary for a consultation session
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<string?> GenerateConsultationSummaryAsync(
        int sessionId,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationTranscriptionService transcriptionService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await transcriptionService.GenerateConsultationSummaryAsync(sessionId);
    }

    /// <summary>
    /// Transcribe a consultation recording
    /// </summary>
    [Authorize]
    public async Task<string?> TranscribeConsultationRecordingAsync(
        int recordingId,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationTranscriptionService transcriptionService)
    {
        return await transcriptionService.TranscribeRecordingAsync(recordingId);
    }

    /// <summary>
    /// Extract clinical data from a transcript
    /// </summary>
    [Authorize(Roles = new[] { "Doctor" })]
    public async Task<ExtractedClinicalDataDto?> ExtractClinicalDataAsync(
        string transcript,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationTranscriptionService transcriptionService)
    {
        var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(doctorId))
            throw new UnauthorizedAccessException("Doctor not authenticated");

        return await transcriptionService.ExtractClinicalDataFromTranscriptAsync(transcript);
    }

    /// <summary>
    /// Rate a completed consultation (for patients)
    /// </summary>
    [Authorize(Roles = new[] { "Patient" })]
    public async Task<bool> RateConsultationSessionAsync(
        RateConsultationDto input,
        ClaimsPrincipal claimsPrincipal,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(patientId))
            return false;

        return await consultationService.RateConsultationAsync(input.SessionId, input.Rating, input.Feedback);
    }

    /// <summary>
    /// Update participant online status
    /// </summary>
    [Authorize]
    public async Task<bool> UpdateParticipantStatusAsync(
        int participantId,
        bool isOnline,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        return await consultationService.UpdateParticipantOnlineStatusAsync(participantId, isOnline);
    }

    /// <summary>
    /// Join consultation session as a guest (using invitation token)
    /// </summary>
    [AllowAnonymous]
    public async Task<ConsultationParticipantDto?> JoinConsultationAsGuestAsync(
        string token,
        [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
    {
        return await consultationService.JoinWithTokenAsync(token);
    }

    // ============================================
    // UTILITY MUTATIONS - DATA FIXES
    // ============================================

    /// <summary>
    /// Fix appointments that have midnight (00:00:00) times by assigning proper appointment times.
    /// This is a utility mutation for fixing data issues where appointments were saved with incorrect times.
    /// </summary>
    [Authorize(Roles = new[] { "Admin", "Doctor" })]
    public async Task<FixMidnightAppointmentsResult> FixMidnightAppointmentsAsync(
        [Service] ApplicationDbContext context,
        [Service] ILogger<Mutation> logger,
        ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;

        logger.LogInformation("FixMidnightAppointmentsAsync called by user {UserId} with role {UserRole}", userId, userRole);

        try
        {
            // Find appointments with midnight times
            var midnightAppointments = await context.Appointments
                .Where(a => a.AppointmentDateTime.TimeOfDay == TimeSpan.Zero &&
                           (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
                .ToListAsync();

            logger.LogInformation("Found {Count} appointments with midnight times", midnightAppointments.Count);

            if (midnightAppointments.Count == 0)
            {
                return new FixMidnightAppointmentsResult
                {
                    Success = true,
                    Message = "No appointments with midnight times found.",
                    AppointmentsFixed = 0,
                    FixedAppointmentIds = new List<int>()
                };
            }

            // Assign reasonable times throughout the day (9 AM, 11 AM, 2 PM, 4 PM, etc.)
            var timesToAssign = new[] { 9, 11, 14, 16, 10, 15 }; // Hours: 9 AM, 11 AM, 2 PM, 4 PM, 10 AM, 3 PM
            var fixedIds = new List<int>();

            for (int i = 0; i < midnightAppointments.Count; i++)
            {
                var appointment = midnightAppointments[i];
                var hourToAdd = timesToAssign[i % timesToAssign.Length];

                var oldDateTime = appointment.AppointmentDateTime;
                appointment.AppointmentDateTime = appointment.AppointmentDateTime.Date.AddHours(hourToAdd);

                logger.LogInformation(
                    "Fixed Appointment {Id}: {OldDate} -> {NewDate}",
                    appointment.Id,
                    oldDateTime,
                    appointment.AppointmentDateTime);

                fixedIds.Add(appointment.Id);
            }

            await context.SaveChangesAsync();

            logger.LogInformation("Successfully fixed {Count} appointments", fixedIds.Count);

            return new FixMidnightAppointmentsResult
            {
                Success = true,
                Message = $"Successfully fixed {fixedIds.Count} appointment(s) with midnight times.",
                AppointmentsFixed = fixedIds.Count,
                FixedAppointmentIds = fixedIds
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fixing midnight appointments");
            return new FixMidnightAppointmentsResult
            {
                Success = false,
                Message = $"Error: {ex.Message}",
                AppointmentsFixed = 0,
                FixedAppointmentIds = new List<int>()
            };
        }
    }

    // ========================
    // Scheduled Reports Mutations
    // ========================

    /// <summary>
    /// Create a new scheduled report
    /// </summary>
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<ScheduledReportResult> CreateScheduledReport(
        CreateScheduledReportInput input,
        [Service] IScheduledReportService scheduledReportService,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var schedule = new ScheduledReport
            {
                ReportId = input.ReportId,
                ReportName = input.ReportName,
                Schedule = input.Schedule,
                Frequency = input.Frequency,
                Recipients = input.Recipients,
                Format = input.Format,
                IsActive = input.IsActive,
                NextRun = input.NextRun
            };

            var result = await scheduledReportService.CreateScheduledReportAsync(schedule);
            
            if (result != null)
            {
                return new ScheduledReportResult
                {
                    Success = true,
                    Message = "Scheduled report created successfully",
                    Schedule = result
                };
            }

            return new ScheduledReportResult
            {
                Success = false,
                Message = "Failed to create scheduled report"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating scheduled report");
            return new ScheduledReportResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Update an existing scheduled report
    /// </summary>
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<ScheduledReportResult> UpdateScheduledReport(
        UpdateScheduledReportInput input,
        [Service] IScheduledReportService scheduledReportService,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var schedule = new ScheduledReport
            {
                Id = input.Id,
                ReportId = input.ReportId,
                ReportName = input.ReportName,
                Schedule = input.Schedule,
                Frequency = input.Frequency,
                Recipients = input.Recipients,
                Format = input.Format,
                IsActive = input.IsActive,
                NextRun = input.NextRun
            };

            var success = await scheduledReportService.UpdateScheduledReportAsync(schedule);
            
            if (success)
            {
                var updated = await scheduledReportService.GetScheduledReportAsync(input.Id);
                return new ScheduledReportResult
                {
                    Success = true,
                    Message = "Scheduled report updated successfully",
                    Schedule = updated
                };
            }

            return new ScheduledReportResult
            {
                Success = false,
                Message = "Failed to update scheduled report"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating scheduled report");
            return new ScheduledReportResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Delete a scheduled report
    /// </summary>
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<ScheduledReportResult> DeleteScheduledReport(
        string id,
        [Service] IScheduledReportService scheduledReportService,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            var success = await scheduledReportService.DeleteScheduledReportAsync(id);
            
            return new ScheduledReportResult
            {
                Success = success,
                Message = success ? "Scheduled report deleted successfully" : "Failed to delete scheduled report"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting scheduled report");
            return new ScheduledReportResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Execute a scheduled report immediately (Run Now)
    /// </summary>
    [Authorize(Roles = new[] { "Admin" })]
    public async Task<ScheduledReportExecutionResult> ExecuteScheduledReportNow(
        string id,
        bool sendEmail,
        [Service] IScheduledReportService scheduledReportService,
        [Service] ILogger<Mutation> logger)
    {
        try
        {
            logger.LogInformation("Manually executing scheduled report: {Id}", id);
            
            var execution = await scheduledReportService.ExecuteScheduledReportAsync(id, sendEmail);
            
            if (execution != null)
            {
                // Get updated schedule info for the response
                var updatedSchedule = await scheduledReportService.GetScheduledReportAsync(id);
                
                // Try to extract report file data from execution
                string? reportBase64 = null;
                string? fileName = null;
                string? mimeType = null;

                if (!string.IsNullOrEmpty(execution.ReportDataJson))
                {
                    try
                    {
                        var reportData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(execution.ReportDataJson);
                        if (reportData.TryGetProperty("ReportBase64", out var base64Prop))
                            reportBase64 = base64Prop.GetString();
                        if (reportData.TryGetProperty("FileName", out var fileNameProp))
                            fileName = fileNameProp.GetString();
                        if (reportData.TryGetProperty("MimeType", out var mimeTypeProp))
                            mimeType = mimeTypeProp.GetString();
                    }
                    catch
                    {
                        // Ignore parsing errors for report data
                    }
                }

                return new ScheduledReportExecutionResult
                {
                    Success = execution.Status == "Success",
                    Message = execution.Status == "Success" 
                        ? $"Report executed successfully. Sent to {execution.RecipientsSent} recipient(s)."
                        : $"Report execution failed: {execution.ErrorMessage}",
                    Execution = execution,
                    ReportBase64 = reportBase64,
                    FileName = fileName,
                    MimeType = mimeType,
                    LastRun = updatedSchedule?.LastRun,
                    NextRun = updatedSchedule?.NextRun
                };
            }

            return new ScheduledReportExecutionResult
            {
                Success = false,
                Message = "Scheduled report not found"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing scheduled report");
            return new ScheduledReportExecutionResult
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }
}

// ========================
// Scheduled Report DTOs
// ========================

public class CreateScheduledReportInput
{
    public string ReportId { get; set; } = "";
    public string ReportName { get; set; } = "";
    public string Schedule { get; set; } = "0 9 * * 1";
    public string Frequency { get; set; } = "Weekly";
    public List<string> Recipients { get; set; } = new();
    public string Format { get; set; } = "PDF";
    public bool IsActive { get; set; } = true;
    public DateTime? NextRun { get; set; }
}

public class UpdateScheduledReportInput
{
    public string Id { get; set; } = "";
    public string ReportId { get; set; } = "";
    public string ReportName { get; set; } = "";
    public string Schedule { get; set; } = "0 9 * * 1";
    public string Frequency { get; set; } = "Weekly";
    public List<string> Recipients { get; set; } = new();
    public string Format { get; set; } = "PDF";
    public bool IsActive { get; set; } = true;
    public DateTime? NextRun { get; set; }
}

public class ScheduledReportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public ScheduledReport? Schedule { get; set; }
}

public class ScheduledReportExecutionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public ScheduledReportExecution? Execution { get; set; }
    public string? ReportBase64 { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
}

/// <summary>
/// Result of fixing midnight appointments
/// </summary>
public class FixMidnightAppointmentsResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int AppointmentsFixed { get; set; }
    public List<int> FixedAppointmentIds { get; set; } = new();
}
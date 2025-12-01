namespace MediChatAI_GraphQl.Core.Interfaces.Services.Shared;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string firstName, string verificationLink);
    Task SendPasswordResetAsync(string email, string firstName, string resetLink);
    Task SendLoginAlertAsync(string email, string firstName, DateTime loginTime);
    Task SendOtpAsync(string email, string firstName, string otpCode);
    Task SendEmailAsync(string email, string subject, string message);

    // 2FA Email Notifications
    Task SendTwoFactorEnabledAsync(string email, string firstName);
    Task SendTwoFactorDisabledAsync(string email, string firstName);

    // Doctor Approval/Rejection Emails
    Task SendDoctorApprovedAsync(string email, string firstName);
    Task SendDoctorRejectedAsync(string email, string firstName, string reason);

    // Appointment Emails
    Task SendAppointmentRequestConfirmationAsync(string email, string firstName, int requestId, string preferredDate, string symptoms);
    Task SendNewAppointmentRequestNotificationAsync(string email, string doctorFirstName, string patientName, string preferredDate, string symptoms, bool isUrgent);
}
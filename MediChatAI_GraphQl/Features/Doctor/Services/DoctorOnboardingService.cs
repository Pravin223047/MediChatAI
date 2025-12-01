using Microsoft.AspNetCore.Identity;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Admin.DTOs;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Features.Authentication.DTOs;
using MediChatAI_GraphQl.Features.Notifications.DTOs;

namespace MediChatAI_GraphQl.Features.Doctor.Services;

public class DoctorOnboardingService : IDoctorOnboardingService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGeminiVerificationService _geminiService;
    private readonly IEmailService _emailService;
    private readonly ILogger<DoctorOnboardingService> _logger;
    private readonly INotificationService _notificationService;

    public DoctorOnboardingService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IGeminiVerificationService geminiService,
        IEmailService emailService,
        ILogger<DoctorOnboardingService> logger,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _geminiService = geminiService;
        _emailService = emailService;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<DoctorProfileResult> CompleteProfileAsync(string userId, DoctorProfileCompletionInput input, string aadhaarImagePath, string? medicalCertificateFilePath = null)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new DoctorProfileResult(false, "User not found", new[] { "Invalid user ID" });

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Doctor"))
                return new DoctorProfileResult(false, "Access denied", new[] { "User is not registered as a doctor" });

            // Use execution strategy for retry logic with SQL Server
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Update user profile
                    user.FirstName = input.FirstName;
                    user.LastName = input.LastName;
                    user.Gender = input.Gender;
                    user.DateOfBirth = input.DateOfBirth;
                    user.PhoneNumber = input.PhoneNumber;
                    user.Address = input.Address;
                    user.City = input.City;
                    user.State = input.State;
                    user.ZipCode = input.ZipCode;
                    user.Specialization = input.Specialization;
                    user.YearsOfExperience = input.YearsOfExperience;
                    user.MedicalRegistrationNumber = input.MedicalRegistrationNumber;
                    user.EducationHistory = input.EducationHistory;
                    user.AffiliatedHospitals = input.AffiliatedHospitals;
                    user.ConsultationHours = input.ConsultationHours;
                    user.AadhaarNumber = input.AadhaarNumber;
                    user.AadhaarCardImagePath = aadhaarImagePath;
                    user.MedicalCertificateUrl = input.MedicalCertificateUrl;
                    user.MedicalCertificateFilePath = medicalCertificateFilePath;
                    user.IsProfileCompleted = true;
                    user.ProfileSubmissionDate = DateTime.UtcNow;
                    user.LastProfileUpdate = DateTime.UtcNow;

                    await _userManager.UpdateAsync(user);

                    // Perform AI verification
                    _logger.LogInformation("Starting AI verification for user {UserId}", userId);
                    var certificateSource = medicalCertificateFilePath ?? input.MedicalCertificateUrl ?? "";
                    var verificationResult = await _geminiService.PerformCompleteVerificationAsync(
                        input.AadhaarNumber, aadhaarImagePath, certificateSource);

                    // Update verification results
                    user.IsAadhaarVerified = verificationResult.IsAadhaarValid;
                    user.IsMedicalCertificateVerified = verificationResult.IsMedicalCertificateValid;
                    user.AiVerificationNotes = verificationResult.CombinedNotes;
                    user.AadhaarVerificationFailureReason = verificationResult.IsAadhaarValid ? null : verificationResult.AadhaarVerificationNotes;
                    user.MedicalCertificateVerificationFailureReason = verificationResult.IsMedicalCertificateValid ? null : verificationResult.MedicalCertificateVerificationNotes;

                    await _userManager.UpdateAsync(user);
                    await transaction.CommitAsync();

                    _logger.LogInformation("Profile completion successful for user {UserId}. Aadhaar: {AadhaarValid}, Certificate: {CertValid}",
                        userId, verificationResult.IsAadhaarValid, verificationResult.IsMedicalCertificateValid);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });

            // Notify doctor of successful submission
            await _notificationService.CreateNotificationAsync(
                userId,
                "Profile Submitted Successfully! 🎉",
                "Your doctor profile has been submitted and is now under review. Our AI verification system has analyzed your documents. You'll be notified once an administrator reviews your application.",
                NotificationType.Success,
                NotificationCategory.Doctor,
                NotificationPriority.High,
                "/doctor/onboarding-status",
                "Check Status");

            // Notify all admins of new submission
            await NotifyAdminOfNewSubmissionAsync(userId);

            return new DoctorProfileResult(true, "Profile completed successfully. AI verification completed. Awaiting admin approval.", Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing doctor profile for user {UserId}", userId);
            return new DoctorProfileResult(false, "An error occurred while completing your profile", new[] { ex.Message });
        }
    }

    public async Task<DoctorOnboardingStatus> GetOnboardingStatusAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        var status = GetStatusString(user);

        return new DoctorOnboardingStatus
        {
            IsProfileCompleted = user.IsProfileCompleted,
            IsAadhaarVerified = user.IsAadhaarVerified,
            IsMedicalCertificateVerified = user.IsMedicalCertificateVerified,
            IsApprovedByAdmin = user.IsApprovedByAdmin,
            AdminRejectionReason = user.AdminRejectionReason,
            AadhaarVerificationFailureReason = user.AadhaarVerificationFailureReason,
            MedicalCertificateVerificationFailureReason = user.MedicalCertificateVerificationFailureReason,
            ProfileSubmissionDate = user.ProfileSubmissionDate,
            AdminApprovalDate = user.AdminApprovalDate,
            Status = status
        };
    }

    public async Task<AdminApprovalResult> ApproveOrRejectDoctorAsync(string adminUserId, AdminApprovalInput input)
    {
        try
        {
            var admin = await _userManager.FindByIdAsync(adminUserId);
            if (admin == null)
                return new AdminApprovalResult(false, "Admin not found", new[] { "Invalid admin ID" });

            var doctor = await _userManager.FindByIdAsync(input.DoctorUserId);
            if (doctor == null)
                return new AdminApprovalResult(false, "Doctor not found", new[] { "Invalid doctor ID" });

            if (!doctor.IsProfileCompleted)
                return new AdminApprovalResult(false, "Doctor profile is not completed", new[] { "Profile must be completed before approval" });

            doctor.IsApprovedByAdmin = input.IsApproved;
            doctor.AdminApprovalDate = DateTime.UtcNow;
            doctor.ApprovedByAdminId = adminUserId;

            if (!input.IsApproved)
            {
                doctor.AdminRejectionReason = input.RejectionReason;
            }
            else
            {
                doctor.AdminRejectionReason = null;
            }

            await _userManager.UpdateAsync(doctor);

            // Send professional notification email to doctor
            if (input.IsApproved)
            {
                await _emailService.SendDoctorApprovedAsync(doctor.Email!, doctor.FirstName);

                // Send in-app notification for approval
                await _notificationService.CreateNotificationAsync(
                    doctor.Id,
                    "Congratulations! Application Approved ✅",
                    "Your doctor application has been approved by our administrative team. You now have full access to all doctor features. Welcome to MediChatAI!",
                    NotificationType.Success,
                    NotificationCategory.Doctor,
                    NotificationPriority.Urgent,
                    "/doctor-dashboard",
                    "Go to Dashboard");
            }
            else
            {
                await _emailService.SendDoctorRejectedAsync(doctor.Email!, doctor.FirstName, input.RejectionReason ?? "No specific reason provided");

                // Send in-app notification for rejection
                await _notificationService.CreateNotificationAsync(
                    doctor.Id,
                    "Application Status Update",
                    $"Unfortunately, your doctor application has been declined. Reason: {input.RejectionReason ?? "No specific reason provided"}. You may contact support for more information or resubmit your application.",
                    NotificationType.Warning,
                    NotificationCategory.Doctor,
                    NotificationPriority.Urgent,
                    null,
                    null);
            }

            var statusMessage = input.IsApproved ? "Doctor approved successfully" : "Doctor application rejected";
            return new AdminApprovalResult(true, statusMessage, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing doctor approval for {DoctorId} by admin {AdminId}", input.DoctorUserId, adminUserId);
            return new AdminApprovalResult(false, "An error occurred while processing the approval", new[] { ex.Message });
        }
    }

    public async Task<IEnumerable<PendingDoctorApproval>> GetPendingApprovalsAsync()
    {
        var pendingDoctors = await _context.Users
            .AsNoTracking()
            .Where(u => u.IsProfileCompleted && !u.IsApprovedByAdmin && string.IsNullOrEmpty(u.AdminRejectionReason))
            .Select(u => new PendingDoctorApproval
            {
                UserId = u.Id,
                Email = u.Email!,
                FullName = $"{u.FirstName} {u.LastName}",
                Specialization = u.Specialization ?? "",
                MedicalRegistrationNumber = u.MedicalRegistrationNumber ?? "",
                YearsOfExperience = u.YearsOfExperience ?? 0,
                IsAadhaarVerified = u.IsAadhaarVerified,
                IsMedicalCertificateVerified = u.IsMedicalCertificateVerified,
                AiVerificationNotes = u.AiVerificationNotes,
                AadhaarVerificationFailureReason = u.AadhaarVerificationFailureReason,
                MedicalCertificateVerificationFailureReason = u.MedicalCertificateVerificationFailureReason,
                ProfileSubmissionDate = u.ProfileSubmissionDate,
                AadhaarCardImagePath = u.AadhaarCardImagePath,
                MedicalCertificateFilePath = u.MedicalCertificateFilePath
            })
            .OrderBy(d => d.ProfileSubmissionDate)
            .ToListAsync();

        return pendingDoctors;
    }

    public async Task NotifyAdminOfNewSubmissionAsync(string doctorUserId)
    {
        try
        {
            var doctor = await _userManager.FindByIdAsync(doctorUserId);
            if (doctor == null) return;

            // Optimize: Query only admin IDs and then fetch full user objects when needed
            var adminRole = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null) return;

            var adminIds = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.RoleId == adminRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var admins = await _context.Users
                .Where(u => adminIds.Contains(u.Id))
                .ToListAsync();

            var subject = "New Doctor Profile Submission";
            var message = $@"
A new doctor has completed their profile and is awaiting approval:

Doctor: {doctor.FirstName} {doctor.LastName}
Email: {doctor.Email}
Specialization: {doctor.Specialization}
Registration Number: {doctor.MedicalRegistrationNumber}
Submission Date: {doctor.ProfileSubmissionDate:yyyy-MM-dd HH:mm}

AI Verification Results:
- Aadhaar Verified: {(doctor.IsAadhaarVerified ? "Yes" : "No")}
- Medical Certificate Verified: {(doctor.IsMedicalCertificateVerified ? "Yes" : "No")}

Please review and approve the application in the admin panel.
";

            // Send email notifications to admins
            foreach (var admin in admins)
            {
                await _emailService.SendEmailAsync(admin.Email!, subject, message);
            }

            // Send in-app notifications to all admins (reuse adminIds from above)
            await _notificationService.SendBulkNotificationsAsync(
                adminIds,
                "New Doctor Application Pending Review",
                $"Dr. {doctor.FirstName} {doctor.LastName} ({doctor.Specialization}) has submitted their profile for approval. AI Verification: Aadhaar {(doctor.IsAadhaarVerified ? "✓" : "✗")}, Certificate {(doctor.IsMedicalCertificateVerified ? "✓" : "✗")}",
                NotificationType.Info,
                NotificationCategory.Admin,
                NotificationPriority.High,
                "/admin/users",
                "Review Application");

            _logger.LogInformation("Admin notification sent for new doctor submission: {DoctorId}", doctorUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin notification for doctor {DoctorId}", doctorUserId);
        }
    }

    private static string GetStatusString(ApplicationUser user)
    {
        if (!user.IsProfileCompleted)
            return "Profile Incomplete";

        if (user.IsApprovedByAdmin)
            return "Approved";

        if (!string.IsNullOrEmpty(user.AdminRejectionReason))
            return "Rejected";

        return "Pending Approval";
    }
}
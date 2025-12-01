using MediChatAI_GraphQl.Features.Doctor.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;

public interface IDoctorOnboardingService
{
    Task<DoctorProfileResult> CompleteProfileAsync(string userId, DoctorProfileCompletionInput input, string aadhaarImagePath, string? medicalCertificateFilePath = null);
    Task<DoctorOnboardingStatus> GetOnboardingStatusAsync(string userId);
    Task<AdminApprovalResult> ApproveOrRejectDoctorAsync(string adminUserId, AdminApprovalInput input);
    Task<IEnumerable<PendingDoctorApproval>> GetPendingApprovalsAsync();
    Task NotifyAdminOfNewSubmissionAsync(string doctorUserId);
}
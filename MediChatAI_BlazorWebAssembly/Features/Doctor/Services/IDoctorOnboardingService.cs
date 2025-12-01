using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public interface IDoctorOnboardingService
{
    Task<DoctorProfileResult> CompleteDoctorProfileAsync(DoctorProfileCompletionModel model, string aadhaarImagePath, string? medicalCertificateFilePath = null);
    Task<DoctorOnboardingStatusModel> GetOnboardingStatusAsync();
    Task<List<PendingDoctorApprovalModel>> GetPendingApprovalsAsync();
    Task<AdminApprovalResult> ApproveOrRejectDoctorAsync(AdminApprovalModel model);
    Task<string?> UploadAadhaarCardAsync(Stream fileStream, string fileName);
    Task<string?> UploadMedicalCertificateAsync(Stream fileStream, string fileName);
    Task<string?> UploadProfileImageAsync(Stream fileStream, string fileName);
}
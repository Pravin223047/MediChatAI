using MediChatAI_GraphQl.Features.Doctor.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;

public interface IGeminiVerificationService
{
    Task<AiVerificationResult> VerifyAadhaarAsync(string aadhaarNumber, string imagePath);
    Task<AiVerificationResult> VerifyMedicalCertificateAsync(string certificateUrl);
    Task<AiVerificationResult> PerformCompleteVerificationAsync(string aadhaarNumber, string imagePath, string certificateUrl);
}
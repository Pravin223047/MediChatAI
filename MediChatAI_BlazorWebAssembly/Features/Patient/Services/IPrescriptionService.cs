using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IPrescriptionService
{
    Task<List<PrescriptionDto>> GetMyPrescriptionsAsync();
    Task<List<PrescriptionDto>> GetMyActivePrescriptionsAsync();
    Task<bool> RequestRefillAsync(int prescriptionId);
}

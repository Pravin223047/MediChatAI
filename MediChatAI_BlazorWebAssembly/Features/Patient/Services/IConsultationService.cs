using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IConsultationService
{
    Task<List<ConsultationHistoryDto>> GetPatientConsultationsAsync(GetConsultationsInput? input = null);
    Task<bool> RateConsultationAsync(int consultationId, int rating, string? feedback = null);
}

using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IHealthMetricsService
{
    Task<List<HealthMetricDto>> GetHealthMetricsAsync(GetHealthMetricsInput input);
    Task<HealthMetricDto?> RecordHealthMetricAsync(RecordHealthMetricInput input);
}

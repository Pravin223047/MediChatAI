using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IDashboardService
{
    Task<PatientDashboardData?> GetDashboardDataAsync();
}

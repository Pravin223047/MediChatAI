using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetMyAppointmentsAsync();
    Task<bool> CancelAppointmentAsync(int appointmentId, string reason);
}

using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetMyAppointmentsAsync();
    Task<List<AppointmentRequestDto>> GetMyAppointmentRequestsAsync();
    Task<bool> CancelAppointmentAsync(int appointmentId, string reason);
    Task<bool> CancelAppointmentRequestAsync(int requestId, string reason);
}

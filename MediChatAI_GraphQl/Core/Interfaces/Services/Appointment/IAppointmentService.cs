using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Appointment;

public interface IAppointmentService
{
    // Appointment CRUD
    Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
    Task<List<AppointmentDto>> GetAppointmentsByPatientIdAsync(string patientId);
    Task<List<AppointmentDto>> GetAppointmentsByDoctorIdAsync(string doctorId);
    Task<List<AppointmentDto>> GetUpcomingAppointmentsByPatientIdAsync(string patientId);
    Task<List<AppointmentDto>> GetUpcomingAppointmentsByDoctorIdAsync(string doctorId);
    Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto);
    Task<AppointmentDto> UpdateAppointmentAsync(UpdateAppointmentDto dto);
    Task<bool> CancelAppointmentAsync(int id, string cancellationReason);
    Task<bool> CompleteAppointmentAsync(int id, string? doctorNotes);

    // Appointment Request Management
    Task<AppointmentRequestDto?> GetAppointmentRequestByIdAsync(int id);
    Task<List<AppointmentRequestDto>> GetPendingRequestsByDoctorIdAsync(string doctorId);
    Task<List<AppointmentRequestDto>> GetRequestsByPatientIdAsync(string patientId);
    Task<AppointmentRequestDto> CreateAppointmentRequestAsync(CreateAppointmentRequestDto dto);
    Task<AppointmentDto> ApproveAppointmentRequestAsync(ReviewAppointmentRequestDto dto);
    Task<bool> RejectAppointmentRequestAsync(int requestId, string doctorId, string reason);

    // Doctor Availability
    Task<List<DoctorAvailabilityDto>> GetDoctorAvailabilityAsync(string doctorId, DateTime startDate, DateTime endDate);
    Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(string doctorId, DateTime date);
}

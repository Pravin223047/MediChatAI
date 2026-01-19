using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetMyAppointmentsAsync();
    Task<List<AppointmentRequestDto>> GetMyAppointmentRequestsAsync();
    Task<bool> CancelAppointmentAsync(int appointmentId, string reason);
    Task<bool> CancelAppointmentRequestAsync(int requestId, string reason);
    Task<AppointmentRequestDto?> CreateQuickAppointmentRequestAsync(QuickAppointmentRequestInput input);
}

/// <summary>
/// Simplified input for creating appointment request from chat/messaging
/// </summary>
public class QuickAppointmentRequestInput
{
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public int PatientAge { get; set; } = 0;
    public string? PatientGender { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime PreferredDateTime { get; set; }
    public string AppointmentType { get; set; } = "Teleconsultation"; // InPerson or Teleconsultation
    public string ReasonForVisit { get; set; } = string.Empty;
}

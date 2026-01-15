using System.Text.Json.Serialization;
using MediChatAI_BlazorWebAssembly.Core.Utilities;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

[JsonConverter(typeof(CaseInsensitiveEnumConverter<AppointmentStatus>))]
public enum AppointmentStatus
{
    Pending,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    Rescheduled,
    NoShow
}

[JsonConverter(typeof(CaseInsensitiveEnumConverter<AppointmentType>))]
public enum AppointmentType
{
    General,
    FollowUp,
    Emergency,
    Consultation,
    Checkup,
    Surgery,
    Vaccination,
    LabTest
}

public class AppointmentDto
{
    public int Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? PatientEmail { get; set; }
    public string? PatientPhone { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorSpecialization { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public AppointmentType Type { get; set; }
    public string? RoomNumber { get; set; }
    public string? Location { get; set; }
    public bool IsVirtual { get; set; }
    public string? MeetingLink { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? DoctorNotes { get; set; }
    public string? PatientNotes { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
}

// GraphQL Response Models
public class AppointmentsResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("myAppointments")]
    public List<AppointmentDto> MyAppointments { get; set; } = new();
}

public class CancelAppointmentResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("cancelAppointment")]
    public bool CancelAppointment { get; set; }
}

// Request Status enum
[JsonConverter(typeof(CaseInsensitiveEnumConverter<RequestStatus>))]
public enum RequestStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    Cancelled
}

// Appointment Request DTOs
public class AppointmentRequestDto
{
    public int Id { get; set; }
    public string? PatientId { get; set; }
    public string? PreferredDoctorId { get; set; }
    public string? PreferredDoctorName { get; set; }
    public string? FullName { get; set; }
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? SymptomDescription { get; set; }
    public string? SymptomSeverity { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? InsuranceProvider { get; set; }
    public DateTime? PreferredDate { get; set; }
    public string? PreferredTimeSlot { get; set; }
    public bool IsUrgent { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public int? AppointmentId { get; set; }
}

public class AppointmentRequestsResponse
{
    [JsonPropertyName("myAppointmentRequests")]
    public List<AppointmentRequestDto> MyAppointmentRequests { get; set; } = new();
}

public class CreateAppointmentRequestResponse
{
    public AppointmentRequestDto? CreateAppointmentRequest { get; set; }
}

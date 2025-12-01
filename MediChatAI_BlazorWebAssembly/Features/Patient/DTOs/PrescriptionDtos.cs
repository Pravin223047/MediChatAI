using System.Text.Json.Serialization;
using MediChatAI_BlazorWebAssembly.Core.Utilities;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

[JsonConverter(typeof(CaseInsensitiveEnumConverter<PrescriptionStatus>))]
public enum PrescriptionStatus
{
    Active,
    Completed,
    Cancelled,
    Expired
}

public class PrescriptionItemDto
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? Route { get; set; }
    public int DurationDays { get; set; }
    public int Quantity { get; set; }
    public string? Form { get; set; }
    public string? Instructions { get; set; }
    public string? Warnings { get; set; }
    public string? SideEffects { get; set; }
}

public class PrescriptionDto
{
    public int Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public int? AppointmentId { get; set; }
    public int? ConsultationSessionId { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = new();
    public DateTime PrescribedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PrescriptionStatus Status { get; set; }
    public int RefillsAllowed { get; set; }
    public int RefillsUsed { get; set; }
    public string? Diagnosis { get; set; }
    public string? DoctorNotes { get; set; }
}


namespace MediChatAI_GraphQl.Features.Patient.DTOs;

public class PatientListDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }

    // Demographics
    public DateTime? DateOfBirth { get; set; }
    public int? Age => DateOfBirth.HasValue
        ? DateTime.UtcNow.Year - DateOfBirth.Value.Year - (DateTime.UtcNow.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
        : null;
    public string? Gender { get; set; }
    public string? BloodType { get; set; }

    // Location
    public string? City { get; set; }
    public string? State { get; set; }

    // Medical Summary
    public string? CurrentCondition { get; set; } // Primary/chronic condition
    public int TotalVisits { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public DateTime? NextAppointmentDate { get; set; }

    // Status
    public string Status { get; set; } = "Active"; // Active, Critical, Follow-up, Stable
    public bool HasCriticalVitals { get; set; }

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

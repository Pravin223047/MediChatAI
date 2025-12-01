namespace MediChatAI_GraphQl.Features.Patient.DTOs;

public class PatientDetailDto
{
    // Basic Information
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

    // Contact Information
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // Medical Information
    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }

    // Insurance Information
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceGroupNumber { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime? LastProfileUpdate { get; set; }

    // Related Data
    public List<AppointmentSummaryDto> RecentAppointments { get; set; } = new();
    public List<PrescriptionSummaryDto> RecentPrescriptions { get; set; } = new();
    public List<VitalSummaryDto> RecentVitals { get; set; } = new();
    public List<DocumentSummaryDto> RecentDocuments { get; set; } = new();

    // Statistics
    public int TotalAppointments { get; set; }
    public int TotalPrescriptions { get; set; }
    public int TotalDocuments { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public DateTime? NextAppointmentDate { get; set; }
    public bool HasCriticalVitals { get; set; }
}

public class AppointmentSummaryDto
{
    public int Id { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ReasonForVisit { get; set; }
    public string? DoctorNotes { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
}

public class PrescriptionSummaryDto
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime PrescribedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public int DurationDays { get; set; }
}

public class VitalSummaryDto
{
    public Guid Id { get; set; }
    public string VitalType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string? Notes { get; set; }
    public int? SystolicValue { get; set; }
    public int? DiastolicValue { get; set; }
}

public class DocumentSummaryDto
{
    public int Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
    public long FileSize { get; set; }
}

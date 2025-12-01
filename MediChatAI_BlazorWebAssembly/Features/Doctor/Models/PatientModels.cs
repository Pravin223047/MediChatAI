using System.ComponentModel.DataAnnotations;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

public class PatientListItemModel
{
    public string PatientId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime? DateOfBirth { get; set; }
    public int Age
    {
        get
        {
            if (!DateOfBirth.HasValue) return 0;
            var age = DateTime.Now.Year - DateOfBirth.Value.Year;
            if (DateOfBirth.Value.Date > DateTime.Now.AddYears(-age)) age--;
            return age;
        }
    }
    public string Gender { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? BloodGroup { get; set; }
    public DateTime LastVisitDate { get; set; }
    public DateTime? NextAppointmentDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Critical, Follow-up, Stable
    public string? CurrentCondition { get; set; }
    public List<string> Tags { get; set; } = new();
    public int TotalVisits { get; set; }
}

public class PatientDetailModel
{
    public string PatientId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime? DateOfBirth { get; set; }
    public int Age
    {
        get
        {
            if (!DateOfBirth.HasValue) return 0;
            var age = DateTime.Now.Year - DateOfBirth.Value.Year;
            if (DateOfBirth.Value.Date > DateTime.Now.AddYears(-age)) age--;
            return age;
        }
    }
    public string Gender { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string? BloodGroup { get; set; }
    public double? Height { get; set; } // in cm
    public double? Weight { get; set; } // in kg
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public List<string> Allergies { get; set; } = new();
    public List<string> CurrentMedications { get; set; } = new();
    public List<string> ChronicConditions { get; set; } = new();
    public string Status { get; set; } = "Active";
    public DateTime RegistrationDate { get; set; }
    public DateTime LastVisitDate { get; set; }
    public DateTime? NextAppointmentDate { get; set; }
    public int TotalVisits { get; set; }
    public List<VitalSignsModel> VitalSignsHistory { get; set; } = new();
    public List<MedicalHistoryItemModel> MedicalHistory { get; set; } = new();
}

public class VitalSignsModel
{
    public DateTime RecordedDate { get; set; }
    public double? BloodPressureSystolic { get; set; }
    public double? BloodPressureDiastolic { get; set; }
    public double? HeartRate { get; set; } // bpm
    public double? Temperature { get; set; } // Fahrenheit
    public double? OxygenSaturation { get; set; } // %
    public double? RespiratoryRate { get; set; } // breaths per minute
    public double? BloodSugar { get; set; } // mg/dL
    public string? Notes { get; set; }
}

public class MedicalHistoryItemModel
{
    public DateTime VisitDate { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string Treatment { get; set; } = string.Empty;
    public string? Prescription { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class PatientSearchFilter
{
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? Gender { get; set; }
    public int? AgeFrom { get; set; }
    public int? AgeTo { get; set; }
    public DateTime? LastVisitFrom { get; set; }
    public DateTime? LastVisitTo { get; set; }
    public List<string> Tags { get; set; } = new();
    public string SortBy { get; set; } = "LastVisit"; // LastVisit, Name, Age
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class PatientListResponse
{
    public List<PatientListItemModel> Patients { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class AddPatientModel
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime? DateOfBirth { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required, Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    public string ZipCode { get; set; } = string.Empty;

    public string? BloodGroup { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

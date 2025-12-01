using System.ComponentModel.DataAnnotations;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

public class DoctorProfileCompletionModel
{
    [Required, StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required]
    public DateTime? DateOfBirth { get; set; }

    [Required, StringLength(15)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required, StringLength(10)]
    public string ZipCode { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Specialization { get; set; } = string.Empty;

    [Required, Range(0, 50)]
    public int YearsOfExperience { get; set; }

    [Required, StringLength(50)]
    public string MedicalRegistrationNumber { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string EducationHistory { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string AffiliatedHospitals { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string ConsultationHours { get; set; } = string.Empty;

    [Required, StringLength(12)]
    public string AadhaarNumber { get; set; } = string.Empty;

    public string? MedicalCertificateUrl { get; set; }
}

public class DoctorProfileResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class DoctorOnboardingStatusModel
{
    public bool IsProfileCompleted { get; set; }
    public bool IsAadhaarVerified { get; set; }
    public bool IsMedicalCertificateVerified { get; set; }
    public bool IsApprovedByAdmin { get; set; }
    public string? AdminRejectionReason { get; set; }
    public string? AadhaarVerificationFailureReason { get; set; }
    public string? MedicalCertificateVerificationFailureReason { get; set; }
    public DateTime? ProfileSubmissionDate { get; set; }
    public DateTime? AdminApprovalDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PendingDoctorApprovalModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? MedicalRegistrationNumber { get; set; }
    public int? YearsOfExperience { get; set; }
    public bool IsAadhaarVerified { get; set; }
    public bool IsMedicalCertificateVerified { get; set; }
    public string? AiVerificationNotes { get; set; }
    public string? AadhaarVerificationFailureReason { get; set; }
    public string? MedicalCertificateVerificationFailureReason { get; set; }
    public DateTime? ProfileSubmissionDate { get; set; }
    public string? AadhaarCardImagePath { get; set; }
    public string? MedicalCertificateFilePath { get; set; }
}

public class AdminApprovalModel
{
    [Required]
    public string DoctorUserId { get; set; } = string.Empty;

    [Required]
    public bool IsApproved { get; set; }

    public string? RejectionReason { get; set; }
}

public class AdminApprovalResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
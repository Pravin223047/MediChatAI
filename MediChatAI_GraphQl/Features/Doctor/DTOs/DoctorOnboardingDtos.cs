using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Features.Doctor.DTOs;

public record DoctorProfileCompletionInput
{
    [Required, StringLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string LastName { get; init; } = string.Empty;

    [Required]
    public string Gender { get; init; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; init; }

    [Required, StringLength(15)]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required, StringLength(500)]
    public string Address { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string State { get; init; } = string.Empty;

    [Required, StringLength(10)]
    public string ZipCode { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string Specialization { get; init; } = string.Empty;

    [Required, Range(0, 50)]
    public int YearsOfExperience { get; init; }

    [Required, StringLength(50)]
    public string MedicalRegistrationNumber { get; init; } = string.Empty;

    [Required, StringLength(1000)]
    public string EducationHistory { get; init; } = string.Empty;

    [Required, StringLength(500)]
    public string AffiliatedHospitals { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string ConsultationHours { get; init; } = string.Empty;

    [Required, StringLength(12)]
    public string AadhaarNumber { get; init; } = string.Empty;

    public string? MedicalCertificateUrl { get; init; }
}

public record DoctorProfileResult(
    bool Success,
    string? Message,
    IEnumerable<string> Errors
);

public record AiVerificationResult
{
    public bool IsAadhaarValid { get; init; }
    public bool IsMedicalCertificateValid { get; init; }
    public string AadhaarVerificationNotes { get; init; } = string.Empty;
    public string MedicalCertificateVerificationNotes { get; init; } = string.Empty;
    public string CombinedNotes { get; init; } = string.Empty;
}

public record AdminApprovalInput
{
    [Required]
    public string DoctorUserId { get; init; } = string.Empty;

    [Required]
    public bool IsApproved { get; init; }

    public string? RejectionReason { get; init; }
}

public record AdminApprovalResult(
    bool Success,
    string Message,
    IEnumerable<string> Errors
);

public record DoctorOnboardingStatus
{
    public bool IsProfileCompleted { get; init; }
    public bool IsAadhaarVerified { get; init; }
    public bool IsMedicalCertificateVerified { get; init; }
    public bool IsApprovedByAdmin { get; init; }
    public string? AdminRejectionReason { get; init; }
    public string? AadhaarVerificationFailureReason { get; init; }
    public string? MedicalCertificateVerificationFailureReason { get; init; }
    public DateTime? ProfileSubmissionDate { get; init; }
    public DateTime? AdminApprovalDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

public record PendingDoctorApproval
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Specialization { get; init; } = string.Empty;
    public string MedicalRegistrationNumber { get; init; } = string.Empty;
    public int YearsOfExperience { get; init; }
    public bool IsAadhaarVerified { get; init; }
    public bool IsMedicalCertificateVerified { get; init; }
    public string? AiVerificationNotes { get; init; }
    public string? AadhaarVerificationFailureReason { get; init; }
    public string? MedicalCertificateVerificationFailureReason { get; init; }
    public DateTime? ProfileSubmissionDate { get; init; }
    public string? AadhaarCardImagePath { get; init; }
    public string? MedicalCertificateFilePath { get; init; }
}
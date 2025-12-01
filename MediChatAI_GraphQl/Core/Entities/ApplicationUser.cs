using Microsoft.AspNetCore.Identity;

namespace MediChatAI_GraphQl.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
    public string? OtpCode { get; set; }
    public DateTime OtpExpiryTime { get; set; }

    // Extended Profile Fields
    public string? ProfileImage { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; } = "United States";
    public string? Specialization { get; set; } // For doctors
    public string? LicenseNumber { get; set; } // For doctors
    public string? Department { get; set; } // For doctors
    public string? EmergencyContactName { get; set; } // For patients
    public string? EmergencyContactPhone { get; set; } // For patients
    public string? BloodType { get; set; } // For patients
    public string? Allergies { get; set; } // For patients
    public string? MedicalHistory { get; set; } // For patients

    // Insurance information (For patients)
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceGroupNumber { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }

    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastProfileUpdate { get; set; }

    // Doctor onboarding fields
    public int? YearsOfExperience { get; set; }
    public string? MedicalRegistrationNumber { get; set; }
    public string? EducationHistory { get; set; }
    public string? AffiliatedHospitals { get; set; }
    public string? ConsultationHours { get; set; }
    public string? AadhaarNumber { get; set; }
    public string? AadhaarCardImagePath { get; set; }
    public string? MedicalCertificateUrl { get; set; }
    public string? MedicalCertificateFilePath { get; set; }
    public bool IsProfileCompleted { get; set; } = false;
    public bool IsAadhaarVerified { get; set; } = false;
    public bool IsMedicalCertificateVerified { get; set; } = false;
    public string? AiVerificationNotes { get; set; }
    public string? AadhaarVerificationFailureReason { get; set; }
    public string? MedicalCertificateVerificationFailureReason { get; set; }
    public DateTime? ProfileSubmissionDate { get; set; }
    public bool IsApprovedByAdmin { get; set; } = false;
    public DateTime? AdminApprovalDate { get; set; }
    public string? ApprovedByAdminId { get; set; }
    public string? AdminRejectionReason { get; set; }

    // Security tracking fields
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? AccountLockedUntil { get; set; }
    public DateTime? PasswordLastChangedAt { get; set; }
    public bool IsPasswordExpired { get; set; } = false;

    // Authenticator app (TOTP) fields
    public string? AuthenticatorKey { get; set; }
    public bool IsAuthenticatorEnabled { get; set; } = false;
}
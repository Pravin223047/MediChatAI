namespace MediChatAI_GraphQl.Core.Entities;

public enum RequestStatus
{
    Pending,
    UnderReview,
    Approved,
    Rejected,
    Cancelled
}

public class AppointmentRequest
{
    public int Id { get; set; }

    // Patient information
    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser? Patient { get; set; }

    // Requested doctor (optional - can be assigned later)
    public string? PreferredDoctorId { get; set; }
    public ApplicationUser? PreferredDoctor { get; set; }

    // Basic information
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternatePhoneNumber { get; set; }

    // Address
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }

    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }

    // Medical history
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? CurrentMedications { get; set; }
    public string? PastMedicalConditions { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? PreviousSurgeries { get; set; }
    public string? FamilyMedicalHistory { get; set; }

    // Symptoms and reason for visit
    public string SymptomDescription { get; set; } = string.Empty;
    public string? SymptomDuration { get; set; }
    public string? SymptomSeverity { get; set; } // Mild, Moderate, Severe
    public string? ReasonForVisit { get; set; }
    public AppointmentType PreferredAppointmentType { get; set; } = AppointmentType.General;

    // Multi-modal data - references to uploaded files and recordings
    public string? VoiceRecordingUrl { get; set; }
    public string? VoiceRecordingTranscript { get; set; }

    // Photo URLs - stored as JSON array
    public string? PhotoUrls { get; set; }

    // Video URLs - stored as JSON array
    public string? VideoUrls { get; set; }

    // Medical document URLs - stored as JSON array
    public string? MedicalDocumentUrls { get; set; }

    // Insurance card URLs - stored as JSON array (separate from InsuranceCardFrontUrl/BackUrl for additional cards)
    public string? InsuranceCardUrls { get; set; }

    // Insurance and payment
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceGroupNumber { get; set; }
    public string? InsuranceCardFrontUrl { get; set; }
    public string? InsuranceCardBackUrl { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public string? PaymentMethod { get; set; } // Cash, Card, Insurance, etc.

    // Preferred timing
    public DateTime? PreferredDate { get; set; }
    public string? PreferredTimeSlot { get; set; } // Morning, Afternoon, Evening
    public bool IsUrgent { get; set; } = false;

    // Request status and tracking
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByDoctorId { get; set; }
    public ApplicationUser? ReviewedByDoctor { get; set; }
    public string? ReviewNotes { get; set; }
    public string? RejectionReason { get; set; }

    // If approved, link to created appointment
    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    // Additional notes
    public string? PatientAdditionalNotes { get; set; }
    public string? AdminNotes { get; set; }
}

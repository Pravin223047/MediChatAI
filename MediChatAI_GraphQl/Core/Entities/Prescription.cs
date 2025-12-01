namespace MediChatAI_GraphQl.Core.Entities;

public enum PrescriptionStatus
{
    Active,
    Completed,
    Cancelled,
    Expired
}

public class Prescription
{
    public int Id { get; set; }

    // Patient and Doctor relationship
    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser? Patient { get; set; }

    public string DoctorId { get; set; } = string.Empty;
    public ApplicationUser? Doctor { get; set; }

    // Related to appointment
    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    // Related to consultation session (for prescriptions issued during live consultation)
    public int? ConsultationSessionId { get; set; }
    public ConsultationSession? ConsultationSession { get; set; }

    // Prescription items (medications)
    public List<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();

    // Prescription details
    public DateTime PrescribedDate { get; set; } = DateTime.UtcNow;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;

    // Refill information
    public int RefillsAllowed { get; set; } = 0;
    public int RefillsUsed { get; set; } = 0;
    public DateTime? LastRefillDate { get; set; }

    // Clinical information
    public string? Diagnosis { get; set; }
    public string? DoctorNotes { get; set; }
    public string? PharmacyNotes { get; set; }

    // Digital signature and verification
    public string? DoctorSignature { get; set; } // Digital signature or URL
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }

    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Pharmacy information (optional)
    public string? PharmacyName { get; set; }
    public string? PharmacyContact { get; set; }
    public bool IsDispensed { get; set; } = false;
    public DateTime? DispensedAt { get; set; }
}

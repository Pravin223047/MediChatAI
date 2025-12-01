namespace MediChatAI_GraphQl.Core.Entities;

public enum DocumentType
{
    MedicalReport,
    LabResult,
    Prescription,
    XRay,
    MRI,
    CTScan,
    Ultrasound,
    InsuranceCard,
    IdentityProof,
    Photo,
    Other
}

public class PatientDocument
{
    public int Id { get; set; }

    // Patient relationship
    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser? Patient { get; set; }

    // Document details
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? PublicId { get; set; } // Cloudinary public ID for deletion
    public DocumentType DocumentType { get; set; } = DocumentType.Other;
    public string? MimeType { get; set; }
    public long FileSizeBytes { get; set; }

    // Metadata
    public string? Description { get; set; }
    public string? Tags { get; set; } // Comma-separated tags for easy searching
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Optional linking to appointment request or appointment
    public int? AppointmentRequestId { get; set; }
    public AppointmentRequest? AppointmentRequest { get; set; }

    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    // Who uploaded (could be patient, doctor, or admin)
    public string UploadedById { get; set; } = string.Empty;
    public ApplicationUser? UploadedBy { get; set; }

    // Access control
    public bool IsVisible { get; set; } = true;
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedByDoctorId { get; set; }
    public ApplicationUser? VerifiedByDoctor { get; set; }

    // Additional notes
    public string? DoctorNotes { get; set; }
}

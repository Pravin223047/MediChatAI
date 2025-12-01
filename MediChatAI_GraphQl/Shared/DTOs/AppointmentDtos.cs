using MediChatAI_GraphQl.Core.Entities;

namespace MediChatAI_GraphQl.Shared.DTOs;

// Week Schedule DTO
public class DoctorWeekScheduleDto
{
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public List<AppointmentDto> Appointments { get; set; } = new();
    public List<TimeBlockDto> TimeBlocks { get; set; } = new();
}

// Appointment DTOs
public class AppointmentDto
{
    public int Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string? PatientEmail { get; set; }
    public string? PatientPhone { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string? DoctorSpecialization { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public int DurationMinutes { get; set; }
    public AppointmentStatus Status { get; set; }
    public AppointmentType Type { get; set; }
    public string? RoomNumber { get; set; }
    public string? Location { get; set; }
    public bool IsVirtual { get; set; }
    public string? MeetingLink { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? DoctorNotes { get; set; }
    public string? PatientNotes { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAppointmentDto
{
    public string PatientId { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public DateTime AppointmentDateTime { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public AppointmentType Type { get; set; } = AppointmentType.General;
    public string? RoomNumber { get; set; }
    public bool IsVirtual { get; set; } = false;
    public string? ReasonForVisit { get; set; }
    public decimal ConsultationFee { get; set; } = 0;
}

public class UpdateAppointmentDto
{
    public int Id { get; set; }
    public DateTime? AppointmentDateTime { get; set; }
    public AppointmentStatus? Status { get; set; }
    public string? RoomNumber { get; set; }
    public string? DoctorNotes { get; set; }
    public string? PatientNotes { get; set; }
}

// Appointment Request DTOs
public class AppointmentRequestDto
{
    public int Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string? PreferredDoctorId { get; set; }
    public string? PreferredDoctorName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string SymptomDescription { get; set; } = string.Empty;
    public string? SymptomSeverity { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? InsuranceProvider { get; set; }
    public DateTime? PreferredDate { get; set; }
    public string? PreferredTimeSlot { get; set; }
    public bool IsUrgent { get; set; }
    public RequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public int? AppointmentId { get; set; }
}

public class CreateAppointmentRequestDto
{
    // Basic Info
    public string PatientId { get; set; } = string.Empty;
    public string? PreferredDoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternatePhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }

    // Medical History
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? CurrentMedications { get; set; }
    public string? PastMedicalConditions { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? PreviousSurgeries { get; set; }
    public string? FamilyMedicalHistory { get; set; }

    // Symptoms
    public string SymptomDescription { get; set; } = string.Empty;
    public string? SymptomDuration { get; set; }
    public string? SymptomSeverity { get; set; }
    public string? ReasonForVisit { get; set; }
    public AppointmentType PreferredAppointmentType { get; set; } = AppointmentType.General;

    // Multi-modal data
    public string? VoiceRecordingUrl { get; set; }
    public string? VoiceRecordingTranscript { get; set; }

    // Photo URLs array - captured during symptom description
    public List<string>? PhotoUrls { get; set; }

    // Video URLs array - captured during symptom description
    public List<string>? VideoUrls { get; set; }

    // Medical document URLs array - uploaded PDFs, images, etc.
    public List<string>? MedicalDocumentUrls { get; set; }

    // Insurance card URLs array - front, back, and additional insurance cards
    public List<string>? InsuranceCardUrls { get; set; }

    // Insurance
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceGroupNumber { get; set; }
    public string? InsuranceCardFrontUrl { get; set; }
    public string? InsuranceCardBackUrl { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public string? PaymentMethod { get; set; }

    // Preferences
    public DateTime? PreferredDate { get; set; }
    public string? PreferredTimeSlot { get; set; }
    public bool IsUrgent { get; set; } = false;
    public string? PatientAdditionalNotes { get; set; }
}

public class ReviewAppointmentRequestDto
{
    public int RequestId { get; set; }
    public string ReviewedByDoctorId { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public string? ReviewNotes { get; set; }
    public string? RejectionReason { get; set; }

    // If approved, provide appointment details
    public DateTime? AppointmentDateTime { get; set; }
    public string? RoomNumber { get; set; }
    public int? DurationMinutes { get; set; }
}

// Patient Document DTOs
public class PatientDocumentDto
{
    public int Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string? MimeType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedById { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public int? AppointmentRequestId { get; set; }
    public int? AppointmentId { get; set; }
}

public class UploadPatientDocumentDto
{
    public string PatientId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? PublicId { get; set; }
    public DocumentType DocumentType { get; set; }
    public string? MimeType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public string UploadedById { get; set; } = string.Empty;
    public int? AppointmentRequestId { get; set; }
    public int? AppointmentId { get; set; }
}

// Prescription DTOs
public class PrescriptionItemDto
{
    public int Id { get; set; }
    public int PrescriptionId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? Route { get; set; }
    public int DurationDays { get; set; } = 7;
    public int Quantity { get; set; } = 1;
    public string? Form { get; set; }
    public string? Instructions { get; set; }
    public string? Warnings { get; set; }
    public string? SideEffects { get; set; }
}

public class CreatePrescriptionItemDto
{
    public string MedicationName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string? Route { get; set; }
    public int DurationDays { get; set; } = 7;
    public int Quantity { get; set; } = 1;
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
    public string? DoctorSignature { get; set; }
    public bool IsVerified { get; set; }
}

public class CreatePrescriptionDto
{
    public string PatientId { get; set; } = string.Empty;
    public string? DoctorId { get; set; } // Set from authenticated user's claims, not required in input
    public int? AppointmentId { get; set; }
    public int? ConsultationSessionId { get; set; }
    public List<CreatePrescriptionItemDto> Items { get; set; } = new();
    public DateTime? StartDate { get; set; }
    public int RefillsAllowed { get; set; } = 0;
    public string? Diagnosis { get; set; }
    public string? DoctorNotes { get; set; }
    public string? DoctorSignature { get; set; }
}

// Time slot DTOs
public class DoctorAvailabilityDto
{
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public DateTime Date { get; set; }
    public List<TimeSlotDto> AvailableSlots { get; set; } = new();
}

public class TimeSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? RoomNumber { get; set; }
}

// Available Doctor DTOs for appointment booking
public class AvailableDoctorDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Specialization { get; set; }
    public string? ProfileImage { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? Department { get; set; }
    public decimal Rating { get; set; } = 0;
    public bool IsAvailable { get; set; } = true;
}

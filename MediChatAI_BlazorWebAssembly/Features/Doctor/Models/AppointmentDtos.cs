namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

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
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
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
    public string? AlternatePhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? BloodType { get; set; }
    public string? Allergies { get; set; }
    public string? CurrentMedications { get; set; }
    public string? PastMedicalConditions { get; set; }
    public string? ChronicDiseases { get; set; }
    public string? PreviousSurgeries { get; set; }
    public string? FamilyMedicalHistory { get; set; }
    public string SymptomDescription { get; set; } = string.Empty;
    public string? SymptomDuration { get; set; }
    public string? SymptomSeverity { get; set; }
    public string? ReasonForVisit { get; set; }
    public string? PreferredAppointmentType { get; set; }
    public string? VoiceRecordingUrl { get; set; }
    public string? VoiceRecordingTranscript { get; set; }
    public List<string>? PhotoUrls { get; set; }
    public List<string>? MedicalDocumentUrls { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public DateTime? PreferredDate { get; set; }
    public string? PreferredTimeSlot { get; set; }
    public bool IsUrgent { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int? AppointmentId { get; set; }
    public string? ReviewNotes { get; set; }
    public string? PatientAdditionalNotes { get; set; }
}

public class ReviewAppointmentRequestInput
{
    public int RequestId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewNotes { get; set; }
    public DateTime? AppointmentDateTime { get; set; }
    public string? RoomNumber { get; set; }
    public int? DurationMinutes { get; set; }
}

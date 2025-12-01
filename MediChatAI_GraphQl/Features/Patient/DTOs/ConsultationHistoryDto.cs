namespace MediChatAI_GraphQl.Features.Patient.DTOs;

public class ConsultationHistoryDto
{
    public int Id { get; set; }
    public string DoctorId { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string Specialization { get; set; } = "";
    public string DoctorProfileImage { get; set; } = "";
    public DateTime ConsultationDate { get; set; }
    public string ChiefComplaint { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string Observations { get; set; } = "";
    public List<string> TreatmentPlan { get; set; } = new();
    public List<ConsultationPrescriptionDto> Prescriptions { get; set; } = new();
    public string? FollowUpInstructions { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    public bool IsRated { get; set; }
    public int? Rating { get; set; } // 1-5 stars
    public string? PatientFeedback { get; set; }
    public string ConsultationType { get; set; } = ""; // In-Person, Virtual
}

public class ConsultationPrescriptionDto
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = "";
    public string Dosage { get; set; } = "";
    public string Frequency { get; set; } = "";
    public string Duration { get; set; } = "";
}

public class RateConsultationInput
{
    public int ConsultationId { get; set; }
    public int Rating { get; set; } // 1-5
    public string? Feedback { get; set; }
}

public class GetConsultationsInput
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DoctorId { get; set; }
    public int? Limit { get; set; } = 50;
    public int? Skip { get; set; } = 0;
}

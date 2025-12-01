namespace MediChatAI_GraphQl.Features.Patient.DTOs;

public class PatientDashboardDto
{
    public PatientOverviewStats OverviewStats { get; set; } = new();
    public List<UpcomingAppointmentSummary> UpcomingAppointments { get; set; } = new();
    public List<ActivePrescriptionSummary> ActivePrescriptions { get; set; } = new();
    public PatientVitalsSummary? LatestVitals { get; set; }
    public List<RecentDocumentSummary> RecentDocuments { get; set; } = new();
    public List<PatientNotificationSummary> RecentNotifications { get; set; } = new();
}

public class PatientOverviewStats
{
    public int TotalAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int ActivePrescriptions { get; set; }
    public int TotalDocuments { get; set; }
    public int UnreadMessages { get; set; }
    public int PendingRefills { get; set; }
}

public class UpcomingAppointmentSummary
{
    public int Id { get; set; }
    public string DoctorName { get; set; } = "";
    public string Specialization { get; set; } = "";
    public string DoctorProfileImage { get; set; } = "";
    public DateTime AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = "";
    public string AppointmentType { get; set; } = ""; // In-Person, Virtual
    public string Status { get; set; } = "";
    public bool IsVirtual { get; set; }
    public string? MeetingLink { get; set; }
    public string? ReasonForVisit { get; set; }
}

public class ActivePrescriptionSummary
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = "";
    public string Dosage { get; set; } = "";
    public string Frequency { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int RefillsRemaining { get; set; }
    public string DoctorName { get; set; } = "";
    public bool IsActive { get; set; }
    public string Instructions { get; set; } = "";
}

public class PatientVitalsSummary
{
    public string BloodPressure { get; set; } = "";
    public string HeartRate { get; set; } = "";
    public string Temperature { get; set; } = "";
    public string Weight { get; set; } = "";
    public string OxygenSaturation { get; set; } = "";
    public DateTime LastRecorded { get; set; }
}

public class RecentDocumentSummary
{
    public int Id { get; set; }
    public string DocumentName { get; set; } = "";
    public string DocumentType { get; set; } = "";
    public string DocumentCategory { get; set; } = "";
    public DateTime UploadedDate { get; set; }
    public string FileUrl { get; set; } = "";
    public long FileSizeBytes { get; set; }
}

public class PatientNotificationSummary
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Category { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}

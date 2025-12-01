namespace MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

// Response Models for GraphQL
public class PatientDashboardDataResponse
{
    public PatientDashboardData? PatientDashboardData { get; set; }
}

public class PatientDashboardData
{
    public OverviewStats OverviewStats { get; set; } = new();
    public List<UpcomingAppointment> UpcomingAppointments { get; set; } = new();
    public List<ActivePrescription> ActivePrescriptions { get; set; } = new();
    public List<RecentDocument> RecentDocuments { get; set; } = new();
    public List<RecentNotification> RecentNotifications { get; set; } = new();
    public LatestVitals? LatestVitals { get; set; }
}

public class OverviewStats
{
    public int TotalAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int ActivePrescriptions { get; set; }
    public int TotalDocuments { get; set; }
    public int UnreadMessages { get; set; }
    public int PendingRefills { get; set; }
}

public class UpcomingAppointment
{
    public int Id { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string DoctorProfileImage { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = string.Empty;
    public string AppointmentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
    public string? MeetingLink { get; set; }
    public string? ReasonForVisit { get; set; }
}

public class ActivePrescription
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int RefillsRemaining { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Instructions { get; set; }
}

public class RecentDocument
{
    public int Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentCategory { get; set; } = string.Empty;
    public DateTime UploadedDate { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}

public class RecentNotification
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}

public class LatestVitals
{
    public string BloodPressure { get; set; } = string.Empty;
    public string HeartRate { get; set; } = string.Empty;
    public string Temperature { get; set; } = string.Empty;
    public string Weight { get; set; } = string.Empty;
    public string OxygenSaturation { get; set; } = string.Empty;
    public DateTime LastRecorded { get; set; }
}

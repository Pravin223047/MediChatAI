namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

// Dashboard Data Models
public class DoctorDashboardData
{
    public DoctorOverviewStats? OverviewStats { get; set; }
    public List<TodayAppointment> TodayAppointments { get; set; } = new();
    public List<RecentActivity> RecentActivities { get; set; } = new();
    public List<PatientVital> CriticalVitals { get; set; } = new();
    public List<EmergencyAlert> ActiveAlerts { get; set; } = new();
    public int UnreadMessages { get; set; }
}

public class DoctorOverviewStats
{
    public int TodayAppointments { get; set; }
    public int ActivePatients { get; set; }
    public int UnreadMessages { get; set; }
    public int PendingReports { get; set; }
    public int CriticalPatients { get; set; }
    public int CompletedToday { get; set; }
    public decimal TodayRevenue { get; set; }
    public int EmergencyAlerts { get; set; }
}

public class TodayAppointment
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RoomNumber { get; set; }
}

public class RecentActivity
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Icon { get; set; }
}

// Patient Vitals Models
public class PatientVital
{
    public Guid Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string? RecordedByDoctorId { get; set; }
    public int VitalType { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public int Severity { get; set; }
    public DateTime RecordedAt { get; set; }
    public string? Notes { get; set; }
    public int? SystolicValue { get; set; }
    public int? DiastolicValue { get; set; }
    public bool IsAbnormal { get; set; }
    public string? PatientName { get; set; }
}

// Emergency Alert Models
public class EmergencyAlert
{
    public Guid Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Severity { get; set; }
    public int Status { get; set; }
    public int Category { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? PatientLocation { get; set; }
    public string? RecommendedAction { get; set; }
    public string? PatientName { get; set; }
}

// Analytics Models
public class DoctorAnalyticsData
{
    public PerformanceMetrics? PerformanceMetrics { get; set; }
    public List<PatientTrendData> PatientTrends { get; set; } = new();
    public AppointmentAnalytics? AppointmentAnalytics { get; set; }
    public RevenueAnalytics? RevenueAnalytics { get; set; }
}

public class PerformanceMetrics
{
    public decimal? AverageSatisfactionRating { get; set; }
    public int? AverageConsultationTime { get; set; }
    public int? AverageWaitingTime { get; set; }
    public int PrescriptionsWritten { get; set; }
    public int LabTestsOrdered { get; set; }
    public int TotalConsultations { get; set; }
    public decimal? SuccessRate { get; set; }
}

public class PatientTrendData
{
    public DateTime Date { get; set; }
    public int TotalPatients { get; set; }
    public int NewPatients { get; set; }
    public int FollowUpPatients { get; set; }
    public int CriticalPatients { get; set; }
}

public class AppointmentAnalytics
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int NoShowAppointments { get; set; }
    public decimal NoShowRate { get; set; }
    public List<AppointmentByHourData> AppointmentsByHour { get; set; } = new();
    public List<AppointmentByDayData> AppointmentsByDay { get; set; } = new();
}

public class AppointmentByHourData
{
    public int Hour { get; set; }
    public int Count { get; set; }
}

public class AppointmentByDayData
{
    public string Day { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RevenueAnalytics
{
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayments { get; set; }
    public decimal AverageRevenuePerConsultation { get; set; }
    public List<RevenueTrendData> RevenueTrends { get; set; } = new();
    public decimal GrowthPercentage { get; set; }
}

public class RevenueTrendData
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
}

// Doctor Preferences Model
public class DoctorPreference
{
    public Guid Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string? ThemePreset { get; set; }
    public string? ThemeMode { get; set; }
    public string? PrimaryColor { get; set; }
    public string? AccentColor { get; set; }
    public bool ShowPatientVitals { get; set; }
    public bool ShowAppointmentFeed { get; set; }
    public bool ShowChatWidget { get; set; }
    public bool ShowEmergencyAlerts { get; set; }
    public bool ShowAnalytics { get; set; }
    public string? DefaultDashboardView { get; set; }
    public bool EnableSoundNotifications { get; set; }
    public bool EnableDesktopNotifications { get; set; }
    public bool EnableAiInsights { get; set; }
}

// Input Models
public class UpdateDoctorPreferencesInput
{
    public string? ThemePreset { get; set; }
    public string? ThemeMode { get; set; }
    public string? PrimaryColor { get; set; }
    public string? AccentColor { get; set; }
    public bool? ShowPatientVitals { get; set; }
    public bool? ShowAppointmentFeed { get; set; }
    public bool? ShowChatWidget { get; set; }
    public bool? ShowEmergencyAlerts { get; set; }
    public bool? ShowAnalytics { get; set; }
    public string? DefaultDashboardView { get; set; }
    public bool? EnableSoundNotifications { get; set; }
    public bool? EnableDesktopNotifications { get; set; }
    public bool? EnableAiInsights { get; set; }
}

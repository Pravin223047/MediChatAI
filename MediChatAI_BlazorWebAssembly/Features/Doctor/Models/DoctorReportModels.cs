namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

// Main container for all reports data
public class DoctorReportsData
{
    public ReportOverviewStats? OverviewStats { get; set; }
    public PatientReportData? PatientReports { get; set; }
    public AppointmentReportData? AppointmentAnalytics { get; set; }
    public PrescriptionReportData? PrescriptionReports { get; set; }
    public RevenueReportData? RevenueReports { get; set; }
    public List<ScheduledReport> ScheduledReports { get; set; } = new();
    public List<ReportTemplate> SavedTemplates { get; set; } = new();
}

// Quick overview statistics
public class ReportOverviewStats
{
    public int TotalReportsGenerated { get; set; }
    public int ReportsThisMonth { get; set; }
    public int ScheduledReportsActive { get; set; }
    public int SavedTemplates { get; set; }
    public int ExportsThisMonth { get; set; }
    public double AvgReportGenerationTime { get; set; } // in seconds
}

// Patient Reports Data
public class PatientReportData
{
    public PatientMetrics? Metrics { get; set; }
    public PatientDemographics? Demographics { get; set; }
    public List<PatientVisitTrend> VisitTrends { get; set; } = new();
    public List<PatientByCondition> TopConditions { get; set; } = new();
    public List<PatientActivityData> RecentActivity { get; set; } = new();
}

public class PatientMetrics
{
    public int TotalActivePatients { get; set; }
    public int NewPatientsThisMonth { get; set; }
    public int PatientsSeenToday { get; set; }
    public int CriticalPatients { get; set; }
    public int FollowUpsDue { get; set; }
    public double AvgPatientsPerDay { get; set; }
    public int PercentageChange { get; set; } // compared to previous period
}

public class PatientDemographics
{
    public Dictionary<string, int> AgeDistribution { get; set; } = new();
    public Dictionary<string, int> GenderDistribution { get; set; } = new();
    public Dictionary<string, int> StatusDistribution { get; set; } = new();
    public Dictionary<string, int> LocationDistribution { get; set; } = new();
}

public class PatientVisitTrend
{
    public string Date { get; set; } = string.Empty;
    public int NewPatients { get; set; }
    public int ReturningPatients { get; set; }
    public int TotalVisits { get; set; }
}

public class PatientByCondition
{
    public string Condition { get; set; } = string.Empty;
    public int PatientCount { get; set; }
    public int Percentage { get; set; }
    public string Severity { get; set; } = string.Empty;
}

public class PatientActivityData
{
    public string PatientName { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public DateTime ActivityDate { get; set; }
    public string Details { get; set; } = string.Empty;
}

// Appointment Analytics Data
public class AppointmentReportData
{
    public AppointmentMetrics? Metrics { get; set; }
    public List<AppointmentTrendData> Trends { get; set; } = new();
    public Dictionary<string, int> StatusBreakdown { get; set; } = new();
    public Dictionary<string, int> TypeBreakdown { get; set; } = new();
    public List<AppointmentByHour> HourlyDistribution { get; set; } = new();
    public List<AppointmentByDayOfWeek> WeeklyPattern { get; set; } = new();
}

public class AppointmentMetrics
{
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int NoShowAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public double NoShowRate { get; set; }
    public double AvgDuration { get; set; } // in minutes
    public int PercentageChange { get; set; } // compared to previous period
}

public class AppointmentTrendData
{
    public string Date { get; set; } = string.Empty;
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
    public int Scheduled { get; set; }
}

public class AppointmentByHour
{
    public int Hour { get; set; }
    public int Count { get; set; }
    public double UtilizationRate { get; set; }
}

public class AppointmentByDayOfWeek
{
    public string DayOfWeek { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AveragePerWeek { get; set; }
}

// Prescription Reports Data
public class PrescriptionReportData
{
    public PrescriptionMetrics? Metrics { get; set; }
    public List<PrescriptionTrendData> Trends { get; set; } = new();
    public List<MedicationByCategory> MedicationBreakdown { get; set; } = new();
    public List<TopPrescribedMedication> TopMedications { get; set; } = new();
    public List<PrescriptionByCondition> ByCondition { get; set; } = new();
}

public class PrescriptionMetrics
{
    public int TotalPrescriptions { get; set; }
    public int PrescriptionsThisMonth { get; set; }
    public int UniqueMedications { get; set; }
    public int RefillRequests { get; set; }
    public int PendingApprovals { get; set; }
    public double AvgMedicationsPerPrescription { get; set; }
    public int PercentageChange { get; set; } // compared to previous period
}

public class PrescriptionTrendData
{
    public string Date { get; set; } = string.Empty;
    public int NewPrescriptions { get; set; }
    public int Refills { get; set; }
    public int Total { get; set; }
}

public class MedicationByCategory
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public int Percentage { get; set; }
    public List<string> TopMedications { get; set; } = new();
}

public class TopPrescribedMedication
{
    public string MedicationName { get; set; } = string.Empty;
    public int PrescriptionCount { get; set; }
    public string Category { get; set; } = string.Empty;
    public int PatientCount { get; set; }
}

public class PrescriptionByCondition
{
    public string Condition { get; set; } = string.Empty;
    public int PrescriptionCount { get; set; }
    public List<string> CommonMedications { get; set; } = new();
}

// Revenue/Financial Reports Data
public class RevenueReportData
{
    public RevenueMetrics? Metrics { get; set; }
    public List<RevenueReportTrendData> Trends { get; set; } = new();
    public Dictionary<string, decimal> RevenueByService { get; set; } = new();
    public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new();
    public List<RevenueByPeriod> MonthlyComparison { get; set; } = new();
}

public class RevenueMetrics
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal AverageDailyRevenue { get; set; }
    public decimal AverageRevenuePerAppointment { get; set; }
    public int TotalTransactions { get; set; }
    public decimal PendingPayments { get; set; }
    public int PercentageChange { get; set; } // compared to previous period
}

public class RevenueReportTrendData
{
    public string Date { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
}

public class RevenueByPeriod
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int Appointments { get; set; }
    public decimal AvgPerAppointment { get; set; }
}

// Scheduled Reports
public class ScheduledReport
{
    public string Id { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty; // Patient, Appointment, Prescription, Revenue
    public string Frequency { get; set; } = string.Empty; // Daily, Weekly, Monthly
    public string ExportFormat { get; set; } = string.Empty; // PDF, Excel, CSV
    public string EmailTo { get; set; } = string.Empty;
    public DateTime NextRunDate { get; set; }
    public DateTime? LastRunDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public Dictionary<string, object> Filters { get; set; } = new();
}

// Report Templates
public class ReportTemplate
{
    public string Id { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public List<string> SelectedMetrics { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUsedDate { get; set; }
    public int UsageCount { get; set; }
}

// Custom Report Builder
public class CustomReportConfiguration
{
    public string ReportName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public List<string> SelectedMetrics { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
    public string GroupBy { get; set; } = string.Empty;
    public bool IncludeCharts { get; set; } = true;
    public bool ComparisonEnabled { get; set; }
}

// Export Options
public class ReportExportOptions
{
    public string ReportType { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty; // PDF, Excel, CSV, PNG
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeRawData { get; set; } = true;
    public bool IncludeSummary { get; set; } = true;
    public string Orientation { get; set; } = "portrait"; // portrait, landscape
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}

// Filter Options
public class ReportFilterOptions
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool ComparisonMode { get; set; }
    public string? PatientStatus { get; set; }
    public string? Gender { get; set; }
    public int? AgeMin { get; set; }
    public int? AgeMax { get; set; }
    public string? AppointmentType { get; set; }
    public string? AppointmentStatus { get; set; }
    public string? MedicationCategory { get; set; }
    public string? PaymentMethod { get; set; }
}

// Comparison Data
public class ComparisonData<T>
{
    public T CurrentPeriod { get; set; } = default!;
    public T PreviousPeriod { get; set; } = default!;
    public double PercentageChange { get; set; }
    public string Trend { get; set; } = string.Empty; // up, down, stable
}

// API Response Models
public class ReportOverviewResponse
{
    public DoctorReportOverview? DoctorReportOverview { get; set; }
}

public class DoctorReportOverview
{
    public ReportOverviewStats? OverviewStats { get; set; }
}

public class PatientReportsResponse
{
    public DoctorPatientReports? DoctorPatientReports { get; set; }
}

public class DoctorPatientReports
{
    public PatientReportData? PatientReports { get; set; }
}

public class AppointmentAnalyticsResponse
{
    public DoctorAppointmentAnalytics? DoctorAppointmentAnalytics { get; set; }
}

public class DoctorAppointmentAnalytics
{
    public AppointmentReportData? AppointmentAnalytics { get; set; }
}

public class PrescriptionReportsResponse
{
    public DoctorPrescriptionReports? DoctorPrescriptionReports { get; set; }
}

public class DoctorPrescriptionReports
{
    public PrescriptionReportData? PrescriptionReports { get; set; }
}

public class RevenueReportsResponse
{
    public DoctorRevenueReports? DoctorRevenueReports { get; set; }
}

public class DoctorRevenueReports
{
    public RevenueReportData? RevenueReports { get; set; }
}

public class ScheduledReportsResponse
{
    public DoctorScheduledReports? DoctorScheduledReports { get; set; }
}

public class DoctorScheduledReports
{
    public List<ScheduledReport> ScheduledReports { get; set; } = new();
}

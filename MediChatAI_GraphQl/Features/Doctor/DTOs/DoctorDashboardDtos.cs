using MediChatAI_GraphQl.Core.Entities;

namespace MediChatAI_GraphQl.Features.Doctor.DTOs;

// Dashboard Data
public record DoctorDashboardData(
    DoctorOverviewStats OverviewStats,
    IEnumerable<TodayAppointment> TodayAppointments,
    IEnumerable<RecentActivity> RecentActivities,
    IEnumerable<PatientVital> CriticalVitals,
    IEnumerable<EmergencyAlert> ActiveAlerts,
    int UnreadMessages
);

public record DoctorOverviewStats(
    int TodayAppointments,
    int ActivePatients,
    int UnreadMessages,
    int PendingReports,
    int CriticalPatients,
    int CompletedToday,
    decimal TodayRevenue,
    int EmergencyAlerts
);

public record TodayAppointment(
    Guid Id,
    string PatientName,
    string PatientId,
    DateTime Time,
    string Type,
    string Status,
    string? RoomNumber
);

public record RecentActivity(
    Guid Id,
    string Type,
    string Description,
    DateTime Timestamp,
    string? Icon
);

// Analytics DTOs
public record DoctorAnalyticsData(
    PerformanceMetrics PerformanceMetrics,
    IEnumerable<PatientTrendData> PatientTrends,
    AppointmentAnalytics AppointmentAnalytics,
    RevenueAnalytics RevenueAnalytics
);

public record PatientTrendData(
    DateTime Date,
    int TotalPatients,
    int NewPatients,
    int FollowUpPatients,
    int CriticalPatients
);

public record PerformanceMetrics(
    decimal? AverageSatisfactionRating,
    int? AverageConsultationTime,
    int? AverageWaitingTime,
    int PrescriptionsWritten,
    int LabTestsOrdered,
    int TotalConsultations,
    decimal? SuccessRate
);

public record AppointmentAnalytics(
    int TotalAppointments,
    int CompletedAppointments,
    int CancelledAppointments,
    int NoShowAppointments,
    decimal NoShowRate,
    IEnumerable<AppointmentByHourData> AppointmentsByHour,
    IEnumerable<AppointmentByDayData> AppointmentsByDay
);

public record AppointmentByHourData(int Hour, int Count);
public record AppointmentByDayData(string Day, int Count);

public record RevenueAnalytics(
    decimal TotalRevenue,
    decimal PendingPayments,
    decimal AverageRevenuePerConsultation,
    IEnumerable<RevenueTrendData> RevenueTrends,
    decimal GrowthPercentage
);

public record RevenueTrendData(DateTime Date, decimal Revenue);

// Patient Vitals DTOs
public record RecordVitalsInput(
    string PatientId,
    string? RecordedByDoctorId,
    VitalType VitalType,
    string Value,
    string? Unit,
    int? SystolicValue,
    int? DiastolicValue,
    string? Notes
);

public record PatientVitalsData(
    string PatientId,
    string PatientName,
    int? HeartRate,
    string? BloodPressure,
    decimal? Temperature,
    int? OxygenSaturation,
    int? RespiratoryRate,
    int? BloodGlucose,
    DateTime? LastUpdated,
    bool HasAbnormalVitals
);

// Emergency Alert DTOs
public record CreateEmergencyAlertInput(
    string PatientId,
    string DoctorId,
    string Title,
    string Description,
    AlertSeverity Severity,
    AlertCategory Category,
    Guid? RelatedVitalId,
    string? PatientLocation,
    string? RecommendedAction
);

// Chat DTOs
public record SendMessageInput(
    string SenderId,
    string ReceiverId,
    string Content,
    MessageType MessageType,
    string? AttachmentUrl,
    string? AttachmentFileName,
    Guid? ReplyToMessageId,
    Guid? ConversationId
);

public record ConversationSummary(
    Guid Id,
    string PartnerId,
    string PartnerName,
    string PartnerRole,
    string? PartnerProfileImage,
    string LastMessage,
    DateTime LastMessageTime,
    int UnreadCount,
    bool IsOnline,
    Guid? ConversationId
);

// Doctor Preferences DTOs
public record UpdateDoctorPreferencesInput(
    string? ThemePreset,
    string? ThemeMode,
    string? PrimaryColor,
    string? AccentColor,
    bool? ShowPatientVitals,
    bool? ShowAppointmentFeed,
    bool? ShowChatWidget,
    bool? ShowEmergencyAlerts,
    bool? ShowAnalytics,
    string? DefaultDashboardView,
    bool? EnableSoundNotifications,
    bool? EnableDesktopNotifications,
    bool? EnableAiInsights,
    bool? EnableVoiceAssistant,
    string? WorkStartTime,
    string? WorkEndTime,
    int? DefaultConsultationDuration,
    string? TimeZone,
    string? PreferredLanguage
);

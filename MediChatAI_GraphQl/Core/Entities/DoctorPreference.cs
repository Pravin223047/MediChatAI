using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public class DoctorPreference
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string DoctorId { get; set; } = string.Empty;

    public ApplicationUser? Doctor { get; set; }

    // Theme Preferences
    [MaxLength(50)]
    public string? ThemePreset { get; set; } = "medical";

    [MaxLength(20)]
    public string? ThemeMode { get; set; } = "light"; // light, dark, system

    [MaxLength(20)]
    public string? PrimaryColor { get; set; }

    [MaxLength(20)]
    public string? AccentColor { get; set; }

    // Dashboard Preferences
    public bool ShowPatientVitals { get; set; } = true;

    public bool ShowAppointmentFeed { get; set; } = true;

    public bool ShowChatWidget { get; set; } = true;

    public bool ShowEmergencyAlerts { get; set; } = true;

    public bool ShowAnalytics { get; set; } = true;

    [MaxLength(50)]
    public string? DefaultDashboardView { get; set; } = "split-panel";

    // Notification Preferences
    public bool EnableSoundNotifications { get; set; } = true;

    public bool EnableDesktopNotifications { get; set; } = true;

    public bool EnableEmailAlerts { get; set; } = false;

    public bool EnableSmsAlerts { get; set; } = false;

    [MaxLength(20)]
    public string? NotificationFrequency { get; set; } = "realtime"; // realtime, batched, daily

    // Work Preferences
    [MaxLength(10)]
    public string? WorkStartTime { get; set; } = "09:00";

    [MaxLength(10)]
    public string? WorkEndTime { get; set; } = "17:00";

    public int? DefaultConsultationDuration { get; set; } = 30; // minutes

    public int? BreakDuration { get; set; } = 15; // minutes

    [MaxLength(100)]
    public string? WorkingDays { get; set; } = "Monday,Tuesday,Wednesday,Thursday,Friday";

    // Analytics Preferences
    [MaxLength(50)]
    public string? PreferredChartType { get; set; } = "line";

    [MaxLength(20)]
    public string? AnalyticsTimeRange { get; set; } = "week"; // day, week, month, year

    public bool ShowRevenueAnalytics { get; set; } = true;

    public bool ShowPerformanceMetrics { get; set; } = true;

    // AI Preferences
    public bool EnableAiInsights { get; set; } = true;

    public bool EnableVoiceAssistant { get; set; } = false;

    public bool EnableSmartScheduling { get; set; } = true;

    // Language and Region
    [MaxLength(10)]
    public string? PreferredLanguage { get; set; } = "en";

    [MaxLength(50)]
    public string? TimeZone { get; set; } = "UTC";

    [MaxLength(10)]
    public string? DateFormat { get; set; } = "MM/DD/YYYY";

    [MaxLength(10)]
    public string? TimeFormat { get; set; } = "12h"; // 12h or 24h

    // Additional Settings stored as JSON
    [MaxLength(5000)]
    public string? CustomSettings { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

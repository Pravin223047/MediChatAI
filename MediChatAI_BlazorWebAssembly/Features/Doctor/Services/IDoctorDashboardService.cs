using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public interface IDoctorDashboardService
{
    // Dashboard Data
    Task<DoctorDashboardData?> GetDashboardDataAsync();
    Task<DoctorOverviewStats?> GetOverviewStatsAsync();
    Task<List<TodayAppointment>> GetTodayAppointmentsAsync();
    Task<List<TodayAppointment>> GetPastAppointmentsAsync(int days = 7);
    Task<List<TodayAppointment>> GetUpcomingAppointmentsAsync(int days = 7);
    Task<List<RecentActivity>> GetRecentActivitiesAsync(int limit = 10);

    // Analytics
    Task<DoctorAnalyticsData?> GetAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Patient Vitals
    Task<List<PatientVital>> GetCriticalVitalsAsync();
    Task<List<PatientVital>> GetPatientVitalsAsync(string patientId, DateTime? startDate = null, DateTime? endDate = null);

    // Emergency Alerts
    Task<List<EmergencyAlert>> GetActiveAlertsAsync();
    Task<List<EmergencyAlert>> GetAlertsAsync(int? status = null);
    Task<bool> AcknowledgeAlertAsync(Guid alertId);
    Task<bool> ResolveAlertAsync(Guid alertId, string? resolutionNotes = null);
    Task<bool> DismissAlertAsync(Guid alertId);
    Task<int> GetActiveAlertCountAsync();

    // Doctor Preferences
    Task<DoctorPreference?> GetPreferencesAsync();
    Task<DoctorPreference?> UpdatePreferencesAsync(UpdateDoctorPreferencesInput input);

    // Unread Messages
    Task<int> GetUnreadMessageCountAsync();
}

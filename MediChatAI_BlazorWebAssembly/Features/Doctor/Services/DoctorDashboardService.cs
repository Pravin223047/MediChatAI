using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public class DoctorDashboardService : IDoctorDashboardService
{
    private readonly IGraphQLService _graphQLService;

    public DoctorDashboardService(IGraphQLService graphQLService)
    {
        _graphQLService = graphQLService;
    }

    public async Task<DoctorDashboardData?> GetDashboardDataAsync()
    {
        var query = @"
            query {
                doctorDashboardData {
                    overviewStats {
                        todayAppointments
                        activePatients
                        unreadMessages
                        pendingReports
                        criticalPatients
                        completedToday
                        todayRevenue
                        emergencyAlerts
                    }
                    todayAppointments {
                        id
                        patientName
                        patientId
                        time
                        type
                        status
                        roomNumber
                    }
                    recentActivities {
                        id
                        type
                        description
                        timestamp
                        icon
                    }
                    criticalVitals {
                        id
                        patientId
                        vitalType
                        value
                        unit
                        severity
                        recordedAt
                        notes
                        isAbnormal
                    }
                    activeAlerts {
                        id
                        patientId
                        doctorId
                        title
                        description
                        severity
                        status
                        category
                        createdAt
                        patientLocation
                        recommendedAction
                    }
                    unreadMessages
                }
            }";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null)
            {
                // Try direct access first
                if (response.ContainsKey("doctorDashboardData"))
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(response["doctorDashboardData"]);
                    return System.Text.Json.JsonSerializer.Deserialize<DoctorDashboardData>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                // Try wrapped access
                else if (response.ContainsKey("data"))
                {
                    var data = response["data"] as Dictionary<string, object>;
                    if (data != null && data.ContainsKey("doctorDashboardData"))
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(data["doctorDashboardData"]);
                        return System.Text.Json.JsonSerializer.Deserialize<DoctorDashboardData>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching dashboard data: {ex.Message}");
        }
        return null;
    }

    public async Task<DoctorOverviewStats?> GetOverviewStatsAsync()
    {
        var query = @"
            query {
                doctorOverviewStats {
                    todayAppointments
                    activePatients
                    unreadMessages
                    pendingReports
                    criticalPatients
                    completedToday
                    todayRevenue
                    emergencyAlerts
                }
            }";

        var response = await _graphQLService.SendQueryAsync<DoctorOverviewStatsResponse>(query);
        return response?.DoctorOverviewStats;
    }

    public async Task<List<TodayAppointment>> GetTodayAppointmentsAsync()
    {
        var query = @"
            query {
                doctorAppointments {
                    id
                    patientName
                    appointmentDateTime
                    type
                    status
                    roomNumber
                }
            }";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("doctorAppointments"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(response["doctorAppointments"]);
                var appointments = System.Text.Json.JsonSerializer.Deserialize<List<AppointmentDto>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AppointmentDto>();
                
                // Filter for today's appointments and convert to TodayAppointment
                var today = DateTime.Now.Date;
                return appointments
                    .Where(a => a.AppointmentDateTime.Date == today)
                    .Select(a => new TodayAppointment
                    {
                        Id = Guid.NewGuid(),
                        PatientName = a.PatientName,
                        PatientId = a.PatientId,
                        Time = a.AppointmentDateTime,
                        Type = a.Type,
                        Status = a.Status,
                        RoomNumber = a.RoomNumber
                    })
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching today appointments: {ex.Message}");
        }
        
        return new List<TodayAppointment>();
    }

    public async Task<List<RecentActivity>> GetRecentActivitiesAsync(int limit = 10)
    {
        var query = $@"
            query {{
                recentActivities(limit: {limit}) {{
                    id
                    type
                    description
                    timestamp
                    icon
                }}
            }}";

        var response = await _graphQLService.SendQueryAsync<RecentActivitiesResponse>(query);
        return response?.RecentActivities ?? new List<RecentActivity>();
    }

    public async Task<DoctorAnalyticsData?> GetAnalyticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var startDateStr = startDate.HasValue ? $"\"{startDate.Value:yyyy-MM-ddTHH:mm:ssZ}\"" : "null";
        var endDateStr = endDate.HasValue ? $"\"{endDate.Value:yyyy-MM-ddTHH:mm:ssZ}\"" : "null";

        var query = $@"
            query {{
                doctorAnalytics(startDate: {startDateStr}, endDate: {endDateStr}) {{
                    performanceMetrics {{
                        averageSatisfactionRating
                        averageConsultationTime
                        averageWaitingTime
                        prescriptionsWritten
                        labTestsOrdered
                        totalConsultations
                        successRate
                    }}
                    patientTrends {{
                        date
                        totalPatients
                        newPatients
                        followUpPatients
                        criticalPatients
                    }}
                    appointmentAnalytics {{
                        totalAppointments
                        completedAppointments
                        cancelledAppointments
                        noShowAppointments
                        noShowRate
                    }}
                    revenueAnalytics {{
                        totalRevenue
                        pendingPayments
                        averageRevenuePerConsultation
                        growthPercentage
                    }}
                }}
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null)
            {
                // Try direct access first
                if (response.ContainsKey("doctorAnalytics"))
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(response["doctorAnalytics"]);
                    return System.Text.Json.JsonSerializer.Deserialize<DoctorAnalyticsData>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                // Try wrapped access
                else if (response.ContainsKey("data"))
                {
                    var data = response["data"] as Dictionary<string, object>;
                    if (data != null && data.ContainsKey("doctorAnalytics"))
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(data["doctorAnalytics"]);
                        return System.Text.Json.JsonSerializer.Deserialize<DoctorAnalyticsData>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching analytics: {ex.Message}");
        }
        return null;
    }

    public async Task<List<PatientVital>> GetCriticalVitalsAsync()
    {
        var query = @"
            query {
                criticalVitals {
                    id
                    patientId
                    recordedByDoctorId
                    vitalType
                    value
                    unit
                    severity
                    recordedAt
                    notes
                    systolicValue
                    diastolicValue
                    isAbnormal
                }
            }";

        var response = await _graphQLService.SendQueryAsync<CriticalVitalsResponse>(query);
        return response?.CriticalVitals ?? new List<PatientVital>();
    }

    public async Task<List<PatientVital>> GetPatientVitalsAsync(string patientId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var startDateStr = startDate.HasValue ? $"\"{startDate.Value:yyyy-MM-ddTHH:mm:ssZ}\"" : "null";
        var endDateStr = endDate.HasValue ? $"\"{endDate.Value:yyyy-MM-ddTHH:mm:ssZ}\"" : "null";

        var query = $@"
            query {{
                patientVitals(patientId: ""{patientId}"", startDate: {startDateStr}, endDate: {endDateStr}) {{
                    id
                    patientId
                    vitalType
                    value
                    unit
                    severity
                    recordedAt
                    notes
                    isAbnormal
                }}
            }}";

        var response = await _graphQLService.SendQueryAsync<PatientVitalsResponse>(query);
        return response?.PatientVitals ?? new List<PatientVital>();
    }

    public async Task<List<EmergencyAlert>> GetActiveAlertsAsync()
    {
        var query = @"
            query {
                activeEmergencyAlerts {
                    id
                    patientId
                    doctorId
                    title
                    description
                    severity
                    status
                    category
                    createdAt
                    patientLocation
                    recommendedAction
                }
            }";

        var response = await _graphQLService.SendQueryAsync<ActiveEmergencyAlertsResponse>(query);
        return response?.ActiveEmergencyAlerts ?? new List<EmergencyAlert>();
    }

    public async Task<List<EmergencyAlert>> GetAlertsAsync(int? status = null)
    {
        var statusParam = status.HasValue ? $"status: {status.Value}" : "";

        var query = $@"
            query {{
                emergencyAlerts({statusParam}) {{
                    id
                    patientId
                    doctorId
                    title
                    description
                    severity
                    status
                    category
                    createdAt
                    patientLocation
                    recommendedAction
                }}
            }}";

        var response = await _graphQLService.SendQueryAsync<EmergencyAlertsResponse>(query);
        return response?.EmergencyAlerts ?? new List<EmergencyAlert>();
    }

    public async Task<bool> AcknowledgeAlertAsync(Guid alertId)
    {
        var mutation = $@"
            mutation {{
                acknowledgeEmergencyAlert(alertId: ""{alertId}"")
            }}";

        var response = await _graphQLService.SendQueryAsync<AcknowledgeEmergencyAlertResponse>(mutation);
        return response?.AcknowledgeEmergencyAlert ?? false;
    }

    public async Task<bool> ResolveAlertAsync(Guid alertId, string? resolutionNotes = null)
    {
        var notesParam = !string.IsNullOrEmpty(resolutionNotes) ? $", resolutionNotes: \"{resolutionNotes}\"" : "";

        var mutation = $@"
            mutation {{
                resolveEmergencyAlert(alertId: ""{alertId}""{notesParam})
            }}";

        var response = await _graphQLService.SendQueryAsync<ResolveEmergencyAlertResponse>(mutation);
        return response?.ResolveEmergencyAlert ?? false;
    }

    public async Task<bool> DismissAlertAsync(Guid alertId)
    {
        var mutation = $@"
            mutation {{
                dismissEmergencyAlert(alertId: ""{alertId}"")
            }}";

        var response = await _graphQLService.SendQueryAsync<DismissEmergencyAlertResponse>(mutation);
        return response?.DismissEmergencyAlert ?? false;
    }

    public async Task<int> GetActiveAlertCountAsync()
    {
        var query = @"
            query {
                activeAlertCount
            }";

        var response = await _graphQLService.SendQueryAsync<ActiveAlertCountResponse>(query);
        return response?.ActiveAlertCount ?? 0;
    }

    public async Task<DoctorPreference?> GetPreferencesAsync()
    {
        var query = @"
            query {
                doctorPreferences {
                    id
                    doctorId
                    themePreset
                    themeMode
                    primaryColor
                    accentColor
                    showPatientVitals
                    showAppointmentFeed
                    showChatWidget
                    showEmergencyAlerts
                    showAnalytics
                    defaultDashboardView
                    enableSoundNotifications
                    enableDesktopNotifications
                    enableAiInsights
                }
            }";

        var response = await _graphQLService.SendQueryAsync<DoctorPreferencesResponse>(query);
        return response?.DoctorPreferences;
    }

    public async Task<DoctorPreference?> UpdatePreferencesAsync(UpdateDoctorPreferencesInput input)
    {
        var mutation = $@"
            mutation {{
                updateDoctorPreferences(input: {{
                    themePreset: ""{input.ThemePreset}""
                    themeMode: ""{input.ThemeMode}""
                    primaryColor: ""{input.PrimaryColor}""
                    accentColor: ""{input.AccentColor}""
                    showPatientVitals: {input.ShowPatientVitals?.ToString().ToLower()}
                    showAppointmentFeed: {input.ShowAppointmentFeed?.ToString().ToLower()}
                    showChatWidget: {input.ShowChatWidget?.ToString().ToLower()}
                    showEmergencyAlerts: {input.ShowEmergencyAlerts?.ToString().ToLower()}
                    showAnalytics: {input.ShowAnalytics?.ToString().ToLower()}
                    defaultDashboardView: ""{input.DefaultDashboardView}""
                    enableSoundNotifications: {input.EnableSoundNotifications?.ToString().ToLower()}
                    enableDesktopNotifications: {input.EnableDesktopNotifications?.ToString().ToLower()}
                    enableAiInsights: {input.EnableAiInsights?.ToString().ToLower()}
                }}) {{
                    id
                    doctorId
                    themePreset
                    themeMode
                    primaryColor
                    accentColor
                }}
            }}";

        var response = await _graphQLService.SendQueryAsync<UpdateDoctorPreferencesResponse>(mutation);
        return response?.UpdateDoctorPreferences;
    }

    public async Task<int> GetUnreadMessageCountAsync()
    {
        var query = @"
            query {
                unreadMessageCount
            }";

        var response = await _graphQLService.SendQueryAsync<UnreadMessageCountResponse>(query);
        return response?.UnreadMessageCount ?? 0;
    }

    public async Task<List<TodayAppointment>> GetPastAppointmentsAsync(int days = 7)
    {
        var query = $@"
            query {{
                pastAppointments(days: {days}) {{
                    id
                    patientName
                    patientId
                    time
                    type
                    status
                    roomNumber
                }}
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("pastAppointments"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(response["pastAppointments"]);
                return System.Text.Json.JsonSerializer.Deserialize<List<TodayAppointment>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TodayAppointment>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching past appointments: {ex.Message}");
        }

        return new List<TodayAppointment>();
    }

    public async Task<List<TodayAppointment>> GetUpcomingAppointmentsAsync(int days = 7)
    {
        var query = $@"
            query {{
                upcomingAppointments(days: {days}) {{
                    id
                    patientName
                    patientId
                    time
                    type
                    status
                    roomNumber
                }}
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("upcomingAppointments"))
            {
                var json = System.Text.Json.JsonSerializer.Serialize(response["upcomingAppointments"]);
                return System.Text.Json.JsonSerializer.Deserialize<List<TodayAppointment>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TodayAppointment>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching upcoming appointments: {ex.Message}");
        }

        return new List<TodayAppointment>();
    }

    // Helper classes for GraphQL responses
    private class DoctorOverviewStatsResponse
    {
        public DoctorOverviewStats? DoctorOverviewStats { get; set; }
    }

    private class RecentActivitiesResponse
    {
        public List<RecentActivity>? RecentActivities { get; set; }
    }

    private class CriticalVitalsResponse
    {
        public List<PatientVital>? CriticalVitals { get; set; }
    }

    private class PatientVitalsResponse
    {
        public List<PatientVital>? PatientVitals { get; set; }
    }

    private class ActiveEmergencyAlertsResponse
    {
        public List<EmergencyAlert>? ActiveEmergencyAlerts { get; set; }
    }

    private class EmergencyAlertsResponse
    {
        public List<EmergencyAlert>? EmergencyAlerts { get; set; }
    }

    private class AcknowledgeEmergencyAlertResponse
    {
        public bool AcknowledgeEmergencyAlert { get; set; }
    }

    private class ResolveEmergencyAlertResponse
    {
        public bool ResolveEmergencyAlert { get; set; }
    }

    private class DismissEmergencyAlertResponse
    {
        public bool DismissEmergencyAlert { get; set; }
    }

    private class ActiveAlertCountResponse
    {
        public int ActiveAlertCount { get; set; }
    }

    private class DoctorPreferencesResponse
    {
        public DoctorPreference? DoctorPreferences { get; set; }
    }

    private class UpdateDoctorPreferencesResponse
    {
        public DoctorPreference? UpdateDoctorPreferences { get; set; }
    }

    private class UnreadMessageCountResponse
    {
        public int UnreadMessageCount { get; set; }
    }
}

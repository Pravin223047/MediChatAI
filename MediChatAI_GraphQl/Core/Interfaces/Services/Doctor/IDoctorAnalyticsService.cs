using MediChatAI_GraphQl.Features.Doctor.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;

public interface IDoctorAnalyticsService
{
    Task<DoctorAnalyticsData> GetAnalyticsAsync(string doctorId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<PatientTrendData>> GetPatientTrendsAsync(string doctorId, string timeRange = "week");
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(string doctorId);
    Task<AppointmentAnalytics> GetAppointmentAnalyticsAsync(string doctorId);
    Task<RevenueAnalytics> GetRevenueAnalyticsAsync(string doctorId, string period = "month");
    Task UpdateDailyAnalyticsAsync(string doctorId);
}

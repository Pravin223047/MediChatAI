using MediChatAI_GraphQl.Features.Doctor.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;

public interface IDoctorDashboardService
{
    Task<DoctorDashboardData> GetDashboardDataAsync(string doctorId);
    Task<DoctorOverviewStats> GetOverviewStatsAsync(string doctorId);
    Task<IEnumerable<TodayAppointment>> GetTodayAppointmentsAsync(string doctorId);
    Task<IEnumerable<TodayAppointment>> GetPastAppointmentsAsync(string doctorId, int days = 7);
    Task<IEnumerable<TodayAppointment>> GetUpcomingAppointmentsAsync(string doctorId, int days = 7);
    Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(string doctorId, int limit = 10);
}

using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Features.Doctor.Services;

public class DoctorAnalyticsService : IDoctorAnalyticsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DoctorAnalyticsService> _logger;

    public DoctorAnalyticsService(ApplicationDbContext context, ILogger<DoctorAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DoctorAnalyticsData> GetAnalyticsAsync(string doctorId, DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var performanceMetrics = await GetPerformanceMetricsAsync(doctorId);
        var patientTrends = await GetPatientTrendsAsync(doctorId, "month");
        var appointmentAnalytics = await GetAppointmentAnalyticsAsync(doctorId);
        var revenueAnalytics = await GetRevenueAnalyticsAsync(doctorId, "month");

        return new DoctorAnalyticsData(performanceMetrics, patientTrends, appointmentAnalytics, revenueAnalytics);
    }

    public async Task<IEnumerable<PatientTrendData>> GetPatientTrendsAsync(string doctorId, string timeRange = "week")
    {
        var days = timeRange switch
        {
            "day" => 1,
            "week" => 7,
            "month" => 30,
            "year" => 365,
            _ => 7
        };

        var startDate = DateTime.UtcNow.Date.AddDays(-days);
        var analytics = await _context.DoctorAnalytics
            .Where(a => a.DoctorId == doctorId && a.Date >= startDate)
            .OrderBy(a => a.Date)
            .ToListAsync();

        return analytics.Select(a => new PatientTrendData(
            a.Date,
            a.TotalPatients,
            a.NewPatients,
            a.FollowUpPatients,
            a.CriticalPatients
        ));
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(string doctorId)
    {
        var last30Days = DateTime.UtcNow.AddDays(-30);
        var analytics = await _context.DoctorAnalytics
            .Where(a => a.DoctorId == doctorId && a.Date >= last30Days)
            .ToListAsync();

        if (!analytics.Any())
        {
            return new PerformanceMetrics(null, null, null, 0, 0, 0, null);
        }

        var avgSatisfaction = analytics.Where(a => a.AverageSatisfactionRating.HasValue)
            .Average(a => a.AverageSatisfactionRating);
        var avgConsultationTime = analytics.Where(a => a.AverageConsultationTime.HasValue)
            .Average(a => a.AverageConsultationTime);
        var avgWaitingTime = analytics.Where(a => a.AverageWaitingTime.HasValue)
            .Average(a => a.AverageWaitingTime);
        var totalPrescriptions = analytics.Sum(a => a.PrescriptionsWritten);
        var totalLabTests = analytics.Sum(a => a.LabTestsOrdered);
        var totalConsultations = analytics.Sum(a => a.CompletedAppointments);

        return new PerformanceMetrics(
            avgSatisfaction,
            avgConsultationTime.HasValue ? (int)avgConsultationTime.Value : null,
            avgWaitingTime.HasValue ? (int)avgWaitingTime.Value : null,
            totalPrescriptions,
            totalLabTests,
            totalConsultations,
            null
        );
    }

    public async Task<AppointmentAnalytics> GetAppointmentAnalyticsAsync(string doctorId)
    {
        var last30Days = DateTime.UtcNow.AddDays(-30);
        var analytics = await _context.DoctorAnalytics
            .Where(a => a.DoctorId == doctorId && a.Date >= last30Days)
            .ToListAsync();

        var total = analytics.Sum(a => a.TotalAppointments);
        var completed = analytics.Sum(a => a.CompletedAppointments);
        var cancelled = analytics.Sum(a => a.CancelledAppointments);
        var noShow = analytics.Sum(a => a.NoShowAppointments);
        var noShowRate = total > 0 ? (decimal)noShow / total * 100 : 0;

        // Placeholder data for hourly and daily distribution
        var byHour = Enumerable.Range(9, 9).Select(h => new AppointmentByHourData(h, Random.Shared.Next(1, 10)));
        var byDay = new[] { "Mon", "Tue", "Wed", "Thu", "Fri" }.Select(d => new AppointmentByDayData(d, Random.Shared.Next(5, 15)));

        return new AppointmentAnalytics(total, completed, cancelled, noShow, noShowRate, byHour, byDay);
    }

    public async Task<RevenueAnalytics> GetRevenueAnalyticsAsync(string doctorId, string period = "month")
    {
        var days = period switch
        {
            "week" => 7,
            "month" => 30,
            "year" => 365,
            _ => 30
        };

        var startDate = DateTime.UtcNow.Date.AddDays(-days);
        var analytics = await _context.DoctorAnalytics
            .Where(a => a.DoctorId == doctorId && a.Date >= startDate)
            .OrderBy(a => a.Date)
            .ToListAsync();

        var totalRevenue = analytics.Sum(a => a.TotalRevenue);
        var pendingPayments = analytics.Sum(a => a.PendingPayments);
        var avgRevenuePerConsultation = analytics.Where(a => a.AverageRevenuePerConsultation.HasValue)
            .Average(a => a.AverageRevenuePerConsultation) ?? 0;

        var trends = analytics.Select(a => new RevenueTrendData(a.Date, a.TotalRevenue));

        return new RevenueAnalytics(totalRevenue, pendingPayments, avgRevenuePerConsultation, trends, 0);
    }

    public async Task UpdateDailyAnalyticsAsync(string doctorId)
    {
        var today = DateTime.UtcNow.Date;
        var existing = await _context.DoctorAnalytics
            .FirstOrDefaultAsync(a => a.DoctorId == doctorId && a.Date == today);

        if (existing == null)
        {
            existing = new DoctorAnalytics
            {
                DoctorId = doctorId,
                Date = today,
                Year = today.Year,
                MonthNumber = today.Month,
                WeekNumber = GetWeekNumber(today)
            };
            _context.DoctorAnalytics.Add(existing);
        }

        // Update metrics (placeholder - would calculate from real data)
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static int GetWeekNumber(DateTime date)
    {
        var day = (int)System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(date);
        return System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
            date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}

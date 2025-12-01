using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Features.Doctor.Services;

public class DoctorDashboardService : IDoctorDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IPatientVitalsService _vitalsService;
    private readonly IEmergencyAlertService _alertService;
    private readonly IDoctorChatService _chatService;
    private readonly ILogger<DoctorDashboardService> _logger;

    public DoctorDashboardService(
        ApplicationDbContext context,
        IPatientVitalsService vitalsService,
        IEmergencyAlertService alertService,
        IDoctorChatService chatService,
        ILogger<DoctorDashboardService> logger)
    {
        _context = context;
        _vitalsService = vitalsService;
        _alertService = alertService;
        _chatService = chatService;
        _logger = logger;
    }

    public async Task<DoctorDashboardData> GetDashboardDataAsync(string doctorId)
    {
        try
        {
            var overviewStats = await GetOverviewStatsAsync(doctorId);
            var todayAppointments = await GetTodayAppointmentsAsync(doctorId);
            var recentActivities = await GetRecentActivitiesAsync(doctorId, 10);
            var criticalVitals = await _vitalsService.GetCriticalVitalsForDoctorAsync(doctorId);
            var activeAlerts = await _alertService.GetAlertsForDoctorAsync(doctorId, AlertStatus.Active);
            var unreadMessages = await _chatService.GetUnreadCountAsync(doctorId);

            return new DoctorDashboardData(
                overviewStats,
                todayAppointments,
                recentActivities,
                criticalVitals,
                activeAlerts,
                unreadMessages
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data for doctor {DoctorId}", doctorId);
            throw;
        }
    }

    public async Task<DoctorOverviewStats> GetOverviewStatsAsync(string doctorId)
    {
        _logger.LogInformation("GetOverviewStatsAsync called for doctor {DoctorId}", doctorId);

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        _logger.LogInformation("Today: {Today}, Tomorrow: {Tomorrow}", today, tomorrow);

        // Get today's appointments
        var todayAppointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                   a.AppointmentDateTime >= today &&
                   a.AppointmentDateTime < tomorrow &&
                   a.Status != AppointmentStatus.Cancelled)
            .CountAsync();

        _logger.LogInformation("Today's appointments count: {Count}", todayAppointments);

        // Get active patients (patients with recent appointments or vitals in last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var activePatients = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                   a.AppointmentDateTime >= thirtyDaysAgo &&
                   a.Status == AppointmentStatus.Completed)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync();

        _logger.LogInformation("Active patients count: {Count} (from {Date} to now)", activePatients, thirtyDaysAgo);

        var unreadMessages = await _chatService.GetUnreadCountAsync(doctorId);

        // Get pending reports (completed appointments without doctor notes in last 7 days)
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var pendingReports = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                   a.AppointmentDateTime >= sevenDaysAgo &&
                   a.Status == AppointmentStatus.Completed &&
                   string.IsNullOrEmpty(a.DoctorNotes))
            .CountAsync();

        var criticalPatients = await _context.PatientVitals
            .Where(v => v.RecordedByDoctorId == doctorId &&
                   v.Severity == VitalSeverity.Critical &&
                   v.RecordedAt >= today)
            .Select(v => v.PatientId)
            .Distinct()
            .CountAsync();

        var completedToday = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                   a.AppointmentDateTime >= today &&
                   a.AppointmentDateTime < tomorrow &&
                   a.Status == AppointmentStatus.Completed)
            .CountAsync();

        // Calculate today's revenue (completed appointments today)
        var todayRevenue = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                   a.AppointmentDateTime >= today &&
                   a.AppointmentDateTime < tomorrow &&
                   a.Status == AppointmentStatus.Completed)
            .SumAsync(a => (decimal?)a.ConsultationFee) ?? 0m;

        var emergencyAlerts = await _alertService.GetActiveAlertCountAsync(doctorId);

        return new DoctorOverviewStats(
            todayAppointments,
            activePatients,
            unreadMessages,
            pendingReports,
            criticalPatients,
            completedToday,
            todayRevenue,
            emergencyAlerts
        );
    }

    public async Task<IEnumerable<TodayAppointment>> GetTodayAppointmentsAsync(string doctorId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var appointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                   a.AppointmentDateTime >= today &&
                   a.AppointmentDateTime < tomorrow &&
                   a.Status != AppointmentStatus.Cancelled)
            .Include(a => a.Patient)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();

        return appointments.Select(a => new TodayAppointment(
            Guid.NewGuid(),
            $"{a.Patient.FirstName} {a.Patient.LastName}",
            a.PatientId,
            a.AppointmentDateTime,
            a.Type.ToString(),
            a.Status.ToString(),
            a.RoomNumber ?? "N/A"
        ));
    }

    public async Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(string doctorId, int limit = 10)
    {
        var activities = new List<RecentActivity>();
        var lookbackDate = DateTime.UtcNow.AddDays(-7); // Look back 7 days for activities

        // Get recent appointment completions/creations
        var recentAppointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.CreatedAt >= lookbackDate)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Include(a => a.Patient)
            .ToListAsync();

        foreach (var appointment in recentAppointments)
        {
            var activityType = appointment.Status == AppointmentStatus.Completed ? "Appointment Completed" : "Appointment Scheduled";
            var icon = appointment.Status == AppointmentStatus.Completed ? "fa-check-circle" : "fa-calendar-plus";
            activities.Add(new RecentActivity(
                Guid.NewGuid(),
                activityType,
                $"{activityType.Replace("Appointment ", "")} with {appointment.Patient?.FirstName} {appointment.Patient?.LastName}",
                appointment.CreatedAt,
                icon
            ));
        }

        // Get recent prescriptions created
        var recentPrescriptions = await _context.Prescriptions
            .Where(p => p.DoctorId == doctorId && p.CreatedAt >= lookbackDate)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .Include(p => p.Patient)
            .ToListAsync();

        foreach (var prescription in recentPrescriptions)
        {
            activities.Add(new RecentActivity(
                Guid.NewGuid(),
                "Prescription",
                $"Prescribed medications for {prescription.Patient?.FirstName} {prescription.Patient?.LastName}",
                prescription.CreatedAt,
                "fa-prescription"
            ));
        }

        // Get recent prescription refills
        var recentRefills = await _context.Prescriptions
            .Where(p => p.DoctorId == doctorId &&
                   p.LastRefillDate.HasValue &&
                   p.LastRefillDate >= lookbackDate)
            .OrderByDescending(p => p.LastRefillDate)
            .Take(limit)
            .Include(p => p.Patient)
            .ToListAsync();

        foreach (var refill in recentRefills)
        {
            activities.Add(new RecentActivity(
                Guid.NewGuid(),
                "Refill",
                $"Prescription refilled for {refill.Patient?.FirstName} {refill.Patient?.LastName}",
                refill.LastRefillDate!.Value,
                "fa-redo"
            ));
        }

        // Get recent consultation notes (appointments with doctor notes)
        var recentConsultations = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                   !string.IsNullOrEmpty(a.DoctorNotes) &&
                   a.UpdatedAt >= lookbackDate &&
                   a.Status == AppointmentStatus.Completed)
            .OrderByDescending(a => a.UpdatedAt)
            .Take(limit)
            .Include(a => a.Patient)
            .ToListAsync();

        foreach (var consultation in recentConsultations)
        {
            activities.Add(new RecentActivity(
                Guid.NewGuid(),
                "Consultation Notes",
                $"Added notes for {consultation.Patient?.FirstName} {consultation.Patient?.LastName}",
                consultation.UpdatedAt ?? consultation.CreatedAt,
                "fa-notes-medical"
            ));
        }

        // Get recent vitals recordings
        var recentVitals = await _context.PatientVitals
            .Where(v => v.RecordedByDoctorId == doctorId && v.RecordedAt >= lookbackDate)
            .OrderByDescending(v => v.RecordedAt)
            .Take(limit)
            .Include(v => v.Patient)
            .ToListAsync();

        foreach (var vital in recentVitals)
        {
            activities.Add(new RecentActivity(
                vital.Id,
                "Vitals",
                $"Recorded {vital.VitalType} for {vital.Patient?.FirstName} {vital.Patient?.LastName}",
                vital.RecordedAt,
                "fa-heartbeat"
            ));
        }

        // Get recent alerts
        var recentAlerts = await _context.EmergencyAlerts
            .Where(a => a.DoctorId == doctorId && a.CreatedAt >= lookbackDate)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Include(a => a.Patient)
            .ToListAsync();

        foreach (var alert in recentAlerts)
        {
            activities.Add(new RecentActivity(
                alert.Id,
                "Alert",
                $"{alert.Severity} alert for {alert.Patient?.FirstName} {alert.Patient?.LastName}",
                alert.CreatedAt,
                "fa-exclamation-triangle"
            ));
        }

        return activities.OrderByDescending(a => a.Timestamp).Take(limit);
    }

    public async Task<IEnumerable<TodayAppointment>> GetPastAppointmentsAsync(string doctorId, int days = 7)
    {
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-days);

        var appointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                   a.AppointmentDateTime >= startDate &&
                   a.AppointmentDateTime < today &&
                   a.Status != AppointmentStatus.Cancelled)
            .Include(a => a.Patient)
            .OrderByDescending(a => a.AppointmentDateTime)
            .ToListAsync();

        return appointments.Select(a => new TodayAppointment(
            Guid.NewGuid(),
            $"{a.Patient.FirstName} {a.Patient.LastName}",
            a.PatientId,
            a.AppointmentDateTime,
            a.Type.ToString(),
            a.Status.ToString(),
            a.RoomNumber ?? "N/A"
        ));
    }

    public async Task<IEnumerable<TodayAppointment>> GetUpcomingAppointmentsAsync(string doctorId, int days = 7)
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var endDate = tomorrow.AddDays(days);

        var appointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                   a.AppointmentDateTime >= tomorrow &&
                   a.AppointmentDateTime < endDate &&
                   a.Status != AppointmentStatus.Cancelled)
            .Include(a => a.Patient)
            .OrderBy(a => a.AppointmentDateTime)
            .ToListAsync();

        return appointments.Select(a => new TodayAppointment(
            Guid.NewGuid(),
            $"{a.Patient.FirstName} {a.Patient.LastName}",
            a.PatientId,
            a.AppointmentDateTime,
            a.Type.ToString(),
            a.Status.ToString(),
            a.RoomNumber ?? "N/A"
        ));
    }
}

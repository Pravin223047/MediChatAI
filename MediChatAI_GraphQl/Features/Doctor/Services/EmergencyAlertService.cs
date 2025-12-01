using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Features.Doctor.Services;

public class EmergencyAlertService : IEmergencyAlertService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmergencyAlertService> _logger;

    public EmergencyAlertService(ApplicationDbContext context, ILogger<EmergencyAlertService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EmergencyAlert> CreateAlertAsync(CreateEmergencyAlertInput input)
    {
        var alert = new EmergencyAlert
        {
            PatientId = input.PatientId,
            DoctorId = input.DoctorId,
            Title = input.Title,
            Description = input.Description,
            Severity = input.Severity,
            Category = input.Category,
            RelatedVitalId = input.RelatedVitalId,
            PatientLocation = input.PatientLocation,
            RecommendedAction = input.RecommendedAction,
            Status = AlertStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        // Set expiration for non-critical alerts
        if (alert.Severity != AlertSeverity.Critical)
        {
            alert.ExpiresAt = DateTime.UtcNow.AddHours(24);
        }

        _context.EmergencyAlerts.Add(alert);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Emergency alert created: {AlertId} for patient {PatientId}", alert.Id, input.PatientId);

        return alert;
    }

    public async Task<IEnumerable<EmergencyAlert>> GetAlertsForDoctorAsync(string doctorId, AlertStatus? status = null)
    {
        var query = _context.EmergencyAlerts
            .Where(a => a.DoctorId == doctorId)
            .Include(a => a.Patient)
            .Include(a => a.RelatedVital)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> AcknowledgeAlertAsync(Guid alertId, string userId)
    {
        var alert = await _context.EmergencyAlerts.FindAsync(alertId);
        if (alert == null) return false;

        alert.Status = AlertStatus.Acknowledged;
        alert.AcknowledgedAt = DateTime.UtcNow;
        alert.AcknowledgedByUserId = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Alert {AlertId} acknowledged by user {UserId}", alertId, userId);

        return true;
    }

    public async Task<bool> ResolveAlertAsync(Guid alertId, string userId, string? resolutionNotes = null)
    {
        var alert = await _context.EmergencyAlerts.FindAsync(alertId);
        if (alert == null) return false;

        alert.Status = AlertStatus.Resolved;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedByUserId = userId;
        alert.ResolutionNotes = resolutionNotes;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Alert {AlertId} resolved by user {UserId}", alertId, userId);

        return true;
    }

    public async Task<bool> DismissAlertAsync(Guid alertId, string userId)
    {
        var alert = await _context.EmergencyAlerts.FindAsync(alertId);
        if (alert == null) return false;

        alert.Status = AlertStatus.Dismissed;
        alert.ResolvedAt = DateTime.UtcNow;
        alert.ResolvedByUserId = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Alert {AlertId} dismissed by user {UserId}", alertId, userId);

        return true;
    }

    public async Task<int> GetActiveAlertCountAsync(string doctorId)
    {
        return await _context.EmergencyAlerts
            .CountAsync(a => a.DoctorId == doctorId && a.Status == AlertStatus.Active);
    }
}

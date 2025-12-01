using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Features.Doctor.Services;

public class PatientVitalsService : IPatientVitalsService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PatientVitalsService> _logger;

    public PatientVitalsService(ApplicationDbContext context, ILogger<PatientVitalsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PatientVital> RecordVitalsAsync(RecordVitalsInput input)
    {
        var vital = new PatientVital
        {
            PatientId = input.PatientId,
            RecordedByDoctorId = input.RecordedByDoctorId,
            VitalType = input.VitalType,
            Value = input.Value,
            Unit = input.Unit,
            SystolicValue = input.SystolicValue,
            DiastolicValue = input.DiastolicValue,
            Notes = input.Notes,
            RecordedAt = DateTime.UtcNow
        };

        // Determine severity based on vital type and value
        vital.Severity = DetermineSeverity(vital);
        vital.IsAbnormal = vital.Severity != VitalSeverity.Normal;

        _context.PatientVitals.Add(vital);
        await _context.SaveChangesAsync();

        // Check if alert should be created for abnormal vitals
        if (vital.IsAbnormal)
        {
            await CheckAndCreateAlertsAsync(vital.Id);
        }

        return vital;
    }

    public async Task<IEnumerable<PatientVital>> GetPatientVitalsAsync(string patientId, DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        return await _context.PatientVitals
            .Where(v => v.PatientId == patientId && v.RecordedAt >= startDate && v.RecordedAt <= endDate)
            .OrderByDescending(v => v.RecordedAt)
            .ToListAsync();
    }

    public async Task<PatientVitalsData> GetLatestVitalsAsync(string patientId)
    {
        var patient = await _context.Users.FindAsync(patientId);
        if (patient == null) return null!;

        var latestVitals = await _context.PatientVitals
            .Where(v => v.PatientId == patientId)
            .GroupBy(v => v.VitalType)
            .Select(g => g.OrderByDescending(v => v.RecordedAt).FirstOrDefault())
            .ToListAsync();

        var heartRate = latestVitals.FirstOrDefault(v => v?.VitalType == VitalType.HeartRate);
        var bp = latestVitals.FirstOrDefault(v => v?.VitalType == VitalType.BloodPressure);
        var temp = latestVitals.FirstOrDefault(v => v?.VitalType == VitalType.Temperature);
        var o2 = latestVitals.FirstOrDefault(v => v?.VitalType == VitalType.OxygenSaturation);
        var rr = latestVitals.FirstOrDefault(v => v?.VitalType == VitalType.RespiratoryRate);
        var glucose = latestVitals.FirstOrDefault(v => v?.VitalType == VitalType.BloodGlucose);

        return new PatientVitalsData(
            patientId,
            $"{patient.FirstName} {patient.LastName}",
            heartRate != null ? int.Parse(heartRate.Value) : null,
            bp?.Value,
            temp != null ? decimal.Parse(temp.Value) : null,
            o2 != null ? int.Parse(o2.Value) : null,
            rr != null ? int.Parse(rr.Value) : null,
            glucose != null ? int.Parse(glucose.Value) : null,
            latestVitals.Max(v => v?.RecordedAt),
            latestVitals.Any(v => v?.IsAbnormal == true)
        );
    }

    public async Task<IEnumerable<PatientVital>> GetCriticalVitalsForDoctorAsync(string doctorId)
    {
        var last24Hours = DateTime.UtcNow.AddHours(-24);
        return await _context.PatientVitals
            .Where(v => v.RecordedByDoctorId == doctorId &&
                       v.Severity == VitalSeverity.Critical &&
                       v.RecordedAt >= last24Hours)
            .Include(v => v.Patient)
            .OrderByDescending(v => v.RecordedAt)
            .Take(10)
            .ToListAsync();
    }

    public async Task CheckAndCreateAlertsAsync(Guid vitalId)
    {
        var vital = await _context.PatientVitals
            .Include(v => v.Patient)
            .FirstOrDefaultAsync(v => v.Id == vitalId);

        if (vital == null || vital.Severity == VitalSeverity.Normal) return;

        // Check if alert already exists for this vital
        var existingAlert = await _context.EmergencyAlerts
            .AnyAsync(a => a.RelatedVitalId == vitalId);

        if (existingAlert) return;

        // Create alert for critical or warning vitals
        var alert = new EmergencyAlert
        {
            PatientId = vital.PatientId,
            DoctorId = vital.RecordedByDoctorId ?? "",
            Title = $"Abnormal {vital.VitalType} Detected",
            Description = $"{vital.VitalType}: {vital.Value} {vital.Unit}. {vital.Notes}",
            Severity = vital.Severity == VitalSeverity.Critical ? AlertSeverity.Critical : AlertSeverity.High,
            Category = AlertCategory.VitalSignAbnormal,
            RelatedVitalId = vital.Id,
            RecommendedAction = GetRecommendedAction(vital)
        };

        _context.EmergencyAlerts.Add(alert);
        vital.IsAlertSent = true;
        await _context.SaveChangesAsync();
    }

    private VitalSeverity DetermineSeverity(PatientVital vital)
    {
        return vital.VitalType switch
        {
            VitalType.HeartRate => DetermineHeartRateSeverity(int.TryParse(vital.Value, out var hr) ? hr : 0),
            VitalType.BloodPressure => DetermineBloodPressureSeverity(vital.SystolicValue ?? 0, vital.DiastolicValue ?? 0),
            VitalType.OxygenSaturation => DetermineOxygenSaturationSeverity(int.TryParse(vital.Value, out var o2) ? o2 : 0),
            VitalType.Temperature => DetermineTemperatureSeverity(decimal.TryParse(vital.Value, out var temp) ? temp : 0),
            _ => VitalSeverity.Normal
        };
    }

    private VitalSeverity DetermineHeartRateSeverity(int hr)
    {
        if (hr < 40 || hr > 150) return VitalSeverity.Critical;
        if (hr < 50 || hr > 120) return VitalSeverity.Warning;
        return VitalSeverity.Normal;
    }

    private VitalSeverity DetermineBloodPressureSeverity(int systolic, int diastolic)
    {
        if (systolic >= 180 || diastolic >= 120) return VitalSeverity.Critical;
        if (systolic >= 140 || diastolic >= 90) return VitalSeverity.Warning;
        return VitalSeverity.Normal;
    }

    private VitalSeverity DetermineOxygenSaturationSeverity(int o2)
    {
        if (o2 < 90) return VitalSeverity.Critical;
        if (o2 < 95) return VitalSeverity.Warning;
        return VitalSeverity.Normal;
    }

    private VitalSeverity DetermineTemperatureSeverity(decimal temp)
    {
        if (temp >= 40 || temp < 35) return VitalSeverity.Critical;
        if (temp >= 38.5m || temp < 36) return VitalSeverity.Warning;
        return VitalSeverity.Normal;
    }

    private string GetRecommendedAction(PatientVital vital)
    {
        return vital.Severity switch
        {
            VitalSeverity.Critical => "Immediate medical attention required",
            VitalSeverity.Warning => "Monitor closely and reassess within 30 minutes",
            _ => "Continue routine monitoring"
        };
    }
}

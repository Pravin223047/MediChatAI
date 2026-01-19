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
        _logger.LogInformation("GetLatestVitalsAsync called for patientId: {PatientId}", patientId);
        
        var patient = await _context.Users.FindAsync(patientId);
        if (patient == null)
        {
            _logger.LogWarning("Patient not found for patientId: {PatientId}", patientId);
            return null!;
        }

        // Get all vitals for the patient ordered by date, then group in memory
        var allVitals = await _context.PatientVitals
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.RecordedAt)
            .ToListAsync();

        _logger.LogInformation("Found {Count} vitals for patientId: {PatientId}", allVitals.Count, patientId);

        // If no vitals exist, return null to indicate no data
        if (allVitals == null || !allVitals.Any())
        {
            _logger.LogInformation("No vitals found for patientId: {PatientId}", patientId);
            return null!;
        }

        // Get latest of each type (in memory to avoid EF Core GroupBy translation issues)
        var heartRate = allVitals.FirstOrDefault(v => v.VitalType == VitalType.HeartRate);
        var bp = allVitals.FirstOrDefault(v => v.VitalType == VitalType.BloodPressure);
        var temp = allVitals.FirstOrDefault(v => v.VitalType == VitalType.Temperature);
        var o2 = allVitals.FirstOrDefault(v => v.VitalType == VitalType.OxygenSaturation);
        var rr = allVitals.FirstOrDefault(v => v.VitalType == VitalType.RespiratoryRate);
        var glucose = allVitals.FirstOrDefault(v => v.VitalType == VitalType.BloodGlucose);

        // Get the most recent recording date
        var lastRecorded = allVitals.First().RecordedAt;
        
        _logger.LogInformation("Latest vitals for {PatientId}: HR={HeartRate}, BP={BloodPressure}, Temp={Temperature}, O2={O2}, LastRecorded={LastRecorded}",
            patientId, heartRate?.Value, bp?.Value, temp?.Value, o2?.Value, lastRecorded);

        return new PatientVitalsData(
            patientId,
            $"{patient.FirstName} {patient.LastName}",
            heartRate != null && int.TryParse(heartRate.Value, out var hr) ? hr : null,
            bp?.Value,
            temp != null && decimal.TryParse(temp.Value, out var t) ? t : null,
            o2 != null && int.TryParse(o2.Value, out var ox) ? ox : null,
            rr != null && int.TryParse(rr.Value, out var r) ? r : null,
            glucose != null && int.TryParse(glucose.Value, out var g) ? g : null,
            lastRecorded,
            allVitals.Any(v => v.IsAbnormal)
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

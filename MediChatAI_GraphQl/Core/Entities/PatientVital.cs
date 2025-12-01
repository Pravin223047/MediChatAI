using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public enum VitalType
{
    HeartRate,
    BloodPressure,
    Temperature,
    OxygenSaturation,
    RespiratoryRate,
    BloodGlucose,
    Weight,
    Height
}

public enum VitalSeverity
{
    Normal,
    Warning,
    Critical
}

public class PatientVital
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string PatientId { get; set; } = string.Empty;

    public ApplicationUser? Patient { get; set; }

    [MaxLength(450)]
    public string? RecordedByDoctorId { get; set; }

    public ApplicationUser? RecordedByDoctor { get; set; }

    [Required]
    public VitalType VitalType { get; set; }

    [Required]
    [MaxLength(50)]
    public string Value { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Unit { get; set; }

    public VitalSeverity Severity { get; set; } = VitalSeverity.Normal;

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// For blood pressure: systolic value
    /// </summary>
    public int? SystolicValue { get; set; }

    /// <summary>
    /// For blood pressure: diastolic value
    /// </summary>
    public int? DiastolicValue { get; set; }

    /// <summary>
    /// Additional metadata stored as JSON
    /// </summary>
    [MaxLength(1000)]
    public string? Metadata { get; set; }

    public bool IsAbnormal { get; set; } = false;

    public bool IsAlertSent { get; set; } = false;
}

using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public enum AlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum AlertStatus
{
    Active,
    Acknowledged,
    Resolved,
    Dismissed
}

public enum AlertCategory
{
    VitalSignAbnormal,
    PatientEmergency,
    MedicationAlert,
    LabResultCritical,
    AppointmentUrgent,
    SystemAlert,
    Other
}

public class EmergencyAlert
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string PatientId { get; set; } = string.Empty;

    public ApplicationUser? Patient { get; set; }

    [Required]
    [MaxLength(450)]
    public string DoctorId { get; set; } = string.Empty;

    public ApplicationUser? Doctor { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;

    public AlertStatus Status { get; set; } = AlertStatus.Active;

    public AlertCategory Category { get; set; } = AlertCategory.Other;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? AcknowledgedAt { get; set; }

    [MaxLength(450)]
    public string? AcknowledgedByUserId { get; set; }

    public ApplicationUser? AcknowledgedByUser { get; set; }

    public DateTime? ResolvedAt { get; set; }

    [MaxLength(450)]
    public string? ResolvedByUserId { get; set; }

    public ApplicationUser? ResolvedByUser { get; set; }

    [MaxLength(1000)]
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Related vital sign ID if alert is triggered by abnormal vital
    /// </summary>
    public Guid? RelatedVitalId { get; set; }

    public PatientVital? RelatedVital { get; set; }

    /// <summary>
    /// Location where patient is (room number, ward, etc.)
    /// </summary>
    [MaxLength(100)]
    public string? PatientLocation { get; set; }

    /// <summary>
    /// Urgent action required
    /// </summary>
    [MaxLength(500)]
    public string? RecommendedAction { get; set; }

    /// <summary>
    /// Additional metadata stored as JSON
    /// </summary>
    [MaxLength(2000)]
    public string? Metadata { get; set; }

    /// <summary>
    /// Whether SMS/Email notification was sent
    /// </summary>
    public bool IsNotificationSent { get; set; } = false;

    public DateTime? NotificationSentAt { get; set; }

    /// <summary>
    /// Auto-dismiss alert after certain time if not critical
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public enum ReminderFrequency
{
    AsNeeded,
    OnceDaily,
    TwiceDaily,
    ThreeTimesDaily,
    FourTimesDaily,
    EveryXHours,
    Weekly,
    BiWeekly,
    Monthly,
    Custom
}

public enum AdherenceStatus
{
    Taken,
    Missed,
    Skipped,
    Late,
    Pending
}

public class MedicationReminder
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string PatientId { get; set; } = string.Empty;

    public ApplicationUser? Patient { get; set; }

    public int? PrescriptionId { get; set; }

    public Prescription? Prescription { get; set; }

    [Required]
    [MaxLength(200)]
    public string MedicationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Dosage { get; set; } = string.Empty;

    public ReminderFrequency Frequency { get; set; } = ReminderFrequency.OnceDaily;

    /// <summary>
    /// Time of day for reminder (e.g., "08:00:00", "14:30:00")
    /// Can be multiple times stored as JSON array for multiple daily doses
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ReminderTimes { get; set; } = string.Empty;

    /// <summary>
    /// For EveryXHours frequency
    /// </summary>
    public int? IntervalHours { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastReminderSent { get; set; }

    public DateTime? NextReminderAt { get; set; }

    [MaxLength(500)]
    public string? Instructions { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// Days of week for weekly reminders (stored as JSON array)
    /// </summary>
    [MaxLength(100)]
    public string? DaysOfWeek { get; set; }

    /// <summary>
    /// Whether to send push notification
    /// </summary>
    public bool EnablePushNotification { get; set; } = true;

    /// <summary>
    /// Whether to send email
    /// </summary>
    public bool EnableEmail { get; set; } = false;

    /// <summary>
    /// Whether to send SMS
    /// </summary>
    public bool EnableSms { get; set; } = false;

    /// <summary>
    /// Snooze duration in minutes
    /// </summary>
    public int SnoozeDurationMinutes { get; set; } = 10;

    public int ConsecutiveMissedDoses { get; set; } = 0;

    public int TotalDosesTaken { get; set; } = 0;

    public int TotalDosesMissed { get; set; } = 0;

    /// <summary>
    /// Adherence percentage (0-100)
    /// </summary>
    public decimal AdherenceRate { get; set; } = 100m;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Collection of adherence logs
    /// </summary>
    public ICollection<MedicationAdherence> AdherenceLogs { get; set; } = new List<MedicationAdherence>();
}

/// <summary>
/// Tracks individual medication doses taken or missed
/// </summary>
public class MedicationAdherence
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ReminderId { get; set; }

    public MedicationReminder? Reminder { get; set; }

    [Required]
    public DateTime ScheduledTime { get; set; }

    public DateTime? ActualTime { get; set; }

    public AdherenceStatus Status { get; set; } = AdherenceStatus.Pending;

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// How many times the reminder was snoozed
    /// </summary>
    public int SnoozeCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}

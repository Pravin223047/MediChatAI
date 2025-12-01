using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public class DoctorAnalytics
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(450)]
    public string DoctorId { get; set; } = string.Empty;

    public ApplicationUser? Doctor { get; set; }

    [Required]
    public DateTime Date { get; set; }

    // Appointment Metrics
    public int TotalAppointments { get; set; } = 0;

    public int CompletedAppointments { get; set; } = 0;

    public int CancelledAppointments { get; set; } = 0;

    public int NoShowAppointments { get; set; } = 0;

    public int RescheduledAppointments { get; set; } = 0;

    /// <summary>
    /// Average appointment duration in minutes
    /// </summary>
    public int? AverageAppointmentDuration { get; set; }

    // Patient Metrics
    public int TotalPatients { get; set; } = 0;

    public int NewPatients { get; set; } = 0;

    public int FollowUpPatients { get; set; } = 0;

    public int CriticalPatients { get; set; } = 0;

    // Performance Metrics
    /// <summary>
    /// Average patient satisfaction rating (1-5)
    /// </summary>
    public decimal? AverageSatisfactionRating { get; set; }

    /// <summary>
    /// Average consultation time in minutes
    /// </summary>
    public int? AverageConsultationTime { get; set; }

    /// <summary>
    /// Average waiting time for patients in minutes
    /// </summary>
    public int? AverageWaitingTime { get; set; }

    /// <summary>
    /// Number of prescriptions written
    /// </summary>
    public int PrescriptionsWritten { get; set; } = 0;

    /// <summary>
    /// Number of lab tests ordered
    /// </summary>
    public int LabTestsOrdered { get; set; } = 0;

    /// <summary>
    /// Number of referrals made
    /// </summary>
    public int ReferralsMade { get; set; } = 0;

    // Revenue Metrics
    /// <summary>
    /// Total consultation fees collected
    /// </summary>
    public decimal TotalRevenue { get; set; } = 0;

    /// <summary>
    /// Pending payments
    /// </summary>
    public decimal PendingPayments { get; set; } = 0;

    /// <summary>
    /// Average revenue per consultation
    /// </summary>
    public decimal? AverageRevenuePerConsultation { get; set; }

    // Communication Metrics
    public int TotalMessages { get; set; } = 0;

    public int UnreadMessages { get; set; } = 0;

    public int EmailsSent { get; set; } = 0;

    public int SmsSent { get; set; } = 0;

    // Emergency Metrics
    public int EmergencyAlertsReceived { get; set; } = 0;

    public int EmergencyAlertsResolved { get; set; } = 0;

    /// <summary>
    /// Average response time to emergencies in minutes
    /// </summary>
    public int? AverageEmergencyResponseTime { get; set; }

    // Work Hours
    /// <summary>
    /// Total hours worked
    /// </summary>
    public decimal? TotalHoursWorked { get; set; }

    /// <summary>
    /// Break time taken in hours
    /// </summary>
    public decimal? BreakTimeTaken { get; set; }

    // Additional metrics stored as JSON
    [MaxLength(5000)]
    public string? AdditionalMetrics { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Week number of the year (1-52)
    /// </summary>
    public int? WeekNumber { get; set; }

    /// <summary>
    /// Month number (1-12)
    /// </summary>
    public int? MonthNumber { get; set; }

    /// <summary>
    /// Year
    /// </summary>
    public int? Year { get; set; }
}

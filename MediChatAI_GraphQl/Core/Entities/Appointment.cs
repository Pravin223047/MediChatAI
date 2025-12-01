namespace MediChatAI_GraphQl.Core.Entities;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    InProgress,
    Completed,
    Cancelled,
    Rescheduled,
    NoShow
}

public enum AppointmentType
{
    General,
    FollowUp,
    Emergency,
    Consultation,
    Checkup,
    Surgery,
    Vaccination,
    LabTest
}

public class Appointment
{
    public int Id { get; set; }

    // Patient and Doctor relationship
    public string PatientId { get; set; } = string.Empty;
    public ApplicationUser? Patient { get; set; }

    public string DoctorId { get; set; } = string.Empty;
    public ApplicationUser? Doctor { get; set; }

    // Appointment details
    public DateTime AppointmentDateTime { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public AppointmentType Type { get; set; } = AppointmentType.General;

    // Location details
    public string? RoomNumber { get; set; }
    public string? Location { get; set; }
    public bool IsVirtual { get; set; } = false;
    public string? MeetingLink { get; set; }

    // Notes and reasons
    public string? ReasonForVisit { get; set; }
    public string? DoctorNotes { get; set; }
    public string? PatientNotes { get; set; }
    public string? CancellationReason { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    // Reminder and follow-up
    public bool ReminderSent { get; set; } = false;
    public DateTime? ReminderSentAt { get; set; }
    public bool FollowUpRequired { get; set; } = false;
    public DateTime? FollowUpDate { get; set; }

    // Payment (optional for future use)
    public decimal ConsultationFee { get; set; } = 0;
    public bool IsPaid { get; set; } = false;
    public string? PaymentTransactionId { get; set; }
}

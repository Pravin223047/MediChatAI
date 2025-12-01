using System.ComponentModel.DataAnnotations;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

public class AppointmentModel
{
    public string AppointmentId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public string Type { get; set; } = "Consultation"; // Consultation, Follow-up, Emergency, Telemedicine
    public string Status { get; set; } = "Scheduled"; // Scheduled, Confirmed, In-Progress, Completed, Cancelled, No-Show
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public bool IsVirtual { get; set; }
    public string? RoomNumber { get; set; }
    public int? PatientAge { get; set; }
    public string? PatientGender { get; set; }
    public bool IsFirstVisit { get; set; }
    public bool ReminderSent { get; set; }
}

public class CreateAppointmentModel
{
    [Required]
    public string PatientId { get; set; } = string.Empty;

    [Required]
    public DateTime AppointmentDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Range(15, 120)]
    public int DurationMinutes { get; set; } = 30;

    [Required]
    public string Type { get; set; } = "Consultation";

    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public bool IsVirtual { get; set; }
    public string? RoomNumber { get; set; }
}

public class UpdateAppointmentModel
{
    [Required]
    public string AppointmentId { get; set; } = string.Empty;

    public DateTime? AppointmentDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public bool? IsVirtual { get; set; }
    public string? RoomNumber { get; set; }
}

public class CancelAppointmentModel
{
    [Required]
    public string AppointmentId { get; set; } = string.Empty;

    [Required]
    public string CancellationReason { get; set; } = string.Empty;

    public bool NotifyPatient { get; set; } = true;
}

public class AppointmentFilterModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<string> Statuses { get; set; } = new();
    public List<string> Types { get; set; } = new();
    public string? PatientId { get; set; }
    public string? SearchTerm { get; set; }
    public bool IncludeVirtualOnly { get; set; }
    public bool IncludeCancelled { get; set; }
}

public class DayScheduleModel
{
    public DateTime Date { get; set; }
    public List<AppointmentModel> Appointments { get; set; } = new();
    public List<TimeBlockDto> TimeBlocks { get; set; } = new();
    public List<TimeSlotModel> AvailableSlots { get; set; } = new();
    public int TotalAppointments => Appointments.Count;
    public int CompletedAppointments => Appointments.Count(a => a.Status == "Completed");
    public int PendingAppointments => Appointments.Count(a => a.Status == "Scheduled" || a.Status == "Confirmed");
    public int CancelledAppointments => Appointments.Count(a => a.Status == "Cancelled");
}

public class TimeSlotModel
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? ReasonUnavailable { get; set; }
}

public class CalendarViewModel
{
    public DateTime CurrentMonth { get; set; }
    public List<CalendarDayModel> Days { get; set; } = new();
    public Dictionary<DateTime, List<AppointmentModel>> AppointmentsByDate { get; set; } = new();
}

public class CalendarDayModel
{
    public DateTime Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public bool IsPast { get; set; }
    public int AppointmentCount { get; set; }
    public bool HasCriticalAppointment { get; set; }
}

public class WeekScheduleModel
{
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public List<DayScheduleModel> Days { get; set; } = new();
}

public class AppointmentStatisticsModel
{
    public int TotalAppointments { get; set; }
    public int TodayAppointments { get; set; }
    public int WeekAppointments { get; set; }
    public int MonthAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public int NoShowAppointments { get; set; }
    public double CompletionRate => TotalAppointments > 0 ? (CompletedAppointments * 100.0 / TotalAppointments) : 0;
    public double CancellationRate => TotalAppointments > 0 ? (CancelledAppointments * 100.0 / TotalAppointments) : 0;
    public double AverageAppointmentsPerDay { get; set; }
    public Dictionary<string, int> AppointmentsByType { get; set; } = new();
    public Dictionary<string, int> AppointmentsByStatus { get; set; } = new();
}

public class ConflictCheckResult
{
    public bool HasConflict { get; set; }
    public List<AppointmentModel> ConflictingAppointments { get; set; } = new();
    public string? Message { get; set; }
}

public class AppointmentReminderModel
{
    public string AppointmentId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public bool EmailSent { get; set; }
    public bool SmsSent { get; set; }
    public DateTime? SentAt { get; set; }
}

public enum AppointmentViewMode
{
    Day,
    Week,
    Month
}

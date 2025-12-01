namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

public class TimeBlockDto
{
    public Guid Id { get; set; }
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string BlockType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public DateTime? RepeatUntilDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTimeBlockInput
{
    public string DoctorId { get; set; } = string.Empty;
    public string BlockType { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public DateTime? RepeatUntilDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateTimeBlockInput
{
    public Guid Id { get; set; }
    public string? BlockType { get; set; }
    public DateTime? Date { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public bool? IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public DateTime? RepeatUntilDate { get; set; }
    public string? Notes { get; set; }
    public bool? IsActive { get; set; }
}

public class DoctorWeekScheduleDto
{
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public List<AppointmentDto> Appointments { get; set; } = new();
    public List<TimeBlockDto> TimeBlocks { get; set; } = new();
}

public class RecurrencePatternDto
{
    public string? Frequency { get; set; }
    public List<string>? DaysOfWeek { get; set; }
    public string? RepeatUntil { get; set; }
}

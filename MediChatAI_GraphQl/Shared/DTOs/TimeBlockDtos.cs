using MediChatAI_GraphQl.Core.Domain.Entities;

namespace MediChatAI_GraphQl.Shared.DTOs;

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

public class CreateTimeBlockDto
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

public class UpdateTimeBlockDto
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

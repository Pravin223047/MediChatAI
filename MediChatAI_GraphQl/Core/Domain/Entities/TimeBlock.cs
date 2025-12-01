using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MediChatAI_GraphQl.Core.Entities;

namespace MediChatAI_GraphQl.Core.Domain.Entities;

public class TimeBlock
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string DoctorId { get; set; } = string.Empty;

    [ForeignKey(nameof(DoctorId))]
    public ApplicationUser? Doctor { get; set; }

    [Required]
    [MaxLength(50)]
    public string BlockType { get; set; } = string.Empty; // Break, Lunch, Meeting, Personal, Other

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public bool IsRecurring { get; set; }

    // Stores JSON pattern for recurring blocks: { "frequency": "Weekly", "daysOfWeek": ["Mon", "Wed"], "repeatUntil": "2025-12-31" }
    public string? RecurrencePattern { get; set; }

    public DateTime? RepeatUntilDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;
}

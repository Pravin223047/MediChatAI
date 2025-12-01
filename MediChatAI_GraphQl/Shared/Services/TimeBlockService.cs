using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Domain.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Shared.DTOs;
using System.Text.Json;

namespace MediChatAI_GraphQl.Shared.Services;

public class TimeBlockService : ITimeBlockService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TimeBlockService> _logger;

    public TimeBlockService(
        ApplicationDbContext context,
        ILogger<TimeBlockService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TimeBlockDto?> GetTimeBlockByIdAsync(Guid id)
    {
        return await _context.TimeBlocks
            .Include(tb => tb.Doctor)
            .Where(tb => tb.Id == id)
            .Select(tb => MapToDto(tb))
            .FirstOrDefaultAsync();
    }

    public async Task<List<TimeBlockDto>> GetTimeBlocksByDoctorIdAsync(string doctorId)
    {
        return await _context.TimeBlocks
            .Include(tb => tb.Doctor)
            .Where(tb => tb.DoctorId == doctorId)
            .OrderBy(tb => tb.Date)
            .ThenBy(tb => tb.StartTime)
            .Select(tb => MapToDto(tb))
            .ToListAsync();
    }

    public async Task<List<TimeBlockDto>> GetTimeBlocksByDoctorIdAndDateRangeAsync(string doctorId, DateTime startDate, DateTime endDate)
    {
        return await _context.TimeBlocks
            .Include(tb => tb.Doctor)
            .Where(tb => tb.DoctorId == doctorId &&
                        tb.Date >= startDate &&
                        tb.Date <= endDate &&
                        tb.IsActive)
            .OrderBy(tb => tb.Date)
            .ThenBy(tb => tb.StartTime)
            .Select(tb => MapToDto(tb))
            .ToListAsync();
    }

    public async Task<List<TimeBlockDto>> GetActiveTimeBlocksByDoctorIdAsync(string doctorId)
    {
        return await _context.TimeBlocks
            .Include(tb => tb.Doctor)
            .Where(tb => tb.DoctorId == doctorId && tb.IsActive)
            .OrderBy(tb => tb.Date)
            .ThenBy(tb => tb.StartTime)
            .Select(tb => MapToDto(tb))
            .ToListAsync();
    }

    public async Task<TimeBlockDto> CreateTimeBlockAsync(CreateTimeBlockDto dto)
    {
        // Check for conflicts
        var hasConflict = await HasConflictAsync(dto.DoctorId, dto.Date, dto.StartTime, dto.EndTime);
        if (hasConflict)
        {
            throw new InvalidOperationException("Time block conflicts with existing appointment or time block");
        }

        var timeBlock = new TimeBlock
        {
            DoctorId = dto.DoctorId,
            BlockType = dto.BlockType,
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            IsRecurring = dto.IsRecurring,
            RecurrencePattern = dto.RecurrencePattern,
            RepeatUntilDate = dto.RepeatUntilDate,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.TimeBlocks.Add(timeBlock);
        await _context.SaveChangesAsync();

        return await GetTimeBlockByIdAsync(timeBlock.Id) ?? throw new Exception("Failed to retrieve created time block");
    }

    public async Task<List<TimeBlockDto>> CreateRecurringTimeBlocksAsync(CreateTimeBlockDto dto)
    {
        if (!dto.IsRecurring || string.IsNullOrEmpty(dto.RecurrencePattern) || !dto.RepeatUntilDate.HasValue)
        {
            throw new ArgumentException("Recurrence information is required for recurring time blocks");
        }

        var createdBlocks = new List<TimeBlockDto>();
        var pattern = JsonSerializer.Deserialize<RecurrencePatternDto>(dto.RecurrencePattern);

        if (pattern == null)
        {
            throw new ArgumentException("Invalid recurrence pattern");
        }

        var currentDate = dto.Date;
        var endDate = dto.RepeatUntilDate.Value;

        while (currentDate <= endDate)
        {
            bool shouldCreateBlock = false;

            switch (pattern.Frequency?.ToLower())
            {
                case "daily":
                    shouldCreateBlock = true;
                    break;
                case "weekly":
                    if (pattern.DaysOfWeek != null && pattern.DaysOfWeek.Contains(GetDayAbbreviation(currentDate.DayOfWeek)))
                    {
                        shouldCreateBlock = true;
                    }
                    break;
                case "monthly":
                    if (currentDate.Day == dto.Date.Day)
                    {
                        shouldCreateBlock = true;
                    }
                    break;
            }

            if (shouldCreateBlock)
            {
                var hasConflict = await HasConflictAsync(dto.DoctorId, currentDate, dto.StartTime, dto.EndTime);
                if (!hasConflict)
                {
                    var timeBlock = new TimeBlock
                    {
                        DoctorId = dto.DoctorId,
                        BlockType = dto.BlockType,
                        Date = currentDate,
                        StartTime = dto.StartTime,
                        EndTime = dto.EndTime,
                        IsRecurring = true,
                        RecurrencePattern = dto.RecurrencePattern,
                        RepeatUntilDate = dto.RepeatUntilDate,
                        Notes = dto.Notes,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.TimeBlocks.Add(timeBlock);
                    await _context.SaveChangesAsync();

                    var createdDto = await GetTimeBlockByIdAsync(timeBlock.Id);
                    if (createdDto != null)
                    {
                        createdBlocks.Add(createdDto);
                    }
                }
            }

            currentDate = currentDate.AddDays(1);
        }

        return createdBlocks;
    }

    public async Task<TimeBlockDto> UpdateTimeBlockAsync(UpdateTimeBlockDto dto)
    {
        var timeBlock = await _context.TimeBlocks.FindAsync(dto.Id);
        if (timeBlock == null)
        {
            throw new System.Collections.Generic.KeyNotFoundException("Time block not found");
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(dto.BlockType))
            timeBlock.BlockType = dto.BlockType;

        if (dto.Date.HasValue)
            timeBlock.Date = dto.Date.Value;

        if (dto.StartTime.HasValue)
            timeBlock.StartTime = dto.StartTime.Value;

        if (dto.EndTime.HasValue)
            timeBlock.EndTime = dto.EndTime.Value;

        if (dto.IsRecurring.HasValue)
            timeBlock.IsRecurring = dto.IsRecurring.Value;

        if (dto.RecurrencePattern != null)
            timeBlock.RecurrencePattern = dto.RecurrencePattern;

        if (dto.RepeatUntilDate.HasValue)
            timeBlock.RepeatUntilDate = dto.RepeatUntilDate;

        if (dto.Notes != null)
            timeBlock.Notes = dto.Notes;

        if (dto.IsActive.HasValue)
            timeBlock.IsActive = dto.IsActive.Value;

        // Check for conflicts if time/date changed
        if (dto.Date.HasValue || dto.StartTime.HasValue || dto.EndTime.HasValue)
        {
            var hasConflict = await HasConflictAsync(
                timeBlock.DoctorId,
                timeBlock.Date,
                timeBlock.StartTime,
                timeBlock.EndTime,
                timeBlock.Id);

            if (hasConflict)
            {
                throw new InvalidOperationException("Updated time block conflicts with existing appointment or time block");
            }
        }

        timeBlock.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetTimeBlockByIdAsync(timeBlock.Id) ?? throw new Exception("Failed to retrieve updated time block");
    }

    public async Task<bool> DeleteTimeBlockAsync(Guid id, string doctorId)
    {
        var timeBlock = await _context.TimeBlocks
            .Where(tb => tb.Id == id && tb.DoctorId == doctorId)
            .FirstOrDefaultAsync();

        if (timeBlock == null)
        {
            return false;
        }

        _context.TimeBlocks.Remove(timeBlock);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateTimeBlockAsync(Guid id, string doctorId)
    {
        var timeBlock = await _context.TimeBlocks
            .Where(tb => tb.Id == id && tb.DoctorId == doctorId)
            .FirstOrDefaultAsync();

        if (timeBlock == null)
        {
            return false;
        }

        timeBlock.IsActive = false;
        timeBlock.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> HasConflictAsync(string doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, Guid? excludeTimeBlockId = null, int? excludeAppointmentId = null)
    {
        // Check for conflicts with existing time blocks
        var timeBlockConflict = await _context.TimeBlocks
            .Where(tb => tb.DoctorId == doctorId &&
                        tb.Date == date &&
                        tb.IsActive &&
                        (!excludeTimeBlockId.HasValue || tb.Id != excludeTimeBlockId.Value) &&
                        ((startTime >= tb.StartTime && startTime < tb.EndTime) ||
                         (endTime > tb.StartTime && endTime <= tb.EndTime) ||
                         (startTime <= tb.StartTime && endTime >= tb.EndTime)))
            .AnyAsync();

        if (timeBlockConflict)
        {
            return true;
        }

        // Check for conflicts with existing appointments
        var appointmentDateTime = date.Date.Add(startTime);
        var appointmentEndTime = date.Date.Add(endTime);

        var appointmentConflict = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.Status != Core.Entities.AppointmentStatus.Cancelled &&
                       (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value) &&
                       ((appointmentDateTime >= a.AppointmentDateTime && appointmentDateTime < a.AppointmentDateTime.AddMinutes(a.DurationMinutes)) ||
                        (appointmentEndTime > a.AppointmentDateTime && appointmentEndTime <= a.AppointmentDateTime.AddMinutes(a.DurationMinutes)) ||
                        (appointmentDateTime <= a.AppointmentDateTime && appointmentEndTime >= a.AppointmentDateTime.AddMinutes(a.DurationMinutes))))
            .AnyAsync();

        return appointmentConflict;
    }

    private static TimeBlockDto MapToDto(TimeBlock timeBlock)
    {
        return new TimeBlockDto
        {
            Id = timeBlock.Id,
            DoctorId = timeBlock.DoctorId,
            DoctorName = timeBlock.Doctor != null ? $"{timeBlock.Doctor.FirstName} {timeBlock.Doctor.LastName}" : string.Empty,
            BlockType = timeBlock.BlockType,
            Date = timeBlock.Date,
            StartTime = timeBlock.StartTime,
            EndTime = timeBlock.EndTime,
            IsRecurring = timeBlock.IsRecurring,
            RecurrencePattern = timeBlock.RecurrencePattern,
            RepeatUntilDate = timeBlock.RepeatUntilDate,
            Notes = timeBlock.Notes,
            CreatedAt = timeBlock.CreatedAt,
            UpdatedAt = timeBlock.UpdatedAt,
            IsActive = timeBlock.IsActive
        };
    }

    private static string GetDayAbbreviation(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Monday => "Mon",
            DayOfWeek.Tuesday => "Tue",
            DayOfWeek.Wednesday => "Wed",
            DayOfWeek.Thursday => "Thu",
            DayOfWeek.Friday => "Fri",
            DayOfWeek.Saturday => "Sat",
            DayOfWeek.Sunday => "Sun",
            _ => string.Empty
        };
    }
}

// Helper DTO for JSON deserialization
public class RecurrencePatternDto
{
    public string? Frequency { get; set; }
    public List<string>? DaysOfWeek { get; set; }
    public string? RepeatUntil { get; set; }
}

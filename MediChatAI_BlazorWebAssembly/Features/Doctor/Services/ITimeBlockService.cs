using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public interface ITimeBlockService
{
    Task<TimeBlockDto?> GetTimeBlockByIdAsync(Guid timeBlockId);
    Task<List<TimeBlockDto>> GetMyTimeBlocksAsync();
    Task<List<TimeBlockDto>> GetMyTimeBlocksByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<List<TimeBlockDto>> GetMyActiveTimeBlocksAsync();
    Task<bool> CheckTimeSlotConflictAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, Guid? excludeTimeBlockId = null);
    Task<TimeBlockDto?> CreateTimeBlockAsync(CreateTimeBlockInput input);
    Task<List<TimeBlockDto>> CreateRecurringTimeBlocksAsync(CreateTimeBlockInput input);
    Task<TimeBlockDto?> UpdateTimeBlockAsync(UpdateTimeBlockInput input);
    Task<bool> DeleteTimeBlockAsync(Guid timeBlockId);
    Task<bool> DeactivateTimeBlockAsync(Guid timeBlockId);
}

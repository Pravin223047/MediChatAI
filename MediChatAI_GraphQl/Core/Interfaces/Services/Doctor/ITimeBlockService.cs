using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;

public interface ITimeBlockService
{
    Task<TimeBlockDto?> GetTimeBlockByIdAsync(Guid id);
    Task<List<TimeBlockDto>> GetTimeBlocksByDoctorIdAsync(string doctorId);
    Task<List<TimeBlockDto>> GetTimeBlocksByDoctorIdAndDateRangeAsync(string doctorId, DateTime startDate, DateTime endDate);
    Task<List<TimeBlockDto>> GetActiveTimeBlocksByDoctorIdAsync(string doctorId);
    Task<TimeBlockDto> CreateTimeBlockAsync(CreateTimeBlockDto dto);
    Task<List<TimeBlockDto>> CreateRecurringTimeBlocksAsync(CreateTimeBlockDto dto);
    Task<TimeBlockDto> UpdateTimeBlockAsync(UpdateTimeBlockDto dto);
    Task<bool> DeleteTimeBlockAsync(Guid id, string doctorId);
    Task<bool> DeactivateTimeBlockAsync(Guid id, string doctorId);
    Task<bool> HasConflictAsync(string doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, Guid? excludeTimeBlockId = null, int? excludeAppointmentId = null);
}

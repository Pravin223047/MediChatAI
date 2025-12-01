using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public class TimeBlockService : ITimeBlockService
{
    private readonly IGraphQLService _graphQLService;

    public TimeBlockService(IGraphQLService graphQLService)
    {
        _graphQLService = graphQLService;
    }

    public async Task<TimeBlockDto?> GetTimeBlockByIdAsync(Guid timeBlockId)
    {
        var query = $@"
            query {{
                getTimeBlockById(timeBlockId: ""{timeBlockId}"") {{
                    id
                    doctorId
                    doctorName
                    blockType
                    date
                    startTime
                    endTime
                    isRecurring
                    recurrencePattern
                    repeatUntilDate
                    notes
                    createdAt
                    updatedAt
                    isActive
                }}
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("getTimeBlockById") && response["getTimeBlockById"] != null)
            {
                var json = JsonSerializer.Serialize(response["getTimeBlockById"]);
                return JsonSerializer.Deserialize<TimeBlockDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching time block: {ex.Message}");
        }

        return null;
    }

    public async Task<List<TimeBlockDto>> GetMyTimeBlocksAsync()
    {
        const string query = @"
            query {
                myTimeBlocks {
                    id
                    doctorId
                    doctorName
                    blockType
                    date
                    startTime
                    endTime
                    isRecurring
                    recurrencePattern
                    repeatUntilDate
                    notes
                    createdAt
                    updatedAt
                    isActive
                }
            }";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("myTimeBlocks"))
            {
                var json = JsonSerializer.Serialize(response["myTimeBlocks"]);
                return JsonSerializer.Deserialize<List<TimeBlockDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TimeBlockDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching time blocks: {ex.Message}");
        }

        return new List<TimeBlockDto>();
    }

    public async Task<List<TimeBlockDto>> GetMyTimeBlocksByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var query = $@"
            query {{
                myTimeBlocksByDateRange(startDate: ""{startDate:yyyy-MM-ddTHH:mm:ssZ}"", endDate: ""{endDate:yyyy-MM-ddTHH:mm:ssZ}"") {{
                    id
                    doctorId
                    doctorName
                    blockType
                    date
                    startTime
                    endTime
                    isRecurring
                    recurrencePattern
                    repeatUntilDate
                    notes
                    createdAt
                    updatedAt
                    isActive
                }}
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("myTimeBlocksByDateRange"))
            {
                var json = JsonSerializer.Serialize(response["myTimeBlocksByDateRange"]);
                return JsonSerializer.Deserialize<List<TimeBlockDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TimeBlockDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching time blocks by date range: {ex.Message}");
        }

        return new List<TimeBlockDto>();
    }

    public async Task<List<TimeBlockDto>> GetMyActiveTimeBlocksAsync()
    {
        const string query = @"
            query {
                myActiveTimeBlocks {
                    id
                    doctorId
                    doctorName
                    blockType
                    date
                    startTime
                    endTime
                    isRecurring
                    recurrencePattern
                    repeatUntilDate
                    notes
                    createdAt
                    updatedAt
                    isActive
                }
            }";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("myActiveTimeBlocks"))
            {
                var json = JsonSerializer.Serialize(response["myActiveTimeBlocks"]);
                return JsonSerializer.Deserialize<List<TimeBlockDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TimeBlockDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching active time blocks: {ex.Message}");
        }

        return new List<TimeBlockDto>();
    }

    public async Task<bool> CheckTimeSlotConflictAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, Guid? excludeTimeBlockId = null)
    {
        var excludeParam = excludeTimeBlockId.HasValue ? $@", excludeTimeBlockId: ""{excludeTimeBlockId.Value}""" : "";
        var query = $@"
            query {{
                checkTimeSlotConflict(date: ""{date:yyyy-MM-ddTHH:mm:ssZ}"", startTime: ""{startTime}"", endTime: ""{endTime}""{excludeParam})
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("checkTimeSlotConflict"))
            {
                return Convert.ToBoolean(response["checkTimeSlotConflict"]);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking time slot conflict: {ex.Message}");
        }

        return false;
    }

    public async Task<TimeBlockDto?> CreateTimeBlockAsync(CreateTimeBlockInput input)
    {
        var mutation = $@"
            mutation {{
                createTimeBlock(input: {{
                    doctorId: ""{input.DoctorId}""
                    blockType: ""{input.BlockType}""
                    date: ""{input.Date:yyyy-MM-ddTHH:mm:ssZ}""
                    startTime: ""{input.StartTime}""
                    endTime: ""{input.EndTime}""
                    isRecurring: {input.IsRecurring.ToString().ToLower()}
                    {(input.RecurrencePattern != null ? $@"recurrencePattern: ""{input.RecurrencePattern}""" : "")}
                    {(input.RepeatUntilDate.HasValue ? $@"repeatUntilDate: ""{input.RepeatUntilDate:yyyy-MM-ddTHH:mm:ssZ}""" : "")}
                    {(input.Notes != null ? $@"notes: ""{input.Notes}""" : "")}
                }}) {{
                    id
                    doctorId
                    doctorName
                    blockType
                    date
                    startTime
                    endTime
                    isRecurring
                    recurrencePattern
                    repeatUntilDate
                    notes
                    createdAt
                    updatedAt
                    isActive
                }}
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(mutation);
            if (response != null && response.ContainsKey("createTimeBlock"))
            {
                var json = JsonSerializer.Serialize(response["createTimeBlock"]);
                return JsonSerializer.Deserialize<TimeBlockDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating time block: {ex.Message}");
        }

        return null;
    }

    public async Task<List<TimeBlockDto>> CreateRecurringTimeBlocksAsync(CreateTimeBlockInput input)
    {
        var mutation = $@"
            mutation {{
                createRecurringTimeBlocks(input: {{
                    doctorId: ""{input.DoctorId}""
                    blockType: ""{input.BlockType}""
                    date: ""{input.Date:yyyy-MM-ddTHH:mm:ssZ}""
                    startTime: ""{input.StartTime}""
                    endTime: ""{input.EndTime}""
                    isRecurring: {input.IsRecurring.ToString().ToLower()}
                    {(input.RecurrencePattern != null ? $@"recurrencePattern: ""{input.RecurrencePattern}""" : "")}
                    {(input.RepeatUntilDate.HasValue ? $@"repeatUntilDate: ""{input.RepeatUntilDate:yyyy-MM-ddTHH:mm:ssZ}""" : "")}
                    {(input.Notes != null ? $@"notes: ""{input.Notes}""" : "")}
                }}) {{
                    id
                    doctorId
                    doctorName
                    blockType
                    date
                    startTime
                    endTime
                    isRecurring
                    recurrencePattern
                    repeatUntilDate
                    notes
                    createdAt
                    updatedAt
                    isActive
                }}
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(mutation);
            if (response != null && response.ContainsKey("createRecurringTimeBlocks"))
            {
                var json = JsonSerializer.Serialize(response["createRecurringTimeBlocks"]);
                return JsonSerializer.Deserialize<List<TimeBlockDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<TimeBlockDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating recurring time blocks: {ex.Message}");
        }

        return new List<TimeBlockDto>();
    }

    public async Task<TimeBlockDto?> UpdateTimeBlockAsync(UpdateTimeBlockInput input)
    {
        var mutation = $@"
            mutation {{
                updateTimeBlock(input: {{
                    id: ""{input.Id}""
                    {(input.BlockType != null ? $@"blockType: ""{input.BlockType}""" : "")}
                    {(input.Date.HasValue ? $@"date: ""{input.Date:yyyy-MM-ddTHH:mm:ssZ}""" : "")}
                    {(input.StartTime.HasValue ? $@"startTime: ""{input.StartTime}""" : "")}
                    {(input.EndTime.HasValue ? $@"endTime: ""{input.EndTime}""" : "")}
                    {(input.IsRecurring.HasValue ? $@"isRecurring: {input.IsRecurring.Value.ToString().ToLower()}" : "")}
                    {(input.RecurrencePattern != null ? $@"recurrencePattern: ""{input.RecurrencePattern}""" : "")}
                    {(input.RepeatUntilDate.HasValue ? $@"repeatUntilDate: ""{input.RepeatUntilDate:yyyy-MM-ddTHH:mm:ssZ}""" : "")}
                    {(input.Notes != null ? $@"notes: ""{input.Notes}""" : "")}
                    {(input.IsActive.HasValue ? $@"isActive: {input.IsActive.Value.ToString().ToLower()}" : "")}
                }}) {{
                    id
                    doctorId
                    doctorName
                    blockType
                    date
                    startTime
                    endTime
                    isRecurring
                    recurrencePattern
                    repeatUntilDate
                    notes
                    createdAt
                    updatedAt
                    isActive
                }}
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(mutation);
            if (response != null && response.ContainsKey("updateTimeBlock"))
            {
                var json = JsonSerializer.Serialize(response["updateTimeBlock"]);
                return JsonSerializer.Deserialize<TimeBlockDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating time block: {ex.Message}");
        }

        return null;
    }

    public async Task<bool> DeleteTimeBlockAsync(Guid timeBlockId)
    {
        var mutation = $@"
            mutation {{
                deleteTimeBlock(timeBlockId: ""{timeBlockId}"")
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(mutation);
            if (response != null && response.ContainsKey("deleteTimeBlock"))
            {
                return Convert.ToBoolean(response["deleteTimeBlock"]);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting time block: {ex.Message}");
        }

        return false;
    }

    public async Task<bool> DeactivateTimeBlockAsync(Guid timeBlockId)
    {
        var mutation = $@"
            mutation {{
                deactivateTimeBlock(timeBlockId: ""{timeBlockId}"")
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(mutation);
            if (response != null && response.ContainsKey("deactivateTimeBlock"))
            {
                return Convert.ToBoolean(response["deactivateTimeBlock"]);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deactivating time block: {ex.Message}");
        }

        return false;
    }
}

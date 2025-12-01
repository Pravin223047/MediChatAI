namespace MediChatAI_GraphQl.Core.Interfaces.Services.Shared;

public interface IActivityLoggingService
{
    Task LogUserActivityAsync(string userId, string activityType, string description,
        string? details = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null);
}
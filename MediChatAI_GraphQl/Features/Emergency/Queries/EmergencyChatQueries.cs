using HotChocolate.Authorization;
using MediChatAI_GraphQl.Core.Interfaces.Services.Emergency;
using MediChatAI_GraphQl.Features.Emergency.DTOs;

namespace MediChatAI_GraphQl.Features.Emergency.Queries;

/// <summary>
/// GraphQL queries for emergency chat functionality
/// </summary>
public class EmergencyChatQueries
{
    /// <summary>
    /// Retrieves chat history for a session
    /// </summary>
    [AllowAnonymous] // Emergency chat history should be accessible
    public async Task<ChatHistoryResponse> GetChatHistory(
        [Service] IGeminiEmergencyChatService chatService,
        string? sessionId = null,
        string? userId = null,
        int limit = 50,
        DateTime? since = null)
    {
        var request = new ChatHistoryRequest
        {
            SessionId = sessionId,
            UserId = userId,
            Limit = limit,
            Since = since
        };

        return await chatService.GetChatHistoryAsync(request);
    }

    /// <summary>
    /// Gets available emergency chat features and capabilities
    /// </summary>
    [AllowAnonymous]
    public ChatCapabilitiesDto GetChatCapabilities()
    {
        return new ChatCapabilitiesDto
        {
            SupportsVoiceInput = true,
            SupportsVoiceOutput = true,
            SupportsHospitalRecommendations = true,
            SupportsEmergencyProcedures = true,
            SupportsUrgencyAnalysis = true,
            SupportedLanguages = new List<string> { "en", "es", "fr", "de", "it", "pt", "hi", "zh" },
            MaxMessageLength = 5000,
            SupportsMultimodalInput = false, // Image input not yet supported
            RateLimitPerMinute = 20
        };
    }
}

/// <summary>
/// Chat capabilities information
/// </summary>
public class ChatCapabilitiesDto
{
    public bool SupportsVoiceInput { get; set; }
    public bool SupportsVoiceOutput { get; set; }
    public bool SupportsHospitalRecommendations { get; set; }
    public bool SupportsEmergencyProcedures { get; set; }
    public bool SupportsUrgencyAnalysis { get; set; }
    public List<string> SupportedLanguages { get; set; } = new();
    public int MaxMessageLength { get; set; }
    public bool SupportsMultimodalInput { get; set; }
    public int RateLimitPerMinute { get; set; }
}

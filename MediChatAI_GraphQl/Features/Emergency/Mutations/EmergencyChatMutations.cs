using HotChocolate.Authorization;
using MediChatAI_GraphQl.Core.Interfaces.Services.Emergency;
using MediChatAI_GraphQl.Features.Emergency.DTOs;

namespace MediChatAI_GraphQl.Features.Emergency.Mutations;

/// <summary>
/// GraphQL mutations for emergency chat functionality
/// </summary>
public class EmergencyChatMutations
{
    /// <summary>
    /// Sends a chat message to the AI assistant
    /// </summary>
    [AllowAnonymous] // Emergency chat should be accessible to everyone
    public async Task<ChatResponseDto> SendChatMessage(
        [Service] IGeminiEmergencyChatService chatService,
        SendChatMessageInput input)
    {
        return await chatService.SendMessageAsync(input);
    }

    /// <summary>
    /// Clears chat history for a session
    /// </summary>
    [AllowAnonymous]
    public async Task<bool> ClearChatHistory(
        [Service] IGeminiEmergencyChatService chatService,
        string sessionId)
    {
        return await chatService.ClearChatHistoryAsync(sessionId);
    }

    /// <summary>
    /// Analyzes the urgency level of an emergency message
    /// </summary>
    [AllowAnonymous]
    public async Task<UrgencyAnalysisResult> AnalyzeEmergencyUrgency(
        [Service] IGeminiEmergencyChatService chatService,
        string message)
    {
        var (urgencyLevel, reasoning) = await chatService.AnalyzeUrgencyAsync(message);

        return new UrgencyAnalysisResult
        {
            UrgencyLevel = urgencyLevel,
            Reasoning = reasoning,
            ShouldCall911 = urgencyLevel >= 4
        };
    }

    /// <summary>
    /// Gets emergency procedure guidance
    /// </summary>
    [AllowAnonymous]
    public async Task<EmergencyProcedureResult> GetEmergencyProcedure(
        [Service] IGeminiEmergencyChatService chatService,
        string emergencyType,
        string? context = null)
    {
        var steps = await chatService.GetEmergencyProcedureAsync(emergencyType, context);

        return new EmergencyProcedureResult
        {
            EmergencyType = emergencyType,
            Steps = steps,
            Success = steps.Any()
        };
    }

    /// <summary>
    /// Gets AI-powered hospital recommendations
    /// </summary>
    [AllowAnonymous]
    public async Task<HospitalRecommendationResult> RecommendHospitals(
        [Service] IGeminiEmergencyChatService chatService,
        string emergencyType,
        List<HospitalContextDto> availableHospitals,
        double userLatitude,
        double userLongitude)
    {
        var recommendations = await chatService.RecommendHospitalsAsync(
            emergencyType,
            availableHospitals,
            (userLatitude, userLongitude));

        return new HospitalRecommendationResult
        {
            Recommendations = recommendations,
            Success = recommendations.Any()
        };
    }
}

/// <summary>
/// Result type for urgency analysis
/// </summary>
public class UrgencyAnalysisResult
{
    public int UrgencyLevel { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public bool ShouldCall911 { get; set; }
}

/// <summary>
/// Result type for emergency procedures
/// </summary>
public class EmergencyProcedureResult
{
    public string EmergencyType { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public bool Success { get; set; }
}

/// <summary>
/// Result type for hospital recommendations
/// </summary>
public class HospitalRecommendationResult
{
    public List<HospitalRecommendationDto> Recommendations { get; set; } = new();
    public bool Success { get; set; }
}

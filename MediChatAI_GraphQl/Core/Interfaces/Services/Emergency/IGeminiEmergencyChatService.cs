using MediChatAI_GraphQl.Features.Emergency.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Emergency;

/// <summary>
/// Service interface for Gemini AI-powered emergency chat functionality
/// </summary>
public interface IGeminiEmergencyChatService
{
    /// <summary>
    /// Sends a chat message and receives an AI response with hospital context
    /// </summary>
    /// <param name="input">The chat message input with context</param>
    /// <returns>AI-generated response with suggestions and recommendations</returns>
    Task<ChatResponseDto> SendMessageAsync(SendChatMessageInput input);

    /// <summary>
    /// Retrieves chat history for a session or user
    /// </summary>
    /// <param name="request">Chat history request parameters</param>
    /// <returns>Chat history with messages</returns>
    Task<ChatHistoryResponse> GetChatHistoryAsync(ChatHistoryRequest request);

    /// <summary>
    /// Analyzes emergency urgency from user message
    /// </summary>
    /// <param name="message">User's message describing the emergency</param>
    /// <returns>Urgency level (1-5, 5 being most urgent) and reasoning</returns>
    Task<(int urgencyLevel, string reasoning)> AnalyzeUrgencyAsync(string message);

    /// <summary>
    /// Gets step-by-step emergency procedure guidance
    /// </summary>
    /// <param name="emergencyType">Type of emergency (e.g., "heart attack", "choking")</param>
    /// <param name="context">Additional context about the situation</param>
    /// <returns>Structured guidance steps</returns>
    Task<List<string>> GetEmergencyProcedureAsync(string emergencyType, string? context = null);

    /// <summary>
    /// Recommends hospitals based on emergency type and user location
    /// </summary>
    /// <param name="emergencyType">Type of emergency</param>
    /// <param name="availableHospitals">List of available hospitals with details</param>
    /// <param name="userLocation">User's current location</param>
    /// <returns>Prioritized list of hospital recommendations</returns>
    Task<List<HospitalRecommendationDto>> RecommendHospitalsAsync(
        string emergencyType,
        List<HospitalContextDto> availableHospitals,
        (double latitude, double longitude) userLocation);

    /// <summary>
    /// Clears chat history for a session
    /// </summary>
    /// <param name="sessionId">Session identifier</param>
    /// <returns>True if successful</returns>
    Task<bool> ClearChatHistoryAsync(string sessionId);
}

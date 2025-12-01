using MediChatAI_GraphQl.Features.AIChat.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.AIChat;

/// <summary>
/// Service interface for AI Chatbot functionality
/// </summary>
public interface IAIChatbotService
{
    /// <summary>
    /// Send a message to the AI chatbot
    /// </summary>
    Task<AIChatMessageResponse> SendMessageAsync(string userId, SendAIChatMessageInput input);

    /// <summary>
    /// Analyze symptoms using AI
    /// </summary>
    Task<AIChatSymptomAnalysisResponse> AnalyzeSymptomsAsync(string userId, AnalyzeSymptomsInput input);

    /// <summary>
    /// Analyze a medical image using AI
    /// </summary>
    Task<AIChatImageAnalysisResponse> AnalyzeImageAsync(string userId, AnalyzeImageInput input);

    /// <summary>
    /// Save a conversation for later access
    /// </summary>
    Task<SaveConversationResponse> SaveConversationAsync(string userId, SaveConversationInput input);

    /// <summary>
    /// Delete a saved conversation
    /// </summary>
    Task<DeleteConversationResponse> DeleteConversationAsync(string userId, string conversationId);

    /// <summary>
    /// Get conversation history for a user
    /// </summary>
    Task<List<SavedConversationDto>> GetConversationHistoryAsync(string userId);

    /// <summary>
    /// Load messages from a saved conversation
    /// </summary>
    Task<List<AIChatMessageDto>> LoadConversationMessagesAsync(string userId, string conversationId);

    /// <summary>
    /// Get suggested questions for the chatbot
    /// </summary>
    Task<SuggestedQuestionsResponse> GetSuggestedQuestionsAsync();
}

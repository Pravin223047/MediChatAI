using HotChocolate.Authorization;
using MediChatAI_GraphQl.Core.Interfaces.Services.AIChat;
using MediChatAI_GraphQl.Features.AIChat.DTOs;
using System.Security.Claims;

namespace MediChatAI_GraphQl.Features.AIChat.Queries;

[ExtendObjectType("Query")]
public class AIChatQueries
{
    /// <summary>
    /// Get conversation history for the current user
    /// </summary>
    [Authorize]
    public async Task<List<SavedConversationDto>> AiChatConversationHistory(
        ClaimsPrincipal claimsPrincipal,
        [Service] IAIChatbotService chatbotService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new List<SavedConversationDto>();
        }

        return await chatbotService.GetConversationHistoryAsync(userId);
    }

    /// <summary>
    /// Load messages from a saved conversation
    /// </summary>
    [Authorize]
    public async Task<List<AIChatMessageDto>> AiChatConversationMessages(
        string conversationId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAIChatbotService chatbotService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new List<AIChatMessageDto>();
        }

        return await chatbotService.LoadConversationMessagesAsync(userId, conversationId);
    }

    /// <summary>
    /// Get suggested questions for the AI chatbot
    /// </summary>
    [Authorize]
    public async Task<SuggestedQuestionsResponse> AiChatSuggestedQuestions(
        [Service] IAIChatbotService chatbotService)
    {
        return await chatbotService.GetSuggestedQuestionsAsync();
    }
}

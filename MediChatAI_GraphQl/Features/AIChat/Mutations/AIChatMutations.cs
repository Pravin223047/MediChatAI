using HotChocolate.Authorization;
using MediChatAI_GraphQl.Core.Interfaces.Services.AIChat;
using MediChatAI_GraphQl.Features.AIChat.DTOs;
using System.Security.Claims;

namespace MediChatAI_GraphQl.Features.AIChat.Mutations;

[ExtendObjectType("Mutation")]
public class AIChatMutations
{
    /// <summary>
    /// Send a message to the AI chatbot
    /// </summary>
    [Authorize]
    public async Task<AIChatMessageResponse> SendAIChatMessage(
        SendAIChatMessageInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAIChatbotService chatbotService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new AIChatMessageResponse
            {
                Success = false,
                Message = "User not authenticated"
            };
        }

        return await chatbotService.SendMessageAsync(userId, input);
    }

    /// <summary>
    /// Analyze symptoms using AI
    /// </summary>
    [Authorize]
    public async Task<AIChatSymptomAnalysisResponse> AnalyzeSymptoms(
        AnalyzeSymptomsInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAIChatbotService chatbotService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new AIChatSymptomAnalysisResponse
            {
                Success = false,
                Message = "User not authenticated"
            };
        }

        return await chatbotService.AnalyzeSymptomsAsync(userId, input);
    }

    /// <summary>
    /// Analyze a medical image using AI
    /// </summary>
    [Authorize]
    public async Task<AIChatImageAnalysisResponse> AnalyzeImage(
        AnalyzeImageInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAIChatbotService chatbotService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new AIChatImageAnalysisResponse
            {
                Success = false,
                Message = "User not authenticated"
            };
        }

        return await chatbotService.AnalyzeImageAsync(userId, input);
    }

    /// <summary>
    /// Save a conversation for later access
    /// </summary>
    [Authorize]
    public async Task<SaveConversationResponse> SaveAIChatConversation(
        SaveConversationInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAIChatbotService chatbotService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new SaveConversationResponse
            {
                Success = false,
                Message = "User not authenticated"
            };
        }

        return await chatbotService.SaveConversationAsync(userId, input);
    }

    /// <summary>
    /// Delete a saved conversation
    /// </summary>
    [Authorize]
    public async Task<DeleteConversationResponse> DeleteAIChatConversation(
        string conversationId,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAIChatbotService chatbotService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new DeleteConversationResponse
            {
                Success = false,
                Message = "User not authenticated"
            };
        }

        return await chatbotService.DeleteConversationAsync(userId, conversationId);
    }

    /// <summary>
    /// Generate AI-powered summary of a doctor-patient conversation
    /// </summary>
    [Authorize]
    public async Task<ConversationSummaryResponse> SummarizeConversation(
        SummarizeConversationInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IAIChatbotService chatbotService)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return new ConversationSummaryResponse
            {
                Success = false,
                Message = "User not authenticated"
            };
        }

        return await chatbotService.SummarizeConversationAsync(input);
    }
}

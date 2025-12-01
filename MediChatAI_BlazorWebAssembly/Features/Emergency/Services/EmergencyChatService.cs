using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Services;

public class EmergencyChatService : IEmergencyChatService
{
    private readonly IGraphQLService _graphQLService;

    public EmergencyChatService(IGraphQLService graphQLService)
    {
        _graphQLService = graphQLService;
    }

    public async Task<ChatResponseDto?> SendMessageAsync(SendChatMessageInput input)
    {
        var query = @"
            mutation SendChatMessage($input: SendChatMessageInput!) {
                sendChatMessage(input: $input) {
                    messageId
                    content
                    timestamp
                    success
                    errorMessage
                    suggestedActions {
                        label
                        action
                        icon
                        url
                        phoneNumber
                    }
                    hospitalRecommendations {
                        hospitalName
                        reasonForRecommendation
                        distanceInKm
                        rating
                        priority
                        phoneNumber
                        latitude
                        longitude
                    }
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<SendChatMessageResponse>(query, variables);
        return response?.SendChatMessage;
    }

    public async Task<bool> ClearChatHistoryAsync(string sessionId)
    {
        var query = @"
            mutation ClearChatHistory($sessionId: String!) {
                clearChatHistory(sessionId: $sessionId)
            }";

        var variables = new { sessionId };
        var response = await _graphQLService.SendQueryAsync<ClearChatHistoryResponse>(query, variables);
        return response?.ClearChatHistory ?? false;
    }

    // Response wrapper classes
    private class SendChatMessageResponse
    {
        public ChatResponseDto? SendChatMessage { get; set; }
    }

    private class ClearChatHistoryResponse
    {
        public bool ClearChatHistory { get; set; }
    }
}

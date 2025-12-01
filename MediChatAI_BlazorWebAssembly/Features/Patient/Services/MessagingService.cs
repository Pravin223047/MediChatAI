using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public class MessagingService : IMessagingService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<MessagingService> _logger;

    public MessagingService(IGraphQLService graphQLService, ILogger<MessagingService> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync()
    {
        try
        {
            const string query = @"
                query GetConversations {
                  conversations {
                    id
                    partnerId
                    partnerName
                    partnerRole
                    partnerProfileImage
                    lastMessage
                    lastMessageTime
                    unreadCount
                    isOnline
                  }
                }
                ";

            var response = await _graphQLService.SendQueryAsync<GetConversationsResponse>(query);

            if (response?.Conversations != null)
            {
                _logger.LogInformation("Successfully loaded {Count} conversations", response.Conversations.Count);
                return response.Conversations;
            }

            _logger.LogWarning("No conversations received from GraphQL");
            return new List<ConversationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading conversations from GraphQL");
            throw;
        }
    }

    public async Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId)
    {
        try
        {
            const string query = @"
                query GetConversationMessages($conversationId: UUID!) {
                  conversationMessages(conversationId: $conversationId) {
                    id
                    senderId
                    receiverId
                    content
                    sentAt
                    readAt
                    status
                    messageType
                    attachmentUrl
                    attachmentFileName
                    isSender
                  }
                }
                ";

            var variables = new { conversationId };
            var response = await _graphQLService.SendQueryAsync<GetConversationMessagesResponse>(query, variables);

            if (response?.ConversationMessages != null)
            {
                _logger.LogInformation("Successfully loaded {Count} messages for conversation {ConversationId}",
                    response.ConversationMessages.Count, conversationId);
                return response.ConversationMessages;
            }

            _logger.LogWarning("No messages received from GraphQL for conversation {ConversationId}", conversationId);
            return new List<MessageDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading messages for conversation {ConversationId}", conversationId);
            throw;
        }
    }

    public async Task<MessageDto?> SendMessageAsync(SendMessageInput input)
    {
        try
        {
            const string mutation = @"
                mutation SendMessage($input: SendMessageInput!) {
                  sendMessage(input: $input) {
                    id
                    senderId
                    receiverId
                    content
                    sentAt
                    readAt
                    status
                    messageType
                    attachmentUrl
                    attachmentFileName
                    isSender
                  }
                }
                ";

            var variables = new { input };
            var response = await _graphQLService.SendQueryAsync<SendMessageResponse>(mutation, variables);

            if (response?.SendMessage != null)
            {
                _logger.LogInformation("Successfully sent message to {ReceiverId}", input.ReceiverId);
                return response.SendMessage;
            }

            _logger.LogWarning("Failed to send message");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            throw;
        }
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid conversationId)
    {
        try
        {
            const string mutation = @"
                mutation MarkMessagesAsRead($conversationId: UUID!) {
                  markMessagesAsRead(conversationId: $conversationId)
                }
                ";

            var variables = new { conversationId };
            var response = await _graphQLService.SendQueryAsync<MarkMessagesAsReadResponse>(mutation, variables);

            if (response != null)
            {
                _logger.LogInformation("Successfully marked messages as read for conversation {ConversationId}", conversationId);
                return response.MarkMessagesAsRead;
            }

            _logger.LogWarning("Failed to mark messages as read");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking messages as read for conversation {ConversationId}", conversationId);
            throw;
        }
    }

    public async Task<int> GetUnreadMessageCountAsync()
    {
        try
        {
            const string query = @"
                query GetUnreadMessageCount {
                  unreadMessageCount
                }
                ";

            var response = await _graphQLService.SendQueryAsync<GetUnreadMessageCountResponse>(query);

            if (response != null)
            {
                _logger.LogInformation("Successfully loaded unread message count: {Count}", response.UnreadMessageCount);
                return response.UnreadMessageCount;
            }

            _logger.LogWarning("Failed to get unread message count");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread message count");
            throw;
        }
    }

    public async Task<UploadAttachmentResponseDto?> UploadAttachmentAsync(string fileBase64, string fileName, string mimeType)
    {
        try
        {
            const string mutation = @"
                mutation UploadMessageAttachment($fileBase64: String!, $fileName: String!, $mimeType: String!) {
                  uploadMessageAttachment(fileBase64: $fileBase64, fileName: $fileName, mimeType: $mimeType) {
                    success
                    url
                    fileName
                    fileSize
                    mimeType
                    errorMessage
                  }
                }
                ";

            var variables = new { fileBase64, fileName, mimeType };
            var response = await _graphQLService.SendQueryAsync<UploadAttachmentMutationResponse>(mutation, variables);

            if (response?.UploadMessageAttachment != null)
            {
                _logger.LogInformation("Successfully uploaded attachment: {FileName}", fileName);
                return response.UploadMessageAttachment;
            }

            _logger.LogWarning("Failed to upload attachment");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment: {FileName}", fileName);
            return new UploadAttachmentResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<MessageReactionResponseDto?> AddReactionAsync(Guid messageId, string emoji)
    {
        try
        {
            const string mutation = @"
                mutation AddMessageReaction($messageId: UUID!, $emoji: String!) {
                  addMessageReaction(messageId: $messageId, emoji: $emoji) {
                    success
                    reactions {
                      emoji
                      count
                    }
                    errorMessage
                  }
                }
                ";

            var variables = new { messageId, emoji };
            var response = await _graphQLService.SendQueryAsync<AddMessageReactionResponse>(mutation, variables);

            if (response?.AddMessageReaction != null)
            {
                _logger.LogInformation("Successfully added reaction {Emoji} to message {MessageId}", emoji, messageId);
                return response.AddMessageReaction;
            }

            _logger.LogWarning("Failed to add reaction");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message {MessageId}", messageId);
            return new MessageReactionResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<MessageReactionResponseDto?> RemoveReactionAsync(Guid messageId)
    {
        try
        {
            const string mutation = @"
                mutation RemoveMessageReaction($messageId: UUID!) {
                  removeMessageReaction(messageId: $messageId) {
                    success
                    reactions {
                      emoji
                      count
                    }
                    errorMessage
                  }
                }
                ";

            var variables = new { messageId };
            var response = await _graphQLService.SendQueryAsync<RemoveMessageReactionResponse>(mutation, variables);

            if (response?.RemoveMessageReaction != null)
            {
                _logger.LogInformation("Successfully removed reaction from message {MessageId}", messageId);
                return response.RemoveMessageReaction;
            }

            _logger.LogWarning("Failed to remove reaction");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from message {MessageId}", messageId);
            return new MessageReactionResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<SearchMessagesResponseDto?> SearchMessagesAsync(SearchMessagesInputDto input)
    {
        try
        {
            const string query = @"
                query SearchMessages($input: SearchMessagesInputInput!) {
                  searchMessages(input: $input) {
                    results {
                      id
                      senderId
                      senderName
                      receiverId
                      receiverName
                      content
                      sentAt
                      messageType
                      attachmentUrl
                      attachmentFileName
                      isSender
                      conversationId
                      matchedText
                    }
                    totalCount
                    pageNumber
                    pageSize
                    totalPages
                  }
                }";

            var variables = new { input };
            var response = await _graphQLService.SendQueryAsync<SearchMessagesQueryResponse>(query, variables);

            if (response?.SearchMessages != null)
            {
                _logger.LogInformation("Search returned {Count} results for query: {Query}",
                    response.SearchMessages.TotalCount, input.SearchQuery);
                return response.SearchMessages;
            }

            _logger.LogWarning("No search results received");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching messages with query: {Query}", input.SearchQuery);
            return new SearchMessagesResponseDto
            {
                Results = new List<MessageSearchResultDto>(),
                TotalCount = 0,
                PageNumber = input.PageNumber,
                PageSize = input.PageSize,
                TotalPages = 0
            };
        }
    }

    public async Task<EditMessageResponseDto?> EditMessageAsync(Guid messageId, string newContent)
    {
        try
        {
            const string mutation = @"
                mutation EditMessage($messageId: UUID!, $newContent: String!) {
                  editMessage(messageId: $messageId, newContent: $newContent) {
                    success
                    messageId
                    newContent
                    editedAt
                    errorMessage
                  }
                }";

            var variables = new { messageId, newContent };
            var response = await _graphQLService.SendQueryAsync<EditMessageMutationResponse>(mutation, variables);

            if (response?.EditMessage != null)
            {
                _logger.LogInformation("Successfully edited message {MessageId}", messageId);
                return response.EditMessage;
            }

            _logger.LogWarning("Failed to edit message");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing message {MessageId}", messageId);
            return new EditMessageResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<DeleteMessageResponseDto?> DeleteMessageAsync(Guid messageId)
    {
        try
        {
            const string mutation = @"
                mutation DeleteMessage($messageId: UUID!) {
                  deleteMessage(messageId: $messageId) {
                    success
                    messageId
                    deletedAt
                    errorMessage
                  }
                }";

            var variables = new { messageId };
            var response = await _graphQLService.SendQueryAsync<DeleteMessageMutationResponse>(mutation, variables);

            if (response?.DeleteMessage != null)
            {
                _logger.LogInformation("Successfully deleted message {MessageId}", messageId);
                return response.DeleteMessage;
            }

            _logger.LogWarning("Failed to delete message");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
            return new DeleteMessageResponseDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private record UploadAttachmentMutationResponse(UploadAttachmentResponseDto UploadMessageAttachment);
}

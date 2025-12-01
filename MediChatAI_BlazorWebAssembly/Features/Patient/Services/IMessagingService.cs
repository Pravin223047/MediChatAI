using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IMessagingService
{
    Task<List<ConversationDto>> GetConversationsAsync();
    Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId);
    Task<MessageDto?> SendMessageAsync(SendMessageInput input);
    Task<bool> MarkMessagesAsReadAsync(Guid conversationId);
    Task<int> GetUnreadMessageCountAsync();
    Task<UploadAttachmentResponseDto?> UploadAttachmentAsync(string fileBase64, string fileName, string mimeType);
    Task<MessageReactionResponseDto?> AddReactionAsync(Guid messageId, string emoji);
    Task<MessageReactionResponseDto?> RemoveReactionAsync(Guid messageId);
    Task<SearchMessagesResponseDto?> SearchMessagesAsync(SearchMessagesInputDto input);
    Task<EditMessageResponseDto?> EditMessageAsync(Guid messageId, string newContent);
    Task<DeleteMessageResponseDto?> DeleteMessageAsync(Guid messageId);
}

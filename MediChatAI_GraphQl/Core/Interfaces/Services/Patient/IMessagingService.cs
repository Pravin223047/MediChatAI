using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Features.Patient.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Patient;

public interface IMessagingService
{
    Task<List<ConversationDto>> GetUserConversationsAsync(string userId);
    Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId, string userId);
    Task<DoctorMessage> SendMessageAsync(string senderId, string receiverId, string content, string? attachmentUrl = null);
    Task<bool> MarkMessagesAsReadAsync(Guid conversationId, string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task<SearchMessagesResponse> SearchMessagesAsync(string userId, SearchMessagesInput input);
}

using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Features.Doctor.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;

public interface IDoctorChatService
{
    Task<DoctorMessage> SendMessageAsync(SendMessageInput input);
    Task<IEnumerable<DoctorMessage>> GetConversationAsync(string senderId, string receiverId, int limit = 50);
    Task<IEnumerable<ConversationSummary>> GetConversationsAsync(string userId);
    Task<bool> MarkAsReadAsync(Guid messageId);
    Task<int> GetUnreadCountAsync(string userId);
}

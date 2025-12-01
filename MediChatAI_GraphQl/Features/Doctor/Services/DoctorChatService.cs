using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Features.Doctor.Services;

public class DoctorChatService : IDoctorChatService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DoctorChatService> _logger;

    public DoctorChatService(ApplicationDbContext context, ILogger<DoctorChatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DoctorMessage> SendMessageAsync(SendMessageInput input)
    {
        var message = new DoctorMessage
        {
            SenderId = input.SenderId,
            ReceiverId = input.ReceiverId,
            Content = input.Content,
            MessageType = input.MessageType,
            AttachmentUrl = input.AttachmentUrl,
            AttachmentFileName = input.AttachmentFileName,
            ReplyToMessageId = input.ReplyToMessageId,
            ConversationId = input.ConversationId ?? Guid.NewGuid(),
            Status = MessageStatus.Sent,
            SentAt = DateTime.UtcNow
        };

        _context.DoctorMessages.Add(message);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Message sent from {SenderId} to {ReceiverId}", input.SenderId, input.ReceiverId);

        return message;
    }

    public async Task<IEnumerable<DoctorMessage>> GetConversationAsync(string senderId, string receiverId, int limit = 50)
    {
        return await _context.DoctorMessages
            .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                       (m.SenderId == receiverId && m.ReceiverId == senderId))
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Take(limit)
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .ToListAsync();
    }

    public async Task<IEnumerable<ConversationSummary>> GetConversationsAsync(string userId)
    {
        // Get all unique conversation partners
        var sentTo = await _context.DoctorMessages
            .Where(m => m.SenderId == userId && !m.IsDeleted)
            .Select(m => m.ReceiverId)
            .Distinct()
            .ToListAsync();

        var receivedFrom = await _context.DoctorMessages
            .Where(m => m.ReceiverId == userId && !m.IsDeleted)
            .Select(m => m.SenderId)
            .Distinct()
            .ToListAsync();

        var allPartners = sentTo.Union(receivedFrom).Distinct();

        var conversations = new List<ConversationSummary>();

        foreach (var partnerId in allPartners)
        {
            var lastMessage = await _context.DoctorMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == partnerId) ||
                           (m.SenderId == partnerId && m.ReceiverId == userId))
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            if (lastMessage == null) continue;

            var unreadCount = await _context.DoctorMessages
                .CountAsync(m => m.SenderId == partnerId &&
                               m.ReceiverId == userId &&
                               m.Status != MessageStatus.Read &&
                               !m.IsDeleted);

            var partner = await _context.Users.FindAsync(partnerId);
            if (partner == null) continue;

            conversations.Add(new ConversationSummary(
                lastMessage.ConversationId ?? Guid.NewGuid(),
                partnerId,
                $"{partner.FirstName} {partner.LastName}",
                "Patient", // Partner role
                partner.ProfileImage,
                lastMessage.Content,
                lastMessage.SentAt,
                unreadCount,
                false, // TODO: Implement online status tracking
                lastMessage.ConversationId
            ));
        }

        return conversations.OrderByDescending(c => c.LastMessageTime);
    }

    public async Task<bool> MarkAsReadAsync(Guid messageId)
    {
        var message = await _context.DoctorMessages.FindAsync(messageId);
        if (message == null) return false;

        message.Status = MessageStatus.Read;
        message.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.DoctorMessages
            .CountAsync(m => m.ReceiverId == userId &&
                           m.Status != MessageStatus.Read &&
                           !m.IsDeleted);
    }
}

using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Patient;
using MediChatAI_GraphQl.Features.Patient.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Features.Patient.Services;

public class MessagingService : IMessagingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MessagingService> _logger;

    public MessagingService(ApplicationDbContext context, ILogger<MessagingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<ConversationDto>> GetUserConversationsAsync(string userId)
    {
        // Get all unique conversation partners
        var sentMessages = _context.DoctorMessages
            .Where(m => m.SenderId == userId && !m.IsDeleted)
            .Select(m => m.ReceiverId)
            .Distinct();

        var receivedMessages = _context.DoctorMessages
            .Where(m => m.ReceiverId == userId && !m.IsDeleted)
            .Select(m => m.SenderId)
            .Distinct();

        var conversationPartnerIds = await sentMessages.Union(receivedMessages).ToListAsync();

        var conversations = new List<ConversationDto>();

        // Get user roles for all partners in one query
        var partnerRoles = await _context.UserRoles
            .Where(ur => conversationPartnerIds.Contains(ur.UserId))
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, RoleName = r.Name })
            .ToListAsync();

        var roleMap = partnerRoles.ToDictionary(pr => pr.UserId, pr => pr.RoleName ?? "User");

        foreach (var partnerId in conversationPartnerIds)
        {
            var partner = await _context.Users.FindAsync(partnerId);
            if (partner == null) continue;

            var lastMessage = await _context.DoctorMessages
                .Where(m => (m.SenderId == userId && m.ReceiverId == partnerId) ||
                           (m.SenderId == partnerId && m.ReceiverId == userId))
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefaultAsync();

            var unreadCount = await _context.DoctorMessages
                .CountAsync(m => m.SenderId == partnerId &&
                               m.ReceiverId == userId &&
                               m.Status != MessageStatus.Read &&
                               !m.IsDeleted);

            // Generate deterministic conversation ID
            var conversationId = GenerateConversationId(userId, partnerId);
            
            // Get partner role
            var partnerRole = roleMap.ContainsKey(partnerId) ? roleMap[partnerId] : "User";

            conversations.Add(new ConversationDto
            {
                Id = conversationId,
                PartnerId = partnerId,
                PartnerName = $"{partner.FirstName} {partner.LastName}",
                PartnerRole = partnerRole,
                PartnerProfileImage = partner.ProfileImage,
                LastMessage = lastMessage?.Content ?? "",
                LastMessageTime = lastMessage?.SentAt ?? DateTime.UtcNow,
                UnreadCount = unreadCount,
                IsOnline = false // Can be enhanced with SignalR presence
            });
        }

        return conversations.OrderByDescending(c => c.LastMessageTime).ToList();
    }

    public async Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId, string userId)
    {
        // Find the partner ID from the conversation ID
        var conversations = await GetUserConversationsAsync(userId);
        var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
        
        if (conversation == null) 
        {
            _logger.LogWarning("Conversation {ConversationId} not found for user {UserId}", conversationId, userId);
            return new List<MessageDto>();
        }

        var partnerId = conversation.PartnerId;

        var messages = await _context.DoctorMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => ((m.SenderId == userId && m.ReceiverId == partnerId) ||
                        (m.SenderId == partnerId && m.ReceiverId == userId)) &&
                       !m.IsDeleted)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                SentAt = m.SentAt,
                ReadAt = m.ReadAt,
                Status = m.Status.ToString(),
                MessageType = m.MessageType.ToString(),
                AttachmentUrl = m.AttachmentUrl,
                AttachmentFileName = m.AttachmentFileName,
                IsSender = m.SenderId == userId
            })
            .ToListAsync();

        return messages;
    }

    public async Task<DoctorMessage> SendMessageAsync(string senderId, string receiverId, string content, string? attachmentUrl = null)
    {
        var message = new DoctorMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            AttachmentUrl = attachmentUrl,
            MessageType = string.IsNullOrEmpty(attachmentUrl) ? MessageType.Text : MessageType.File,
            Status = MessageStatus.Sent,
            SentAt = DateTime.UtcNow
        };

        _context.DoctorMessages.Add(message);
        await _context.SaveChangesAsync();

        return message;
    }

    public async Task<bool> MarkMessagesAsReadAsync(Guid conversationId, string userId)
    {
        // Find the partner ID from the conversation ID
        var conversations = await GetUserConversationsAsync(userId);
        var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
        
        if (conversation == null) return false;

        var partnerId = conversation.PartnerId;

        var unreadMessages = await _context.DoctorMessages
            .Where(m => m.SenderId == partnerId &&
                       m.ReceiverId == userId &&
                       m.Status != MessageStatus.Read &&
                       !m.IsDeleted)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.Status = MessageStatus.Read;
            message.ReadAt = DateTime.UtcNow;
        }

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

    public async Task<SearchMessagesResponse> SearchMessagesAsync(string userId, SearchMessagesInput input)
    {
        // Start with base query - messages where user is sender or receiver
        var query = _context.DoctorMessages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Where(m => (m.SenderId == userId || m.ReceiverId == userId) && !m.IsDeleted);

        // Apply search query filter (search in content and attachment filenames)
        if (!string.IsNullOrWhiteSpace(input.SearchQuery))
        {
            var searchTerm = input.SearchQuery.ToLower();
            query = query.Where(m =>
                m.Content.ToLower().Contains(searchTerm) ||
                (m.AttachmentFileName != null && m.AttachmentFileName.ToLower().Contains(searchTerm))
            );
        }

        // Apply date range filters
        if (input.StartDate.HasValue)
        {
            query = query.Where(m => m.SentAt >= input.StartDate.Value);
        }

        if (input.EndDate.HasValue)
        {
            var endOfDay = input.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(m => m.SentAt <= endOfDay);
        }

        // Apply message type filter
        if (!string.IsNullOrWhiteSpace(input.MessageType) && input.MessageType != "All")
        {
            if (Enum.TryParse<MessageType>(input.MessageType, out var messageType))
            {
                query = query.Where(m => m.MessageType == messageType);
            }
        }

        // Apply partner filter (search in specific conversation)
        if (!string.IsNullOrWhiteSpace(input.PartnerId))
        {
            query = query.Where(m =>
                (m.SenderId == userId && m.ReceiverId == input.PartnerId) ||
                (m.SenderId == input.PartnerId && m.ReceiverId == userId)
            );
        }

        // Apply attachment filter
        if (input.HasAttachment)
        {
            query = query.Where(m => !string.IsNullOrEmpty(m.AttachmentUrl));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Calculate total pages
        var totalPages = (int)Math.Ceiling(totalCount / (double)input.PageSize);

        // Apply pagination and order by most recent first
        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .Skip((input.PageNumber - 1) * input.PageSize)
            .Take(input.PageSize)
            .ToListAsync();

        // Map to search result DTOs
        var results = messages.Select(m =>
        {
            var isSender = m.SenderId == userId;
            var conversationId = GenerateConversationId(m.SenderId, m.ReceiverId);

            // Extract matched text context (50 chars before and after match)
            var matchedText = GetMatchedTextContext(m.Content, input.SearchQuery);

            return new MessageSearchResultDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderName = $"{m.Sender?.FirstName} {m.Sender?.LastName}",
                ReceiverId = m.ReceiverId,
                ReceiverName = $"{m.Receiver?.FirstName} {m.Receiver?.LastName}",
                Content = m.Content,
                SentAt = m.SentAt,
                MessageType = m.MessageType.ToString(),
                AttachmentUrl = m.AttachmentUrl,
                AttachmentFileName = m.AttachmentFileName,
                IsSender = isSender,
                ConversationId = conversationId,
                MatchedText = matchedText
            };
        }).ToList();

        return new SearchMessagesResponse
        {
            Results = results,
            TotalCount = totalCount,
            PageNumber = input.PageNumber,
            PageSize = input.PageSize,
            TotalPages = totalPages
        };
    }

    private Guid GenerateConversationId(string userId1, string userId2)
    {
        // Generate deterministic conversation ID from two user IDs
        var sortedIds = new[] { userId1, userId2 }.OrderBy(id => id).ToArray();
        var combined = $"{sortedIds[0]}_{sortedIds[1]}";
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
            return new Guid(hash);
        }
    }

    private string GetMatchedTextContext(string content, string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery) || string.IsNullOrWhiteSpace(content))
            return content;

        var index = content.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase);
        if (index == -1)
            return content; // No match found (might be in attachment name)

        // Extract context: 50 chars before and after the match
        var contextLength = 50;
        var start = Math.Max(0, index - contextLength);
        var length = Math.Min(content.Length - start, contextLength * 2 + searchQuery.Length);

        var context = content.Substring(start, length);

        // Add ellipsis if truncated
        if (start > 0) context = "..." + context;
        if (start + length < content.Length) context = context + "...";

        return context;
    }
}

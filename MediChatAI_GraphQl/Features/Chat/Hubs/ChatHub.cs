using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace MediChatAI_GraphQl.Features.Chat.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChatHub> _logger;

    // Track online users and their connection IDs
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = new();

    // Track typing status
    private static readonly ConcurrentDictionary<string, HashSet<string>> TypingUsers = new();

    public ChatHub(ApplicationDbContext context, ILogger<ChatHub> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Add connection
            UserConnections.AddOrUpdate(
                userId,
                new HashSet<string> { Context.ConnectionId },
                (key, existing) =>
                {
                    existing.Add(Context.ConnectionId);
                    return existing;
                });

            _logger.LogInformation($"User {userId} connected with connection {Context.ConnectionId}");

            // Notify others that user is online
            await Clients.Others.SendAsync("UserOnline", userId);

            // Send online status to the connected user for all their contacts
            await SendOnlineUsersToClient(userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            if (UserConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(Context.ConnectionId);

                // If no more connections, user is offline
                if (connections.Count == 0)
                {
                    UserConnections.TryRemove(userId, out _);
                    await Clients.Others.SendAsync("UserOffline", userId);
                    _logger.LogInformation($"User {userId} is now offline");
                }
            }

            // Remove from typing status
            RemoveFromTypingStatus(userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid messageId, string receiverId, string content, string messageType)
    {
        var senderId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(senderId))
        {
            _logger.LogWarning("Unauthorized message send attempt");
            return;
        }

        try
        {
            // Get the message from database to ensure it exists
            var message = await _context.DoctorMessages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                _logger.LogWarning($"Message {messageId} not found");
                return;
            }

            // Send to receiver if they're online
            if (UserConnections.TryGetValue(receiverId, out var connections))
            {
                foreach (var connectionId in connections)
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveMessage", new
                    {
                        id = message.Id,
                        senderId = message.SenderId,
                        senderName = message.Sender != null ? $"{message.Sender.FirstName} {message.Sender.LastName}" : "Unknown",
                        senderProfileImage = message.Sender?.ProfileImage,
                        receiverId = message.ReceiverId,
                        content = message.Content,
                        messageType = message.MessageType.ToString(),
                        status = message.Status.ToString(),
                        sentAt = message.SentAt,
                        conversationId = message.ConversationId,
                        attachmentUrl = message.AttachmentUrl,
                        attachmentFileName = message.AttachmentFileName,
                        attachmentFileSize = message.AttachmentFileSize,
                        attachmentMimeType = message.AttachmentMimeType,
                        replyToMessageId = message.ReplyToMessageId
                    });
                }

                // Update message status to Delivered
                message.Status = MessageStatus.Delivered;
                message.DeliveredAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Notify sender about delivery
                await Clients.Caller.SendAsync("MessageStatusChanged", messageId, "Delivered", DateTime.UtcNow);
            }

            // Stop typing indicator since message was sent
            await StopTyping(receiverId);

            _logger.LogInformation($"Message {messageId} sent from {senderId} to {receiverId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending message {messageId}");
            throw;
        }
    }

    public async Task UserTyping(string receiverId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(receiverId))
            return;

        // Add to typing status
        var typingKey = $"{receiverId}";
        TypingUsers.AddOrUpdate(
            typingKey,
            new HashSet<string> { userId },
            (key, existing) =>
            {
                existing.Add(userId);
                return existing;
            });

        // Notify receiver
        if (UserConnections.TryGetValue(receiverId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("UserTyping", userId);
            }
        }
    }

    public async Task StopTyping(string receiverId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(receiverId))
            return;

        RemoveFromTypingStatus(userId);

        // Notify receiver
        if (UserConnections.TryGetValue(receiverId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("UserStoppedTyping", userId);
            }
        }
    }

    public async Task MarkMessageAsRead(Guid messageId, string senderId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        try
        {
            var message = await _context.DoctorMessages.FindAsync(messageId);

            if (message != null && message.ReceiverId == userId)
            {
                message.Status = MessageStatus.Read;
                message.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Notify sender about read status
                if (UserConnections.TryGetValue(senderId, out var connections))
                {
                    foreach (var connectionId in connections)
                    {
                        await Clients.Client(connectionId).SendAsync("MessageStatusChanged", messageId, "Read", DateTime.UtcNow);
                    }
                }

                _logger.LogInformation($"Message {messageId} marked as read by {userId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking message {messageId} as read");
        }
    }

    public async Task MarkMessagesAsRead(Guid conversationId, string senderId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        try
        {
            var messages = await _context.DoctorMessages
                .Where(m => m.ConversationId == conversationId &&
                           m.ReceiverId == userId &&
                           m.Status != MessageStatus.Read)
                .ToListAsync();

            var readTime = DateTime.UtcNow;

            foreach (var message in messages)
            {
                message.Status = MessageStatus.Read;
                message.ReadAt = readTime;
            }

            await _context.SaveChangesAsync();

            // Notify sender about read status for all messages
            if (UserConnections.TryGetValue(senderId, out var connections))
            {
                foreach (var connectionId in connections)
                {
                    await Clients.Client(connectionId).SendAsync("ConversationMessagesRead", conversationId, readTime);
                }
            }

            _logger.LogInformation($"Marked {messages.Count} messages as read in conversation {conversationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error marking messages as read for conversation {conversationId}");
        }
    }

    public async Task BroadcastReaction(Guid messageId, string emoji, List<object> reactions, string receiverId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        try
        {
            // Notify the receiver if they're online
            if (UserConnections.TryGetValue(receiverId, out var connections))
            {
                foreach (var connectionId in connections)
                {
                    await Clients.Client(connectionId).SendAsync("MessageReactionUpdated", messageId, reactions);
                }
            }

            // Also notify the sender (for multi-device support)
            if (UserConnections.TryGetValue(userId, out var senderConnections))
            {
                foreach (var connectionId in senderConnections)
                {
                    if (connectionId != Context.ConnectionId) // Don't send to the same connection
                    {
                        await Clients.Client(connectionId).SendAsync("MessageReactionUpdated", messageId, reactions);
                    }
                }
            }

            _logger.LogInformation($"Broadcasted reaction update for message {messageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting reaction for message {messageId}");
        }
    }

    public async Task BroadcastMessageEdited(Guid messageId, string newContent, DateTime editedAt, string receiverId)
    {
        try
        {
            // Notify receiver if online
            if (UserConnections.TryGetValue(receiverId, out var receiverConnections))
            {
                foreach (var connectionId in receiverConnections)
                {
                    await Clients.Client(connectionId).SendAsync("MessageEdited", messageId, newContent, editedAt);
                }
            }

            // Notify sender's other devices (if any)
            var senderId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(senderId) && UserConnections.TryGetValue(senderId, out var senderConnections))
            {
                foreach (var connectionId in senderConnections.Where(c => c != Context.ConnectionId))
                {
                    await Clients.Client(connectionId).SendAsync("MessageEdited", messageId, newContent, editedAt);
                }
            }

            _logger.LogInformation($"Broadcasted message edit for message {messageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting message edit for message {messageId}");
        }
    }

    public async Task BroadcastMessageDeleted(Guid messageId, DateTime deletedAt, string receiverId)
    {
        try
        {
            // Notify receiver if online
            if (UserConnections.TryGetValue(receiverId, out var receiverConnections))
            {
                foreach (var connectionId in receiverConnections)
                {
                    await Clients.Client(connectionId).SendAsync("MessageDeleted", messageId, deletedAt);
                }
            }

            // Notify sender's other devices (if any)
            var senderId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(senderId) && UserConnections.TryGetValue(senderId, out var senderConnections))
            {
                foreach (var connectionId in senderConnections.Where(c => c != Context.ConnectionId))
                {
                    await Clients.Client(connectionId).SendAsync("MessageDeleted", messageId, deletedAt);
                }
            }

            _logger.LogInformation($"Broadcasted message deletion for message {messageId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting message deletion for message {messageId}");
        }
    }

    public async Task<List<string>> GetOnlineUsers()
    {
        return await Task.FromResult(UserConnections.Keys.ToList());
    }

    public bool IsUserOnline(string userId)
    {
        return UserConnections.ContainsKey(userId);
    }

    // Private helper methods

    private async Task SendOnlineUsersToClient(string userId)
    {
        try
        {
            // Get all users this person has conversations with
            var conversationPartners = await _context.DoctorMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            // Check which ones are online
            var onlinePartners = conversationPartners
                .Where(partnerId => UserConnections.ContainsKey(partnerId!))
                .ToList();

            // Send to caller
            await Clients.Caller.SendAsync("OnlineUsers", onlinePartners);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending online users to client");
        }
    }

    private void RemoveFromTypingStatus(string userId)
    {
        foreach (var key in TypingUsers.Keys)
        {
            if (TypingUsers.TryGetValue(key, out var typingSet))
            {
                typingSet.Remove(userId);

                if (typingSet.Count == 0)
                {
                    TypingUsers.TryRemove(key, out _);
                }
            }
        }
    }

    // WebRTC Signaling Methods

    public async Task InitiateCall(string receiverId, bool isVideoCall)
    {
        var callerId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(callerId) || string.IsNullOrEmpty(receiverId))
            return;

        // Notify receiver about incoming call
        if (UserConnections.TryGetValue(receiverId, out var connections))
        {
            var caller = await _context.Users.FindAsync(callerId);
            var callerName = caller != null ? $"{caller.FirstName} {caller.LastName}" : "Unknown";

            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("IncomingCall",
                    callerId,
                    callerName,
                    isVideoCall);
            }

            _logger.LogInformation($"Call initiated from {callerId} to {receiverId}, video: {isVideoCall}");
        }
        else
        {
            // Receiver is offline
            await Clients.Caller.SendAsync("CallFailed", "User is offline");
        }
    }

    public async Task SendCallOffer(string receiverId, string offerJson)
    {
        var senderId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(receiverId))
            return;

        if (UserConnections.TryGetValue(receiverId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveCallOffer", senderId, offerJson);
            }
            _logger.LogInformation($"Call offer sent from {senderId} to {receiverId}");
        }
    }

    public async Task SendCallAnswer(string callerId, string answerJson)
    {
        var responderId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(responderId) || string.IsNullOrEmpty(callerId))
            return;

        if (UserConnections.TryGetValue(callerId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveCallAnswer", responderId, answerJson);
            }
            _logger.LogInformation($"Call answer sent from {responderId} to {callerId}");
        }
    }

    public async Task SendIceCandidate(string peerId, string candidateJson)
    {
        var senderId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(senderId) || string.IsNullOrEmpty(peerId))
            return;

        if (UserConnections.TryGetValue(peerId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveIceCandidate", senderId, candidateJson);
            }
        }
    }

    public async Task AcceptCall(string callerId)
    {
        var receiverId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(receiverId))
        {
            _logger.LogWarning("AcceptCall: receiverId is null or empty");
            return;
        }

        if (string.IsNullOrEmpty(callerId))
        {
            _logger.LogWarning("AcceptCall: callerId is null or empty");
            return;
        }

        if (UserConnections.TryGetValue(callerId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("CallAccepted", receiverId);
            }
            _logger.LogInformation($"Call accepted by {receiverId}, caller: {callerId}");
        }
        else
        {
            _logger.LogWarning($"AcceptCall: Caller {callerId} not found in connections");
        }
    }

    public async Task RejectCall(string callerId, string reason)
    {
        var receiverId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(receiverId) || string.IsNullOrEmpty(callerId))
            return;

        if (UserConnections.TryGetValue(callerId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("CallRejected", receiverId, reason);
            }
            _logger.LogInformation($"Call rejected by {receiverId}, caller: {callerId}");
        }
    }

    public async Task EndCall(string peerId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(peerId))
            return;

        if (UserConnections.TryGetValue(peerId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("CallEnded", userId);
            }
            _logger.LogInformation($"Call ended by {userId}, peer: {peerId}");
        }
    }

    // Group Chat Methods
    public async Task JoinGroupChat(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
        _logger.LogInformation($"User {Context.ConnectionId} joined group {groupId}");
    }

    public async Task LeaveGroupChat(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");
        _logger.LogInformation($"User {Context.ConnectionId} left group {groupId}");
    }

    public async Task BroadcastGroupMessage(string groupId, string messageId, string senderId, string senderName, string content, string messageType, DateTime sentAt)
    {
        await Clients.Group($"group_{groupId}").SendAsync("ReceiveGroupMessage", new
        {
            Id = messageId,
            GroupConversationId = groupId,
            SenderId = senderId,
            SenderName = senderName,
            Content = content,
            MessageType = messageType,
            SentAt = sentAt
        });
    }

    public async Task BroadcastGroupMemberJoined(string groupId, string userId, string userName)
    {
        await Clients.Group($"group_{groupId}").SendAsync("GroupMemberJoined", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName,
            JoinedAt = DateTime.UtcNow
        });
    }

    public async Task BroadcastGroupMemberLeft(string groupId, string userId, string userName)
    {
        await Clients.Group($"group_{groupId}").SendAsync("GroupMemberLeft", new
        {
            GroupId = groupId,
            UserId = userId,
            UserName = userName,
            LeftAt = DateTime.UtcNow
        });
    }

    public async Task MarkGroupMessageAsRead(string groupId, string messageId, string userId)
    {
        await Clients.Group($"group_{groupId}").SendAsync("GroupMessageRead", new
        {
            MessageId = messageId,
            UserId = userId,
            ReadAt = DateTime.UtcNow
        });
    }

    // ============================================
    // CONSULTATION ROOM METHODS
    // ============================================

    /// <summary>
    /// Join a consultation room
    /// </summary>
    public async Task JoinConsultationRoom(string roomId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("JoinConsultationRoom: userId is null");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"consultation_{roomId}");

        // Get user details
        var user = await _context.Users.FindAsync(userId);
        var userName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";

        // Notify all participants that a new user joined
        await Clients.Group($"consultation_{roomId}").SendAsync("ParticipantJoined", new
        {
            UserId = userId,
            UserName = userName,
            ProfileImage = user?.ProfileImage,
            JoinedAt = DateTime.UtcNow
        });

        _logger.LogInformation($"User {userId} joined consultation room {roomId}");
    }

    /// <summary>
    /// Leave a consultation room
    /// </summary>
    public async Task LeaveConsultationRoom(string roomId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"consultation_{roomId}");

        // Get user details
        var user = await _context.Users.FindAsync(userId);
        var userName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";

        // Notify all participants
        await Clients.Group($"consultation_{roomId}").SendAsync("ParticipantLeft", new
        {
            UserId = userId,
            UserName = userName,
            LeftAt = DateTime.UtcNow
        });

        _logger.LogInformation($"User {userId} left consultation room {roomId}");
    }

    /// <summary>
    /// Send WebRTC offer to all participants in consultation room
    /// </summary>
    public async Task SendConsultationOffer(string roomId, string targetUserId, string offerJson)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        if (UserConnections.TryGetValue(targetUserId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveConsultationOffer", userId, offerJson);
            }
            _logger.LogInformation($"Consultation offer sent from {userId} to {targetUserId} in room {roomId}");
        }
    }

    /// <summary>
    /// Send WebRTC answer in consultation room
    /// </summary>
    public async Task SendConsultationAnswer(string roomId, string targetUserId, string answerJson)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        if (UserConnections.TryGetValue(targetUserId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveConsultationAnswer", userId, answerJson);
            }
            _logger.LogInformation($"Consultation answer sent from {userId} to {targetUserId} in room {roomId}");
        }
    }

    /// <summary>
    /// Send ICE candidate for consultation peer connection
    /// </summary>
    public async Task SendConsultationIceCandidate(string roomId, string targetUserId, string candidateJson)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        if (UserConnections.TryGetValue(targetUserId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveConsultationIceCandidate", userId, candidateJson);
            }
        }
    }

    /// <summary>
    /// Toggle participant audio status
    /// </summary>
    public async Task ToggleParticipantAudio(string roomId, bool isAudioEnabled)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        await Clients.Group($"consultation_{roomId}").SendAsync("ParticipantAudioToggled", new
        {
            UserId = userId,
            IsAudioEnabled = isAudioEnabled,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation($"User {userId} toggled audio to {isAudioEnabled} in room {roomId}");
    }

    /// <summary>
    /// Toggle participant video status
    /// </summary>
    public async Task ToggleParticipantVideo(string roomId, bool isVideoEnabled)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        await Clients.Group($"consultation_{roomId}").SendAsync("ParticipantVideoToggled", new
        {
            UserId = userId,
            IsVideoEnabled = isVideoEnabled,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation($"User {userId} toggled video to {isVideoEnabled} in room {roomId}");
    }

    /// <summary>
    /// Start screen sharing
    /// </summary>
    public async Task StartScreenShare(string roomId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        var user = await _context.Users.FindAsync(userId);
        var userName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";

        await Clients.Group($"consultation_{roomId}").SendAsync("ScreenShareStarted", new
        {
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation($"User {userId} started screen sharing in room {roomId}");
    }

    /// <summary>
    /// Stop screen sharing
    /// </summary>
    public async Task StopScreenShare(string roomId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        await Clients.Group($"consultation_{roomId}").SendAsync("ScreenShareStopped", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation($"User {userId} stopped screen sharing in room {roomId}");
    }

    /// <summary>
    /// Send screen share offer to specific participant
    /// </summary>
    public async Task SendScreenShareOffer(string roomId, string targetUserId, string offerJson)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        if (UserConnections.TryGetValue(targetUserId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveScreenShareOffer", userId, offerJson);
            }
            _logger.LogInformation($"Screen share offer sent from {userId} to {targetUserId} in room {roomId}");
        }
    }

    /// <summary>
    /// Send screen share answer
    /// </summary>
    public async Task SendScreenShareAnswer(string roomId, string targetUserId, string answerJson)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        if (UserConnections.TryGetValue(targetUserId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveScreenShareAnswer", userId, answerJson);
            }
        }
    }

    /// <summary>
    /// Send screen share ICE candidate
    /// </summary>
    public async Task SendScreenShareIceCandidate(string roomId, string targetUserId, string candidateJson)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        if (UserConnections.TryGetValue(targetUserId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveScreenShareIceCandidate", userId, candidateJson);
            }
        }
    }

    /// <summary>
    /// Broadcast recording started
    /// </summary>
    public async Task BroadcastRecordingStarted(string roomId, int recordingId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        await Clients.Group($"consultation_{roomId}").SendAsync("RecordingStarted", new
        {
            RecordingId = recordingId,
            StartedByUserId = userId,
            StartedAt = DateTime.UtcNow
        });

        _logger.LogInformation($"Recording {recordingId} started in consultation room {roomId}");
    }

    /// <summary>
    /// Broadcast recording stopped
    /// </summary>
    public async Task BroadcastRecordingStopped(string roomId, int recordingId)
    {
        await Clients.Group($"consultation_{roomId}").SendAsync("RecordingStopped", new
        {
            RecordingId = recordingId,
            StoppedAt = DateTime.UtcNow
        });

        _logger.LogInformation($"Recording {recordingId} stopped in consultation room {roomId}");
    }

    /// <summary>
    /// Broadcast consultation notes updated
    /// </summary>
    public async Task BroadcastNotesUpdated(string roomId, string noteType, string content)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        await Clients.Group($"consultation_{roomId}").SendAsync("ConsultationNotesUpdated", new
        {
            UpdatedByUserId = userId,
            NoteType = noteType,
            Content = content,
            UpdatedAt = DateTime.UtcNow
        });

        _logger.LogInformation($"Consultation notes updated in room {roomId} by {userId}");
    }

    /// <summary>
    /// Broadcast consultation status change
    /// </summary>
    public async Task BroadcastConsultationStatusChanged(string roomId, string status)
    {
        await Clients.Group($"consultation_{roomId}").SendAsync("ConsultationStatusChanged", new
        {
            Status = status,
            ChangedAt = DateTime.UtcNow
        });

        _logger.LogInformation($"Consultation status changed to {status} in room {roomId}");
    }

    /// <summary>
    /// Request all participants to send their current media state
    /// </summary>
    public async Task RequestParticipantsStatus(string roomId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        await Clients.OthersInGroup($"consultation_{roomId}").SendAsync("ParticipantStatusRequested", userId);
    }

    /// <summary>
    /// Send participant status to requester
    /// </summary>
    public async Task SendParticipantStatus(string roomId, string requesterId, bool isAudioEnabled, bool isVideoEnabled, bool isScreenSharing)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        if (UserConnections.TryGetValue(requesterId, out var connections))
        {
            foreach (var connectionId in connections)
            {
                await Clients.Client(connectionId).SendAsync("ParticipantStatusUpdate", new
                {
                    UserId = userId,
                    IsAudioEnabled = isAudioEnabled,
                    IsVideoEnabled = isVideoEnabled,
                    IsScreenSharing = isScreenSharing
                });
            }
        }
    }

    /// <summary>
    /// Broadcast participant removed from consultation
    /// </summary>
    public async Task BroadcastParticipantRemoved(string roomId, string removedUserId, string reason)
    {
        await Clients.Group($"consultation_{roomId}").SendAsync("ParticipantRemoved", new
        {
            UserId = removedUserId,
            Reason = reason,
            RemovedAt = DateTime.UtcNow
        });

        _logger.LogInformation($"Participant {removedUserId} removed from room {roomId}");
    }

    /// <summary>
    /// Send chat message in consultation room
    /// </summary>
    public async Task SendConsultationChatMessage(string roomId, string content)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        var user = await _context.Users.FindAsync(userId);
        var userName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";

        await Clients.Group($"consultation_{roomId}").SendAsync("ConsultationChatMessage", new
        {
            UserId = userId,
            UserName = userName,
            ProfileImage = user?.ProfileImage,
            Content = content,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Raise hand in consultation
    /// </summary>
    public async Task RaiseHand(string roomId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        var user = await _context.Users.FindAsync(userId);
        var userName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";

        await Clients.Group($"consultation_{roomId}").SendAsync("ParticipantRaisedHand", new
        {
            UserId = userId,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation($"User {userId} raised hand in room {roomId}");
    }

    /// <summary>
    /// Lower hand in consultation
    /// </summary>
    public async Task LowerHand(string roomId)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return;

        await Clients.Group($"consultation_{roomId}").SendAsync("ParticipantLoweredHand", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation($"User {userId} lowered hand in room {roomId}");
    }
}

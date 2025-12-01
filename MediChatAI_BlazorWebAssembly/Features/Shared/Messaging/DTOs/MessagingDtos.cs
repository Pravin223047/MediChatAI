namespace MediChatAI_BlazorWebAssembly.Features.Shared.Messaging.DTOs;

public class ConversationDto
{
    public Guid Id { get; set; }
    public string PartnerId { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public string PartnerRole { get; set; } = string.Empty;
    public string? PartnerProfileImage { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsOnline { get; set; }
}

public class MessageDto
{
    public Guid Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public bool IsSender { get; set; }
    public List<ReactionCountDto> Reactions { get; set; } = new();
}

public class SendMessageInput
{
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}

public class MarkMessagesAsReadInput
{
    public Guid ConversationId { get; set; }
}

// GraphQL Response Models
public class GetConversationsResponse
{
    public List<ConversationDto> Conversations { get; set; } = new();
}

public class GetConversationMessagesResponse
{
    public List<MessageDto> ConversationMessages { get; set; } = new();
}

public class SendMessageResponse
{
    public MessageDto SendMessage { get; set; } = new();
}

public class MarkMessagesAsReadResponse
{
    public bool MarkMessagesAsRead { get; set; }
}

public class GetUnreadMessageCountResponse
{
    public int UnreadMessageCount { get; set; }
}

// Reaction DTOs
public class ReactionCountDto
{
    public string Emoji { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool UserReacted { get; set; }
}

public class MessageReactionResponseDto
{
    public bool Success { get; set; }
    public List<ReactionCountDto> Reactions { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class AddMessageReactionResponse
{
    public MessageReactionResponseDto AddMessageReaction { get; set; } = new();
}

public class RemoveMessageReactionResponse
{
    public MessageReactionResponseDto RemoveMessageReaction { get; set; } = new();
}

// Search DTOs
public class SearchMessagesInputDto
{
    public string SearchQuery { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? MessageType { get; set; }
    public string? PartnerId { get; set; }
    public bool HasAttachment { get; set; } = false;
    public int PageSize { get; set; } = 50;
    public int PageNumber { get; set; } = 1;
}

public class MessageSearchResultDto
{
    public Guid Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string? AttachmentFileName { get; set; }
    public bool IsSender { get; set; }
    public Guid ConversationId { get; set; }
    public string MatchedText { get; set; } = string.Empty;
}

public class SearchMessagesResponseDto
{
    public List<MessageSearchResultDto> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class SearchMessagesQueryResponse
{
    public SearchMessagesResponseDto SearchMessages { get; set; } = new();
}

// Typing Status DTOs
public class TypingStatusDto
{
    public string UserId { get; set; } = string.Empty;
    public bool IsTyping { get; set; }
    public bool IsAttaching { get; set; }
    public DateTime Timestamp { get; set; }
}

// Edit/Delete DTOs
public class EditMessageResponseDto
{
    public bool Success { get; set; }
    public Guid MessageId { get; set; }
    public string NewContent { get; set; } = string.Empty;
    public DateTime EditedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DeleteMessageResponseDto
{
    public bool Success { get; set; }
    public Guid MessageId { get; set; }
    public DateTime DeletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class EditMessageMutationResponse
{
    public EditMessageResponseDto EditMessage { get; set; } = new();
}

public class DeleteMessageMutationResponse
{
    public DeleteMessageResponseDto DeleteMessage { get; set; } = new();
}

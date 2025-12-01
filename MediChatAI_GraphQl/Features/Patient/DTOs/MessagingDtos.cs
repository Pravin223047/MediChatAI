namespace MediChatAI_GraphQl.Features.Patient.DTOs;

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
}

public class SendMessageInput
{
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}

public class SearchMessagesInput
{
    public string SearchQuery { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? MessageType { get; set; } // "Text", "File", "All"
    public string? PartnerId { get; set; } // Search in specific conversation
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
    public string MatchedText { get; set; } = string.Empty; // Highlighted match context
}

public class SearchMessagesResponse
{
    public List<MessageSearchResultDto> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

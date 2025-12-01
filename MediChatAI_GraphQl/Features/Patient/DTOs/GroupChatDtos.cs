namespace MediChatAI_GraphQl.Features.Patient.DTOs;

public class CreateGroupConversationInput
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> MemberUserIds { get; set; } = new();
}

public class SendGroupMessageInput
{
    public Guid GroupConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string MessageType { get; set; } = "Text";
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public long? AttachmentSize { get; set; }
    public Guid? ReplyToMessageId { get; set; }
}

public class AddGroupMembersInput
{
    public Guid GroupConversationId { get; set; }
    public List<string> UserIds { get; set; } = new();
}

public class GroupConversationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? GroupAvatarUrl { get; set; }
    public int MemberCount { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
    public GroupMessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}

public class GroupMemberDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}

public class GroupMessageDto
{
    public Guid Id { get; set; }
    public Guid GroupConversationId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public string MessageType { get; set; } = "Text";
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public long? AttachmentSize { get; set; }
    public List<GroupMessageReactionDto> Reactions { get; set; } = new();
    public List<ReadStatusDto> ReadBy { get; set; } = new();
    public bool IsSentByMe { get; set; }
}

public class GroupMessageReactionDto
{
    public string Emoji { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool UserReacted { get; set; }
    public List<string> ReactedUserNames { get; set; } = new();
}

public class ReadStatusDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime ReadAt { get; set; }
}

public class GroupChatResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public GroupConversationDto? Group { get; set; }
    public GroupMessageDto? GroupMessage { get; set; }
}

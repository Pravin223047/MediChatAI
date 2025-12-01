namespace MediChatAI_GraphQl.Core.Entities;

public class Conversation
{
    public Guid Id { get; set; }

    // Conversation Type: OneToOne, Group
    public ConversationType Type { get; set; } = ConversationType.OneToOne;

    // For one-to-one conversations
    public string? User1Id { get; set; }
    public ApplicationUser? User1 { get; set; }

    public string? User2Id { get; set; }
    public ApplicationUser? User2 { get; set; }

    // For group conversations
    public string? GroupName { get; set; }
    public string? GroupDescription { get; set; }
    public string? GroupImageUrl { get; set; }
    public string? CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    // Conversation metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public Guid? LastMessageId { get; set; }
    public DoctorMessage? LastMessage { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    // For group conversations - track participants
    public ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();

    // All messages in this conversation
    public ICollection<DoctorMessage> Messages { get; set; } = new List<DoctorMessage>();

    // Additional metadata (JSON)
    public string? Metadata { get; set; }

    // Helper method to get conversation ID for two users
    public static Guid GenerateConversationId(string user1Id, string user2Id)
    {
        // Sort user IDs to ensure consistent conversation ID regardless of order
        var sortedIds = new[] { user1Id, user2Id }.OrderBy(id => id).ToArray();
        var combined = $"{sortedIds[0]}_{sortedIds[1]}";

        // Generate deterministic GUID from combined string
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
            return new Guid(hash);
        }
    }
}

public enum ConversationType
{
    OneToOne = 0,
    Group = 1
}

public class ConversationParticipant
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Conversation? Conversation { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Participant role in group (Admin, Member)
    public ParticipantRole Role { get; set; } = ParticipantRole.Member;

    // Notification settings per conversation
    public bool IsMuted { get; set; }
    public DateTime? MutedUntil { get; set; }

    // Last read message tracking
    public Guid? LastReadMessageId { get; set; }
    public DateTime? LastReadAt { get; set; }
}

public enum ParticipantRole
{
    Member = 0,
    Admin = 1
}

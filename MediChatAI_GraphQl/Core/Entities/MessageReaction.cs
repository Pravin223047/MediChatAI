namespace MediChatAI_GraphQl.Core.Entities;

public class MessageReaction
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public DoctorMessage? Message { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string Emoji { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // For tracking when user changes reaction
    public DateTime? UpdatedAt { get; set; }
}

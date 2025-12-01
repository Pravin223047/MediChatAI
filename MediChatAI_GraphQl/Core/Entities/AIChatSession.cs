using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediChatAI_GraphQl.Core.Entities;

/// <summary>
/// Represents an AI chatbot conversation session
/// </summary>
public class AIChatSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string UserId { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public bool IsSaved { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public int MessageCount { get; set; }

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    public ICollection<AIChatMessage> Messages { get; set; } = new List<AIChatMessage>();
}

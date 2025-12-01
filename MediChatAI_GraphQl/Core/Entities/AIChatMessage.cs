using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediChatAI_GraphQl.Core.Entities;

/// <summary>
/// Represents a message in an AI chatbot conversation
/// </summary>
public class AIChatMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "user"; // "user" or "assistant"

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? MessageType { get; set; } // "text", "image", "voice", "symptomanalysis", "imageanalysis"

    public string? AttachmentsJson { get; set; } // JSON serialized attachments

    public string? MetadataJson { get; set; } // JSON serialized metadata

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public AIChatSession? Session { get; set; }
}

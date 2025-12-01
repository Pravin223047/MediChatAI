namespace MediChatAI_GraphQl.Core.Entities;

public class PrescriptionItem
{
    public int Id { get; set; }

    // Foreign key to Prescription
    public int PrescriptionId { get; set; }
    public Prescription? Prescription { get; set; }

    // Medication details
    public string MedicationName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string Dosage { get; set; } = string.Empty; // e.g., "500mg"
    public string Frequency { get; set; } = string.Empty; // e.g., "Twice daily", "Every 8 hours"
    public string? Route { get; set; } // Oral, Injection, Topical, etc.
    public int DurationDays { get; set; } = 7;
    public int Quantity { get; set; } = 1;
    public string? Form { get; set; } // Tablet, Capsule, Syrup, Injection, etc.

    // Instructions
    public string? Instructions { get; set; } // e.g., "Take after meals"
    public string? Warnings { get; set; } // e.g., "Do not drive after taking"
    public string? SideEffects { get; set; }

    // Tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

public class DrugModel
{
    public string DrugId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Antibiotic, Analgesic, Antihypertensive, etc.
    public List<string> AvailableDosages { get; set; } = new();
    public string DefaultDosageUnit { get; set; } = "mg";
    public List<string> CommonSideEffects { get; set; } = new();
    public List<string> Contraindications { get; set; } = new();
    public List<string> InteractsWith { get; set; } = new(); // DrugIds that interact
    public bool IsControlledSubstance { get; set; }
    public string? ControlledSubstanceSchedule { get; set; }
    public bool RequiresPrescription { get; set; } = true;
    public string? Manufacturer { get; set; }
    public double? AverageCost { get; set; }
}

public class PrescriptionModel
{
    public string PrescriptionId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime PrescriptionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public List<PrescriptionItemModel> Items { get; set; } = new();
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool IsActive { get; set; } = true;
    public string Status { get; set; } = "Active"; // Active, Expired, Cancelled, Completed
    public string? DigitalSignature { get; set; }
    public DateTime? SignedAt { get; set; }
    public int RefillsAllowed { get; set; }
    public int RefillsUsed { get; set; }
    public string? PharmacyNotes { get; set; }

    // Consultation Integration
    public int? ConsultationSessionId { get; set; }
}

public class PrescriptionItemModel
{
    public string ItemId { get; set; } = Guid.NewGuid().ToString();
    public string DrugId { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string Dosage { get; set; } = string.Empty; // e.g., "500mg"
    public string Frequency { get; set; } = string.Empty; // e.g., "Twice daily"
    public string Route { get; set; } = "Oral"; // Oral, Topical, Injection, etc.
    public int DurationDays { get; set; }
    public int Quantity { get; set; }
    public string? Form { get; set; } // Tablet, Capsule, Syrup, etc.
    public string? Instructions { get; set; }
    public string? Warnings { get; set; }
    public string? SideEffects { get; set; }
    public string? TimingInstructions { get; set; } // Before meals, After meals, etc.
}

public class CreatePrescriptionModel
{
    [Required]
    public string PatientId { get; set; } = string.Empty;

    [Required]
    public List<PrescriptionItemModel> Items { get; set; } = new();

    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public string? SpecialInstructions { get; set; }
    public int RefillsAllowed { get; set; }
}

public class DrugInteractionModel
{
    public string Drug1Id { get; set; } = string.Empty;
    public string Drug1Name { get; set; } = string.Empty;
    public string Drug2Id { get; set; } = string.Empty;
    public string Drug2Name { get; set; } = string.Empty;
    public string InteractionLevel { get; set; } = "Moderate"; // Mild, Moderate, Severe
    public string Description { get; set; } = string.Empty;
    public string? Recommendation { get; set; }
}

public class DrugInteractionCheckResult
{
    public bool HasInteractions { get; set; }
    public List<DrugInteractionModel> Interactions { get; set; } = new();
    public int SevereCount { get; set; }
    public int ModerateCount { get; set; }
    public int MildCount { get; set; }
}

public class AllergyCheckResult
{
    public bool HasAllergyConflict { get; set; }
    public List<string> ConflictingDrugs { get; set; } = new();
    public string? Warning { get; set; }
}

public class DosageCalculationModel
{
    public string DrugId { get; set; } = string.Empty;
    public double? PatientWeight { get; set; } // in kg
    public int? PatientAge { get; set; }
    public string? PatientCondition { get; set; }
    public string? RenalFunction { get; set; } // Normal, Mild, Moderate, Severe
    public string? HepaticFunction { get; set; } // Normal, Mild, Moderate, Severe
}

public class DosageRecommendation
{
    public string RecommendedDosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public string MaxDailyDose { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
    public string? AdjustmentReason { get; set; }
}

public class PrescriptionTemplateModel
{
    public string TemplateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public List<PrescriptionItemModel> Items { get; set; } = new();
    public string? Notes { get; set; }
    public int UsageCount { get; set; }
    public bool IsCustom { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class PrescriptionSearchFilter
{
    public string? PatientId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? DrugName { get; set; }
    public bool ActiveOnly { get; set; } = true;
}

public class PrescriptionStatisticsModel
{
    public int TotalPrescriptions { get; set; }
    public int ActivePrescriptions { get; set; }
    public int ExpiredPrescriptions { get; set; }
    public Dictionary<string, int> PrescriptionsByCategory { get; set; } = new();
    public Dictionary<string, int> MostPrescribedDrugs { get; set; } = new();
    public int AverageItemsPerPrescription { get; set; }
    public int PrescriptionsThisMonth { get; set; }
    public int PrescriptionsThisWeek { get; set; }
}

public class DrugSearchResult
{
    public List<DrugModel> Drugs { get; set; } = new();
    public int TotalCount { get; set; }
}

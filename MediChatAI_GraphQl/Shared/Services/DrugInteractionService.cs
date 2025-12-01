using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Shared.Services;

public interface IDrugInteractionService
{
    Task<List<DrugInteractionWarning>> CheckInteractionsAsync(List<CreatePrescriptionItemDto> medications);
}

public class DrugInteractionService : IDrugInteractionService
{
    private readonly ILogger<DrugInteractionService> _logger;

    // Common drug interaction database (simplified version)
    private static readonly Dictionary<string, List<string>> InteractionDatabase = new()
    {
        { "warfarin", new List<string> { "aspirin", "ibuprofen", "naproxen" } },
        { "aspirin", new List<string> { "warfarin", "ibuprofen", "naproxen" } },
        { "ibuprofen", new List<string> { "warfarin", "aspirin", "lisinopril" } },
        { "metformin", new List<string> { "alcohol" } },
        { "lisinopril", new List<string> { "potassium", "ibuprofen" } },
        { "simvastatin", new List<string> { "grapefruit", "amlodipine" } },
        { "levothyroxine", new List<string> { "calcium", "iron" } },
        { "amoxicillin", new List<string> { "methotrexate" } }
    };

    public DrugInteractionService(ILogger<DrugInteractionService> logger)
    {
        _logger = logger;
    }

    public async Task<List<DrugInteractionWarning>> CheckInteractionsAsync(List<CreatePrescriptionItemDto> medications)
    {
        var warnings = new List<DrugInteractionWarning>();

        // Check interactions between prescribed medications
        for (int i = 0; i < medications.Count; i++)
        {
            for (int j = i + 1; j < medications.Count; j++)
            {
                var drug1 = medications[i].MedicationName.ToLower();
                var drug2 = medications[j].MedicationName.ToLower();

                if (HasInteraction(drug1, drug2))
                {
                    warnings.Add(new DrugInteractionWarning
                    {
                        Severity = "Moderate",
                        Drug1 = medications[i].MedicationName,
                        Drug2 = medications[j].MedicationName,
                        Description = $"Potential interaction between {medications[i].MedicationName} and {medications[j].MedicationName}. Monitor patient closely.",
                        Recommendation = "Consider alternative medications or adjust dosages. Consult with pharmacist if needed."
                    });

                    _logger.LogWarning("Drug interaction detected: {Drug1} and {Drug2}", drug1, drug2);
                }
            }
        }

        return await Task.FromResult(warnings);
    }

    private bool HasInteraction(string drug1, string drug2)
    {
        // Check if drug1 interacts with drug2
        if (InteractionDatabase.TryGetValue(drug1, out var interactions1))
        {
            if (interactions1.Any(d => drug2.Contains(d)))
                return true;
        }

        // Check if drug2 interacts with drug1
        if (InteractionDatabase.TryGetValue(drug2, out var interactions2))
        {
            if (interactions2.Any(d => drug1.Contains(d)))
                return true;
        }

        return false;
    }
}

public class DrugInteractionWarning
{
    public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe
    public string Drug1 { get; set; } = string.Empty;
    public string Drug2 { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

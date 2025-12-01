namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Models;

public class SymptomAnalysisResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<string> Symptoms { get; set; } = new();
    public List<PossibleCondition> PossibleConditions { get; set; } = new();
    public string UrgencyLevel { get; set; } = "low"; // "low", "medium", "high", "emergency"
    public string UrgencyMessage { get; set; } = string.Empty;
    public List<string> RecommendedActions { get; set; } = new();
    public List<string> Questions { get; set; } = new();
    public string? Disclaimer { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class PossibleCondition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Probability { get; set; } // 0.0 to 1.0
    public string Severity { get; set; } = "mild"; // "mild", "moderate", "severe"
    public List<string> CommonSymptoms { get; set; } = new();
    public List<string> WhenToSeekCare { get; set; } = new();
}

public class SymptomInput
{
    public List<string> Symptoms { get; set; } = new();
    public string? Duration { get; set; }
    public string? Severity { get; set; }
    public Dictionary<string, string>? AdditionalInfo { get; set; }
}

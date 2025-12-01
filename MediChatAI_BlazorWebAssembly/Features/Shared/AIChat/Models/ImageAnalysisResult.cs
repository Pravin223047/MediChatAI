namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Models;

public class ImageAnalysisResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ImageUrl { get; set; } = string.Empty;
    public string ImageType { get; set; } = string.Empty; // "skin_condition", "medical_report", "xray", "lab_result", etc.
    public string Analysis { get; set; } = string.Empty;
    public List<Finding> Findings { get; set; } = new();
    public string Confidence { get; set; } = "medium"; // "low", "medium", "high"
    public List<string> Recommendations { get; set; } = new();
    public string? UrgencyLevel { get; set; }
    public string Disclaimer { get; set; } = "This AI analysis is for informational purposes only and should not replace professional medical diagnosis.";
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

public class Finding
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "normal"; // "normal", "abnormal", "concerning"
    public double Confidence { get; set; } // 0.0 to 1.0
    public List<string>? Notes { get; set; }
}

public class ImageAnalysisRequest
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? ImageType { get; set; }
    public string? Context { get; set; }
    public Dictionary<string, string>? AdditionalInfo { get; set; }
}

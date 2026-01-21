namespace MediChatAI_GraphQl.Core.Entities;

public class ScheduledReportExecution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ScheduledReportId { get; set; } = "";
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Running, Success, Failed
    public string? ErrorMessage { get; set; }
    public int RecipientsSent { get; set; }
    public int RecipientsFailed { get; set; }
    public string? ReportDataJson { get; set; }
    public TimeSpan Duration { get; set; }
    
    // Navigation property
    public virtual ScheduledReport? ScheduledReport { get; set; }
}

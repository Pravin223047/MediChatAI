namespace MediChatAI_GraphQl.Core.Entities;

public class ScheduledReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReportId { get; set; } = "";
    public string ReportName { get; set; } = "";
    public string Schedule { get; set; } = "0 9 * * 1"; // Cron expression
    public string Frequency { get; set; } = "Weekly"; // Daily, Weekly, Bi-Weekly, Monthly, Quarterly
    public List<string> Recipients { get; set; } = new();
    public string Format { get; set; } = "PDF"; // PDF, Excel, CSV, HTML
    public bool IsActive { get; set; } = true;
    public DateTime? NextRun { get; set; }
    public DateTime? LastRun { get; set; }
    public string? LastRunStatus { get; set; }
    public string? LastRunError { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "";
    public DateTime? UpdatedAt { get; set; }
}

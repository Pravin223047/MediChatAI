using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Admin.Services;

public interface IReportService
{
    // Report Generation
    Task<ReportDataDto?> GenerateReportAsync(string reportType, ReportFilterModel? filters = null);
    Task<Dictionary<string, object>> GetReportDataAsync(string reportType, ReportFilterModel? filters = null);

    // Report Templates
    Task<List<ReportTemplateDto>> GetReportTemplatesAsync();
    Task<ReportTemplateDto?> GetReportTemplateAsync(string templateId);
    Task<bool> SaveReportTemplateAsync(ReportTemplateDto template);
    Task<bool> DeleteReportTemplateAsync(string templateId);

    // Custom Reports
    Task<CustomReportDto?> CreateCustomReportAsync(CustomReportDto report);
    Task<List<CustomReportDto>> GetCustomReportsAsync(string userId);

    // Export
    Task<byte[]> ExportReportAsync(ReportDataDto reportData, ExportFormat format);
    Task<string> ExportReportToBase64Async(ReportDataDto reportData, ExportFormat format);

    // Scheduling
    Task<ScheduledReportDto?> ScheduleReportAsync(ScheduledReportDto schedule);
    Task<List<ScheduledReportDto>> GetScheduledReportsAsync();
    Task<bool> UpdateScheduledReportAsync(ScheduledReportDto schedule);
    Task<bool> DeleteScheduledReportAsync(string scheduleId);

    // History
    Task<List<ReportHistoryDto>> GetReportHistoryAsync(int skip = 0, int take = 50);
    Task<ReportHistoryDto?> GetReportHistoryItemAsync(string historyId);

    // Comparison
    Task<Dictionary<string, object>> CompareReportsAsync(string reportType, DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End);

    // Analytics Data
    Task<Dictionary<string, int>> GetUserRegistrationTrendAsync(DateTime fromDate, DateTime toDate, string groupBy = "day");
    Task<Dictionary<string, int>> GetUserActivityHeatmapAsync(DateTime fromDate, DateTime toDate);
    Task<Dictionary<string, double>> GetEngagementMetricsAsync(DateTime fromDate, DateTime toDate);
    Task<Dictionary<string, int>> GetRoleDistributionAsync();
    Task<Dictionary<string, double>> GetRetentionAnalysisAsync(DateTime fromDate, DateTime toDate);
    Task<Dictionary<string, int>> GetActivityTypeDistributionAsync(DateTime fromDate, DateTime toDate);
    Task<Dictionary<int, int>> GetPeakUsageHoursAsync(DateTime fromDate, DateTime toDate);
    Task<List<TimelineDataPoint>> GetActivityTimelineAsync(DateTime fromDate, DateTime toDate);
}

public enum ExportFormat
{
    PDF,
    Excel,
    CSV,
    PNG,
    JSON
}

public class TimelineDataPoint
{
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = "";
    public string Description { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
}

public class ReportFilterModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Role { get; set; } = "";
    public string ActivityType { get; set; } = "";
    public string Status { get; set; } = "";
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeTables { get; set; } = true;
    public bool IncludeMetrics { get; set; } = true;
    public string GroupBy { get; set; } = "none";
    public string SortBy { get; set; } = "date";
    public string SortOrder { get; set; } = "desc";
}

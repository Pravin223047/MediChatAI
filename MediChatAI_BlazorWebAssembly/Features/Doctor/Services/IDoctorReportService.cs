using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public interface IDoctorReportService
{
    /// <summary>
    /// Get overview statistics for the reports dashboard
    /// </summary>
    Task<ReportOverviewStats?> GetReportOverviewAsync();

    /// <summary>
    /// Get patient reports with optional filters
    /// </summary>
    Task<PatientReportData?> GetPatientReportsAsync(DateTime? dateFrom = null, DateTime? dateTo = null, ReportFilterOptions? filters = null);

    /// <summary>
    /// Get appointment analytics data
    /// </summary>
    Task<AppointmentReportData?> GetAppointmentAnalyticsAsync(DateTime? dateFrom = null, DateTime? dateTo = null, ReportFilterOptions? filters = null);

    /// <summary>
    /// Get prescription reports data
    /// </summary>
    Task<PrescriptionReportData?> GetPrescriptionReportsAsync(DateTime? dateFrom = null, DateTime? dateTo = null, ReportFilterOptions? filters = null);

    /// <summary>
    /// Get revenue/financial reports data
    /// </summary>
    Task<RevenueReportData?> GetRevenueReportsAsync(DateTime? dateFrom = null, DateTime? dateTo = null, ReportFilterOptions? filters = null);

    /// <summary>
    /// Get all scheduled reports
    /// </summary>
    Task<List<ScheduledReport>> GetScheduledReportsAsync();

    /// <summary>
    /// Create a new scheduled report
    /// </summary>
    Task<bool> CreateScheduledReportAsync(ScheduledReport config);

    /// <summary>
    /// Update an existing scheduled report
    /// </summary>
    Task<bool> UpdateScheduledReportAsync(ScheduledReport config);

    /// <summary>
    /// Delete a scheduled report
    /// </summary>
    Task<bool> DeleteScheduledReportAsync(string reportId);

    /// <summary>
    /// Get all saved report templates
    /// </summary>
    Task<List<ReportTemplate>> GetReportTemplatesAsync();

    /// <summary>
    /// Save a new report template
    /// </summary>
    Task<bool> SaveReportTemplateAsync(ReportTemplate template);

    /// <summary>
    /// Delete a report template
    /// </summary>
    Task<bool> DeleteReportTemplateAsync(string templateId);

    /// <summary>
    /// Generate a custom report based on configuration
    /// </summary>
    Task<DoctorReportsData?> GenerateCustomReportAsync(CustomReportConfiguration config);

    /// <summary>
    /// Export a report to the specified format
    /// </summary>
    Task<byte[]?> ExportReportAsync(string reportType, ReportExportOptions options);

    /// <summary>
    /// Get comparison data for a specific report type
    /// </summary>
    Task<ComparisonData<T>?> GetComparisonDataAsync<T>(string reportType, DateTime dateFrom, DateTime dateTo);
}

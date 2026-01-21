using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Features.Notifications.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace MediChatAI_GraphQl.Shared.Services;

public class ScheduledReportBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledReportBackgroundService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly TimeSpan _maxWaitTime = TimeSpan.FromMinutes(5); // Maximum time to wait before rechecking
    
    // IST TimeZone (UTC+5:30)
    private static readonly TimeZoneInfo IstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
    private static DateTime NowIst => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IstTimeZone);

    public ScheduledReportBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ScheduledReportBackgroundService> logger,
        IHubContext<NotificationHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduled Report Background Service is starting with precise timing.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Get the next scheduled report time and wait precisely until then
                var waitTime = await GetTimeUntilNextReportAsync();
                
                if (waitTime > TimeSpan.Zero)
                {
                    // Cap the wait time to max wait time for safety (in case schedules change)
                    var actualWait = waitTime > _maxWaitTime ? _maxWaitTime : waitTime;
                    
                    if (actualWait > TimeSpan.FromSeconds(10))
                    {
                        _logger.LogInformation("Next scheduled report in {Minutes:F1} minutes. Waiting...", actualWait.TotalMinutes);
                    }
                    
                    await Task.Delay(actualWait, stoppingToken);
                }
                
                // Process any due reports
                await ProcessScheduledReportsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing scheduled reports");
                // Wait a short time before retrying on error
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("Scheduled Report Background Service is stopping.");
    }

    private async Task<TimeSpan> GetTimeUntilNextReportAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var scheduledReportService = scope.ServiceProvider.GetRequiredService<IScheduledReportService>();

        try
        {
            var activeReports = await scheduledReportService.GetActiveScheduledReportsAsync();
            
            if (!activeReports.Any())
            {
                // No active reports, wait max time before checking again
                return _maxWaitTime;
            }

            var now = DateTime.UtcNow;
            
            // Find the earliest next run time
            var nextRun = activeReports
                .Where(r => r.NextRun.HasValue)
                .Select(r => r.NextRun!.Value)
                .OrderBy(t => t)
                .FirstOrDefault();

            if (nextRun == default)
            {
                return _maxWaitTime;
            }

            var timeUntilNext = nextRun - now;
            
            // If it's already due or past due, return zero to process immediately
            if (timeUntilNext <= TimeSpan.Zero)
            {
                return TimeSpan.Zero;
            }

            return timeUntilNext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next report time");
            return _maxWaitTime;
        }
    }

    private async Task ProcessScheduledReportsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scheduledReportService = scope.ServiceProvider.GetRequiredService<IScheduledReportService>();

        try
        {
            var dueReports = await scheduledReportService.GetDueReportsAsync();
            
            if (dueReports.Any())
            {
                _logger.LogInformation("Found {Count} scheduled reports due for execution", dueReports.Count);
            }

            foreach (var report in dueReports)
            {
                if (stoppingToken.IsCancellationRequested) break;

                try
                {
                    _logger.LogInformation("Executing scheduled report: {ReportName} (ID: {Id})", 
                        report.ReportName, report.Id);
                    
                    var execution = await scheduledReportService.ExecuteScheduledReportAsync(report.Id, sendEmail: true);
                    
                    if (execution?.Status == "Success")
                    {
                        _logger.LogInformation("Successfully executed and emailed report: {ReportName}", report.ReportName);
                        
                        // Extract report data for download
                        string? reportBase64 = null;
                        string? fileName = null;
                        string? mimeType = null;
                        
                        if (!string.IsNullOrEmpty(execution.ReportDataJson))
                        {
                            try
                            {
                                var reportDataObj = JsonSerializer.Deserialize<JsonElement>(execution.ReportDataJson);
                                if (reportDataObj.TryGetProperty("ReportBase64", out var base64Prop))
                                    reportBase64 = base64Prop.GetString();
                                if (reportDataObj.TryGetProperty("FileName", out var fileNameProp))
                                    fileName = fileNameProp.GetString();
                                if (reportDataObj.TryGetProperty("MimeType", out var mimeTypeProp))
                                    mimeType = mimeTypeProp.GetString();
                            }
                            catch (Exception parseEx)
                            {
                                _logger.LogWarning(parseEx, "Could not parse report data for download");
                            }
                        }
                        
                        // Send real-time notification to all admin users with download option
                        await SendReportExecutionNotificationAsync(
                            report.ReportName, 
                            execution.RecipientsSent, 
                            report.Id,
                            reportBase64,
                            fileName,
                            mimeType);
                    }
                    else
                    {
                        _logger.LogWarning("Report execution completed with status: {Status} for {ReportName}", 
                            execution?.Status, report.ReportName);
                        
                        // Notify about failed execution too
                        await SendReportFailureNotificationAsync(report.ReportName, execution?.ErrorMessage ?? "Unknown error");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute scheduled report: {ReportId}", report.Id);
                    await SendReportFailureNotificationAsync(report.ReportName, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting due reports");
        }
    }

    private async Task SendReportExecutionNotificationAsync(
        string reportName, 
        int recipientsSent, 
        string reportId,
        string? reportBase64,
        string? fileName,
        string? mimeType)
    {
        try
        {
            var notification = new
            {
                Type = "ScheduledReportExecuted",
                ReportName = reportName,
                ReportId = reportId,
                RecipientsSent = recipientsSent,
                ExecutedAt = NowIst,
                Message = $"Scheduled report '{reportName}' has been executed and sent to {recipientsSent} recipient(s).",
                ReportBase64 = reportBase64,
                FileName = fileName,
                MimeType = mimeType,
                HasDownload = !string.IsNullOrEmpty(reportBase64)
            };

            await _hubContext.Clients.Group("admins").SendAsync("ScheduledReportExecuted", notification);
            _logger.LogInformation("Sent scheduled report notification to admins for: {ReportName} (with download: {HasDownload})", 
                reportName, !string.IsNullOrEmpty(reportBase64));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification for scheduled report");
        }
    }

    private async Task SendReportFailureNotificationAsync(string reportName, string errorMessage)
    {
        try
        {
            var notification = new
            {
                Type = "ScheduledReportFailed",
                ReportName = reportName,
                Error = errorMessage,
                FailedAt = NowIst,
                Message = $"Scheduled report '{reportName}' failed: {errorMessage}"
            };

            await _hubContext.Clients.Group("admins").SendAsync("ScheduledReportFailed", notification);
            _logger.LogInformation("Sent scheduled report failure notification to admins for: {ReportName}", reportName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR failure notification");
        }
    }
}

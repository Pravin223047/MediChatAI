using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MediChatAI_GraphQl.Features.Admin.Services;

public class ScheduledReportService : IScheduledReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ScheduledReportService> _logger;
    
    // IST TimeZone (UTC+5:30)
    private static readonly TimeZoneInfo IstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
    
    // Helper to get current IST time (for display purposes only)
    private static DateTime NowIst => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IstTimeZone);
    
    // Convert IST DateTime to UTC for storage
    private static DateTime IstToUtc(DateTime istTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(istTime, DateTimeKind.Unspecified), IstTimeZone);
    }
    
    // Convert UTC DateTime to IST for display
    private static DateTime UtcToIst(DateTime utcTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcTime, DateTimeKind.Utc), IstTimeZone);
    }

    public ScheduledReportService(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<ScheduledReportService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ScheduledReport?> CreateScheduledReportAsync(ScheduledReport schedule)
    {
        try
        {
            schedule.Id = Guid.NewGuid().ToString();
            schedule.CreatedAt = DateTime.UtcNow;
            schedule.NextRun = CalculateNextRun(schedule);
            
            _context.ScheduledReports.Add(schedule);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created scheduled report: {ReportName} (ID: {Id})", schedule.ReportName, schedule.Id);
            return schedule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating scheduled report");
            return null;
        }
    }

    public async Task<ScheduledReport?> GetScheduledReportAsync(string id)
    {
        return await _context.ScheduledReports.FindAsync(id);
    }

    public async Task<List<ScheduledReport>> GetAllScheduledReportsAsync()
    {
        return await _context.ScheduledReports
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<ScheduledReport>> GetActiveScheduledReportsAsync()
    {
        return await _context.ScheduledReports
            .Where(s => s.IsActive)
            .OrderBy(s => s.NextRun)
            .ToListAsync();
    }

    public async Task<bool> UpdateScheduledReportAsync(ScheduledReport schedule)
    {
        try
        {
            var existing = await _context.ScheduledReports.FindAsync(schedule.Id);
            if (existing == null) return false;

            existing.ReportName = schedule.ReportName;
            existing.ReportId = schedule.ReportId;
            existing.Schedule = schedule.Schedule;
            existing.Frequency = schedule.Frequency;
            existing.Recipients = schedule.Recipients;
            existing.Format = schedule.Format;
            existing.IsActive = schedule.IsActive;
            // Always recalculate NextRun based on the schedule, ignoring any value from frontend
            existing.NextRun = CalculateNextRun(existing);
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated scheduled report: {ReportName} (ID: {Id})", schedule.ReportName, schedule.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating scheduled report {Id}", schedule.Id);
            return false;
        }
    }

    public async Task<bool> DeleteScheduledReportAsync(string id)
    {
        try
        {
            var schedule = await _context.ScheduledReports.FindAsync(id);
            if (schedule == null) return false;

            _context.ScheduledReports.Remove(schedule);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted scheduled report: {ReportName} (ID: {Id})", schedule.ReportName, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting scheduled report {Id}", id);
            return false;
        }
    }

    public async Task<ScheduledReportExecution?> ExecuteScheduledReportAsync(string scheduleId, bool sendEmail = true)
    {
        var schedule = await _context.ScheduledReports.FindAsync(scheduleId);
        if (schedule == null)
        {
            _logger.LogWarning("Scheduled report not found: {ScheduleId}", scheduleId);
            return null;
        }

        var execution = new ScheduledReportExecution
        {
            Id = Guid.NewGuid().ToString(),
            ScheduledReportId = scheduleId,
            ExecutedAt = DateTime.UtcNow,  // Store UTC in database
            Status = "Running"
        };

        var startTime = DateTime.UtcNow;
        var nowIstForDisplay = NowIst;  // IST only for filenames and display

        try
        {
            _logger.LogInformation("Executing scheduled report: {ReportName}", schedule.ReportName);

            // Generate report data
            var reportData = await GenerateReportDataAsync(schedule.ReportId);
            execution.ReportDataJson = JsonSerializer.Serialize(reportData);

            // Generate the actual report file (PDF or Excel)
            byte[]? reportBytes = null;
            string? attachmentName = null;
            string? mimeType = null;

            if (schedule.Format.Equals("Excel", StringComparison.OrdinalIgnoreCase) || 
                schedule.Format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
            {
                reportBytes = GenerateExcelReport(schedule, reportData);
                attachmentName = $"{schedule.ReportName.Replace(" ", "_")}_{nowIstForDisplay:yyyyMMdd_HHmmss}.xlsx";
                mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            else if (schedule.Format.Equals("CSV", StringComparison.OrdinalIgnoreCase))
            {
                reportBytes = GenerateCsvReport(schedule, reportData);
                attachmentName = $"{schedule.ReportName.Replace(" ", "_")}_{nowIstForDisplay:yyyyMMdd_HHmmss}.csv";
                mimeType = "text/csv";
            }
            else
            {
                // Generate PDF report
                reportBytes = GeneratePdfReport(schedule, reportData);
                attachmentName = $"{schedule.ReportName.Replace(" ", "_")}_{nowIstForDisplay:yyyyMMdd_HHmmss}.pdf";
                mimeType = "application/pdf";
            }

            // Store report bytes in execution for download
            if (reportBytes != null)
            {
                execution.ReportDataJson = JsonSerializer.Serialize(new
                {
                    Data = reportData,
                    ReportBase64 = Convert.ToBase64String(reportBytes),
                    FileName = attachmentName,
                    MimeType = mimeType
                });
            }

            // Send email with attachment if requested
            if (sendEmail && schedule.Recipients.Any())
            {
                var emailContent = BuildReportEmailContent(schedule, reportData);
                var successCount = 0;
                var failCount = 0;

                foreach (var recipient in schedule.Recipients)
                {
                    try
                    {
                        if (reportBytes != null && attachmentName != null && mimeType != null)
                        {
                            await _emailService.SendEmailWithAttachmentAsync(
                                recipient,
                                $"Scheduled Report: {schedule.ReportName}",
                                emailContent,
                                reportBytes,
                                attachmentName,
                                mimeType
                            );
                        }
                        else
                        {
                            await _emailService.SendEmailAsync(
                                recipient,
                                $"Scheduled Report: {schedule.ReportName}",
                                emailContent
                            );
                        }
                        successCount++;
                        _logger.LogInformation("Sent report email to {Recipient}", recipient);
                    }
                    catch (Exception emailEx)
                    {
                        failCount++;
                        _logger.LogError(emailEx, "Failed to send report email to {Recipient}", recipient);
                    }
                }

                execution.RecipientsSent = successCount;
                execution.RecipientsFailed = failCount;
            }

            execution.Status = "Success";
            execution.Duration = DateTime.UtcNow - startTime;

            // Update schedule last run info (store UTC in database)
            schedule.LastRun = DateTime.UtcNow;
            schedule.LastRunStatus = "Success";
            schedule.LastRunError = null;
            schedule.NextRun = CalculateNextRun(schedule);

            _context.ScheduledReportExecutions.Add(execution);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully executed scheduled report: {ReportName}, sent to {Count} recipients", 
                schedule.ReportName, execution.RecipientsSent);

            return execution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing scheduled report: {ScheduleId}", scheduleId);

            execution.Status = "Failed";
            execution.ErrorMessage = ex.Message;
            execution.Duration = DateTime.UtcNow - startTime;

            schedule.LastRun = DateTime.UtcNow;
            schedule.LastRunStatus = "Failed";
            schedule.LastRunError = ex.Message;
            schedule.NextRun = CalculateNextRun(schedule);

            _context.ScheduledReportExecutions.Add(execution);
            await _context.SaveChangesAsync();

            return execution;
        }
    }

    public async Task<List<ScheduledReportExecution>> GetExecutionHistoryAsync(string scheduleId, int take = 10)
    {
        return await _context.ScheduledReportExecutions
            .Where(e => e.ScheduledReportId == scheduleId)
            .OrderByDescending(e => e.ExecutedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<ScheduledReportExecution>> GetAllExecutionHistoryAsync(int skip = 0, int take = 50)
    {
        return await _context.ScheduledReportExecutions
            .Include(e => e.ScheduledReport)
            .OrderByDescending(e => e.ExecutedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<ScheduledReport>> GetDueReportsAsync()
    {
        var now = DateTime.UtcNow;  // Compare with UTC since we store UTC
        return await _context.ScheduledReports
            .Where(s => s.IsActive && s.NextRun.HasValue && s.NextRun.Value <= now)
            .ToListAsync();
    }

    public async Task ProcessDueReportsAsync()
    {
        var dueReports = await GetDueReportsAsync();
        _logger.LogInformation("Processing {Count} due scheduled reports", dueReports.Count);

        foreach (var report in dueReports)
        {
            try
            {
                await ExecuteScheduledReportAsync(report.Id, sendEmail: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled report: {ReportId}", report.Id);
            }
        }
    }

    public async Task<bool> SendReportEmailAsync(string scheduleId, string reportContent, string format)
    {
        var schedule = await _context.ScheduledReports.FindAsync(scheduleId);
        if (schedule == null || !schedule.Recipients.Any()) return false;

        try
        {
            foreach (var recipient in schedule.Recipients)
            {
                await _emailService.SendEmailAsync(
                    recipient,
                    $"Report: {schedule.ReportName}",
                    reportContent
                );
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending report email for schedule: {ScheduleId}", scheduleId);
            return false;
        }
    }

    private async Task<Dictionary<string, object>> GenerateReportDataAsync(string reportId)
    {
        // In production, this would generate actual report data based on reportId
        // For now, return sample data
        var data = new Dictionary<string, object>
        {
            ["reportId"] = reportId,
            ["generatedAt"] = NowIst,
            ["summary"] = "Report generated successfully"
        };

        // Add some sample metrics based on report type
        switch (reportId)
        {
            case "new-registrations":
                var userCount = await _context.Users.CountAsync();
                data["totalUsers"] = userCount;
                data["newThisWeek"] = await _context.Users
                    .CountAsync(u => u.CreatedAt >= NowIst.AddDays(-7));
                break;
            case "user-activity":
                data["totalActivities"] = await _context.UserActivities
                    .CountAsync(a => a.Timestamp >= NowIst.AddDays(-30));
                break;
            default:
                data["status"] = "Generated";
                break;
        }

        return data;
    }

    private string BuildReportEmailContent(ScheduledReport schedule, Dictionary<string, object> data)
    {
        var dataHtml = string.Join("", data.Select(kvp => 
            $"<tr><td style='padding: 10px; border: 1px solid #e5e7eb;'><strong>{kvp.Key}</strong></td>" +
            $"<td style='padding: 10px; border: 1px solid #e5e7eb;'>{kvp.Value}</td></tr>"));

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='color: white; margin: 0;'>MediChat.AI Report</h1>
    </div>
    <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <h2 style='color: #667eea;'>{schedule.ReportName}</h2>
        <p>Your scheduled report has been generated and is ready for review.</p>

        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #667eea;'>
            <h3 style='margin-top: 0; color: #667eea;'>Report Details</h3>
            <p><strong>Report Type:</strong> {schedule.ReportId}</p>
            <p><strong>Frequency:</strong> {schedule.Frequency}</p>
            <p><strong>Format:</strong> {schedule.Format}</p>
            <p><strong>Generated At:</strong> {NowIst:MMMM dd, yyyy 'at' HH:mm} IST</p>
        </div>

        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h3 style='margin-top: 0; color: #667eea;'>Report Summary</h3>
            <table style='width: 100%; border-collapse: collapse;'>
                {dataHtml}
            </table>
        </div>

        <p style='margin-top: 20px; font-size: 12px; color: #666;'>
            This is an automated email from MediChat.AI. If you no longer wish to receive these reports, 
            please update your schedule settings in the admin panel.
        </p>

        <p style='margin-top: 30px;'>Best regards,<br><strong>The MediChat.AI Team</strong></p>
    </div>
    <div style='text-align: center; margin-top: 20px; color: #888; font-size: 12px;'>
        <p>&copy; {NowIst.Year} MediChat.AI. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    private DateTime CalculateNextRun(ScheduledReport schedule)
    {
        // Parse the cron expression to calculate next run
        // Format: minute hour dayOfMonth month dayOfWeek
        // The hour/minute in cron are in IST (user's local time)
        try
        {
            var parts = schedule.Schedule.Split(' ');
            if (parts.Length < 5) return DateTime.UtcNow.AddDays(1);

            int.TryParse(parts[0], out var minute);
            int.TryParse(parts[1], out var hour);  // This is IST hour

            var nowUtc = DateTime.UtcNow;
            var nowIst = UtcToIst(nowUtc);
            
            // Calculate next run in IST
            DateTime nextRunIst;

            // Based on frequency, calculate the next appropriate run time in IST
            switch (schedule.Frequency)
            {
                case "Daily":
                    // Create today's scheduled time in IST
                    var todayScheduledTime = new DateTime(nowIst.Year, nowIst.Month, nowIst.Day, hour, minute, 0);
                    
                    if (todayScheduledTime > nowIst)
                    {
                        // Today's time hasn't passed yet, schedule for today
                        nextRunIst = todayScheduledTime;
                    }
                    else
                    {
                        // Today's time has passed, schedule for tomorrow
                        nextRunIst = todayScheduledTime.AddDays(1);
                    }
                    break;

                case "Weekly":
                    nextRunIst = new DateTime(nowIst.Year, nowIst.Month, nowIst.Day, hour, minute, 0);
                    // If today's time has passed, start from tomorrow
                    if (nextRunIst <= nowIst) nextRunIst = nextRunIst.AddDays(1);
                    // Find the next matching day of week
                    while (nextRunIst <= nowIst) nextRunIst = nextRunIst.AddDays(7);
                    break;

                case "Bi-Weekly":
                    nextRunIst = new DateTime(nowIst.Year, nowIst.Month, nowIst.Day, hour, minute, 0);
                    if (nextRunIst <= nowIst) nextRunIst = nextRunIst.AddDays(1);
                    while (nextRunIst <= nowIst) nextRunIst = nextRunIst.AddDays(14);
                    break;

                case "Monthly":
                    nextRunIst = new DateTime(nowIst.Year, nowIst.Month, nowIst.Day, hour, minute, 0);
                    if (nextRunIst <= nowIst) nextRunIst = nextRunIst.AddMonths(1);
                    break;

                case "Quarterly":
                    nextRunIst = new DateTime(nowIst.Year, nowIst.Month, nowIst.Day, hour, minute, 0);
                    if (nextRunIst <= nowIst) nextRunIst = nextRunIst.AddMonths(3);
                    break;

                default:
                    nextRunIst = new DateTime(nowIst.Year, nowIst.Month, nowIst.Day, hour, minute, 0);
                    if (nextRunIst <= nowIst) nextRunIst = nextRunIst.AddDays(1);
                    break;
            }

            // Convert IST next run to UTC for storage
            var nextRunUtc = IstToUtc(nextRunIst);

            _logger.LogInformation("Calculated next run for {ReportName}: {NextRunIst} IST = {NextRunUtc} UTC (Frequency: {Frequency})", 
                schedule.ReportName, nextRunIst, nextRunUtc, schedule.Frequency);

            return nextRunUtc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next run for scheduled report");
            return DateTime.UtcNow.AddDays(1);
        }
    }

    private byte[] GenerateExcelReport(ScheduledReport schedule, Dictionary<string, object> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(schedule.ReportName.Length > 31 ? schedule.ReportName[..31] : schedule.ReportName);

        // Add header
        worksheet.Cell(1, 1).Value = "MediChat.AI - " + schedule.ReportName;
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Range(1, 1, 1, 3).Merge();

        worksheet.Cell(2, 1).Value = $"Generated: {NowIst:MMMM dd, yyyy 'at' HH:mm} IST";
        worksheet.Cell(3, 1).Value = $"Frequency: {schedule.Frequency}";

        // Add data table
        var row = 5;
        worksheet.Cell(row, 1).Value = "Metric";
        worksheet.Cell(row, 2).Value = "Value";
        worksheet.Range(row, 1, row, 2).Style.Font.Bold = true;
        worksheet.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

        row++;
        foreach (var kvp in data)
        {
            worksheet.Cell(row, 1).Value = kvp.Key;
            worksheet.Cell(row, 2).Value = kvp.Value?.ToString() ?? "";
            row++;
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private byte[] GenerateCsvReport(ScheduledReport schedule, Dictionary<string, object> data)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine($"MediChat.AI - {schedule.ReportName}");
        sb.AppendLine($"Generated: {NowIst:yyyy-MM-dd HH:mm:ss} IST");
        sb.AppendLine($"Frequency: {schedule.Frequency}");
        sb.AppendLine();
        sb.AppendLine("Metric,Value");

        foreach (var kvp in data)
        {
            var value = kvp.Value?.ToString()?.Replace(",", ";") ?? "";
            sb.AppendLine($"{kvp.Key},{value}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private byte[] GeneratePdfReport(ScheduledReport schedule, Dictionary<string, object> data)
    {
        // Configure QuestPDF license for community use
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Header
                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("MediChat.AI")
                                .FontSize(24)
                                .Bold()
                                .FontColor(Colors.Indigo.Medium);
                            
                            col.Item().Text(schedule.ReportName)
                                .FontSize(18)
                                .SemiBold()
                                .FontColor(Colors.Grey.Darken2);
                        });
                        
                        row.ConstantItem(150).AlignRight().Column(col =>
                        {
                            col.Item().Text($"Generated: {NowIst:MMM dd, yyyy}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                            col.Item().Text($"Frequency: {schedule.Frequency}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        });
                    });
                });

                // Content
                page.Content().PaddingVertical(20).Column(col =>
                {
                    // Report Details Section
                    col.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(details =>
                    {
                        details.Item().Text("Report Details")
                            .FontSize(14)
                            .Bold()
                            .FontColor(Colors.Indigo.Medium);
                        
                        details.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Text($"Report Type: {schedule.ReportId}");
                            row.RelativeItem().Text($"Format: {schedule.Format}");
                        });
                        
                        details.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text($"Recipients: {schedule.Recipients.Count} email(s)");
                            row.RelativeItem().Text($"Time: {NowIst:HH:mm} IST");
                        });
                    });

                    col.Item().PaddingVertical(15);

                    // Data Table
                    col.Item().Text("Report Summary")
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Indigo.Medium);
                    
                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        // Table Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Indigo.Medium).Padding(8)
                                .Text("Metric").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Indigo.Medium).Padding(8)
                                .Text("Value").FontColor(Colors.White).Bold();
                        });

                        // Table Rows
                        var isAlternate = false;
                        foreach (var kvp in data)
                        {
                            var bgColor = isAlternate ? Colors.Grey.Lighten4 : Colors.White;
                            
                            table.Cell().Background(bgColor).Padding(8)
                                .Text(kvp.Key).SemiBold();
                            table.Cell().Background(bgColor).Padding(8)
                                .Text(kvp.Value?.ToString() ?? "N/A");
                            
                            isAlternate = !isAlternate;
                        }
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("© ").FontSize(9).FontColor(Colors.Grey.Medium);
                    text.Span($"{NowIst.Year} MediChat.AI - Automated Report System")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}

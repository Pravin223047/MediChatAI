using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using MediChatAI_BlazorWebAssembly.Core.Services;
using MediChatAI_BlazorWebAssembly.Core.Services.Theme;
using Blazored.LocalStorage;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Admin.Services;

public class ReportService : IReportService
{
    private readonly IAdminService _adminService;
    private readonly ILocalStorageService _localStorage;
    private readonly IThemeService _themeService;
    private const string TEMPLATES_KEY = "report_templates";
    private const string HISTORY_KEY = "report_history";
    private const string SCHEDULES_KEY = "report_schedules";

    public ReportService(IAdminService adminService, ILocalStorageService localStorage, IThemeService themeService)
    {
        _adminService = adminService;
        _localStorage = localStorage;
        _themeService = themeService;
    }

    public async Task<ReportDataDto?> GenerateReportAsync(string reportType, ReportFilterModel? filters = null)
    {
        filters ??= new ReportFilterModel
        {
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow
        };

        var data = await GetReportDataAsync(reportType, filters);

        var reportData = new ReportDataDto(
            Guid.NewGuid().ToString(),
            GetReportName(reportType),
            data,
            DateTime.UtcNow,
            new ReportFilterDto(
                filters.FromDate,
                filters.ToDate,
                filters.Role,
                filters.ActivityType,
                filters.Status,
                filters.GroupBy,
                filters.SortBy,
                filters.SortOrder
            )
        );

        // Save to history with report configuration
        await SaveToHistoryAsync(reportData, reportType, filters);

        return reportData;
    }

    public async Task<Dictionary<string, object>> GetReportDataAsync(string reportType, ReportFilterModel? filters = null)
    {
        filters ??= new ReportFilterModel
        {
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow
        };

        return reportType switch
        {
            "new-registrations" => await GetNewRegistrationsDataAsync(filters),
            "user-activity" => await GetUserActivityDataAsync(filters),
            "engagement" => await GetEngagementDataAsync(filters),
            "role-distribution" => await GetRoleDistributionDataAsync(filters),
            "retention" => await GetRetentionDataAsync(filters),
            "performance-metrics" => await GetPerformanceMetricsDataAsync(filters),
            "response-times" => await GetResponseTimesDataAsync(filters),
            "error-rate" => await GetErrorRateDataAsync(filters),
            "uptime" => await GetUptimeDataAsync(filters),
            "activity-types" => await GetActivityTypesDataAsync(filters),
            "peak-hours" => await GetPeakHoursDataAsync(filters),
            "timeline" => await GetTimelineDataAsync(filters),
            "user-journey" => await GetUserJourneyDataAsync(filters),
            "comparative" => await GetComparativeDataAsync(filters),
            _ => new Dictionary<string, object>()
        };
    }

    // Report-specific data methods
    private async Task<Dictionary<string, object>> GetNewRegistrationsDataAsync(ReportFilterModel filters)
    {
        var trend = await GetUserRegistrationTrendAsync(filters.FromDate, filters.ToDate, filters.GroupBy == "none" ? "day" : filters.GroupBy);

        return new Dictionary<string, object>
        {
            ["labels"] = trend.Keys.ToList(),
            ["values"] = trend.Values.ToList(),
            ["total"] = trend.Values.Sum(),
            ["average"] = trend.Values.Any() ? trend.Values.Average() : 0,
            ["chartType"] = "line"
        };
    }

    private async Task<Dictionary<string, object>> GetUserActivityDataAsync(ReportFilterModel filters)
    {
        var heatmap = await GetUserActivityHeatmapAsync(filters.FromDate, filters.ToDate);

        return new Dictionary<string, object>
        {
            ["labels"] = heatmap.Keys.ToList(),
            ["values"] = heatmap.Values.ToList(),
            ["chartType"] = "bar"
        };
    }

    private async Task<Dictionary<string, object>> GetEngagementDataAsync(ReportFilterModel filters)
    {
        var metrics = await GetEngagementMetricsAsync(filters.FromDate, filters.ToDate);

        return new Dictionary<string, object>
        {
            ["labels"] = metrics.Keys.ToList(),
            ["values"] = metrics.Values.ToList(),
            ["chartType"] = "radar"
        };
    }

    private async Task<Dictionary<string, object>> GetRoleDistributionDataAsync(ReportFilterModel filters)
    {
        var distribution = await GetRoleDistributionAsync();

        return new Dictionary<string, object>
        {
            ["labels"] = distribution.Keys.ToList(),
            ["values"] = distribution.Values.ToList(),
            ["chartType"] = "pie"
        };
    }

    private async Task<Dictionary<string, object>> GetRetentionDataAsync(ReportFilterModel filters)
    {
        var retention = await GetRetentionAnalysisAsync(filters.FromDate, filters.ToDate);

        return new Dictionary<string, object>
        {
            ["labels"] = retention.Keys.ToList(),
            ["values"] = retention.Values.ToList(),
            ["chartType"] = "line"
        };
    }

    private Task<Dictionary<string, object>> GetPerformanceMetricsDataAsync(ReportFilterModel filters)
    {
        // Mock performance data - in production, this would come from actual metrics
        return Task.FromResult(new Dictionary<string, object>
        {
            ["labels"] = new List<string> { "CPU Usage", "Memory", "Disk I/O", "Network", "Response Time" },
            ["values"] = new List<double> { 45.2, 62.8, 38.5, 51.3, 28.7 },
            ["chartType"] = "radar"
        });
    }

    private Task<Dictionary<string, object>> GetResponseTimesDataAsync(ReportFilterModel filters)
    {
        var days = (int)(filters.ToDate - filters.FromDate).TotalDays;
        var labels = new List<string>();
        var values = new List<double>();
        var random = new Random();

        for (int i = 0; i < days; i++)
        {
            labels.Add(filters.FromDate.AddDays(i).ToString("MMM dd"));
            values.Add(50 + random.NextDouble() * 100); // Mock data
        }

        return Task.FromResult(new Dictionary<string, object>
        {
            ["labels"] = labels,
            ["values"] = values,
            ["average"] = values.Average(),
            ["chartType"] = "line"
        });
    }

    private Task<Dictionary<string, object>> GetErrorRateDataAsync(ReportFilterModel filters)
    {
        var days = (int)(filters.ToDate - filters.FromDate).TotalDays;
        var labels = new List<string>();
        var values = new List<double>();
        var random = new Random();

        for (int i = 0; i < days; i++)
        {
            labels.Add(filters.FromDate.AddDays(i).ToString("MMM dd"));
            values.Add(random.NextDouble() * 5); // Mock error rate 0-5%
        }

        return Task.FromResult(new Dictionary<string, object>
        {
            ["labels"] = labels,
            ["values"] = values,
            ["average"] = values.Average(),
            ["chartType"] = "area"
        });
    }

    private Task<Dictionary<string, object>> GetUptimeDataAsync(ReportFilterModel filters)
    {
        return Task.FromResult(new Dictionary<string, object>
        {
            ["labels"] = new List<string> { "Uptime", "Downtime" },
            ["values"] = new List<double> { 99.8, 0.2 },
            ["chartType"] = "donut"
        });
    }

    private async Task<Dictionary<string, object>> GetActivityTypesDataAsync(ReportFilterModel filters)
    {
        var distribution = await GetActivityTypeDistributionAsync(filters.FromDate, filters.ToDate);

        return new Dictionary<string, object>
        {
            ["labels"] = distribution.Keys.ToList(),
            ["values"] = distribution.Values.ToList(),
            ["chartType"] = "bar"
        };
    }

    private async Task<Dictionary<string, object>> GetPeakHoursDataAsync(ReportFilterModel filters)
    {
        var peakHours = await GetPeakUsageHoursAsync(filters.FromDate, filters.ToDate);

        return new Dictionary<string, object>
        {
            ["labels"] = peakHours.Keys.Select(h => $"{h:00}:00").ToList(),
            ["values"] = peakHours.Values.ToList(),
            ["chartType"] = "bar"
        };
    }

    private async Task<Dictionary<string, object>> GetTimelineDataAsync(ReportFilterModel filters)
    {
        var timeline = await GetActivityTimelineAsync(filters.FromDate, filters.ToDate);

        return new Dictionary<string, object>
        {
            ["labels"] = timeline.Select(t => t.Timestamp.ToString("MMM dd HH:mm")).ToList(),
            ["values"] = timeline.Select((t, i) => i + 1).ToList(),
            ["events"] = timeline,
            ["chartType"] = "line"
        };
    }

    private async Task<Dictionary<string, object>> GetUserJourneyDataAsync(ReportFilterModel filters)
    {
        var activities = await _adminService.GetUserActivitiesAsync(new GetUserActivitiesInput(
            FromDate: filters.FromDate,
            ToDate: filters.ToDate,
            Take: 1000
        ));

        if (activities?.Activities == null || !activities.Activities.Any())
        {
            return new Dictionary<string, object>
            {
                ["labels"] = new List<string>(),
                ["values"] = new List<int>(),
                ["chartType"] = "line"
            };
        }

        var journeySteps = activities.Activities
            .GroupBy(a => a.ActivityType)
            .OrderBy(g => g.Min(a => a.Timestamp))
            .Select(g => new { Step = g.Key, Count = g.Count() })
            .ToList();

        return new Dictionary<string, object>
        {
            ["labels"] = journeySteps.Select(s => s.Step).ToList(),
            ["values"] = journeySteps.Select(s => s.Count).ToList(),
            ["chartType"] = "line"
        };
    }

    private async Task<Dictionary<string, object>> GetComparativeDataAsync(ReportFilterModel filters)
    {
        var midpoint = filters.FromDate.AddDays((filters.ToDate - filters.FromDate).TotalDays / 2);
        var period1 = await GetUserActivityHeatmapAsync(filters.FromDate, midpoint);
        var period2 = await GetUserActivityHeatmapAsync(midpoint, filters.ToDate);

        return new Dictionary<string, object>
        {
            ["labels"] = period1.Keys.Union(period2.Keys).ToList(),
            ["period1"] = period1,
            ["period2"] = period2,
            ["chartType"] = "bar"
        };
    }

    // Analytics data methods
    public async Task<Dictionary<string, int>> GetUserRegistrationTrendAsync(DateTime fromDate, DateTime toDate, string groupBy = "day")
    {
        var activities = await _adminService.GetUserActivitiesAsync(new GetUserActivitiesInput(
            ActivityType: "Register",
            FromDate: fromDate,
            ToDate: toDate,
            Take: 10000
        ));

        if (activities?.Activities == null) return new Dictionary<string, int>();

        return activities.Activities
            .GroupBy(a => groupBy switch
            {
                "day" => a.Timestamp.ToString("yyyy-MM-dd"),
                "week" => $"Week {GetWeekOfYear(a.Timestamp)}",
                "month" => a.Timestamp.ToString("yyyy-MM"),
                "quarter" => $"Q{((a.Timestamp.Month - 1) / 3) + 1} {a.Timestamp.Year}",
                "year" => a.Timestamp.ToString("yyyy"),
                _ => a.Timestamp.ToString("yyyy-MM-dd")
            })
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetUserActivityHeatmapAsync(DateTime fromDate, DateTime toDate)
    {
        var activities = await _adminService.GetUserActivitiesAsync(new GetUserActivitiesInput(
            FromDate: fromDate,
            ToDate: toDate,
            Take: 10000
        ));

        if (activities?.Activities == null) return new Dictionary<string, int>();

        return activities.Activities
            .GroupBy(a => a.Timestamp.ToString("yyyy-MM-dd"))
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, double>> GetEngagementMetricsAsync(DateTime fromDate, DateTime toDate)
    {
        var activities = await _adminService.GetUserActivitiesAsync(new GetUserActivitiesInput(
            FromDate: fromDate,
            ToDate: toDate,
            Take: 10000
        ));

        var totalActivities = activities?.Activities.Count ?? 0;
        var uniqueUsers = activities?.Activities.Select(a => a.UserId).Distinct().Count() ?? 0;
        var loginCount = activities?.Activities.Count(a => a.ActivityType == "Login") ?? 0;

        return new Dictionary<string, double>
        {
            ["Total Activities"] = totalActivities,
            ["Unique Users"] = uniqueUsers,
            ["Avg Activities/User"] = uniqueUsers > 0 ? (double)totalActivities / uniqueUsers : 0,
            ["Login Rate"] = totalActivities > 0 ? (double)loginCount / totalActivities * 100 : 0
        };
    }

    public async Task<Dictionary<string, int>> GetRoleDistributionAsync()
    {
        var users = await _adminService.GetUsersAsync(new GetUsersInput(Take: 10000));

        if (users?.Users == null) return new Dictionary<string, int>();

        return users.Users
            .GroupBy(u => u.Role)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, double>> GetRetentionAnalysisAsync(DateTime fromDate, DateTime toDate)
    {
        var users = await _adminService.GetUsersAsync(new GetUsersInput(Take: 10000));

        if (users?.Users == null) return new Dictionary<string, double>();

        var totalUsers = users.Users.Count;
        var activeUsers = users.Users.Count(u => u.LastLoginAt.HasValue && u.LastLoginAt >= fromDate);
        var retentionRate = totalUsers > 0 ? (double)activeUsers / totalUsers * 100 : 0;

        return new Dictionary<string, double>
        {
            ["Total Users"] = totalUsers,
            ["Active Users"] = activeUsers,
            ["Retention Rate"] = retentionRate
        };
    }

    public async Task<Dictionary<string, int>> GetActivityTypeDistributionAsync(DateTime fromDate, DateTime toDate)
    {
        var activities = await _adminService.GetUserActivitiesAsync(new GetUserActivitiesInput(
            FromDate: fromDate,
            ToDate: toDate,
            Take: 10000
        ));

        if (activities?.Activities == null) return new Dictionary<string, int>();

        return activities.Activities
            .GroupBy(a => a.ActivityType)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<int, int>> GetPeakUsageHoursAsync(DateTime fromDate, DateTime toDate)
    {
        var activities = await _adminService.GetUserActivitiesAsync(new GetUserActivitiesInput(
            FromDate: fromDate,
            ToDate: toDate,
            Take: 10000
        ));

        if (activities?.Activities == null) return new Dictionary<int, int>();

        return activities.Activities
            .GroupBy(a => a.Timestamp.Hour)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<List<TimelineDataPoint>> GetActivityTimelineAsync(DateTime fromDate, DateTime toDate)
    {
        var activities = await _adminService.GetUserActivitiesAsync(new GetUserActivitiesInput(
            FromDate: fromDate,
            ToDate: toDate,
            Take: 100,
            SortBy: "Timestamp",
            SortDescending: true
        ));

        if (activities?.Activities == null) return new List<TimelineDataPoint>();

        return activities.Activities.Select(a => new TimelineDataPoint
        {
            Timestamp = a.Timestamp,
            EventType = a.ActivityType,
            Description = a.Description,
            UserId = a.UserId ?? "",
            UserName = a.UserName
        }).ToList();
    }

    // Template management
    public async Task<List<ReportTemplateDto>> GetReportTemplatesAsync()
    {
        try
        {
            var templates = await _localStorage.GetItemAsync<List<ReportTemplateDto>>(TEMPLATES_KEY);
            return templates ?? GetDefaultTemplates();
        }
        catch
        {
            return GetDefaultTemplates();
        }
    }

    public async Task<ReportTemplateDto?> GetReportTemplateAsync(string templateId)
    {
        var templates = await GetReportTemplatesAsync();
        return templates.FirstOrDefault(t => t.Id == templateId);
    }

    public async Task<bool> SaveReportTemplateAsync(ReportTemplateDto template)
    {
        try
        {
            var templates = await GetReportTemplatesAsync();
            templates.Add(template);
            await _localStorage.SetItemAsync(TEMPLATES_KEY, templates);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteReportTemplateAsync(string templateId)
    {
        try
        {
            var templates = await GetReportTemplatesAsync();
            templates.RemoveAll(t => t.Id == templateId);
            await _localStorage.SetItemAsync(TEMPLATES_KEY, templates);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Custom reports
    public Task<CustomReportDto?> CreateCustomReportAsync(CustomReportDto report)
    {
        // This would integrate with backend in production
        return Task.FromResult<CustomReportDto?>(report);
    }

    public Task<List<CustomReportDto>> GetCustomReportsAsync(string userId)
    {
        // This would fetch from backend in production
        return Task.FromResult(new List<CustomReportDto>());
    }

    // Export functionality
    public Task<byte[]> ExportReportAsync(ReportDataDto reportData, ExportFormat format)
    {
        // This would be implemented with actual export libraries
        return Task.FromResult(Array.Empty<byte>());
    }

    public async Task<string> ExportReportToBase64Async(ReportDataDto reportData, ExportFormat format)
    {
        var bytes = await ExportReportAsync(reportData, format);
        return Convert.ToBase64String(bytes);
    }

    // Scheduling
    public async Task<ScheduledReportDto?> ScheduleReportAsync(ScheduledReportDto schedule)
    {
        try
        {
            var schedules = await GetScheduledReportsAsync();
            schedules.Add(schedule);
            await _localStorage.SetItemAsync(SCHEDULES_KEY, schedules);
            return schedule;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ScheduledReportDto>> GetScheduledReportsAsync()
    {
        try
        {
            var schedules = await _localStorage.GetItemAsync<List<ScheduledReportDto>>(SCHEDULES_KEY);
            return schedules ?? new List<ScheduledReportDto>();
        }
        catch
        {
            return new List<ScheduledReportDto>();
        }
    }

    public async Task<bool> UpdateScheduledReportAsync(ScheduledReportDto schedule)
    {
        try
        {
            var schedules = await GetScheduledReportsAsync();
            var index = schedules.FindIndex(s => s.Id == schedule.Id);
            if (index >= 0)
            {
                schedules[index] = schedule;
                await _localStorage.SetItemAsync(SCHEDULES_KEY, schedules);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteScheduledReportAsync(string scheduleId)
    {
        try
        {
            var schedules = await GetScheduledReportsAsync();
            schedules.RemoveAll(s => s.Id == scheduleId);
            await _localStorage.SetItemAsync(SCHEDULES_KEY, schedules);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // History
    public async Task<List<ReportHistoryDto>> GetReportHistoryAsync(int skip = 0, int take = 50)
    {
        try
        {
            var history = await _localStorage.GetItemAsync<List<ReportHistoryDto>>(HISTORY_KEY);
            return history?.OrderByDescending(h => h.GeneratedAt).Skip(skip).Take(take).ToList()
                ?? new List<ReportHistoryDto>();
        }
        catch
        {
            return new List<ReportHistoryDto>();
        }
    }

    public async Task<ReportHistoryDto?> GetReportHistoryItemAsync(string historyId)
    {
        var history = await GetReportHistoryAsync(0, 1000);
        return history.FirstOrDefault(h => h.Id == historyId);
    }

    // Comparison
    public async Task<Dictionary<string, object>> CompareReportsAsync(string reportType, DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End)
    {
        var filter1 = new ReportFilterModel { FromDate = period1Start, ToDate = period1End };
        var filter2 = new ReportFilterModel { FromDate = period2Start, ToDate = period2End };

        var data1 = await GetReportDataAsync(reportType, filter1);
        var data2 = await GetReportDataAsync(reportType, filter2);

        return new Dictionary<string, object>
        {
            ["period1"] = data1,
            ["period2"] = data2,
            ["comparison"] = CalculateComparison(data1, data2)
        };
    }

    // Helper methods
    private async Task SaveToHistoryAsync(ReportDataDto reportData, string reportTypeId, ReportFilterModel filters)
    {
        try
        {
            var history = await GetReportHistoryAsync(0, 1000);

            // Create configuration JSON
            var config = new
            {
                reportType = reportTypeId,
                filters = new
                {
                    fromDate = filters.FromDate,
                    toDate = filters.ToDate,
                    role = filters.Role,
                    activityType = filters.ActivityType,
                    status = filters.Status,
                    groupBy = filters.GroupBy,
                    sortBy = filters.SortBy,
                    sortOrder = filters.SortOrder
                }
            };
            var configJson = JsonSerializer.Serialize(config);

            var historyItem = new ReportHistoryDto(
                Guid.NewGuid().ToString(),
                reportData.ReportName,
                "Generated",
                "System",
                reportData.GeneratedAt,
                "PDF",
                0, // File size not applicable for on-demand generation
                reportTypeId,
                filters.FromDate,
                filters.ToDate,
                configJson
            );
            history.Insert(0, historyItem);
            await _localStorage.SetItemAsync(HISTORY_KEY, history.Take(100).ToList()); // Keep last 100
        }
        catch
        {
            // Silently fail
        }
    }

    private string GetReportName(string reportType)
    {
        return reportType switch
        {
            "new-registrations" => "New Registrations Trend",
            "user-activity" => "User Activity Heatmap",
            "engagement" => "Engagement Metrics",
            "role-distribution" => "Role Distribution",
            "retention" => "Retention Analysis",
            "performance-metrics" => "Performance Metrics",
            "response-times" => "API Response Times",
            "error-rate" => "Error Rate Analysis",
            "uptime" => "Uptime/Downtime Report",
            "activity-types" => "Top Activities by Type",
            "peak-hours" => "Peak Usage Hours",
            "timeline" => "Activity Timeline",
            "user-journey" => "User Journey Analysis",
            "comparative" => "Comparative Activity",
            _ => "Custom Report"
        };
    }

    private List<ReportTemplateDto> GetDefaultTemplates()
    {
        return new List<ReportTemplateDto>();
    }

    private Dictionary<string, object> CalculateComparison(Dictionary<string, object> data1, Dictionary<string, object> data2)
    {
        var comparison = new Dictionary<string, object>();

        if (data1.TryGetValue("total", out var total1Obj) && data2.TryGetValue("total", out var total2Obj))
        {
            var total1 = Convert.ToDouble(total1Obj);
            var total2 = Convert.ToDouble(total2Obj);
            var change = total1 > 0 ? ((total2 - total1) / total1 * 100) : 0;

            comparison["change"] = change;
            comparison["direction"] = change >= 0 ? "up" : "down";
        }

        return comparison;
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var day = (int)System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(date);
        return System.Globalization.CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
            date.AddDays(4 - (day == 0 ? 7 : day)),
            System.Globalization.CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday);
    }
}

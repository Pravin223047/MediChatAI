using MediChatAI_GraphQl.Core.Entities;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Admin;

public interface IScheduledReportService
{
    // CRUD Operations
    Task<ScheduledReport?> CreateScheduledReportAsync(ScheduledReport schedule);
    Task<ScheduledReport?> GetScheduledReportAsync(string id);
    Task<List<ScheduledReport>> GetAllScheduledReportsAsync();
    Task<List<ScheduledReport>> GetActiveScheduledReportsAsync();
    Task<bool> UpdateScheduledReportAsync(ScheduledReport schedule);
    Task<bool> DeleteScheduledReportAsync(string id);
    
    // Execution
    Task<ScheduledReportExecution?> ExecuteScheduledReportAsync(string scheduleId, bool sendEmail = true);
    Task<List<ScheduledReportExecution>> GetExecutionHistoryAsync(string scheduleId, int take = 10);
    Task<List<ScheduledReportExecution>> GetAllExecutionHistoryAsync(int skip = 0, int take = 50);
    
    // Scheduling
    Task<List<ScheduledReport>> GetDueReportsAsync();
    Task ProcessDueReportsAsync();
    
    // Email
    Task<bool> SendReportEmailAsync(string scheduleId, string reportContent, string format);
}

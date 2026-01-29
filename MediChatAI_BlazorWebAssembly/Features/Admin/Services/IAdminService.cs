using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Admin.Services;

public interface IAdminService
{
    // User Management
    Task<UsersResult?> GetUsersAsync(GetUsersInput input);
    Task<UserDetailsResult?> GetUserDetailsAsync(string userId);
    Task<AdminOperationResult?> UpdateUserStatusAsync(UpdateUserStatusInput input);
    Task<AdminOperationResult?> UpdateUserRoleAsync(UpdateUserRoleInput input);
    Task<AdminOperationResult?> DeleteUserAsync(string userId);

    // Activity Tracking
    Task<UserActivitiesResult?> GetUserActivitiesAsync(GetUserActivitiesInput input);

    // Admin Statistics
    Task<AdminStatsDto?> GetAdminStatsAsync();

    // System Operations
    Task<AdminOperationResult?> SendSystemNotificationAsync(string title, string message, string? targetRole = null);
    Task<AdminOperationResult?> ExportUserDataAsync(string? userId = null);

    // Appointments Management
    Task<AppointmentsResult?> GetAllAppointmentsAsync(int skip = 0, int take = 20, string? searchTerm = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
}
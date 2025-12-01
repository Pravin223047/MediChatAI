using MediChatAI_GraphQl.Features.Admin.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Admin;

public interface IAdminService
{
    // User Management
    Task<UsersResult> GetUsersAsync(GetUsersInput input);
    Task<UserDetailsResult> GetUserDetailsAsync(string userId);
    Task<AdminOperationResult> UpdateUserStatusAsync(string adminUserId, UpdateUserStatusInput input);
    Task<AdminOperationResult> UpdateUserRoleAsync(string adminUserId, UpdateUserRoleInput input);
    Task<AdminOperationResult> DeleteUserAsync(string adminUserId, string userId);

    // Activity Tracking
    Task<UserActivitiesResult> GetUserActivitiesAsync(GetUserActivitiesInput input);

    // Admin Statistics
    Task<AdminStatsDto> GetAdminStatsAsync();

    // System Operations
    Task<AdminOperationResult> SendSystemNotificationAsync(string adminUserId, string title, string message, string? targetRole = null);
    Task<AdminOperationResult> ExportUserDataAsync(string adminUserId, string? userId = null);
}
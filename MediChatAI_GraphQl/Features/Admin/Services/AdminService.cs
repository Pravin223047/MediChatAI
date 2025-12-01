using Microsoft.AspNetCore.Identity;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Admin.DTOs;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Features.Authentication.DTOs;
using MediChatAI_GraphQl.Features.Notifications.DTOs;
using System.Text.Json;

namespace MediChatAI_GraphQl.Features.Admin.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IActivityLoggingService _activityLoggingService;
    private readonly INotificationService _notificationService;

    public AdminService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IActivityLoggingService activityLoggingService,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _activityLoggingService = activityLoggingService;
        _notificationService = notificationService;
    }

    public async Task<UsersResult> GetUsersAsync(GetUsersInput input)
    {
        try
        {
            var query = _context.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(input.SearchTerm))
            {
                var searchLower = input.SearchTerm.ToLower();
                query = query.Where(u =>
                    (u.Email ?? string.Empty).ToLower().Contains(searchLower) ||
                    (u.FirstName ?? string.Empty).ToLower().Contains(searchLower) ||
                    (u.LastName ?? string.Empty).ToLower().Contains(searchLower));
            }

            // Apply role filter
            if (!string.IsNullOrEmpty(input.Role))
            {
                // Optimize: Query only user IDs instead of full user objects
                var roleEntity = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == input.Role);
                if (roleEntity != null)
                {
                    var filteredUserIds = await _context.UserRoles
                        .AsNoTracking()
                        .Where(ur => ur.RoleId == roleEntity.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();
                    query = query.Where(u => filteredUserIds.Contains(u.Id));
                }
                else
                {
                    // Role doesn't exist, return empty query
                    query = query.Where(u => false);
                }
            }

            // Apply active filter
            if (input.IsActive.HasValue)
            {
                query = query.Where(u => !u.LockoutEnd.HasValue == input.IsActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = input.SortBy?.ToLower() switch
            {
                "email" => input.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "firstname" => input.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
                "lastname" => input.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName),
                "lastlogin" => input.SortDescending ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt),
                _ => input.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };

            // Apply pagination and add AsNoTracking for read-only query
            var users = await query
                .AsNoTracking()
                .Skip(input.Skip)
                .Take(input.Take)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.CreatedAt,
                    u.LastLoginAt,
                    u.EmailConfirmed,
                    u.LockoutEnd
                })
                .ToListAsync();

            // Pre-fetch all user roles in a single query to avoid N+1 problem
            var userIds = users.Select(u => u.Id).ToList();
            var userRoles = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, RoleName = r.Name })
                .ToListAsync();

            var userRoleDict = userRoles
                .GroupBy(ur => ur.UserId)
                .ToDictionary(g => g.Key, g => g.First().RoleName ?? "User");

            var userListItems = users.Select(user => new UserListItem(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                userRoleDict.GetValueOrDefault(user.Id, "User"),
                user.EmailConfirmed,
                user.CreatedAt,
                user.LastLoginAt,
                !user.LockoutEnd.HasValue
            )).ToList();

            return new UsersResult(userListItems, totalCount, true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            return new UsersResult(new List<UserListItem>(), 0, false, new[] { ex.Message });
        }
    }

    public async Task<UserDetailsResult> GetUserDetailsAsync(string userId)
    {
        try
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return new UserDetailsResult(null, false, new[] { "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var userDetails = new UserDetailsDto(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                role,
                user.EmailConfirmed,
                user.CreatedAt,
                user.LastLoginAt,
                user.LastProfileUpdate,
                user.ProfileImage,
                user.DateOfBirth,
                user.Gender,
                user.Address,
                user.City,
                user.State,
                user.ZipCode,
                user.Country,
                user.Specialization,
                user.LicenseNumber,
                user.Department,
                user.EmergencyContactName,
                user.EmergencyContactPhone,
                user.BloodType,
                user.Allergies,
                user.MedicalHistory,
                !user.LockoutEnd.HasValue
            );

            return new UserDetailsResult(userDetails, true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            return new UserDetailsResult(null, false, new[] { ex.Message });
        }
    }

    public async Task<AdminOperationResult> UpdateUserStatusAsync(string adminUserId, UpdateUserStatusInput input)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(input.UserId);
            if (user == null)
            {
                return new AdminOperationResult(false, new[] { "User not found" });
            }

            if (input.IsActive)
            {
                // Unlock the user
                await _userManager.SetLockoutEndDateAsync(user, null);
                await _activityLoggingService.LogUserActivityAsync(input.UserId, ActivityTypes.AccountUnlocked,
                    $"Account unlocked by admin", $"Admin: {adminUserId}");

                // Send notification to user
                await _notificationService.CreateNotificationAsync(
                    user.Id,
                    "Account Activated ✓",
                    "Your account has been reactivated by an administrator. You can now log in and access all features.",
                    NotificationType.Success,
                    NotificationCategory.Admin,
                    NotificationPriority.High,
                    "/login",
                    "Login Now");
            }
            else
            {
                // Lock the user
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                await _activityLoggingService.LogUserActivityAsync(input.UserId, ActivityTypes.AccountLocked,
                    $"Account locked by admin", $"Admin: {adminUserId}");

                // Send notification to user
                await _notificationService.CreateNotificationAsync(
                    user.Id,
                    "Account Deactivated",
                    "Your account has been deactivated by an administrator. Please contact support for more information.",
                    NotificationType.Warning,
                    NotificationCategory.Admin,
                    NotificationPriority.Urgent,
                    null,
                    null);
            }

            return new AdminOperationResult(true, Array.Empty<string>(),
                $"User {(input.IsActive ? "activated" : "deactivated")} successfully");
        }
        catch (Exception ex)
        {
            return new AdminOperationResult(false, new[] { ex.Message });
        }
    }

    public async Task<AdminOperationResult> UpdateUserRoleAsync(string adminUserId, UpdateUserRoleInput input)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(input.UserId);
            if (user == null)
            {
                return new AdminOperationResult(false, new[] { "User not found" });
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                return new AdminOperationResult(false, removeResult.Errors.Select(e => e.Description).ToArray());
            }

            var addResult = await _userManager.AddToRoleAsync(user, input.NewRole);
            if (!addResult.Succeeded)
            {
                return new AdminOperationResult(false, addResult.Errors.Select(e => e.Description).ToArray());
            }

            await _activityLoggingService.LogUserActivityAsync(input.UserId, ActivityTypes.RoleChanged,
                $"Role changed from {string.Join(", ", currentRoles)} to {input.NewRole}",
                $"Admin: {adminUserId}");

            // Send notification to user about role change
            var oldRole = currentRoles.FirstOrDefault() ?? "User";
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "Role Updated",
                $"Your account role has been changed from {oldRole} to {input.NewRole} by an administrator.",
                NotificationType.Info,
                NotificationCategory.Admin,
                NotificationPriority.High,
                "/profile",
                "View Profile");

            return new AdminOperationResult(true, Array.Empty<string>(),
                $"User role updated to {input.NewRole} successfully");
        }
        catch (Exception ex)
        {
            return new AdminOperationResult(false, new[] { ex.Message });
        }
    }

    public async Task<AdminOperationResult> DeleteUserAsync(string adminUserId, string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AdminOperationResult(false, new[] { "User not found" });
            }

            // Send notification to user before deletion
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "Account Deletion Notice",
                "Your account has been scheduled for deletion by an administrator. This action cannot be undone.",
                NotificationType.Error,
                NotificationCategory.Admin,
                NotificationPriority.Urgent,
                null,
                null);

            // Log the deletion before actually deleting the user
            await _activityLoggingService.LogUserActivityAsync(userId, ActivityTypes.AccountDeleted,
                "Account deleted by admin", $"Admin: {adminUserId}");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return new AdminOperationResult(false, result.Errors.Select(e => e.Description).ToArray());
            }

            return new AdminOperationResult(true, Array.Empty<string>(), "User deleted successfully");
        }
        catch (Exception ex)
        {
            return new AdminOperationResult(false, new[] { ex.Message });
        }
    }

    public async Task<UserActivitiesResult> GetUserActivitiesAsync(GetUserActivitiesInput input)
    {
        try
        {
            var query = _context.UserActivities.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(input.UserId))
            {
                query = query.Where(a => a.UserId == input.UserId);
            }

            if (!string.IsNullOrEmpty(input.ActivityType))
            {
                query = query.Where(a => a.ActivityType == input.ActivityType);
            }

            if (input.FromDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= input.FromDate.Value);
            }

            if (input.ToDate.HasValue)
            {
                query = query.Where(a => a.Timestamp <= input.ToDate.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = input.SortBy?.ToLower() switch
            {
                "activitytype" => input.SortDescending ?
                    query.OrderByDescending(a => a.ActivityType) : query.OrderBy(a => a.ActivityType),
                "useremail" => input.SortDescending ?
                    query.OrderByDescending(a => a.UserEmail) : query.OrderBy(a => a.UserEmail),
                _ => input.SortDescending ?
                    query.OrderByDescending(a => a.Timestamp) : query.OrderBy(a => a.Timestamp)
            };

            // Apply pagination
            var activities = await query
                .Skip(input.Skip)
                .Take(input.Take)
                .Select(a => new UserActivityDto(
                    a.Id,
                    a.UserId,
                    a.UserEmail,
                    a.UserName,
                    a.UserRole,
                    a.ActivityType,
                    a.Description,
                    a.Details,
                    a.IpAddress,
                    a.Timestamp,
                    a.AdditionalData
                ))
                .ToListAsync();

            return new UserActivitiesResult(activities, totalCount, true, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            return new UserActivitiesResult(new List<UserActivityDto>(), 0, false, new[] { ex.Message });
        }
    }


    public async Task<AdminStatsDto> GetAdminStatsAsync()
    {
        try
        {
            var totalUsers = await _context.Users.CountAsync();

            var patientRole = await _roleManager.FindByNameAsync("Patient");
            var doctorRole = await _roleManager.FindByNameAsync("Doctor");
            var adminRole = await _roleManager.FindByNameAsync("Admin");

            var totalPatients = patientRole != null ?
                await _context.UserRoles.CountAsync(ur => ur.RoleId == patientRole.Id) : 0;
            var totalDoctors = doctorRole != null ?
                await _context.UserRoles.CountAsync(ur => ur.RoleId == doctorRole.Id) : 0;
            var totalAdmins = adminRole != null ?
                await _context.UserRoles.CountAsync(ur => ur.RoleId == adminRole.Id) : 0;

            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);

            var activeUsersToday = await _context.UserActivities
                .Where(a => a.Timestamp >= today)
                .Select(a => a.UserId)
                .Distinct()
                .CountAsync();

            var newUsersThisWeek = await _context.Users
                .CountAsync(u => u.CreatedAt >= weekAgo);

            var totalActivities = await _context.UserActivities.CountAsync();

            return new AdminStatsDto(
                totalUsers,
                totalPatients,
                totalDoctors,
                totalAdmins,
                activeUsersToday,
                newUsersThisWeek,
                totalActivities,
                DateTime.UtcNow // Placeholder for last backup
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get admin statistics: {ex.Message}");
        }
    }

    public async Task<AdminOperationResult> SendSystemNotificationAsync(string adminUserId, string title, string message, string? targetRole = null)
    {
        try
        {
            List<string> targetUserIds = new List<string>();

            if (string.IsNullOrEmpty(targetRole))
            {
                // Send to all users
                targetUserIds = await _context.Users.AsNoTracking().Select(u => u.Id).ToListAsync();
            }
            else
            {
                // Optimize: Query only user IDs instead of full user objects
                var roleEntity = await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Name == targetRole);
                if (roleEntity != null)
                {
                    targetUserIds = await _context.UserRoles
                        .AsNoTracking()
                        .Where(ur => ur.RoleId == roleEntity.Id)
                        .Select(ur => ur.UserId)
                        .ToListAsync();
                }
            }

            // Create notifications for all target users
            await _notificationService.SendBulkNotificationsAsync(
                targetUserIds,
                title,
                message,
                NotificationType.Info,
                NotificationCategory.System,
                NotificationPriority.Normal,
                null,
                null);

            await _activityLoggingService.LogUserActivityAsync(adminUserId, "SystemNotification",
                $"System notification sent: {title}",
                $"Message: {message}, Target: {targetRole ?? "All users"}, Recipients: {targetUserIds.Count}");

            return new AdminOperationResult(true, Array.Empty<string>(),
                $"Notification sent successfully to {targetUserIds.Count} user(s)");
        }
        catch (Exception ex)
        {
            return new AdminOperationResult(false, new[] { ex.Message });
        }
    }

    public async Task<AdminOperationResult> ExportUserDataAsync(string adminUserId, string? userId = null)
    {
        try
        {
            var query = _context.Users.AsQueryable();

            // Filter by specific user if provided
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(u => u.Id == userId);
            }

            var users = await query
                .OrderBy(u => u.CreatedAt)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.EmailConfirmed,
                    u.PhoneNumber,
                    u.DateOfBirth,
                    u.Gender,
                    u.Address,
                    u.City,
                    u.State,
                    u.ZipCode,
                    u.Country,
                    u.ProfileImage,
                    u.Specialization,
                    u.LicenseNumber,
                    u.Department,
                    u.EmergencyContactName,
                    u.EmergencyContactPhone,
                    u.BloodType,
                    u.Allergies,
                    u.MedicalHistory,
                    u.CreatedAt,
                    u.LastLoginAt,
                    u.LastProfileUpdate,
                    u.LockoutEnd
                })
                .ToListAsync();

            // Pre-fetch all user roles in one query to avoid N+1 problem
            var userIds = users.Select(u => u.Id).ToList();
            var userRolesQuery = from ur in _context.UserRoles
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where userIds.Contains(ur.UserId)
                                 select new { ur.UserId, RoleName = r.Name };

            var userRolesDict = (await userRolesQuery.ToListAsync())
                .GroupBy(x => x.UserId)
                .ToDictionary(g => g.Key, g => g.First().RoleName ?? "User");

            // Build CSV content
            var csv = new System.Text.StringBuilder();

            // Header
            csv.AppendLine("ID,Email,First Name,Last Name,Role,Email Confirmed,Phone,Date of Birth,Gender,Address,City,State,ZIP,Country,Specialization,License Number,Department,Emergency Contact,Emergency Phone,Blood Type,Allergies,Is Active,Created At,Last Login,Last Profile Update");

            // Data rows
            foreach (var user in users)
            {
                var role = userRolesDict.ContainsKey(user.Id) ? userRolesDict[user.Id] : "User";
                var isActive = !user.LockoutEnd.HasValue;

                var lastLogin = user.LastLoginAt.ToString("yyyy-MM-dd HH:mm:ss");
                var lastUpdate = user.LastProfileUpdate.HasValue ? user.LastProfileUpdate.Value.ToString("yyyy-MM-dd HH:mm:ss") : "";
                var dateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : "";

                csv.AppendLine(
                    $"\"{user.Id}\"," +
                    $"\"{EscapeCsv(user.Email)}\"," +
                    $"\"{EscapeCsv(user.FirstName)}\"," +
                    $"\"{EscapeCsv(user.LastName)}\"," +
                    $"\"{role}\"," +
                    $"\"{user.EmailConfirmed}\"," +
                    $"\"{EscapeCsv(user.PhoneNumber)}\"," +
                    $"\"{dateOfBirth}\"," +
                    $"\"{EscapeCsv(user.Gender)}\"," +
                    $"\"{EscapeCsv(user.Address)}\"," +
                    $"\"{EscapeCsv(user.City)}\"," +
                    $"\"{EscapeCsv(user.State)}\"," +
                    $"\"{EscapeCsv(user.ZipCode)}\"," +
                    $"\"{EscapeCsv(user.Country)}\"," +
                    $"\"{EscapeCsv(user.Specialization)}\"," +
                    $"\"{EscapeCsv(user.LicenseNumber)}\"," +
                    $"\"{EscapeCsv(user.Department)}\"," +
                    $"\"{EscapeCsv(user.EmergencyContactName)}\"," +
                    $"\"{EscapeCsv(user.EmergencyContactPhone)}\"," +
                    $"\"{EscapeCsv(user.BloodType)}\"," +
                    $"\"{EscapeCsv(user.Allergies)}\"," +
                    $"\"{isActive}\"," +
                    $"\"{user.CreatedAt:yyyy-MM-dd HH:mm:ss}\"," +
                    $"\"{lastLogin}\"," +
                    $"\"{lastUpdate}\""
                );
            }

            var csvContent = csv.ToString();
            var base64Csv = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(csvContent));

            var description = userId != null ?
                $"User data exported for user {userId}" :
                $"All user data exported ({users.Count} users)";

            await _activityLoggingService.LogUserActivityAsync(adminUserId, ActivityTypes.DataExport, description);

            return new AdminOperationResult(true, Array.Empty<string>(), base64Csv);
        }
        catch (Exception ex)
        {
            return new AdminOperationResult(false, new[] { ex.Message });
        }
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
    }
}
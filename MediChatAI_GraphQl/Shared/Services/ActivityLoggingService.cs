using Microsoft.AspNetCore.Identity;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Entities;

namespace MediChatAI_GraphQl.Shared.Services;

public class ActivityLoggingService : IActivityLoggingService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ActivityLoggingService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task LogUserActivityAsync(string userId, string activityType, string description,
        string? details = null, string? ipAddress = null, string? userAgent = null, string? additionalData = null)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "User";

            var activity = new UserActivity
            {
                UserId = userId,
                UserEmail = user.Email ?? string.Empty,
                UserName = $"{user.FirstName} {user.LastName}".Trim(),
                UserRole = role,
                ActivityType = activityType,
                Description = description,
                Details = details,
                IpAddress = ipAddress ?? "Unknown",
                UserAgent = userAgent ?? "Unknown",
                AdditionalData = additionalData,
                Timestamp = DateTime.UtcNow
            };

            _context.UserActivities.Add(activity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't throw to avoid breaking the main operation
            Console.WriteLine($"Failed to log user activity: {ex.Message}");
        }
    }
}
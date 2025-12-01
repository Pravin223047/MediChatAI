using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public class UserActivity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string? UserId { get; set; }

    public string UserEmail { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string UserRole { get; set; } = string.Empty;

    public string ActivityType { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Details { get; set; }

    public string IpAddress { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? AdditionalData { get; set; }

    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}

public static class ActivityTypes
{
    public const string Login = "Login";
    public const string Logout = "Logout";
    public const string Register = "Register";
    public const string ProfileUpdate = "ProfileUpdate";
    public const string PasswordChange = "PasswordChange";
    public const string EmailVerification = "EmailVerification";
    public const string PasswordReset = "PasswordReset";
    public const string AccountLocked = "AccountLocked";
    public const string AccountUnlocked = "AccountUnlocked";
    public const string RoleChanged = "RoleChanged";
    public const string AccountDeleted = "AccountDeleted";
    public const string DataExport = "DataExport";
    public const string SystemAccess = "SystemAccess";
}
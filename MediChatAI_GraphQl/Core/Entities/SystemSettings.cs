using System.ComponentModel.DataAnnotations;

namespace MediChatAI_GraphQl.Core.Entities;

public class SystemSettings
{
    [Key]
    public int Id { get; set; }

    // General Settings
    public string SiteName { get; set; } = "MediChat.AI";
    public string SiteDescription { get; set; } = "Healthcare Communication Platform";
    public string ContactEmail { get; set; } = "contact@medichat.ai";
    public string ContactPhone { get; set; } = "";
    public string Timezone { get; set; } = "UTC";
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public string TimeFormat { get; set; } = "12-hour";

    // Security Settings
    public int SessionTimeoutMinutes { get; set; } = 30;
    public int MaxLoginAttempts { get; set; } = 5;
    public int PasswordMinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireNumbers { get; set; } = true;
    public bool RequireSpecialChars { get; set; } = true;
    public bool EnableTwoFactor { get; set; } = false;
    public bool RequireEmailVerification { get; set; } = true;
    public int PasswordExpiryDays { get; set; } = 90;
    public int AccountLockoutMinutes { get; set; } = 15;

    // SMTP Email Settings
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public bool SmtpEnableSsl { get; set; } = true;
    public string SmtpFromEmail { get; set; } = "";
    public string SmtpFromName { get; set; } = "MediChat.AI";
    public bool SmtpIsConfigured { get; set; } = false;
    public DateTime? SmtpLastTestedAt { get; set; }
    public bool SmtpLastTestSuccessful { get; set; } = false;
    public string? SmtpLastTestMessage { get; set; }

    // Email Template Theme Settings
    public string EmailTemplateTheme { get; set; } = "Professional"; // Professional, Modern, Minimal, Medical
    public string EmailPrimaryColor { get; set; } = "#667eea";
    public string EmailSecondaryColor { get; set; } = "#764ba2";
    public string EmailBackgroundColor { get; set; } = "#f8fafc";
    public string EmailTextColor { get; set; } = "#333333";
    public string EmailHeaderImageUrl { get; set; } = "";
    public bool EmailIncludeFooter { get; set; } = true;
    public string EmailFooterText { get; set; } = "This is an automated email from MediChat.AI. Please do not reply.";
    public bool EmailIncludeSocialLinks { get; set; } = false;
    public string? EmailFacebookUrl { get; set; }
    public string? EmailTwitterUrl { get; set; }
    public string? EmailLinkedInUrl { get; set; }
    public string? EmailInstagramUrl { get; set; }

    // Notification Settings
    public bool EnableEmailNotifications { get; set; } = true;
    public bool NotifyAdminOnNewDoctor { get; set; } = true;
    public bool NotifyDoctorOnApproval { get; set; } = true;
    public bool NotifyUserOnLogin { get; set; } = true;
    public bool NotifyUserOnPasswordChange { get; set; } = true;
    public bool NotifyUserOn2FAChange { get; set; } = true;

    // Appearance Settings
    public string ThemeMode { get; set; } = "light"; // light, dark, system
    public string ThemePrimaryColor { get; set; } = "#3b82f6"; // Blue
    public string ThemeAccentColor { get; set; } = "#8b5cf6"; // Purple
    public string ThemeBackgroundColor { get; set; } = "#f8fafc"; // Light gray
    public string ThemeTextColor { get; set; } = "#1f2937"; // Dark gray
    public string ThemeSidebarStyle { get; set; } = "expanded"; // expanded, collapsed, floating
    public string ThemeFontSize { get; set; } = "medium"; // small, medium, large
    public bool ThemeEnableAnimations { get; set; } = true;
    public bool ThemeCompactMode { get; set; } = false;
    public string ThemePreset { get; set; } = "default"; // default, medical, forest, royal, sunset, custom
    public bool ThemeHighContrast { get; set; } = false;
    public string ThemeBorderRadius { get; set; } = "medium"; // none, small, medium, large

    // Session Management
    public int SessionVersion { get; set; } = 1;
    public DateTime? SessionVersionUpdatedAt { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? LastUpdatedBy { get; set; }
}

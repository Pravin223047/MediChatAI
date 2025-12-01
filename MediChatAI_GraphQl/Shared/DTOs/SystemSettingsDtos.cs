namespace MediChatAI_GraphQl.Shared.DTOs;

// Input for updating SMTP settings
public record UpdateSmtpSettingsInput(
    string SmtpServer,
    int SmtpPort,
    string SmtpUsername,
    string SmtpPassword,
    bool SmtpEnableSsl,
    string SmtpFromEmail,
    string SmtpFromName
);

// Input for updating email template theme
public record UpdateEmailThemeInput(
    string EmailTemplateTheme,
    string EmailPrimaryColor,
    string EmailSecondaryColor,
    string EmailBackgroundColor,
    string EmailTextColor,
    string EmailHeaderImageUrl,
    bool EmailIncludeFooter,
    string EmailFooterText,
    bool EmailIncludeSocialLinks,
    string? EmailFacebookUrl,
    string? EmailTwitterUrl,
    string? EmailLinkedInUrl,
    string? EmailInstagramUrl
);

// Input for updating notification settings
public record UpdateNotificationSettingsInput(
    bool EnableEmailNotifications,
    bool NotifyAdminOnNewDoctor,
    bool NotifyDoctorOnApproval,
    bool NotifyUserOnLogin,
    bool NotifyUserOnPasswordChange,
    bool NotifyUserOn2FAChange
);

// Input for updating general settings
public record UpdateGeneralSettingsInput(
    string SiteName,
    string SiteDescription,
    string ContactEmail,
    string ContactPhone,
    string Timezone,
    string DateFormat,
    string TimeFormat
);

// Input for updating security settings
public record UpdateSecuritySettingsInput(
    int SessionTimeoutMinutes,
    int MaxLoginAttempts,
    int PasswordMinLength,
    bool RequireUppercase,
    bool RequireLowercase,
    bool RequireNumbers,
    bool RequireSpecialChars,
    bool EnableTwoFactor,
    bool RequireEmailVerification,
    int PasswordExpiryDays,
    int AccountLockoutMinutes
);

// Result for SMTP test
public record SmtpTestResult(
    bool Success,
    string Message,
    DateTime TestedAt
);

// Result for settings update
public record SettingsUpdateResult(
    bool Success,
    string Message,
    DateTime UpdatedAt
);

// Complete settings data for queries
public class SystemSettingsData
{
    // General Settings
    public string SiteName { get; set; } = "";
    public string SiteDescription { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Timezone { get; set; } = "";
    public string DateFormat { get; set; } = "";
    public string TimeFormat { get; set; } = "";

    // Security Settings
    public int SessionTimeoutMinutes { get; set; }
    public int MaxLoginAttempts { get; set; }
    public int PasswordMinLength { get; set; }
    public bool RequireUppercase { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireNumbers { get; set; }
    public bool RequireSpecialChars { get; set; }
    public bool EnableTwoFactor { get; set; }
    public bool RequireEmailVerification { get; set; }
    public int PasswordExpiryDays { get; set; }
    public int AccountLockoutMinutes { get; set; }
    public int SessionVersion { get; set; }
    public DateTime? SessionVersionUpdatedAt { get; set; }

    // SMTP Settings (password excluded for security)
    public string SmtpServer { get; set; } = "";
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = "";
    public bool SmtpEnableSsl { get; set; }
    public string SmtpFromEmail { get; set; } = "";
    public string SmtpFromName { get; set; } = "";
    public bool SmtpIsConfigured { get; set; }
    public DateTime? SmtpLastTestedAt { get; set; }
    public bool SmtpLastTestSuccessful { get; set; }
    public string? SmtpLastTestMessage { get; set; }

    // Email Template Theme Settings
    public string EmailTemplateTheme { get; set; } = "";
    public string EmailPrimaryColor { get; set; } = "";
    public string EmailSecondaryColor { get; set; } = "";
    public string EmailBackgroundColor { get; set; } = "";
    public string EmailTextColor { get; set; } = "";
    public string EmailHeaderImageUrl { get; set; } = "";
    public bool EmailIncludeFooter { get; set; }
    public string EmailFooterText { get; set; } = "";
    public bool EmailIncludeSocialLinks { get; set; }
    public string? EmailFacebookUrl { get; set; }
    public string? EmailTwitterUrl { get; set; }
    public string? EmailLinkedInUrl { get; set; }
    public string? EmailInstagramUrl { get; set; }

    // Notification Settings
    public bool EnableEmailNotifications { get; set; }
    public bool NotifyAdminOnNewDoctor { get; set; }
    public bool NotifyDoctorOnApproval { get; set; }
    public bool NotifyUserOnLogin { get; set; }
    public bool NotifyUserOnPasswordChange { get; set; }
    public bool NotifyUserOn2FAChange { get; set; }

    // Appearance Settings
    public string ThemeMode { get; set; } = "";
    public string ThemePrimaryColor { get; set; } = "";
    public string ThemeAccentColor { get; set; } = "";
    public string ThemeBackgroundColor { get; set; } = "";
    public string ThemeTextColor { get; set; } = "";
    public string ThemeSidebarStyle { get; set; } = "";
    public string ThemeFontSize { get; set; } = "";
    public bool ThemeEnableAnimations { get; set; }
    public bool ThemeCompactMode { get; set; }
    public string ThemePreset { get; set; } = "";
    public bool ThemeHighContrast { get; set; }
    public string ThemeBorderRadius { get; set; } = "";

    // Metadata
    public DateTime UpdatedAt { get; set; }
    public string? LastUpdatedBy { get; set; }
}

// Email template preview input
public record EmailTemplatePreviewInput(
    string TemplateName, // EmailVerification, PasswordReset, etc.
    string Theme,
    string PrimaryColor,
    string SecondaryColor,
    string BackgroundColor,
    string TextColor
);

// Email template preview result
public record EmailTemplatePreviewResult(
    bool Success,
    string? HtmlContent,
    string? ErrorMessage
);

// Input for updating appearance settings
public record UpdateAppearanceSettingsInput(
    string ThemeMode,
    string ThemePrimaryColor,
    string ThemeAccentColor,
    string ThemeBackgroundColor,
    string ThemeTextColor,
    string ThemeSidebarStyle,
    string ThemeFontSize,
    bool ThemeEnableAnimations,
    bool ThemeCompactMode,
    string ThemePreset,
    bool ThemeHighContrast,
    string ThemeBorderRadius
);

// Public general settings data - accessible to all users (including unauthenticated)
public class GeneralSettingsData
{
    public string SiteName { get; set; } = "MediChat.AI";
    public string SiteDescription { get; set; } = "Healthcare Communication Platform";
    public string ContactEmail { get; set; } = "contact@medichat.ai";
    public string ContactPhone { get; set; } = "";
    public string Timezone { get; set; } = "UTC";
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public string TimeFormat { get; set; } = "12-hour";
}

// Public appearance settings data - accessible to all users
public class AppearanceSettingsData
{
    public string ThemeMode { get; set; } = "light";
    public string ThemePrimaryColor { get; set; } = "#3b82f6";
    public string ThemeAccentColor { get; set; } = "#8b5cf6";
    public string ThemeBackgroundColor { get; set; } = "#f8fafc";
    public string ThemeTextColor { get; set; } = "#1f2937";
    public string ThemeSidebarStyle { get; set; } = "expanded";
    public string ThemeFontSize { get; set; } = "medium";
    public bool ThemeEnableAnimations { get; set; } = true;
    public bool ThemeCompactMode { get; set; } = false;
    public string ThemePreset { get; set; } = "default";
    public bool ThemeHighContrast { get; set; } = false;
    public string ThemeBorderRadius { get; set; } = "medium";
}

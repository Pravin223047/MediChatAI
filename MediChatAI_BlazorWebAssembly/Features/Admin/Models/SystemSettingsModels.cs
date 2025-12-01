namespace MediChatAI_BlazorWebAssembly.Features.Admin.Models;

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

// Input for updating SMTP settings
public class UpdateSmtpSettingsInput
{
    public string SmtpServer { get; set; } = "";
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = "";
    public string SmtpPassword { get; set; } = "";
    public bool SmtpEnableSsl { get; set; }
    public string SmtpFromEmail { get; set; } = "";
    public string SmtpFromName { get; set; } = "";

    public UpdateSmtpSettingsInput() { }

    public UpdateSmtpSettingsInput(string smtpServer, int smtpPort, string smtpUsername,
        string smtpPassword, bool smtpEnableSsl, string smtpFromEmail, string smtpFromName)
    {
        SmtpServer = smtpServer;
        SmtpPort = smtpPort;
        SmtpUsername = smtpUsername;
        SmtpPassword = smtpPassword;
        SmtpEnableSsl = smtpEnableSsl;
        SmtpFromEmail = smtpFromEmail;
        SmtpFromName = smtpFromName;
    }
}

// Input for updating email template theme
public class UpdateEmailThemeInput
{
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

    public UpdateEmailThemeInput() { }

    public UpdateEmailThemeInput(string emailTemplateTheme, string emailPrimaryColor, string emailSecondaryColor,
        string emailBackgroundColor, string emailTextColor, string emailHeaderImageUrl, bool emailIncludeFooter,
        string emailFooterText, bool emailIncludeSocialLinks, string? emailFacebookUrl, string? emailTwitterUrl,
        string? emailLinkedInUrl, string? emailInstagramUrl)
    {
        EmailTemplateTheme = emailTemplateTheme;
        EmailPrimaryColor = emailPrimaryColor;
        EmailSecondaryColor = emailSecondaryColor;
        EmailBackgroundColor = emailBackgroundColor;
        EmailTextColor = emailTextColor;
        EmailHeaderImageUrl = emailHeaderImageUrl;
        EmailIncludeFooter = emailIncludeFooter;
        EmailFooterText = emailFooterText;
        EmailIncludeSocialLinks = emailIncludeSocialLinks;
        EmailFacebookUrl = emailFacebookUrl;
        EmailTwitterUrl = emailTwitterUrl;
        EmailLinkedInUrl = emailLinkedInUrl;
        EmailInstagramUrl = emailInstagramUrl;
    }
}

// Input for updating notification settings
public class UpdateNotificationSettingsInput
{
    public bool EnableEmailNotifications { get; set; }
    public bool NotifyAdminOnNewDoctor { get; set; }
    public bool NotifyDoctorOnApproval { get; set; }
    public bool NotifyUserOnLogin { get; set; }
    public bool NotifyUserOnPasswordChange { get; set; }
    public bool NotifyUserOn2FAChange { get; set; }

    public UpdateNotificationSettingsInput() { }

    public UpdateNotificationSettingsInput(bool enableEmailNotifications, bool notifyAdminOnNewDoctor,
        bool notifyDoctorOnApproval, bool notifyUserOnLogin, bool notifyUserOnPasswordChange, bool notifyUserOn2FAChange)
    {
        EnableEmailNotifications = enableEmailNotifications;
        NotifyAdminOnNewDoctor = notifyAdminOnNewDoctor;
        NotifyDoctorOnApproval = notifyDoctorOnApproval;
        NotifyUserOnLogin = notifyUserOnLogin;
        NotifyUserOnPasswordChange = notifyUserOnPasswordChange;
        NotifyUserOn2FAChange = notifyUserOn2FAChange;
    }
}

// Result for settings update
public record SettingsUpdateResult(
    bool Success,
    string Message,
    DateTime UpdatedAt
);

// Result for SMTP test
public record SmtpTestResult(
    bool Success,
    string Message,
    DateTime TestedAt
);

// Email template preview input
public record EmailTemplatePreviewInput(
    string TemplateName,
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
public class UpdateAppearanceSettingsInput
{
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

    public UpdateAppearanceSettingsInput() { }

    public UpdateAppearanceSettingsInput(string themeMode, string themePrimaryColor, string themeAccentColor,
        string themeBackgroundColor, string themeTextColor, string themeSidebarStyle, string themeFontSize,
        bool themeEnableAnimations, bool themeCompactMode, string themePreset, bool themeHighContrast, string themeBorderRadius)
    {
        ThemeMode = themeMode;
        ThemePrimaryColor = themePrimaryColor;
        ThemeAccentColor = themeAccentColor;
        ThemeBackgroundColor = themeBackgroundColor;
        ThemeTextColor = themeTextColor;
        ThemeSidebarStyle = themeSidebarStyle;
        ThemeFontSize = themeFontSize;
        ThemeEnableAnimations = themeEnableAnimations;
        ThemeCompactMode = themeCompactMode;
        ThemePreset = themePreset;
        ThemeHighContrast = themeHighContrast;
        ThemeBorderRadius = themeBorderRadius;
    }
}

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

// GraphQL Response Wrappers
public record GetSystemSettingsResponse(SystemSettingsData SystemSettings);
public record GetPublicSettingsResponse(GeneralSettingsData PublicSettings);
public record GetAppearanceSettingsResponse(AppearanceSettingsData AppearanceSettings);
public record UpdateSmtpSettingsResponse(SettingsUpdateResult UpdateSmtpSettings);
public record UpdateEmailThemeResponse(SettingsUpdateResult UpdateEmailThemeSettings);
public record UpdateNotificationSettingsResponse(SettingsUpdateResult UpdateNotificationSettings);
public record UpdateAppearanceSettingsResponse(SettingsUpdateResult UpdateAppearanceSettings);
public record TestSmtpConnectionResponse(SmtpTestResult TestSmtpConnection);
public record PreviewEmailTemplateResponse(EmailTemplatePreviewResult PreviewEmailTemplate);

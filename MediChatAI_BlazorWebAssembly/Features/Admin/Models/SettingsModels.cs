namespace MediChatAI_BlazorWebAssembly.Features.Admin.Models;

public record GeneralSettingsDto(
    string SiteName,
    string SiteDescription,
    string ContactEmail,
    string ContactPhone,
    string Timezone,
    string DateFormat,
    string TimeFormat
);

public class UpdateGeneralSettingsInput
{
    public string SiteName { get; set; } = "";
    public string SiteDescription { get; set; } = "";
    public string ContactEmail { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string Timezone { get; set; } = "";
    public string DateFormat { get; set; } = "";
    public string TimeFormat { get; set; } = "";

    public UpdateGeneralSettingsInput() { }

    public UpdateGeneralSettingsInput(string siteName, string siteDescription, string contactEmail,
        string contactPhone, string timezone, string dateFormat, string timeFormat)
    {
        SiteName = siteName;
        SiteDescription = siteDescription;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        Timezone = timezone;
        DateFormat = dateFormat;
        TimeFormat = timeFormat;
    }
}

public record SecuritySettingsDto(
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

public class UpdateSecuritySettingsInput
{
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

    public UpdateSecuritySettingsInput() { }

    public UpdateSecuritySettingsInput(int sessionTimeoutMinutes, int maxLoginAttempts, int passwordMinLength,
        bool requireUppercase, bool requireLowercase, bool requireNumbers, bool requireSpecialChars,
        bool enableTwoFactor, bool requireEmailVerification, int passwordExpiryDays, int accountLockoutMinutes)
    {
        SessionTimeoutMinutes = sessionTimeoutMinutes;
        MaxLoginAttempts = maxLoginAttempts;
        PasswordMinLength = passwordMinLength;
        RequireUppercase = requireUppercase;
        RequireLowercase = requireLowercase;
        RequireNumbers = requireNumbers;
        RequireSpecialChars = requireSpecialChars;
        EnableTwoFactor = enableTwoFactor;
        RequireEmailVerification = requireEmailVerification;
        PasswordExpiryDays = passwordExpiryDays;
        AccountLockoutMinutes = accountLockoutMinutes;
    }
}

public record AppearanceSettingsDto(
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

// GraphQL Response Wrappers
public record GetGeneralSettingsResponse(GeneralSettingsDto GeneralSettings);
public record UpdateGeneralSettingsResponse(SettingsUpdateResult UpdateGeneralSettings);
public record GetSecuritySettingsResponse(SecuritySettingsDto SecuritySettings);
public record UpdateSecuritySettingsResponse(SettingsUpdateResult UpdateSecuritySettings);

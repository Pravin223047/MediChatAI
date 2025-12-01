using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

namespace MediChatAI_BlazorWebAssembly.Features.Admin.Services;

public class SettingsService : ISettingsService
{
    private readonly IGraphQLService _graphQLService;

    public SettingsService(IGraphQLService graphQLService)
    {
        _graphQLService = graphQLService;
    }

    /// <summary>
    /// Gets all system settings in a single query (requires admin authorization)
    /// </summary>
    public async Task<SystemSettingsData?> GetSystemSettingsAsync()
    {
        var query = @"
            query {
                systemSettings {
                    siteName
                    siteDescription
                    contactEmail
                    contactPhone
                    timezone
                    dateFormat
                    timeFormat
                    sessionTimeoutMinutes
                    maxLoginAttempts
                    passwordMinLength
                    requireUppercase
                    requireLowercase
                    requireNumbers
                    requireSpecialChars
                    enableTwoFactor
                    requireEmailVerification
                    passwordExpiryDays
                    accountLockoutMinutes
                    smtpServer
                    smtpPort
                    smtpUsername
                    smtpEnableSsl
                    smtpFromEmail
                    smtpFromName
                    smtpIsConfigured
                    smtpLastTestedAt
                    smtpLastTestSuccessful
                    smtpLastTestMessage
                    emailTemplateTheme
                    emailPrimaryColor
                    emailSecondaryColor
                    emailBackgroundColor
                    emailTextColor
                    emailHeaderImageUrl
                    emailIncludeFooter
                    emailFooterText
                    emailIncludeSocialLinks
                    emailFacebookUrl
                    emailTwitterUrl
                    emailLinkedInUrl
                    emailInstagramUrl
                    enableEmailNotifications
                    notifyAdminOnNewDoctor
                    notifyDoctorOnApproval
                    notifyUserOnLogin
                    notifyUserOnPasswordChange
                    notifyUserOn2FAChange
                    themeMode
                    themePrimaryColor
                    themeAccentColor
                    themeBackgroundColor
                    themeTextColor
                    themeSidebarStyle
                    themeFontSize
                    themeEnableAnimations
                    themeCompactMode
                    themePreset
                    themeHighContrast
                    themeBorderRadius
                    updatedAt
                    lastUpdatedBy
                }
            }";

        var response = await _graphQLService.SendQueryAsync<GetSystemSettingsResponse>(query);
        return response?.SystemSettings;
    }

    /// <summary>
    /// Gets public/general settings (site name, description, contact info, etc.)
    /// Public endpoint - no authorization required - accessible to all users
    /// </summary>
    public async Task<GeneralSettingsData?> GetPublicSettingsAsync()
    {
        var query = @"
            query {
                publicSettings {
                    siteName
                    siteDescription
                    contactEmail
                    contactPhone
                    timezone
                    dateFormat
                    timeFormat
                }
            }";

        var response = await _graphQLService.SendQueryAsync<GetPublicSettingsResponse>(query);
        return response?.PublicSettings;
    }

    /// <summary>
    /// Gets appearance/theme settings only (public - no authorization required)
    /// </summary>
    public async Task<AppearanceSettingsData?> GetAppearanceSettingsAsync()
    {
        var query = @"
            query {
                appearanceSettings {
                    themeMode
                    themePrimaryColor
                    themeAccentColor
                    themeBackgroundColor
                    themeTextColor
                    themeSidebarStyle
                    themeFontSize
                    themeEnableAnimations
                    themeCompactMode
                    themePreset
                    themeHighContrast
                    themeBorderRadius
                }
            }";

        var response = await _graphQLService.SendQueryAsync<GetAppearanceSettingsResponse>(query);
        return response?.AppearanceSettings;
    }

    /// <summary>
    /// Updates general settings (site info, contact, format preferences)
    /// </summary>
    public async Task<SettingsUpdateResult?> UpdateGeneralSettingsAsync(UpdateGeneralSettingsInput input)
    {
        var mutation = @"
            mutation UpdateGeneralSettings($input: UpdateGeneralSettingsInput!) {
                updateGeneralSettings(input: $input) {
                    success
                    message
                    updatedAt
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<UpdateGeneralSettingsResponse>(mutation, variables);
        return response?.UpdateGeneralSettings;
    }

    /// <summary>
    /// Updates security settings (password policy, lockout, session timeout)
    /// </summary>
    public async Task<SettingsUpdateResult?> UpdateSecuritySettingsAsync(UpdateSecuritySettingsInput input)
    {
        var mutation = @"
            mutation UpdateSecuritySettings($input: UpdateSecuritySettingsInput!) {
                updateSecuritySettings(input: $input) {
                    success
                    message
                    updatedAt
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<UpdateSecuritySettingsResponse>(mutation, variables);
        return response?.UpdateSecuritySettings;
    }

    /// <summary>
    /// Updates SMTP settings for email delivery
    /// </summary>
    public async Task<SettingsUpdateResult?> UpdateSmtpSettingsAsync(UpdateSmtpSettingsInput input)
    {
        var mutation = @"
            mutation UpdateSmtpSettings($input: UpdateSmtpSettingsInput!) {
                updateSmtpSettings(input: $input) {
                    success
                    message
                    updatedAt
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<UpdateSmtpSettingsResponse>(mutation, variables);
        return response?.UpdateSmtpSettings;
    }

    /// <summary>
    /// Updates email template theme (colors, branding, footer)
    /// </summary>
    public async Task<SettingsUpdateResult?> UpdateEmailThemeSettingsAsync(UpdateEmailThemeInput input)
    {
        var mutation = @"
            mutation UpdateEmailThemeSettings($input: UpdateEmailThemeInput!) {
                updateEmailThemeSettings(input: $input) {
                    success
                    message
                    updatedAt
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<UpdateEmailThemeResponse>(mutation, variables);
        return response?.UpdateEmailThemeSettings;
    }

    /// <summary>
    /// Updates notification preferences for system events
    /// </summary>
    public async Task<SettingsUpdateResult?> UpdateNotificationSettingsAsync(UpdateNotificationSettingsInput input)
    {
        var mutation = @"
            mutation UpdateNotificationSettings($input: UpdateNotificationSettingsInput!) {
                updateNotificationSettings(input: $input) {
                    success
                    message
                    updatedAt
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<UpdateNotificationSettingsResponse>(mutation, variables);
        return response?.UpdateNotificationSettings;
    }

    /// <summary>
    /// Updates appearance settings (theme, colors, UI preferences)
    /// </summary>
    public async Task<SettingsUpdateResult?> UpdateAppearanceSettingsAsync(UpdateAppearanceSettingsInput input)
    {
        var mutation = @"
            mutation UpdateAppearanceSettings($input: UpdateAppearanceSettingsInput!) {
                updateAppearanceSettings(input: $input) {
                    success
                    message
                    updatedAt
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<UpdateAppearanceSettingsResponse>(mutation, variables);
        return response?.UpdateAppearanceSettings;
    }

    /// <summary>
    /// Tests the SMTP connection by sending a test email
    /// </summary>
    public async Task<SmtpTestResult?> TestSmtpConnectionAsync()
    {
        var mutation = @"
            mutation {
                testSmtpConnection {
                    success
                    message
                    testedAt
                }
            }";

        var response = await _graphQLService.SendQueryAsync<TestSmtpConnectionResponse>(mutation);
        return response?.TestSmtpConnection;
    }

    /// <summary>
    /// Previews an email template with custom theme settings
    /// </summary>
    public async Task<EmailTemplatePreviewResult?> PreviewEmailTemplateAsync(
        string templateName,
        string theme,
        string primaryColor,
        string secondaryColor)
    {
        var query = @"
            query PreviewEmailTemplate(
                $templateName: String!
                $theme: String!
                $primaryColor: String!
                $secondaryColor: String!
            ) {
                previewEmailTemplate(
                    templateName: $templateName
                    theme: $theme
                    primaryColor: $primaryColor
                    secondaryColor: $secondaryColor
                ) {
                    success
                    htmlContent
                    errorMessage
                }
            }";

        var variables = new
        {
            templateName,
            theme,
            primaryColor,
            secondaryColor
        };

        var response = await _graphQLService.SendQueryAsync<PreviewEmailTemplateResponse>(query, variables);
        return response?.PreviewEmailTemplate;
    }
}

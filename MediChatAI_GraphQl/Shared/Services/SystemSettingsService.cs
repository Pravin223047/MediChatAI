using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Admin.DTOs;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Features.Authentication.DTOs;
using MediChatAI_GraphQl.Features.Notifications.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Mail;

namespace MediChatAI_GraphQl.Shared.Services;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLoggingService _activityLogger;
    private readonly ILogger<SystemSettingsService> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly INotificationService _notificationService;
    private readonly IMemoryCache _cache;
    private const string SETTINGS_CACHE_KEY = "SystemSettings";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(5);

    public SystemSettingsService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IActivityLoggingService activityLogger,
        ILogger<SystemSettingsService> logger,
        IWebHostEnvironment environment,
        INotificationService notificationService,
        IMemoryCache cache)
    {
        _context = context;
        _userManager = userManager;
        _activityLogger = activityLogger;
        _logger = logger;
        _environment = environment;
        _notificationService = notificationService;
        _cache = cache;
    }

    public async Task<SystemSettings> GetSystemSettingsAsync()
    {
        // Try to get from cache first
        if (_cache.TryGetValue(SETTINGS_CACHE_KEY, out SystemSettings? cachedSettings) && cachedSettings != null)
        {
            return cachedSettings;
        }

        // Get or create the settings record (we only have one)
        var settings = await _context.SystemSettings.AsNoTracking().FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new SystemSettings
            {
                SiteName = "MediChat.AI",
                SiteDescription = "Healthcare Communication Platform",
                ContactEmail = "contact@medichat.ai",
                ContactPhone = "",
                Timezone = "UTC",
                DateFormat = "MM/dd/yyyy",
                TimeFormat = "12-hour",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SystemSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        // Cache the settings for 5 minutes
        _cache.Set(SETTINGS_CACHE_KEY, settings, CACHE_DURATION);

        return settings;
    }

    private void InvalidateCache()
    {
        _cache.Remove(SETTINGS_CACHE_KEY);
    }

    public async Task<SystemSettingsData> GetSystemSettingsDataAsync()
    {
        var settings = await GetSystemSettingsAsync();

        return new SystemSettingsData
        {
            SiteName = settings.SiteName,
            SiteDescription = settings.SiteDescription,
            ContactEmail = settings.ContactEmail,
            ContactPhone = settings.ContactPhone,
            Timezone = settings.Timezone,
            DateFormat = settings.DateFormat,
            TimeFormat = settings.TimeFormat,
            SessionTimeoutMinutes = settings.SessionTimeoutMinutes,
            MaxLoginAttempts = settings.MaxLoginAttempts,
            PasswordMinLength = settings.PasswordMinLength,
            RequireUppercase = settings.RequireUppercase,
            RequireLowercase = settings.RequireLowercase,
            RequireNumbers = settings.RequireNumbers,
            RequireSpecialChars = settings.RequireSpecialChars,
            EnableTwoFactor = settings.EnableTwoFactor,
            RequireEmailVerification = settings.RequireEmailVerification,
            PasswordExpiryDays = settings.PasswordExpiryDays,
            AccountLockoutMinutes = settings.AccountLockoutMinutes,
            SmtpServer = settings.SmtpServer,
            SmtpPort = settings.SmtpPort,
            SmtpUsername = settings.SmtpUsername,
            SmtpEnableSsl = settings.SmtpEnableSsl,
            SmtpFromEmail = settings.SmtpFromEmail,
            SmtpFromName = settings.SmtpFromName,
            SmtpIsConfigured = settings.SmtpIsConfigured,
            SmtpLastTestedAt = settings.SmtpLastTestedAt,
            SmtpLastTestSuccessful = settings.SmtpLastTestSuccessful,
            SmtpLastTestMessage = settings.SmtpLastTestMessage,
            EmailTemplateTheme = settings.EmailTemplateTheme,
            EmailPrimaryColor = settings.EmailPrimaryColor,
            EmailSecondaryColor = settings.EmailSecondaryColor,
            EmailBackgroundColor = settings.EmailBackgroundColor,
            EmailTextColor = settings.EmailTextColor,
            EmailHeaderImageUrl = settings.EmailHeaderImageUrl,
            EmailIncludeFooter = settings.EmailIncludeFooter,
            EmailFooterText = settings.EmailFooterText,
            EmailIncludeSocialLinks = settings.EmailIncludeSocialLinks,
            EmailFacebookUrl = settings.EmailFacebookUrl,
            EmailTwitterUrl = settings.EmailTwitterUrl,
            EmailLinkedInUrl = settings.EmailLinkedInUrl,
            EmailInstagramUrl = settings.EmailInstagramUrl,
            EnableEmailNotifications = settings.EnableEmailNotifications,
            NotifyAdminOnNewDoctor = settings.NotifyAdminOnNewDoctor,
            NotifyDoctorOnApproval = settings.NotifyDoctorOnApproval,
            NotifyUserOnLogin = settings.NotifyUserOnLogin,
            NotifyUserOnPasswordChange = settings.NotifyUserOnPasswordChange,
            NotifyUserOn2FAChange = settings.NotifyUserOn2FAChange,
            ThemeMode = settings.ThemeMode,
            ThemePrimaryColor = settings.ThemePrimaryColor,
            ThemeAccentColor = settings.ThemeAccentColor,
            ThemeBackgroundColor = settings.ThemeBackgroundColor,
            ThemeTextColor = settings.ThemeTextColor,
            ThemeSidebarStyle = settings.ThemeSidebarStyle,
            ThemeFontSize = settings.ThemeFontSize,
            ThemeEnableAnimations = settings.ThemeEnableAnimations,
            ThemeCompactMode = settings.ThemeCompactMode,
            ThemePreset = settings.ThemePreset,
            ThemeHighContrast = settings.ThemeHighContrast,
            ThemeBorderRadius = settings.ThemeBorderRadius,
            SessionVersion = settings.SessionVersion,
            SessionVersionUpdatedAt = settings.SessionVersionUpdatedAt,
            UpdatedAt = settings.UpdatedAt,
            LastUpdatedBy = settings.LastUpdatedBy
        };
    }

    public async Task<SettingsUpdateResult> UpdateGeneralSettingsAsync(
        string adminUserId,
        UpdateGeneralSettingsInput input)
    {
        try
        {
            if (!await IsAdminAsync(adminUserId))
                return new SettingsUpdateResult(false, "Unauthorized: Admin role required", DateTime.UtcNow);

            // Validate inputs
            if (string.IsNullOrWhiteSpace(input.SiteName))
                return new SettingsUpdateResult(false, "Site name is required", DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(input.ContactEmail))
                return new SettingsUpdateResult(false, "Contact email is required", DateTime.UtcNow);

            if (!IsValidEmail(input.ContactEmail))
                return new SettingsUpdateResult(false, "Invalid contact email format", DateTime.UtcNow);

            var settings = await GetOrCreateSettingsAsync();

            settings.SiteName = input.SiteName.Trim();
            settings.SiteDescription = input.SiteDescription?.Trim() ?? "";
            settings.ContactEmail = input.ContactEmail.Trim();
            settings.ContactPhone = input.ContactPhone?.Trim() ?? "";
            settings.Timezone = input.Timezone ?? "UTC";
            settings.DateFormat = input.DateFormat ?? "MM/dd/yyyy";
            settings.TimeFormat = input.TimeFormat ?? "12-hour";
            settings.UpdatedAt = DateTime.UtcNow;
            settings.LastUpdatedBy = adminUserId;

            await _context.SaveChangesAsync();
            InvalidateCache(); // Invalidate cache after update

            await _activityLogger.LogUserActivityAsync(
                adminUserId,
                "SettingsUpdate",
                "Updated general system settings",
                $"Site: {input.SiteName}, Contact: {input.ContactEmail}"
            );

            // Notify all admins about settings change
            await _notificationService.SendNotificationToRoleAsync(
                "Admin",
                "General Settings Updated",
                $"System general settings have been updated by an administrator. Site Name: {input.SiteName}",
                NotificationType.Info,
                NotificationCategory.System,
                NotificationPriority.Normal,
                "/admin/settings",
                "View Settings");

            return new SettingsUpdateResult(
                true,
                "General settings updated successfully",
                DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating general settings");
            return new SettingsUpdateResult(false, $"Error updating settings: {ex.Message}", DateTime.UtcNow);
        }
    }

    public async Task<SettingsUpdateResult> UpdateSecuritySettingsAsync(
        string adminUserId,
        UpdateSecuritySettingsInput input)
    {
        try
        {
            if (!await IsAdminAsync(adminUserId))
                return new SettingsUpdateResult(false, "Unauthorized: Admin role required", DateTime.UtcNow);

            // Validate inputs
            if (input.SessionTimeoutMinutes < 5 || input.SessionTimeoutMinutes > 1440)
                return new SettingsUpdateResult(false, "Session timeout must be between 5 and 1440 minutes", DateTime.UtcNow);

            if (input.MaxLoginAttempts < 1 || input.MaxLoginAttempts > 20)
                return new SettingsUpdateResult(false, "Max login attempts must be between 1 and 20", DateTime.UtcNow);

            if (input.PasswordMinLength < 6 || input.PasswordMinLength > 128)
                return new SettingsUpdateResult(false, "Password minimum length must be between 6 and 128", DateTime.UtcNow);

            if (input.PasswordExpiryDays < 0 || input.PasswordExpiryDays > 365)
                return new SettingsUpdateResult(false, "Password expiry days must be between 0 and 365", DateTime.UtcNow);

            if (input.AccountLockoutMinutes < 1 || input.AccountLockoutMinutes > 1440)
                return new SettingsUpdateResult(false, "Account lockout must be between 1 and 1440 minutes", DateTime.UtcNow);

            var settings = await GetOrCreateSettingsAsync();

            // Check if session timeout is changing
            bool isSessionTimeoutChanging = settings.SessionTimeoutMinutes != input.SessionTimeoutMinutes;

            // Update security settings
            settings.SessionTimeoutMinutes = input.SessionTimeoutMinutes;
            settings.MaxLoginAttempts = input.MaxLoginAttempts;
            settings.PasswordMinLength = input.PasswordMinLength;
            settings.RequireUppercase = input.RequireUppercase;
            settings.RequireLowercase = input.RequireLowercase;
            settings.RequireNumbers = input.RequireNumbers;
            settings.RequireSpecialChars = input.RequireSpecialChars;
            settings.EnableTwoFactor = input.EnableTwoFactor;
            settings.RequireEmailVerification = input.RequireEmailVerification;
            settings.PasswordExpiryDays = input.PasswordExpiryDays;
            settings.AccountLockoutMinutes = input.AccountLockoutMinutes;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.LastUpdatedBy = adminUserId;

            // Increment session version if session timeout changed
            if (isSessionTimeoutChanging)
            {
                settings.SessionVersion++;
                settings.SessionVersionUpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("Session version incremented to {Version} due to session timeout change", settings.SessionVersion);
            }

            await _context.SaveChangesAsync();
            InvalidateCache(); // Invalidate cache after update

            // Log the activity
            await _activityLogger.LogUserActivityAsync(
                adminUserId,
                "SettingsUpdate",
                "Updated security system settings",
                $"Password length: {input.PasswordMinLength}, Max attempts: {input.MaxLoginAttempts}"
            );

            // Notify all users about security policy changes
            var allUserIds = await _context.Users.Select(u => u.Id).ToListAsync();
            var message = isSessionTimeoutChanging
                ? $"System security settings have been updated, including session timeout. You may be logged out soon to apply new settings. Please save your work."
                : $"System security settings have been updated. Password requirements and login policies may have changed. Please review the new security policies.";

            await _notificationService.SendBulkNotificationsAsync(
                allUserIds,
                "Security Policy Updated",
                message,
                NotificationType.Info,
                NotificationCategory.Security,
                isSessionTimeoutChanging ? NotificationPriority.Urgent : NotificationPriority.High,
                "/profile",
                "View Profile");

            var resultMessage = isSessionTimeoutChanging
                ? "Security settings updated successfully. Session version incremented - users will be required to re-authenticate."
                : "Security settings updated successfully";

            return new SettingsUpdateResult(
                true,
                resultMessage,
                DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating security settings");
            return new SettingsUpdateResult(false, $"Error updating settings: {ex.Message}", DateTime.UtcNow);
        }
    }

    public async Task<SettingsUpdateResult> UpdateSmtpSettingsAsync(
        string adminUserId,
        UpdateSmtpSettingsInput input)
    {
        try
        {
            if (!await IsAdminAsync(adminUserId))
                return new SettingsUpdateResult(false, "Unauthorized: Admin role required", DateTime.UtcNow);

            // Validate inputs
            if (string.IsNullOrWhiteSpace(input.SmtpServer))
                return new SettingsUpdateResult(false, "SMTP server is required", DateTime.UtcNow);

            if (input.SmtpPort <= 0 || input.SmtpPort > 65535)
                return new SettingsUpdateResult(false, "SMTP port must be between 1 and 65535", DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(input.SmtpUsername))
                return new SettingsUpdateResult(false, "SMTP username is required", DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(input.SmtpPassword))
                return new SettingsUpdateResult(false, "SMTP password is required", DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(input.SmtpFromEmail))
                return new SettingsUpdateResult(false, "From email is required", DateTime.UtcNow);

            if (!IsValidEmail(input.SmtpFromEmail))
                return new SettingsUpdateResult(false, "Invalid from email format", DateTime.UtcNow);

            var settings = await GetOrCreateSettingsAsync();

            settings.SmtpServer = input.SmtpServer.Trim();
            settings.SmtpPort = input.SmtpPort;
            settings.SmtpUsername = input.SmtpUsername.Trim();
            settings.SmtpPassword = input.SmtpPassword; // In production, encrypt this
            settings.SmtpEnableSsl = input.SmtpEnableSsl;
            settings.SmtpFromEmail = input.SmtpFromEmail.Trim();
            settings.SmtpFromName = input.SmtpFromName?.Trim() ?? "MediChat.AI";
            settings.SmtpIsConfigured = true;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.LastUpdatedBy = adminUserId;

            await _context.SaveChangesAsync();
            InvalidateCache(); // Invalidate cache after update

            await _activityLogger.LogUserActivityAsync(
                adminUserId,
                "SettingsUpdate",
                "Updated SMTP email settings",
                $"Server: {input.SmtpServer}, Port: {input.SmtpPort}"
            );

            // Notify all admins about SMTP configuration change
            await _notificationService.SendNotificationToRoleAsync(
                "Admin",
                "SMTP Configuration Updated",
                $"Email server settings have been updated. SMTP Server: {input.SmtpServer}:{input.SmtpPort}. Please test the connection to ensure emails are working properly.",
                NotificationType.Info,
                NotificationCategory.System,
                NotificationPriority.High,
                "/admin/settings?tab=EmailSmtp",
                "Test Connection");

            return new SettingsUpdateResult(
                true,
                "SMTP settings updated successfully",
                DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SMTP settings");
            return new SettingsUpdateResult(false, $"Error updating SMTP settings: {ex.Message}", DateTime.UtcNow);
        }
    }

    public async Task<SettingsUpdateResult> UpdateEmailThemeSettingsAsync(
        string adminUserId,
        UpdateEmailThemeInput input)
    {
        try
        {
            if (!await IsAdminAsync(adminUserId))
                return new SettingsUpdateResult(false, "Unauthorized: Admin role required", DateTime.UtcNow);

            // Validate inputs
            if (string.IsNullOrWhiteSpace(input.EmailTemplateTheme))
                return new SettingsUpdateResult(false, "Email template theme is required", DateTime.UtcNow);

            var validThemes = new[] { "Default", "Modern", "Professional", "Custom" };
            if (!validThemes.Contains(input.EmailTemplateTheme))
                return new SettingsUpdateResult(false, "Invalid theme. Must be: Default, Modern, Professional, or Custom", DateTime.UtcNow);

            // Validate color formats (basic validation)
            if (!IsValidColorCode(input.EmailPrimaryColor))
                return new SettingsUpdateResult(false, "Invalid primary color format", DateTime.UtcNow);

            if (!IsValidColorCode(input.EmailSecondaryColor))
                return new SettingsUpdateResult(false, "Invalid secondary color format", DateTime.UtcNow);

            var settings = await GetOrCreateSettingsAsync();

            settings.EmailTemplateTheme = input.EmailTemplateTheme;
            settings.EmailPrimaryColor = input.EmailPrimaryColor;
            settings.EmailSecondaryColor = input.EmailSecondaryColor;
            settings.EmailBackgroundColor = input.EmailBackgroundColor;
            settings.EmailTextColor = input.EmailTextColor;
            settings.EmailHeaderImageUrl = input.EmailHeaderImageUrl?.Trim() ?? "";
            settings.EmailIncludeFooter = input.EmailIncludeFooter;
            settings.EmailFooterText = input.EmailFooterText?.Trim() ?? "";
            settings.EmailIncludeSocialLinks = input.EmailIncludeSocialLinks;
            settings.EmailFacebookUrl = input.EmailFacebookUrl?.Trim();
            settings.EmailTwitterUrl = input.EmailTwitterUrl?.Trim();
            settings.EmailLinkedInUrl = input.EmailLinkedInUrl?.Trim();
            settings.EmailInstagramUrl = input.EmailInstagramUrl?.Trim();
            settings.UpdatedAt = DateTime.UtcNow;
            settings.LastUpdatedBy = adminUserId;

            await _context.SaveChangesAsync();
            InvalidateCache(); // Invalidate cache after update

            await _activityLogger.LogUserActivityAsync(
                adminUserId,
                "SettingsUpdate",
                "Updated email theme settings",
                $"Theme: {input.EmailTemplateTheme}, Primary: {input.EmailPrimaryColor}"
            );

            // Notify all admins about email theme change
            await _notificationService.SendNotificationToRoleAsync(
                "Admin",
                "Email Theme Updated",
                $"Email template theme has been changed to '{input.EmailTemplateTheme}'. All outgoing emails will now use the new design.",
                NotificationType.Info,
                NotificationCategory.System,
                NotificationPriority.Normal,
                "/admin/settings?tab=EmailSmtp",
                "View Settings");

            return new SettingsUpdateResult(
                true,
                "Email theme settings updated successfully",
                DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email theme settings");
            return new SettingsUpdateResult(false, $"Error updating email theme settings: {ex.Message}", DateTime.UtcNow);
        }
    }

    public async Task<SettingsUpdateResult> UpdateNotificationSettingsAsync(
        string adminUserId,
        UpdateNotificationSettingsInput input)
    {
        try
        {
            if (!await IsAdminAsync(adminUserId))
                return new SettingsUpdateResult(false, "Unauthorized: Admin role required", DateTime.UtcNow);

            var settings = await GetOrCreateSettingsAsync();

            settings.EnableEmailNotifications = input.EnableEmailNotifications;
            settings.NotifyAdminOnNewDoctor = input.NotifyAdminOnNewDoctor;
            settings.NotifyDoctorOnApproval = input.NotifyDoctorOnApproval;
            settings.NotifyUserOnLogin = input.NotifyUserOnLogin;
            settings.NotifyUserOnPasswordChange = input.NotifyUserOnPasswordChange;
            settings.NotifyUserOn2FAChange = input.NotifyUserOn2FAChange;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.LastUpdatedBy = adminUserId;

            await _context.SaveChangesAsync();
            InvalidateCache(); // Invalidate cache after update

            await _activityLogger.LogUserActivityAsync(
                adminUserId,
                "SettingsUpdate",
                "Updated notification settings",
                $"Email notifications enabled: {input.EnableEmailNotifications}"
            );

            // Notify all users about notification policy changes
            var allUserIds = await _context.Users.Select(u => u.Id).ToListAsync();
            var notificationStatus = input.EnableEmailNotifications ? "enabled" : "disabled";
            await _notificationService.SendBulkNotificationsAsync(
                allUserIds,
                "Notification Settings Updated",
                $"System notification settings have been changed. Email notifications are now {notificationStatus}. Your notification preferences may be affected.",
                NotificationType.Info,
                NotificationCategory.System,
                NotificationPriority.High,
                "/notification-center",
                "Manage Preferences");

            return new SettingsUpdateResult(
                true,
                "Notification settings updated successfully",
                DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating notification settings");
            return new SettingsUpdateResult(false, $"Error updating notification settings: {ex.Message}", DateTime.UtcNow);
        }
    }

    public async Task<SettingsUpdateResult> UpdateAppearanceSettingsAsync(
        string adminUserId,
        UpdateAppearanceSettingsInput input)
    {
        try
        {
            if (!await IsAdminAsync(adminUserId))
                return new SettingsUpdateResult(false, "Unauthorized: Admin role required", DateTime.UtcNow);

            var settings = await GetOrCreateSettingsAsync();

            settings.ThemeMode = input.ThemeMode;
            settings.ThemePrimaryColor = input.ThemePrimaryColor;
            settings.ThemeAccentColor = input.ThemeAccentColor;
            settings.ThemeBackgroundColor = input.ThemeBackgroundColor;
            settings.ThemeTextColor = input.ThemeTextColor;
            settings.ThemeSidebarStyle = input.ThemeSidebarStyle;
            settings.ThemeFontSize = input.ThemeFontSize;
            settings.ThemeEnableAnimations = input.ThemeEnableAnimations;
            settings.ThemeCompactMode = input.ThemeCompactMode;
            settings.ThemePreset = input.ThemePreset;
            settings.ThemeHighContrast = input.ThemeHighContrast;
            settings.ThemeBorderRadius = input.ThemeBorderRadius;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.LastUpdatedBy = adminUserId;

            await _context.SaveChangesAsync();
            InvalidateCache(); // Invalidate cache after update

            await _activityLogger.LogUserActivityAsync(
                adminUserId,
                "SettingsUpdate",
                "Updated appearance settings",
                $"Theme mode: {input.ThemeMode}, Preset: {input.ThemePreset}"
            );

            // Notify all users about theme changes
            var allUserIds = await _context.Users.Select(u => u.Id).ToListAsync();
            await _notificationService.SendBulkNotificationsAsync(
                allUserIds,
                "Application Theme Updated",
                $"The application appearance has been updated. The new theme '{input.ThemePreset}' with {input.ThemeMode} mode is now active. Please refresh your browser to see the changes.",
                NotificationType.Info,
                NotificationCategory.System,
                NotificationPriority.Normal,
                "/",
                "Refresh Page");

            return new SettingsUpdateResult(
                true,
                "Appearance settings updated successfully. Changes will apply on next page load.",
                DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appearance settings");
            return new SettingsUpdateResult(false, $"Error updating appearance settings: {ex.Message}", DateTime.UtcNow);
        }
    }

    public async Task<SmtpTestResult> TestSmtpConnectionAsync(string adminUserId)
    {
        try
        {
            if (!await IsAdminAsync(adminUserId))
                return new SmtpTestResult(false, "Unauthorized: Admin role required", DateTime.UtcNow);

            var settings = await GetOrCreateSettingsAsync();

            if (!settings.SmtpIsConfigured)
                return new SmtpTestResult(false, "SMTP is not configured", DateTime.UtcNow);

            // Get admin user email to send test email
            var admin = await _userManager.FindByIdAsync(adminUserId);
            if (admin == null || string.IsNullOrWhiteSpace(admin.Email))
                return new SmtpTestResult(false, "Admin email not found", DateTime.UtcNow);

            var testDateTime = DateTime.UtcNow;
            string testMessage;
            bool testSuccess;

            try
            {
                // Create SMTP client and send test email
                using (var smtpClient = new SmtpClient(settings.SmtpServer, settings.SmtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword);
                    smtpClient.EnableSsl = settings.SmtpEnableSsl;
                    smtpClient.Timeout = 10000; // 10 seconds timeout

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(settings.SmtpFromEmail, settings.SmtpFromName),
                        Subject = "SMTP Test - MediChat.AI",
                        Body = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif;'>
                                <h2>SMTP Connection Test</h2>
                                <p>This is a test email from MediChat.AI to verify SMTP configuration.</p>
                                <p><strong>Test Time:</strong> {testDateTime:yyyy-MM-dd HH:mm:ss} UTC</p>
                                <p><strong>SMTP Server:</strong> {settings.SmtpServer}:{settings.SmtpPort}</p>
                                <p><strong>SSL Enabled:</strong> {settings.SmtpEnableSsl}</p>
                                <p>If you received this email, your SMTP configuration is working correctly.</p>
                            </body>
                            </html>",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(admin.Email);

                    await smtpClient.SendMailAsync(mailMessage);
                }

                testSuccess = true;
                testMessage = $"Test email sent successfully to {admin.Email}";
                _logger.LogInformation("SMTP test successful for admin {AdminId}", adminUserId);
            }
            catch (SmtpException ex)
            {
                testSuccess = false;
                testMessage = $"SMTP Error: {ex.Message}";
                _logger.LogError(ex, "SMTP test failed for admin {AdminId}", adminUserId);
            }
            catch (Exception ex)
            {
                testSuccess = false;
                testMessage = $"Connection Error: {ex.Message}";
                _logger.LogError(ex, "SMTP test failed for admin {AdminId}", adminUserId);
            }

            // Update test results in settings
            settings.SmtpLastTestedAt = testDateTime;
            settings.SmtpLastTestSuccessful = testSuccess;
            settings.SmtpLastTestMessage = testMessage;
            settings.UpdatedAt = DateTime.UtcNow;
            settings.LastUpdatedBy = adminUserId;

            await _context.SaveChangesAsync();
            InvalidateCache(); // Invalidate cache after update

            await _activityLogger.LogUserActivityAsync(
                adminUserId,
                "SmtpTest",
                "Tested SMTP connection",
                $"Result: {(testSuccess ? "Success" : "Failed")} - {testMessage}"
            );

            return new SmtpTestResult(testSuccess, testMessage, testDateTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SMTP test");
            return new SmtpTestResult(false, $"Error testing SMTP: {ex.Message}", DateTime.UtcNow);
        }
    }

    public async Task<EmailTemplatePreviewResult> PreviewEmailTemplateAsync(
        string templateName,
        string theme,
        string primaryColor,
        string secondaryColor)
    {
        try
        {
            // Validate template name
            var validTemplates = new[]
            {
                "EmailVerification",
                "PasswordReset",
                "OtpVerification",
                "DoctorApproved",
                "DoctorRejected",
                "LoginAlert",
                "TwoFactorEnabled",
                "TwoFactorDisabled"
            };

            if (!validTemplates.Contains(templateName))
                return new EmailTemplatePreviewResult(false, null, $"Invalid template name. Valid templates: {string.Join(", ", validTemplates)}");

            // Build template file path
            var templatePath = System.IO.Path.Combine(_environment.ContentRootPath, "EmailTemplates", $"{templateName}.html");

            if (!System.IO.File.Exists(templatePath))
                return new EmailTemplatePreviewResult(false, null, $"Template file not found: {templateName}.html");

            // Read template content
            var templateContent = await System.IO.File.ReadAllTextAsync(templatePath);

            // Apply theme colors by replacing existing color values
            var previewContent = templateContent;

            // Replace gradient colors in header
            previewContent = System.Text.RegularExpressions.Regex.Replace(
                previewContent,
                @"linear-gradient\(135deg,\s*#[0-9a-fA-F]{6}\s+0%,\s*#[0-9a-fA-F]{6}\s+100%\)",
                $"linear-gradient(135deg, {primaryColor} 0%, {secondaryColor} 100%)"
            );

            // Replace button background colors
            previewContent = System.Text.RegularExpressions.Regex.Replace(
                previewContent,
                @"background:\s*#[0-9a-fA-F]{6};",
                $"background: {primaryColor};"
            );

            // Replace link colors
            previewContent = System.Text.RegularExpressions.Regex.Replace(
                previewContent,
                @"color:\s*#[0-9a-fA-F]{6};",
                $"color: {primaryColor};",
                System.Text.RegularExpressions.RegexOptions.Multiline
            );

            // Replace heading colors
            previewContent = previewContent.Replace("#667eea", primaryColor);
            previewContent = previewContent.Replace("#764ba2", secondaryColor);

            // Add preview-specific placeholders for demonstration
            previewContent = previewContent.Replace("{{FirstName}}", "John");
            previewContent = previewContent.Replace("{{LastName}}", "Doe");
            previewContent = previewContent.Replace("{{Email}}", "john.doe@example.com");
            previewContent = previewContent.Replace("{{VerificationLink}}", "https://medichat.ai/verify/preview-link");
            previewContent = previewContent.Replace("{{ResetLink}}", "https://medichat.ai/reset/preview-link");
            previewContent = previewContent.Replace("{{OtpCode}}", "123456");
            previewContent = previewContent.Replace("{{LoginTime}}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            previewContent = previewContent.Replace("{{IpAddress}}", "192.168.1.1");
            previewContent = previewContent.Replace("{{Browser}}", "Chrome on Windows");
            previewContent = previewContent.Replace("{{RejectionReason}}", "Sample rejection reason for preview");

            // Add preview banner at the top
            var previewBanner = @"
<div style='background: #fef3c7; color: #92400e; padding: 15px; text-align: center; font-family: Arial, sans-serif; border: 2px solid #fbbf24; margin-bottom: 20px;'>
    <strong>PREVIEW MODE</strong> - This is a preview of the email template with sample data
</div>";

            previewContent = previewBanner + previewContent;

            _logger.LogInformation("Generated preview for template {TemplateName} with theme {Theme}", templateName, theme);

            return new EmailTemplatePreviewResult(true, previewContent, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating email template preview");
            return new EmailTemplatePreviewResult(false, null, $"Error generating preview: {ex.Message}");
        }
    }

    // Helper Methods

    private async Task<SystemSettings> GetOrCreateSettingsAsync()
    {
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new SystemSettings
            {
                SiteName = "MediChat.AI",
                SiteDescription = "Healthcare Communication Platform",
                ContactEmail = "contact@medichat.ai",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SystemSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    private async Task<bool> IsAdminAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var roles = await _userManager.GetRolesAsync(user);
            return roles.Contains("Admin");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking admin role for user {UserId}", userId);
            return false;
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidColorCode(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        // Check if it's a valid hex color (#RRGGBB or #RGB)
        return System.Text.RegularExpressions.Regex.IsMatch(color, @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6})$");
    }
}

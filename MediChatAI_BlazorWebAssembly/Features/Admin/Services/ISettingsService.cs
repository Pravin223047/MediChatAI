using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Admin.Services;

public interface ISettingsService
{
    // Get all system settings in one call (requires admin authorization)
    Task<SystemSettingsData?> GetSystemSettingsAsync();

    // Get public/general settings (site name, description, contact info, etc.) - no authorization required
    Task<GeneralSettingsData?> GetPublicSettingsAsync();

    // Get appearance settings only (public - no authorization required)
    Task<AppearanceSettingsData?> GetAppearanceSettingsAsync();

    // Update settings by category
    Task<SettingsUpdateResult?> UpdateGeneralSettingsAsync(UpdateGeneralSettingsInput input);
    Task<SettingsUpdateResult?> UpdateSecuritySettingsAsync(UpdateSecuritySettingsInput input);
    Task<SettingsUpdateResult?> UpdateSmtpSettingsAsync(UpdateSmtpSettingsInput input);
    Task<SettingsUpdateResult?> UpdateEmailThemeSettingsAsync(UpdateEmailThemeInput input);
    Task<SettingsUpdateResult?> UpdateNotificationSettingsAsync(UpdateNotificationSettingsInput input);
    Task<SettingsUpdateResult?> UpdateAppearanceSettingsAsync(UpdateAppearanceSettingsInput input);

    // SMTP testing and email template preview
    Task<SmtpTestResult?> TestSmtpConnectionAsync();
    Task<EmailTemplatePreviewResult?> PreviewEmailTemplateAsync(string templateName, string theme, string primaryColor, string secondaryColor);
}

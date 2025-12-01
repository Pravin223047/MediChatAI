using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Shared;

public interface ISystemSettingsService
{
    Task<SystemSettings> GetSystemSettingsAsync();
    Task<SystemSettingsData> GetSystemSettingsDataAsync();

    Task<SettingsUpdateResult> UpdateGeneralSettingsAsync(
        string adminUserId,
        UpdateGeneralSettingsInput input);

    Task<SettingsUpdateResult> UpdateSecuritySettingsAsync(
        string adminUserId,
        UpdateSecuritySettingsInput input);

    Task<SettingsUpdateResult> UpdateSmtpSettingsAsync(
        string adminUserId,
        UpdateSmtpSettingsInput input);

    Task<SettingsUpdateResult> UpdateEmailThemeSettingsAsync(
        string adminUserId,
        UpdateEmailThemeInput input);

    Task<SettingsUpdateResult> UpdateNotificationSettingsAsync(
        string adminUserId,
        UpdateNotificationSettingsInput input);

    Task<SettingsUpdateResult> UpdateAppearanceSettingsAsync(
        string adminUserId,
        UpdateAppearanceSettingsInput input);

    Task<SmtpTestResult> TestSmtpConnectionAsync(string adminUserId);

    Task<EmailTemplatePreviewResult> PreviewEmailTemplateAsync(
        string templateName,
        string theme,
        string primaryColor,
        string secondaryColor);
}

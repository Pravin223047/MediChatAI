using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Services;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Core.Services.State;

public class AppSettingsState
{
    public GeneralSettingsDto Settings { get; private set; } = new GeneralSettingsDto(
        "MediChat.AI",
        "Healthcare Communication Platform",
        "contact@medichat.ai",
        "",
        "UTC",
        "MM/dd/yyyy",
        "12-hour"
    );

    public SecuritySettingsDto SecuritySettings { get; private set; } = new SecuritySettingsDto(
        30,     // SessionTimeoutMinutes
        5,      // MaxLoginAttempts
        8,      // PasswordMinLength
        true,   // RequireUppercase
        true,   // RequireLowercase
        true,   // RequireNumbers
        true,   // RequireSpecialChars
        false,  // EnableTwoFactor
        true,   // RequireEmailVerification
        90,     // PasswordExpiryDays
        15      // AccountLockoutMinutes
    );

    public AppearanceSettingsDto AppearanceSettings { get; private set; } = new AppearanceSettingsDto(
        "light",
        "#3b82f6",
        "#8b5cf6",
        "#f8fafc",
        "#1f2937",
        "expanded",
        "medium",
        true,
        false,
        "default",
        false,
        "medium"
    );

    public event Action? OnChange;
    public event Action? OnAppearanceChange;

    public async Task LoadSettingsAsync(ISettingsService settingsService)
    {
        try
        {
            // Load public settings (accessible to all users, no authentication required)
            var publicSettings = await settingsService.GetPublicSettingsAsync();

            if (publicSettings != null)
            {
                Settings = new GeneralSettingsDto(
                    publicSettings.SiteName,
                    publicSettings.SiteDescription,
                    publicSettings.ContactEmail,
                    publicSettings.ContactPhone,
                    publicSettings.Timezone,
                    publicSettings.DateFormat,
                    publicSettings.TimeFormat
                );

                Console.WriteLine($"Public settings loaded from database: SiteName={publicSettings.SiteName}");
            }
            else
            {
                Console.WriteLine("Warning: GetPublicSettingsAsync returned null - using default settings");
            }

            // Load appearance settings (also public, loaded separately in App.razor)
            // Security settings are not loaded here as they require admin authorization

            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading public settings: {ex.Message}");
            Console.WriteLine("Using default settings as fallback");
            // Use default settings if load fails
        }
    }

    public void UpdateSettings(GeneralSettingsDto settings)
    {
        Settings = settings;
        NotifyStateChanged();
    }

    public void UpdateSecuritySettings(SecuritySettingsDto securitySettings)
    {
        SecuritySettings = securitySettings;
        NotifyStateChanged();
    }

    public void UpdateAppearanceSettings(AppearanceSettingsDto appearanceSettings)
    {
        AppearanceSettings = appearanceSettings;
        NotifyStateChanged();
        NotifyAppearanceChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
    private void NotifyAppearanceChanged() => OnAppearanceChange?.Invoke();
}

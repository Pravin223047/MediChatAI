using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Core.Services.Theme;

public interface IThemeService
{
    /// <summary>
    /// Gets the current theme configuration
    /// </summary>
    Task<ThemeConfiguration> GetCurrentThemeAsync();

    /// <summary>
    /// Applies a theme configuration to the document
    /// </summary>
    Task ApplyThemeAsync(ThemeConfiguration theme);

    /// <summary>
    /// Loads theme from system settings
    /// </summary>
    Task LoadThemeFromSettingsAsync(SystemSettingsData settings);

    /// <summary>
    /// Loads theme from appearance settings (public endpoint)
    /// </summary>
    Task LoadThemeFromAppearanceSettingsAsync(AppearanceSettingsData settings);

    /// <summary>
    /// Gets predefined theme presets
    /// </summary>
    Dictionary<string, ThemeConfiguration> GetThemePresets();

    /// <summary>
    /// Detects the system's preferred color scheme (light/dark)
    /// </summary>
    Task<string> DetectSystemThemeAsync();

    /// <summary>
    /// Ensures the current theme is persisted to localStorage
    /// </summary>
    Task EnsureThemePersistedAsync();
}

/// <summary>
/// Theme configuration model
/// </summary>
public class ThemeConfiguration
{
    public string Mode { get; set; } = "light"; // light, dark, system
    public string PrimaryColor { get; set; } = "#3b82f6";
    public string AccentColor { get; set; } = "#8b5cf6";
    public string BackgroundColor { get; set; } = "#f8fafc";
    public string TextColor { get; set; } = "#1f2937";
    public string SidebarStyle { get; set; } = "expanded";
    public string FontSize { get; set; } = "medium";
    public bool EnableAnimations { get; set; } = true;
    public bool CompactMode { get; set; } = false;
    public string Preset { get; set; } = "default";
    public bool HighContrast { get; set; } = false;
    public string BorderRadius { get; set; } = "medium";
}

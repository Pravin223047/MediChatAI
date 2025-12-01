using Microsoft.JSInterop;
using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using Blazored.LocalStorage;

namespace MediChatAI_BlazorWebAssembly.Core.Services.Theme;

public class ThemeService : IThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILocalStorageService _localStorage;
    private readonly ThemeState _themeState;
    private ThemeConfiguration _currentTheme;
    private const string THEME_STORAGE_KEY = "medichat_theme_config";

    public ThemeService(IJSRuntime jsRuntime, ILocalStorageService localStorage, ThemeState themeState)
    {
        _jsRuntime = jsRuntime;
        _localStorage = localStorage;
        _themeState = themeState;
        _currentTheme = GetDefaultTheme();
    }

    public async Task<ThemeConfiguration> GetCurrentThemeAsync()
    {
        // NOTE: Database theme always takes precedence over localStorage
        // localStorage is only used as a fallback if database is unavailable
        try
        {
            // Try to load from localStorage only (database theme is loaded at app startup)
            var savedTheme = await _localStorage.GetItemAsync<ThemeConfiguration>(THEME_STORAGE_KEY);
            if (savedTheme != null)
            {
                _currentTheme = savedTheme;
            }
        }
        catch
        {
            // Use default theme if loading fails
        }

        return _currentTheme;
    }

    public async Task EnsureThemePersistedAsync()
    {
        // Save current theme to localStorage to ensure persistence
        try
        {
            await _localStorage.SetItemAsync(THEME_STORAGE_KEY, _currentTheme);
        }
        catch
        {
            // Silently fail if localStorage is unavailable
        }
    }

    public async Task ApplyThemeAsync(ThemeConfiguration theme)
    {
        _currentTheme = theme;

        // Save to localStorage
        await _localStorage.SetItemAsync(THEME_STORAGE_KEY, theme);

        // Determine actual mode (resolve 'system' to 'light' or 'dark')
        var actualMode = theme.Mode;
        if (actualMode == "system")
        {
            actualMode = await DetectSystemThemeAsync();
        }

        // Apply theme via JavaScript
        await _jsRuntime.InvokeVoidAsync("applyTheme", new
        {
            mode = actualMode,
            primaryColor = theme.PrimaryColor,
            accentColor = theme.AccentColor,
            backgroundColor = theme.BackgroundColor,
            textColor = theme.TextColor,
            fontSize = theme.FontSize,
            enableAnimations = theme.EnableAnimations,
            compactMode = theme.CompactMode,
            highContrast = theme.HighContrast,
            borderRadius = theme.BorderRadius
        });

        // Update singleton state to notify all subscribers across all scopes
        _themeState.UpdateTheme(theme);
    }

    public async Task LoadThemeFromSettingsAsync(SystemSettingsData settings)
    {
        var theme = new ThemeConfiguration
        {
            Mode = settings.ThemeMode,
            PrimaryColor = settings.ThemePrimaryColor,
            AccentColor = settings.ThemeAccentColor,
            BackgroundColor = settings.ThemeBackgroundColor,
            TextColor = settings.ThemeTextColor,
            SidebarStyle = settings.ThemeSidebarStyle,
            FontSize = settings.ThemeFontSize,
            EnableAnimations = settings.ThemeEnableAnimations,
            CompactMode = settings.ThemeCompactMode,
            Preset = settings.ThemePreset,
            HighContrast = settings.ThemeHighContrast,
            BorderRadius = settings.ThemeBorderRadius
        };

        await ApplyThemeAsync(theme);
    }

    public async Task LoadThemeFromAppearanceSettingsAsync(AppearanceSettingsData settings)
    {
        var theme = new ThemeConfiguration
        {
            Mode = settings.ThemeMode,
            PrimaryColor = settings.ThemePrimaryColor,
            AccentColor = settings.ThemeAccentColor,
            BackgroundColor = settings.ThemeBackgroundColor,
            TextColor = settings.ThemeTextColor,
            SidebarStyle = settings.ThemeSidebarStyle,
            FontSize = settings.ThemeFontSize,
            EnableAnimations = settings.ThemeEnableAnimations,
            CompactMode = settings.ThemeCompactMode,
            Preset = settings.ThemePreset,
            HighContrast = settings.ThemeHighContrast,
            BorderRadius = settings.ThemeBorderRadius
        };

        await ApplyThemeAsync(theme);
    }

    public Dictionary<string, ThemeConfiguration> GetThemePresets()
    {
        // Use centralized default theme presets
        return DefaultThemeConfiguration.GetAllPresets();
    }

    public async Task<string> DetectSystemThemeAsync()
    {
        try
        {
            var prefersDark = await _jsRuntime.InvokeAsync<bool>("detectSystemTheme");
            return prefersDark ? "dark" : "light";
        }
        catch
        {
            return "light"; // Default to light if detection fails
        }
    }

    private ThemeConfiguration GetDefaultTheme()
    {
        // Use centralized default theme configuration
        return DefaultThemeConfiguration.GetDefaultTheme();
    }
}

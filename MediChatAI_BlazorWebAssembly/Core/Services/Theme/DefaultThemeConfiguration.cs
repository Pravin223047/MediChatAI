using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;

namespace MediChatAI_BlazorWebAssembly.Core.Services.Theme;

/// <summary>
/// Static configuration class for default theme presets and fallback values.
/// This ensures themes can be applied immediately without waiting for database or async operations.
/// </summary>
public static class DefaultThemeConfiguration
{
    /// <summary>
    /// Gets the primary default theme configuration
    /// </summary>
    public static ThemeConfiguration GetDefaultTheme()
    {
        return new ThemeConfiguration
        {
            Mode = "light",
            PrimaryColor = "#6366f1",      // Modern indigo
            AccentColor = "#8b5cf6",        // Purple
            BackgroundColor = "#f8fafc",    // Light gray
            TextColor = "#0f172a",          // Dark gray for contrast
            SidebarStyle = "expanded",
            FontSize = "medium",
            EnableAnimations = true,
            CompactMode = false,
            Preset = "default",
            HighContrast = false,
            BorderRadius = "0.75rem"
        };
    }

    /// <summary>
    /// Gets all available theme presets with complete configurations
    /// </summary>
    public static Dictionary<string, ThemeConfiguration> GetAllPresets()
    {
        return new Dictionary<string, ThemeConfiguration>
        {
            ["default"] = new ThemeConfiguration
            {
                Preset = "default",
                Mode = "light",
                PrimaryColor = "#6366f1",    // Indigo
                AccentColor = "#8b5cf6",      // Purple
                BackgroundColor = "#f8fafc",  // Light slate
                TextColor = "#0f172a",        // Dark slate
                SidebarStyle = "expanded",
                FontSize = "medium",
                EnableAnimations = true,
                CompactMode = false,
                HighContrast = false,
                BorderRadius = "0.75rem"
            },
            ["medical"] = new ThemeConfiguration
            {
                Preset = "medical",
                Mode = "light",
                PrimaryColor = "#10b981",     // Emerald green
                AccentColor = "#06b6d4",      // Cyan
                BackgroundColor = "#f0fdf4",  // Light green
                TextColor = "#064e3b",        // Dark green
                SidebarStyle = "expanded",
                FontSize = "medium",
                EnableAnimations = true,
                CompactMode = false,
                HighContrast = false,
                BorderRadius = "0.75rem"
            },
            ["forest"] = new ThemeConfiguration
            {
                Preset = "forest",
                Mode = "light",
                PrimaryColor = "#059669",     // Forest green
                AccentColor = "#84cc16",      // Lime
                BackgroundColor = "#ecfdf5",  // Mint
                TextColor = "#064e3b",        // Dark green
                SidebarStyle = "expanded",
                FontSize = "medium",
                EnableAnimations = true,
                CompactMode = false,
                HighContrast = false,
                BorderRadius = "0.75rem"
            },
            ["royal"] = new ThemeConfiguration
            {
                Preset = "royal",
                Mode = "light",
                PrimaryColor = "#7c3aed",     // Violet
                AccentColor = "#ec4899",      // Pink
                BackgroundColor = "#faf5ff",  // Light purple
                TextColor = "#581c87",        // Dark purple
                SidebarStyle = "expanded",
                FontSize = "medium",
                EnableAnimations = true,
                CompactMode = false,
                HighContrast = false,
                BorderRadius = "0.75rem"
            },
            ["sunset"] = new ThemeConfiguration
            {
                Preset = "sunset",
                Mode = "light",
                PrimaryColor = "#f59e0b",     // Amber
                AccentColor = "#ef4444",      // Red
                BackgroundColor = "#fffbeb",  // Light amber
                TextColor = "#78350f",        // Dark amber
                SidebarStyle = "expanded",
                FontSize = "medium",
                EnableAnimations = true,
                CompactMode = false,
                HighContrast = false,
                BorderRadius = "0.75rem"
            },
            ["ocean"] = new ThemeConfiguration
            {
                Preset = "ocean",
                Mode = "light",
                PrimaryColor = "#0ea5e9",     // Sky blue
                AccentColor = "#6366f1",      // Indigo
                BackgroundColor = "#f0f9ff",  // Light blue
                TextColor = "#0c4a6e",        // Dark blue
                SidebarStyle = "expanded",
                FontSize = "medium",
                EnableAnimations = true,
                CompactMode = false,
                HighContrast = false,
                BorderRadius = "0.75rem"
            },
            ["dark"] = new ThemeConfiguration
            {
                Preset = "dark",
                Mode = "dark",
                PrimaryColor = "#60a5fa",     // Light blue
                AccentColor = "#a78bfa",      // Light purple
                BackgroundColor = "#111827",  // Dark gray
                TextColor = "#f9fafb",        // Light gray
                SidebarStyle = "expanded",
                FontSize = "medium",
                EnableAnimations = true,
                CompactMode = false,
                HighContrast = false,
                BorderRadius = "0.75rem"
            },
            ["midnight"] = new ThemeConfiguration
            {
                Preset = "midnight",
                Mode = "dark",
                PrimaryColor = "#3b82f6",     // Blue
                AccentColor = "#10b981",      // Green
                BackgroundColor = "#0f172a",  // Very dark slate
                TextColor = "#f1f5f9",        // Light slate
                SidebarStyle = "expanded",
                FontSize = "medium",
                EnableAnimations = true,
                CompactMode = false,
                HighContrast = false,
                BorderRadius = "0.75rem"
            },
            ["highContrast"] = new ThemeConfiguration
            {
                Preset = "highContrast",
                Mode = "light",
                PrimaryColor = "#000000",     // Black
                AccentColor = "#0000ff",      // Pure blue
                BackgroundColor = "#ffffff",  // White
                TextColor = "#000000",        // Black
                SidebarStyle = "expanded",
                FontSize = "large",
                EnableAnimations = false,
                CompactMode = false,
                HighContrast = true,
                BorderRadius = "0.25rem"
            }
        };
    }

    /// <summary>
    /// Gets a specific preset by name, falls back to default if not found
    /// </summary>
    public static ThemeConfiguration GetPreset(string presetName)
    {
        var presets = GetAllPresets();
        return presets.ContainsKey(presetName) ? presets[presetName] : GetDefaultTheme();
    }

    /// <summary>
    /// Gets CSS variables for a theme configuration
    /// </summary>
    public static string GetCssVariables(ThemeConfiguration theme)
    {
        return $@"
            --primary-color: {theme.PrimaryColor};
            --accent-color: {theme.AccentColor};
            --background-color: {theme.BackgroundColor};
            --text-color: {theme.TextColor};
            --border-radius: {theme.BorderRadius};
            --font-size-base: {GetFontSize(theme.FontSize)};
        ";
    }

    /// <summary>
    /// Converts font size setting to CSS value
    /// </summary>
    private static string GetFontSize(string fontSize)
    {
        return fontSize switch
        {
            "small" => "14px",
            "medium" => "16px",
            "large" => "18px",
            _ => "16px"
        };
    }
}

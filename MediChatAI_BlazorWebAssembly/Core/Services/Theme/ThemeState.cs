namespace MediChatAI_BlazorWebAssembly.Core.Services.Theme;

/// <summary>
/// Singleton state container for theme configuration.
/// Provides event-based notification for theme changes across all components.
/// </summary>
public class ThemeState
{
    private ThemeConfiguration _currentTheme;

    public ThemeState()
    {
        // Use centralized default theme configuration
        _currentTheme = DefaultThemeConfiguration.GetDefaultTheme();
    }

    /// <summary>
    /// Gets the current theme configuration
    /// </summary>
    public ThemeConfiguration CurrentTheme => _currentTheme;

    /// <summary>
    /// Event triggered when theme changes
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Updates the current theme and notifies all subscribers
    /// </summary>
    public void UpdateTheme(ThemeConfiguration theme)
    {
        _currentTheme = theme;
        NotifyStateChanged();
    }

    /// <summary>
    /// Resets theme to default configuration
    /// </summary>
    public void ResetToDefault()
    {
        _currentTheme = DefaultThemeConfiguration.GetDefaultTheme();
        NotifyStateChanged();
    }

    /// <summary>
    /// Applies a preset theme by name
    /// </summary>
    public void ApplyPreset(string presetName)
    {
        _currentTheme = DefaultThemeConfiguration.GetPreset(presetName);
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

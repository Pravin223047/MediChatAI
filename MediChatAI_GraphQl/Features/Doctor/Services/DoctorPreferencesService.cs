using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Features.Doctor.Services;

public class DoctorPreferencesService : IDoctorPreferencesService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DoctorPreferencesService> _logger;

    public DoctorPreferencesService(ApplicationDbContext context, ILogger<DoctorPreferencesService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DoctorPreference> GetPreferencesAsync(string doctorId)
    {
        var preferences = await _context.DoctorPreferences
            .FirstOrDefaultAsync(p => p.DoctorId == doctorId);

        return preferences ?? await GetOrCreateDefaultPreferencesAsync(doctorId);
    }

    public async Task<DoctorPreference> UpdatePreferencesAsync(string doctorId, UpdateDoctorPreferencesInput input)
    {
        var preferences = await _context.DoctorPreferences
            .FirstOrDefaultAsync(p => p.DoctorId == doctorId);

        if (preferences == null)
        {
            preferences = await GetOrCreateDefaultPreferencesAsync(doctorId);
        }

        // Update theme preferences
        if (!string.IsNullOrEmpty(input.ThemePreset))
            preferences.ThemePreset = input.ThemePreset;
        if (!string.IsNullOrEmpty(input.ThemeMode))
            preferences.ThemeMode = input.ThemeMode;
        if (!string.IsNullOrEmpty(input.PrimaryColor))
            preferences.PrimaryColor = input.PrimaryColor;
        if (!string.IsNullOrEmpty(input.AccentColor))
            preferences.AccentColor = input.AccentColor;

        // Update dashboard preferences
        if (input.ShowPatientVitals.HasValue)
            preferences.ShowPatientVitals = input.ShowPatientVitals.Value;
        if (input.ShowAppointmentFeed.HasValue)
            preferences.ShowAppointmentFeed = input.ShowAppointmentFeed.Value;
        if (input.ShowChatWidget.HasValue)
            preferences.ShowChatWidget = input.ShowChatWidget.Value;
        if (input.ShowEmergencyAlerts.HasValue)
            preferences.ShowEmergencyAlerts = input.ShowEmergencyAlerts.Value;
        if (input.ShowAnalytics.HasValue)
            preferences.ShowAnalytics = input.ShowAnalytics.Value;
        if (!string.IsNullOrEmpty(input.DefaultDashboardView))
            preferences.DefaultDashboardView = input.DefaultDashboardView;

        // Update notification preferences
        if (input.EnableSoundNotifications.HasValue)
            preferences.EnableSoundNotifications = input.EnableSoundNotifications.Value;
        if (input.EnableDesktopNotifications.HasValue)
            preferences.EnableDesktopNotifications = input.EnableDesktopNotifications.Value;

        // Update AI preferences
        if (input.EnableAiInsights.HasValue)
            preferences.EnableAiInsights = input.EnableAiInsights.Value;
        if (input.EnableVoiceAssistant.HasValue)
            preferences.EnableVoiceAssistant = input.EnableVoiceAssistant.Value;

        // Update work preferences
        if (!string.IsNullOrEmpty(input.WorkStartTime))
            preferences.WorkStartTime = input.WorkStartTime;
        if (!string.IsNullOrEmpty(input.WorkEndTime))
            preferences.WorkEndTime = input.WorkEndTime;
        if (input.DefaultConsultationDuration.HasValue)
            preferences.DefaultConsultationDuration = input.DefaultConsultationDuration.Value;

        // Update language and region
        if (!string.IsNullOrEmpty(input.TimeZone))
            preferences.TimeZone = input.TimeZone;
        if (!string.IsNullOrEmpty(input.PreferredLanguage))
            preferences.PreferredLanguage = input.PreferredLanguage;

        preferences.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Preferences updated for doctor {DoctorId}", doctorId);

        return preferences;
    }

    public async Task<DoctorPreference> GetOrCreateDefaultPreferencesAsync(string doctorId)
    {
        var existing = await _context.DoctorPreferences
            .FirstOrDefaultAsync(p => p.DoctorId == doctorId);

        if (existing != null) return existing;

        var defaultPreferences = new DoctorPreference
        {
            DoctorId = doctorId,
            ThemePreset = "medical",
            ThemeMode = "light",
            ShowPatientVitals = true,
            ShowAppointmentFeed = true,
            ShowChatWidget = true,
            ShowEmergencyAlerts = true,
            ShowAnalytics = true,
            DefaultDashboardView = "split-panel",
            EnableSoundNotifications = true,
            EnableDesktopNotifications = true,
            EnableAiInsights = true,
            EnableVoiceAssistant = false,
            EnableSmartScheduling = true,
            WorkStartTime = "09:00",
            WorkEndTime = "17:00",
            DefaultConsultationDuration = 30,
            TimeZone = "UTC",
            PreferredLanguage = "en",
            DateFormat = "MM/DD/YYYY",
            TimeFormat = "12h",
            CreatedAt = DateTime.UtcNow
        };

        _context.DoctorPreferences.Add(defaultPreferences);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Default preferences created for doctor {DoctorId}", doctorId);

        return defaultPreferences;
    }
}

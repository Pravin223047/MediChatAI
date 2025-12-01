using Blazored.LocalStorage;
using MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Models;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services;

public interface IConversationStorageService
{
    Task<ChatSession?> GetCurrentSessionAsync();
    Task SaveCurrentSessionAsync(ChatSession session);
    Task<List<ChatSession>> GetLocalSessionsAsync();
    Task SaveLocalSessionAsync(ChatSession session);
    Task DeleteLocalSessionAsync(string sessionId);
    Task ClearCurrentSessionAsync();
    Task<bool> HasUnsavedChangesAsync(string sessionId);
}

public class ConversationStorageService : IConversationStorageService
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<ConversationStorageService> _logger;
    private const string CURRENT_SESSION_KEY = "ai_chat_current_session";
    private const string LOCAL_SESSIONS_KEY = "ai_chat_local_sessions";

    public ConversationStorageService(ILocalStorageService localStorage, ILogger<ConversationStorageService> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<ChatSession?> GetCurrentSessionAsync()
    {
        try
        {
            var sessionJson = await _localStorage.GetItemAsync<string>(CURRENT_SESSION_KEY);
            if (string.IsNullOrEmpty(sessionJson))
                return null;

            return JsonSerializer.Deserialize<ChatSession>(sessionJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current session");
            return null;
        }
    }

    public async Task SaveCurrentSessionAsync(ChatSession session)
    {
        try
        {
            session.LastMessageAt = DateTime.UtcNow;
            var sessionJson = JsonSerializer.Serialize(session);
            await _localStorage.SetItemAsync(CURRENT_SESSION_KEY, sessionJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving current session");
        }
    }

    public async Task<List<ChatSession>> GetLocalSessionsAsync()
    {
        try
        {
            var sessionsJson = await _localStorage.GetItemAsync<string>(LOCAL_SESSIONS_KEY);
            if (string.IsNullOrEmpty(sessionsJson))
                return new List<ChatSession>();

            return JsonSerializer.Deserialize<List<ChatSession>>(sessionsJson) ?? new List<ChatSession>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting local sessions");
            return new List<ChatSession>();
        }
    }

    public async Task SaveLocalSessionAsync(ChatSession session)
    {
        try
        {
            var sessions = await GetLocalSessionsAsync();

            // Remove existing session with same ID if it exists
            sessions.RemoveAll(s => s.Id == session.Id);

            // Add the updated session
            session.IsSaved = true;
            sessions.Insert(0, session);

            // Keep only the last 50 sessions
            if (sessions.Count > 50)
            {
                sessions = sessions.Take(50).ToList();
            }

            var sessionsJson = JsonSerializer.Serialize(sessions);
            await _localStorage.SetItemAsync(LOCAL_SESSIONS_KEY, sessionsJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving local session");
        }
    }

    public async Task DeleteLocalSessionAsync(string sessionId)
    {
        try
        {
            var sessions = await GetLocalSessionsAsync();
            sessions.RemoveAll(s => s.Id == sessionId);

            var sessionsJson = JsonSerializer.Serialize(sessions);
            await _localStorage.SetItemAsync(LOCAL_SESSIONS_KEY, sessionsJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting local session");
        }
    }

    public async Task ClearCurrentSessionAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync(CURRENT_SESSION_KEY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing current session");
        }
    }

    public async Task<bool> HasUnsavedChangesAsync(string sessionId)
    {
        try
        {
            var currentSession = await GetCurrentSessionAsync();
            if (currentSession == null || currentSession.Id != sessionId)
                return false;

            var savedSessions = await GetLocalSessionsAsync();
            var savedSession = savedSessions.FirstOrDefault(s => s.Id == sessionId);

            if (savedSession == null)
                return currentSession.Messages.Any();

            return currentSession.LastMessageAt > savedSession.LastMessageAt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for unsaved changes");
            return false;
        }
    }
}

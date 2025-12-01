using Microsoft.Extensions.Caching.Memory;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Services;

/// <summary>
/// Caches hospital context to reduce bandwidth on subsequent messages
/// </summary>
public class HospitalCacheService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _sessionsWithContext = new();

    public HospitalCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Caches hospital context by location (rounded to ~100m precision)
    /// </summary>
    public void CacheHospitalContext(double lat, double lng, List<HospitalContextDto> hospitals)
    {
        var key = GetLocationKey(lat, lng);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromMinutes(10), // Refresh if accessed within 10 min
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // Hard limit 30 min
        };

        _cache.Set(key, hospitals, cacheOptions);
        Console.WriteLine($"✅ Cached {hospitals.Count} hospitals for location {lat:F3},{lng:F3}");
    }

    /// <summary>
    /// Retrieves cached hospitals for a location
    /// </summary>
    public List<HospitalContextDto>? GetCachedHospitals(double lat, double lng)
    {
        var key = GetLocationKey(lat, lng);

        if (_cache.TryGetValue(key, out List<HospitalContextDto>? cached))
        {
            Console.WriteLine($"♻️ Retrieved {cached?.Count ?? 0} hospitals from cache");
            return cached;
        }

        Console.WriteLine($"❌ No cached hospitals found for {lat:F3},{lng:F3}");
        return null;
    }

    /// <summary>
    /// Determines if hospital context should be sent with the message
    /// Returns true for first message in session, false for subsequent messages if context is cached
    /// </summary>
    public bool ShouldSendContext(string sessionId, double? lat, double? lng)
    {
        // Always send if no location
        if (!lat.HasValue || !lng.HasValue)
        {
            return true;
        }

        // First message in this session - send context
        if (!_sessionsWithContext.Contains(sessionId))
        {
            _sessionsWithContext.Add(sessionId);
            Console.WriteLine($"📤 First message in session {sessionId} - sending hospital context");
            return true;
        }

        // Check if context is cached on backend
        var cached = GetCachedHospitals(lat.Value, lng.Value);
        if (cached != null)
        {
            Console.WriteLine($"⚡ Skipping hospital context - using cached data (bandwidth saved: ~{EstimateBandwidthSaved(cached.Count)}KB)");
            return false; // Backend has context cached, don't send
        }

        // Cache expired or not found - send again
        Console.WriteLine($"🔄 Cache expired - sending hospital context again");
        return true;
    }

    /// <summary>
    /// Marks that a session has been sent context
    /// </summary>
    public void MarkContextSent(string sessionId)
    {
        _sessionsWithContext.Add(sessionId);
    }

    /// <summary>
    /// Clears session tracking (call when chat is cleared)
    /// </summary>
    public void ClearSession(string sessionId)
    {
        _sessionsWithContext.Remove(sessionId);
        Console.WriteLine($"🗑️ Cleared session {sessionId} from cache tracking");
    }

    /// <summary>
    /// Gets the cache key for a location (rounded to 3 decimal places ≈ 100m precision)
    /// </summary>
    private static string GetLocationKey(double lat, double lng)
    {
        return $"hospitals_{lat:F3}_{lng:F3}";
    }

    /// <summary>
    /// Estimates bandwidth saved by not sending hospital context
    /// Rough estimate: each hospital ~200-300 bytes in JSON
    /// </summary>
    private static int EstimateBandwidthSaved(int hospitalCount)
    {
        // Average ~250 bytes per hospital in JSON
        var bytes = hospitalCount * 250;
        return bytes / 1024; // Convert to KB
    }

    /// <summary>
    /// Gets cache statistics for debugging
    /// </summary>
    public (int TotalSessions, int CacheSize) GetStats()
    {
        // Note: IMemoryCache doesn't expose count directly
        // This is an approximation
        return (_sessionsWithContext.Count, 0);
    }
}

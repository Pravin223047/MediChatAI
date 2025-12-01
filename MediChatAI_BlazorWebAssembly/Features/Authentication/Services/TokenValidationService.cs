using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Authentication.Services;

public class TokenValidationService : ITokenValidationService
{
    public bool IsTokenExpired(string? token)
    {
        var expiration = GetTokenExpiration(token);
        if (!expiration.HasValue)
            return true;

        return DateTime.UtcNow >= expiration.Value;
    }

    public DateTime? GetTokenExpiration(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var payload = parts[1];

            // Add padding if necessary
            var paddingLength = (4 - payload.Length % 4) % 4;
            payload = payload.PadRight(payload.Length + paddingLength, '=');

            // Replace URL-safe characters
            payload = payload.Replace('-', '+').Replace('_', '/');

            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);

            var doc = JsonDocument.Parse(payloadJson);
            if (doc.RootElement.TryGetProperty("exp", out var expElement))
            {
                var expUnix = expElement.GetInt64();
                var expiration = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                return expiration;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public TimeSpan? GetTimeUntilExpiration(string? token)
    {
        var expiration = GetTokenExpiration(token);
        if (!expiration.HasValue)
            return null;

        var timeRemaining = expiration.Value - DateTime.UtcNow;
        return timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero;
    }

    public bool IsTokenValid(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                return false;

            return !IsTokenExpired(token);
        }
        catch
        {
            return false;
        }
    }
}

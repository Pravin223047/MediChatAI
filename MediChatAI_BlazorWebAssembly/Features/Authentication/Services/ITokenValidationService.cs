namespace MediChatAI_BlazorWebAssembly.Features.Authentication.Services;

public interface ITokenValidationService
{
    /// <summary>
    /// Checks if the token is expired
    /// </summary>
    bool IsTokenExpired(string? token);

    /// <summary>
    /// Gets the expiration time from the token
    /// </summary>
    DateTime? GetTokenExpiration(string? token);

    /// <summary>
    /// Gets the time remaining until token expiration
    /// </summary>
    TimeSpan? GetTimeUntilExpiration(string? token);

    /// <summary>
    /// Validates if token is still valid (not expired and properly formatted)
    /// </summary>
    bool IsTokenValid(string? token);
}

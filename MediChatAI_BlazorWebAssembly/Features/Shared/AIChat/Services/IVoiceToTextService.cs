namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services;

/// <summary>
/// Service for converting voice/audio to text
/// </summary>
public interface IVoiceToTextService
{
    /// <summary>
    /// Converts audio data to text
    /// </summary>
    /// <param name="audioDataUrl">The audio data URL (base64 encoded)</param>
    /// <returns>The transcribed text</returns>
    Task<string?> ConvertToTextAsync(string audioDataUrl);

    /// <summary>
    /// Starts voice recognition using the browser's Web Speech API
    /// </summary>
    /// <returns>True if recognition started successfully</returns>
    Task<bool> StartRecognitionAsync();

    /// <summary>
    /// Stops voice recognition
    /// </summary>
    /// <returns>The recognized text</returns>
    Task<string?> StopRecognitionAsync();

    /// <summary>
    /// Event raised when recognition results are available
    /// </summary>
    event Action<string>? OnRecognitionResult;

    /// <summary>
    /// Event raised when recognition encounters an error
    /// </summary>
    event Action<string>? OnRecognitionError;
}

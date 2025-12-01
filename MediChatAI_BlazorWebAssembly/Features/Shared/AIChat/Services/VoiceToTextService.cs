using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;

namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services;

/// <summary>
/// Service for converting voice/audio to text using Web Speech API
/// </summary>
public class VoiceToTextService : IVoiceToTextService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<VoiceToTextService> _logger;
    private DotNetObjectReference<VoiceToTextService>? _dotNetReference;
    private bool _isRecognizing = false;

    public event Action<string>? OnRecognitionResult;
    public event Action<string>? OnRecognitionError;

    public VoiceToTextService(IJSRuntime jsRuntime, ILogger<VoiceToTextService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task<string?> ConvertToTextAsync(string audioDataUrl)
    {
        try
        {
            // This would typically call a server-side speech-to-text API
            // For now, we'll use the Web Speech API via JS interop
            _logger.LogInformation("Converting audio to text");

            // Extract base64 audio data
            var base64Data = audioDataUrl.Contains(",") ? audioDataUrl.Split(',')[1] : audioDataUrl;

            // Note: Web Speech API doesn't support converting audio files directly
            // This would require sending to a server-side API like Azure Speech, Google Cloud Speech, etc.
            _logger.LogWarning("Audio file conversion not yet implemented. Use StartRecognitionAsync for real-time recognition.");

            await Task.CompletedTask;
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting audio to text");
            return null;
        }
    }

    public async Task<bool> StartRecognitionAsync()
    {
        try
        {
            if (_isRecognizing)
            {
                _logger.LogWarning("Recognition already in progress");
                return false;
            }

            _dotNetReference = DotNetObjectReference.Create(this);

            var result = await _jsRuntime.InvokeAsync<bool>("speechRecognition.start", _dotNetReference);

            if (result)
            {
                _isRecognizing = true;
                _logger.LogInformation("Voice recognition started");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting voice recognition");
            OnRecognitionError?.Invoke($"Failed to start recognition: {ex.Message}");
            return false;
        }
    }

    public async Task<string?> StopRecognitionAsync()
    {
        try
        {
            if (!_isRecognizing)
            {
                _logger.LogWarning("No recognition in progress");
                return null;
            }

            var result = await _jsRuntime.InvokeAsync<string>("speechRecognition.stop");
            _isRecognizing = false;

            _logger.LogInformation("Voice recognition stopped");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping voice recognition");
            _isRecognizing = false;
            return null;
        }
    }

    /// <summary>
    /// Called by JavaScript when recognition results are available
    /// </summary>
    [JSInvokable]
    public void HandleRecognitionResult(string text)
    {
        _logger.LogInformation("Recognition result: {Text}", text);
        OnRecognitionResult?.Invoke(text);
    }

    /// <summary>
    /// Called by JavaScript when recognition encounters an error
    /// </summary>
    [JSInvokable]
    public void HandleRecognitionError(string error)
    {
        _logger.LogError("Recognition error: {Error}", error);
        _isRecognizing = false;
        OnRecognitionError?.Invoke(error);
    }

    public async ValueTask DisposeAsync()
    {
        if (_isRecognizing)
        {
            await StopRecognitionAsync();
        }

        _dotNetReference?.Dispose();
    }
}

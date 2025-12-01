using Microsoft.JSInterop;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using System.Text;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Core.Services.Consultation;

/// <summary>
/// Service for managing consultation session recordings using browser MediaRecorder API
/// </summary>
public class ConsultationRecordingService : IConsultationRecordingService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<ConsultationRecordingService> _logger;
    private DotNetObjectReference<ConsultationRecordingService>? _dotNetRef;
    private int _currentSessionId;
    private bool _isInitialized;

    #region Events

    public event Action? OnRecordingStarted;
    public event Action<long>? OnRecordingStopped;
    public event Action<long>? OnChunkRecorded;
    public event Action<UploadProgress>? OnUploadProgress;
    public event Action<string>? OnRecordingError;

    #endregion

    public ConsultationRecordingService(
        IJSRuntime jsRuntime,
        IGraphQLService graphQLService,
        ILogger<ConsultationRecordingService> logger)
    {
        _jsRuntime = jsRuntime;
        _graphQLService = graphQLService;
        _logger = logger;
    }

    #region JavaScript Callbacks

    /// <summary>
    /// Called by JavaScript when recording starts
    /// </summary>
    [JSInvokable]
    public void OnRecordingStartedCallback()
    {
        _logger.LogInformation("Recording started for session {SessionId}", _currentSessionId);
        OnRecordingStarted?.Invoke();
    }

    /// <summary>
    /// Called by JavaScript when recording stops
    /// </summary>
    [JSInvokable]
    public void OnRecordingStoppedCallback(long durationMs)
    {
        _logger.LogInformation("Recording stopped for session {SessionId}, duration: {Duration}ms",
            _currentSessionId, durationMs);
        OnRecordingStopped?.Invoke(durationMs);
    }

    /// <summary>
    /// Called by JavaScript when a chunk is recorded
    /// </summary>
    [JSInvokable]
    public void OnChunkRecordedCallback(long totalSizeBytes)
    {
        _logger.LogDebug("Chunk recorded, total size: {Size} bytes", totalSizeBytes);
        OnChunkRecorded?.Invoke(totalSizeBytes);
    }

    /// <summary>
    /// Called by JavaScript when upload progress changes
    /// </summary>
    [JSInvokable]
    public void OnUploadProgressCallback(double percentComplete, long bytesUploaded, long totalBytes)
    {
        _logger.LogDebug("Upload progress: {Percent}% ({Uploaded}/{Total} bytes)",
            percentComplete.ToString("F1"), bytesUploaded, totalBytes);

        OnUploadProgress?.Invoke(new UploadProgress
        {
            PercentComplete = percentComplete,
            BytesUploaded = bytesUploaded,
            TotalBytes = totalBytes
        });
    }

    /// <summary>
    /// Called by JavaScript when an error occurs
    /// </summary>
    [JSInvokable]
    public void OnRecordingErrorCallback(string errorMessage)
    {
        _logger.LogError("Recording error: {Error}", errorMessage);
        OnRecordingError?.Invoke(errorMessage);
    }

    #endregion

    #region Recording Control

    public async Task<(string camera, string microphone)?> CheckMediaPermissionsAsync()
    {
        try
        {
            await EnsureInitializedAsync();

            var result = await _jsRuntime.InvokeAsync<JsonElement?>(
                "consultationRecorder.checkMediaPermissions");

            if (result == null || result.Value.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            var camera = result.Value.GetProperty("camera").GetString() ?? "prompt";
            var microphone = result.Value.GetProperty("microphone").GetString() ?? "prompt";

            _logger.LogInformation("Media permissions - Camera: {Camera}, Microphone: {Microphone}",
                camera, microphone);

            return (camera, microphone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking media permissions");
            return null;
        }
    }

    public async Task<bool> StartRecordingAsync(
        int sessionId,
        RecordingType type = RecordingType.Video,
        int videoBitrate = 2500000,
        int audioBitrate = 128000)
    {
        try
        {
            await EnsureInitializedAsync();

            _currentSessionId = sessionId;

            var options = new
            {
                type = type.ToString().ToLower(),
                videoBitrate,
                audioBitrate,
                video = type != RecordingType.Audio,
                audio = true
            };

            var result = await _jsRuntime.InvokeAsync<JsonElement>(
                "consultationRecorder.startRecording", _dotNetRef, options);

            var success = result.GetProperty("success").GetBoolean();

            if (success)
            {
                _logger.LogInformation("Started {Type} recording for session {SessionId}", type, sessionId);
            }
            else
            {
                var error = result.TryGetProperty("error", out var errorProp) ? errorProp.GetString() : "Unknown error";
                _logger.LogError("Failed to start recording: {Error}", error);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting recording for session {SessionId}", sessionId);
            OnRecordingError?.Invoke(ex.Message);
            return false;
        }
    }

    public async Task<RecordingResult?> StopRecordingAsync()
    {
        try
        {
            await EnsureInitializedAsync();

            var result = await _jsRuntime.InvokeAsync<JsonElement>(
                "consultationRecorder.stopRecording");

            if (result.TryGetProperty("success", out var successProp) && successProp.GetBoolean())
            {
                var recordingResult = new RecordingResult
                {
                    Success = true,
                    ObjectUrl = result.GetProperty("objectUrl").GetString(),
                    SizeInBytes = result.GetProperty("sizeInBytes").GetInt64(),
                    DurationMs = result.GetProperty("durationMs").GetInt64(),
                    MimeType = result.GetProperty("mimeType").GetString(),
                    ChunkCount = result.GetProperty("chunkCount").GetInt32()
                };

                _logger.LogInformation("Stopped recording for session {SessionId}, size: {Size}MB, duration: {Duration}s",
                    _currentSessionId,
                    (recordingResult.SizeInBytes / (1024.0 * 1024.0)).ToString("F2"),
                    (recordingResult.DurationMs / 1000.0).ToString("F1"));

                return recordingResult;
            }
            else
            {
                var error = result.TryGetProperty("error", out var errorProp) ? errorProp.GetString() : "Unknown error";
                _logger.LogError("Failed to stop recording: {Error}", error);
                return new RecordingResult { Success = false, Error = error };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping recording for session {SessionId}", _currentSessionId);
            return new RecordingResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<bool> PauseRecordingAsync()
    {
        try
        {
            await EnsureInitializedAsync();

            var result = await _jsRuntime.InvokeAsync<bool>("consultationRecorder.pauseRecording");

            if (result)
            {
                _logger.LogInformation("Paused recording for session {SessionId}", _currentSessionId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing recording");
            return false;
        }
    }

    public async Task<bool> ResumeRecordingAsync()
    {
        try
        {
            await EnsureInitializedAsync();

            var result = await _jsRuntime.InvokeAsync<bool>("consultationRecorder.resumeRecording");

            if (result)
            {
                _logger.LogInformation("Resumed recording for session {SessionId}", _currentSessionId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming recording");
            return false;
        }
    }

    public async Task<bool> CancelRecordingAsync()
    {
        try
        {
            await EnsureInitializedAsync();

            var result = await _jsRuntime.InvokeAsync<JsonElement>("consultationRecorder.cancelRecording");
            var success = result.GetProperty("success").GetBoolean();

            if (success)
            {
                _logger.LogInformation("Cancelled recording for session {SessionId}", _currentSessionId);
                _currentSessionId = 0;
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling recording");
            return false;
        }
    }

    public async Task<RecordingState> GetRecordingStateAsync()
    {
        try
        {
            await EnsureInitializedAsync();

            var state = await _jsRuntime.InvokeAsync<string>("consultationRecorder.getRecordingState");
            var duration = await _jsRuntime.InvokeAsync<long>("consultationRecorder.getRecordingDuration");
            var size = await _jsRuntime.InvokeAsync<long>("consultationRecorder.getRecordingSize");

            return new RecordingState
            {
                State = state,
                DurationMs = duration,
                SizeBytes = size,
                StartTime = state == "recording" ? DateTime.UtcNow.AddMilliseconds(-duration) : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recording state");
            return new RecordingState { State = "inactive" };
        }
    }

    #endregion

    #region Upload

    public async Task<bool> UploadRecordingAsync(int sessionId, string objectUrl, string recordedByUserId)
    {
        try
        {
            _logger.LogInformation("Uploading recording for session {SessionId}", sessionId);

            // First, convert the blob to base64
            var blob = await _jsRuntime.InvokeAsync<string>("consultationRecorder.objectUrlToBlob", objectUrl);
            var base64Data = await _jsRuntime.InvokeAsync<string>("consultationRecorder.blobToBase64", blob);

            // Call GraphQL mutation to create recording
            const string mutation = """
                mutation CreateRecording($input: CreateRecordingInput!) {
                  createRecording(input: $input) {
                    id
                    recordingUrl
                    status
                  }
                }
                """;

            var input = new
            {
                sessionId,
                recordedByUserId,
                recordingData = base64Data,
                type = "FullSession"
            };

            var variables = new { input };
            var response = await _graphQLService.SendQueryAsync<CreateRecordingResponse>(mutation, variables);

            if (response?.Recording != null)
            {
                _logger.LogInformation("Successfully uploaded recording {RecordingId} for session {SessionId}",
                    response.Recording.Id, sessionId);
                return true;
            }

            _logger.LogWarning("Failed to upload recording for session {SessionId}", sessionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading recording for session {SessionId}", sessionId);
            OnRecordingError?.Invoke($"Upload failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UploadRecordingChunkedAsync(
        int sessionId,
        string objectUrl,
        string recordedByUserId,
        int chunkSizeMB = 5)
    {
        try
        {
            _logger.LogInformation("Uploading recording for session {SessionId} in {ChunkSize}MB chunks",
                sessionId, chunkSizeMB);

            // Get blob from object URL
            var blob = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                "consultationRecorder.objectUrlToBlob", objectUrl);

            // Convert to chunks
            var chunkSizeBytes = chunkSizeMB * 1024 * 1024;
            var chunks = await _jsRuntime.InvokeAsync<JsonElement>(
                "consultationRecorder.blobToChunks", blob, chunkSizeBytes);

            var chunkArray = chunks.EnumerateArray().ToList();
            var totalChunks = chunkArray.Count;

            _logger.LogInformation("Recording split into {TotalChunks} chunks", totalChunks);

            // Upload each chunk
            for (int i = 0; i < totalChunks; i++)
            {
                var chunk = chunkArray[i];
                var chunkData = chunk.GetProperty("data").GetString();
                var isLast = chunk.GetProperty("isLast").GetBoolean();

                const string mutation = """
                    mutation UploadRecordingChunk($input: UploadChunkInput!) {
                      uploadRecordingChunk(input: $input)
                    }
                    """;

                var chunkInput = new
                {
                    sessionId,
                    recordedByUserId,
                    chunkIndex = i,
                    chunkData,
                    isLastChunk = isLast,
                    totalChunks
                };

                var variables = new { input = chunkInput };
                var response = await _graphQLService.SendQueryAsync<UploadChunkResponse>(mutation, variables);

                if (response?.Success != true)
                {
                    _logger.LogError("Failed to upload chunk {ChunkIndex}/{TotalChunks}", i + 1, totalChunks);
                    return false;
                }

                // Notify progress
                var progress = ((i + 1) / (double)totalChunks) * 100;
                OnUploadProgress?.Invoke(new UploadProgress
                {
                    PercentComplete = progress,
                    BytesUploaded = (i + 1) * chunkSizeBytes,
                    TotalBytes = totalChunks * chunkSizeBytes
                });

                _logger.LogDebug("Uploaded chunk {ChunkIndex}/{TotalChunks} ({Progress}%)",
                    i + 1, totalChunks, progress.ToString("F1"));
            }

            _logger.LogInformation("Successfully uploaded all {TotalChunks} chunks for session {SessionId}",
                totalChunks, sessionId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading recording chunks for session {SessionId}", sessionId);
            OnRecordingError?.Invoke($"Chunked upload failed: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Utility

    public async Task<string?> GetVideoThumbnailAsync(string objectUrl, int seekTimeSeconds = 1)
    {
        try
        {
            await EnsureInitializedAsync();

            var thumbnailUrl = await _jsRuntime.InvokeAsync<string>(
                "consultationRecorder.getVideoThumbnail", objectUrl, seekTimeSeconds);

            _logger.LogDebug("Generated video thumbnail from {ObjectUrl}", objectUrl);

            return thumbnailUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating video thumbnail");
            return null;
        }
    }

    public async Task RevokeObjectUrlAsync(string objectUrl)
    {
        try
        {
            await EnsureInitializedAsync();
            await _jsRuntime.InvokeVoidAsync("consultationRecorder.revokeObjectURL", objectUrl);
            _logger.LogDebug("Revoked object URL: {ObjectUrl}", objectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking object URL");
        }
    }

    public async Task<MediaDevices?> GetMediaDevicesAsync()
    {
        try
        {
            await EnsureInitializedAsync();

            var result = await _jsRuntime.InvokeAsync<JsonElement>("consultationRecorder.getMediaDevices");

            var devices = new MediaDevices
            {
                VideoInputs = result.GetProperty("videoInputs")
                    .EnumerateArray()
                    .Select(d => new MediaDevice
                    {
                        DeviceId = d.GetProperty("deviceId").GetString() ?? "",
                        Label = d.GetProperty("label").GetString() ?? ""
                    })
                    .ToList(),

                AudioInputs = result.GetProperty("audioInputs")
                    .EnumerateArray()
                    .Select(d => new MediaDevice
                    {
                        DeviceId = d.GetProperty("deviceId").GetString() ?? "",
                        Label = d.GetProperty("label").GetString() ?? ""
                    })
                    .ToList(),

                AudioOutputs = result.GetProperty("audioOutputs")
                    .EnumerateArray()
                    .Select(d => new MediaDevice
                    {
                        DeviceId = d.GetProperty("deviceId").GetString() ?? "",
                        Label = d.GetProperty("label").GetString() ?? ""
                    })
                    .ToList()
            };

            _logger.LogInformation("Found {VideoCount} video, {AudioInCount} audio input, {AudioOutCount} audio output devices",
                devices.VideoInputs.Count, devices.AudioInputs.Count, devices.AudioOutputs.Count);

            return devices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media devices");
            return null;
        }
    }

    public async Task CleanupAsync()
    {
        try
        {
            if (_isInitialized)
            {
                await _jsRuntime.InvokeVoidAsync("consultationRecorder.cleanup");
                _logger.LogDebug("Cleaned up consultation recorder");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up consultation recorder");
        }
    }

    #endregion

    #region Private Methods

    private Task EnsureInitializedAsync()
    {
        if (!_isInitialized)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _isInitialized = true;
            _logger.LogDebug("Initialized ConsultationRecordingService");
        }
        return Task.CompletedTask;
    }

    #endregion

    #region Response Models

    private class CreateRecordingResponse
    {
        public RecordingDto? Recording { get; set; }
    }

    private class RecordingDto
    {
        public int Id { get; set; }
        public string RecordingUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    private class UploadChunkResponse
    {
        public bool Success { get; set; }
    }

    #endregion

    #region IAsyncDisposable

    public async ValueTask DisposeAsync()
    {
        await CleanupAsync();

        _dotNetRef?.Dispose();
        _dotNetRef = null;

        _logger.LogDebug("Disposed ConsultationRecordingService");
    }

    #endregion
}

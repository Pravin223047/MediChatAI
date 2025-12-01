namespace MediChatAI_BlazorWebAssembly.Core.Services.Consultation;

/// <summary>
/// Recording types supported by the consultation recorder
/// </summary>
public enum RecordingType
{
    Video,
    Audio,
    Screen
}

/// <summary>
/// Recording state information
/// </summary>
public class RecordingState
{
    public string State { get; set; } = "inactive"; // inactive, recording, paused
    public long DurationMs { get; set; }
    public long SizeBytes { get; set; }
    public DateTime? StartTime { get; set; }
}

/// <summary>
/// Recording result after stopping
/// </summary>
public class RecordingResult
{
    public bool Success { get; set; }
    public string? ObjectUrl { get; set; }
    public long SizeInBytes { get; set; }
    public long DurationMs { get; set; }
    public string? MimeType { get; set; }
    public int ChunkCount { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Upload progress information
/// </summary>
public class UploadProgress
{
    public double PercentComplete { get; set; }
    public long BytesUploaded { get; set; }
    public long TotalBytes { get; set; }
}

/// <summary>
/// Service for managing consultation session recordings using browser MediaRecorder API
/// </summary>
public interface IConsultationRecordingService
{
    #region Events

    /// <summary>
    /// Fired when recording starts
    /// </summary>
    event Action? OnRecordingStarted;

    /// <summary>
    /// Fired when recording stops
    /// </summary>
    event Action<long>? OnRecordingStopped; // duration in ms

    /// <summary>
    /// Fired when a chunk is recorded (for size tracking)
    /// </summary>
    event Action<long>? OnChunkRecorded; // total size in bytes

    /// <summary>
    /// Fired when upload progress changes
    /// </summary>
    event Action<UploadProgress>? OnUploadProgress;

    /// <summary>
    /// Fired when an error occurs
    /// </summary>
    event Action<string>? OnRecordingError;

    #endregion

    #region Recording Control

    /// <summary>
    /// Checks if camera and microphone permissions are granted
    /// </summary>
    Task<(string camera, string microphone)?> CheckMediaPermissionsAsync();

    /// <summary>
    /// Starts recording the consultation
    /// </summary>
    Task<bool> StartRecordingAsync(int sessionId, RecordingType type = RecordingType.Video, int videoBitrate = 2500000, int audioBitrate = 128000);

    /// <summary>
    /// Stops recording and returns the recording result
    /// </summary>
    Task<RecordingResult?> StopRecordingAsync();

    /// <summary>
    /// Pauses the current recording
    /// </summary>
    Task<bool> PauseRecordingAsync();

    /// <summary>
    /// Resumes a paused recording
    /// </summary>
    Task<bool> ResumeRecordingAsync();

    /// <summary>
    /// Cancels the current recording without saving
    /// </summary>
    Task<bool> CancelRecordingAsync();

    /// <summary>
    /// Gets the current recording state
    /// </summary>
    Task<RecordingState> GetRecordingStateAsync();

    #endregion

    #region Upload

    /// <summary>
    /// Uploads a recording to the backend
    /// </summary>
    Task<bool> UploadRecordingAsync(int sessionId, string objectUrl, string recordedByUserId);

    /// <summary>
    /// Uploads a recording in chunks for large files
    /// </summary>
    Task<bool> UploadRecordingChunkedAsync(int sessionId, string objectUrl, string recordedByUserId, int chunkSizeMB = 5);

    #endregion

    #region Utility

    /// <summary>
    /// Gets a video thumbnail from the recording
    /// </summary>
    Task<string?> GetVideoThumbnailAsync(string objectUrl, int seekTimeSeconds = 1);

    /// <summary>
    /// Revokes an object URL to free memory
    /// </summary>
    Task RevokeObjectUrlAsync(string objectUrl);

    /// <summary>
    /// Gets available media devices (cameras, microphones)
    /// </summary>
    Task<MediaDevices?> GetMediaDevicesAsync();

    /// <summary>
    /// Cleans up recording resources
    /// </summary>
    Task CleanupAsync();

    #endregion
}

/// <summary>
/// Media devices information
/// </summary>
public class MediaDevices
{
    public List<MediaDevice> VideoInputs { get; set; } = new();
    public List<MediaDevice> AudioInputs { get; set; } = new();
    public List<MediaDevice> AudioOutputs { get; set; } = new();
}

/// <summary>
/// Individual media device
/// </summary>
public class MediaDevice
{
    public string DeviceId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

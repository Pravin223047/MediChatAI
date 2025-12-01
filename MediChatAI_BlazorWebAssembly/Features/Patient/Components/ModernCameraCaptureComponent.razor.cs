using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Components;

public partial class ModernCameraCaptureComponent : ComponentBase, IAsyncDisposable
{
    // Parameters
    [Parameter] public EventCallback<List<CapturedMediaInfo>> OnMediaCaptured { get; set; }
    [Parameter] public bool IsDarkMode { get; set; }
    [Parameter] public int MaxPhotos { get; set; } = 10;
    [Parameter] public int MaxVideoDuration { get; set; } = 30; // seconds

    // Element References
    private ElementReference videoElement;
    private ElementReference canvasElement;
    private ElementReference previewImageElement;
    private ElementReference annotationCanvas;

    // State
    private bool isCameraActive = false;
    private bool isStartingCamera = false;
    private bool isPreviewingCapture = false;
    private bool isAnnotating = false;
    private bool showGalleryView = false;
    private bool isRecording = false;
    private bool flashEnabled = false;
    private bool showGrid = false;
    private bool aiEnhancementEnabled = true;
    private string errorMessage = string.Empty;
    private string currentFacingMode = "user";
    private string gridType = "rule-of-thirds";
    private string annotationColor = "#ff0000";

    // Capture Mode
    private CaptureMode captureMode = CaptureMode.Photo;

    // Annotation
    private AnnotationTool currentAnnotationTool = AnnotationTool.Circle;
    private List<Annotation> annotations = new();

    // Media Storage
    private List<CapturedMediaInfo> capturedMedia = new();
    private CapturedMediaInfo? currentPreviewMedia = null;

    // Quality Metrics
    private QualityMetrics? qualityMetrics = null;
    private System.Threading.Timer? qualityCheckTimer;

    // Recording
    private TimeSpan recordingDuration = TimeSpan.Zero;
    private System.Threading.Timer? recordingTimer;
    private DateTime recordingStartTime;

    // JS Interop
    private IJSObjectReference? cameraModule;
    private DotNetObjectReference<ModernCameraCaptureComponent>? dotNetReference;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Load the modern camera JavaScript module
                cameraModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/modernCamera.js");
                dotNetReference = DotNetObjectReference.Create(this);

                // Setup quality update listener
                await JS.InvokeVoidAsync("window.addEventListener", "cameraQualityUpdate",
                    DotNetObjectReference.Create(new QualityUpdateListener(this)));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error initializing modern camera module");
                errorMessage = "Failed to initialize camera module";
            }
        }
    }

    private async Task StartCamera()
    {
        try
        {
            isStartingCamera = true;
            errorMessage = string.Empty;
            isCameraActive = true; // Set to true first so the video element gets rendered
            StateHasChanged();

            // Wait for the video element to be rendered
            await Task.Delay(100);

            Logger.LogInformation("Starting camera with facing mode: {FacingMode}", currentFacingMode);

            var result = await JS.InvokeAsync<CameraInitResult>("initializeModernCamera", videoElement, currentFacingMode);

            if (result.Success)
            {
                Logger.LogInformation("Camera started successfully");
                StartQualityMonitoring();
            }
            else
            {
                Logger.LogError("Camera initialization failed: {Error}", result.Error);
                errorMessage = $"Camera error: {result.Error}";
                isCameraActive = false; // Revert if failed
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting camera");

            // Provide more helpful error messages
            if (ex.Message.Contains("Permission") || ex.Message.Contains("NotAllowedError"))
            {
                errorMessage = "Camera permission denied. Please allow camera access in your browser settings.";
            }
            else if (ex.Message.Contains("NotFoundError") || ex.Message.Contains("not found"))
            {
                errorMessage = "No camera found. Please connect a camera and try again.";
            }
            else if (ex.Message.Contains("NotReadableError") || ex.Message.Contains("in use"))
            {
                errorMessage = "Camera is already in use by another application. Please close other apps and try again.";
            }
            else
            {
                errorMessage = $"Failed to access camera: {ex.Message}. Please check permissions.";
            }

            isCameraActive = false;
        }
        finally
        {
            isStartingCamera = false;
            StateHasChanged();
        }
    }

    private async Task StopCamera()
    {
        try
        {
            StopQualityMonitoring();
            await JS.InvokeVoidAsync("stopModernCamera");
            isCameraActive = false;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error stopping camera");
        }
    }

    private async Task SwitchCamera()
    {
        try
        {
            Logger.LogInformation("Switching camera from {CurrentMode} to {NewMode}",
                currentFacingMode, currentFacingMode == "user" ? "environment" : "user");

            currentFacingMode = currentFacingMode == "user" ? "environment" : "user";
            await StopCamera();
            await Task.Delay(300); // Give camera time to release
            await StartCamera();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error switching camera");
            errorMessage = "Failed to switch camera. Please try again.";
        }
    }

    private async Task CapturePhoto()
    {
        try
        {
            var result = await JS.InvokeAsync<PhotoCaptureResult>("capturePhotoWithEnhancements",
                videoElement, canvasElement, aiEnhancementEnabled);

            if (result.Success)
            {
                currentPreviewMedia = new CapturedMediaInfo
                {
                    Type = MediaType.Photo,
                    DataUrl = result.ImageData,
                    Timestamp = DateTime.UtcNow,
                    Width = result.Width,
                    Height = result.Height
                };

                isPreviewingCapture = true;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error capturing photo");
            errorMessage = "Failed to capture photo";
        }
    }

    private async Task ToggleVideoRecording()
    {
        if (!isRecording)
        {
            await StartVideoRecording();
        }
        else
        {
            await StopVideoRecording();
        }
    }

    private async Task StartVideoRecording()
    {
        try
        {
            var result = await JS.InvokeAsync<RecordingResult>("startVideoRecording", videoElement);

            if (result.Success)
            {
                isRecording = true;
                recordingStartTime = DateTime.UtcNow;
                recordingDuration = TimeSpan.Zero;

                // Start recording timer
                recordingTimer = new System.Threading.Timer(_ =>
                {
                    recordingDuration = DateTime.UtcNow - recordingStartTime;
                    InvokeAsync(StateHasChanged);

                    // Auto-stop at max duration
                    if (recordingDuration.TotalSeconds >= MaxVideoDuration)
                    {
                        InvokeAsync(StopVideoRecording);
                    }
                }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));

                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting video recording");
            errorMessage = "Failed to start recording";
        }
    }

    private async Task StopVideoRecording()
    {
        try
        {
            recordingTimer?.Dispose();
            recordingTimer = null;

            var result = await JS.InvokeAsync<VideoStopResult>("stopVideoRecording");

            if (result.Success)
            {
                currentPreviewMedia = new CapturedMediaInfo
                {
                    Type = MediaType.Video,
                    DataUrl = result.VideoData,
                    Timestamp = DateTime.UtcNow,
                    Duration = TimeSpan.FromSeconds(result.Duration),
                    Size = result.Size
                };

                isRecording = false;
                isPreviewingCapture = true;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error stopping video recording");
            errorMessage = "Failed to stop recording";
        }
    }

    private void SetCaptureMode(CaptureMode mode)
    {
        if (isRecording)
        {
            return; // Can't change mode while recording
        }

        captureMode = mode;
        StateHasChanged();
    }

    private void ToggleFlash()
    {
        flashEnabled = !flashEnabled;
        // Note: Flash control would require additional JS implementation
        StateHasChanged();
    }

    private void CycleGridOverlay()
    {
        if (!showGrid)
        {
            showGrid = true;
            gridType = "rule-of-thirds";
        }
        else if (gridType == "rule-of-thirds")
        {
            gridType = "medical-grid";
        }
        else
        {
            showGrid = false;
        }
        StateHasChanged();
    }

    private void ToggleAIEnhancement()
    {
        aiEnhancementEnabled = !aiEnhancementEnabled;
        StateHasChanged();
    }

    private async Task UsePreview()
    {
        if (currentPreviewMedia != null)
        {
            // Apply annotations if any
            if (isAnnotating && annotations.Count > 0)
            {
                var annotatedImage = await JS.InvokeAsync<string>("addAnnotationToImage",
                    currentPreviewMedia.DataUrl, annotations);
                currentPreviewMedia.DataUrl = annotatedImage;
            }

            capturedMedia.Add(currentPreviewMedia);

            // Notify parent component
            await OnMediaCaptured.InvokeAsync(capturedMedia);

            // Reset preview state
            currentPreviewMedia = null;
            isPreviewingCapture = false;
            isAnnotating = false;
            annotations.Clear();

            StateHasChanged();
        }
    }

    private void DiscardPreview()
    {
        currentPreviewMedia = null;
        isPreviewingCapture = false;
        isAnnotating = false;
        annotations.Clear();
        StateHasChanged();
    }

    private async Task RotatePreview()
    {
        if (currentPreviewMedia != null && currentPreviewMedia.Type == MediaType.Photo)
        {
            var rotated = await JS.InvokeAsync<string>("rotateImage", currentPreviewMedia.DataUrl, 90);
            currentPreviewMedia.DataUrl = rotated;
            StateHasChanged();
        }
    }

    private void ToggleAnnotation()
    {
        isAnnotating = !isAnnotating;
        StateHasChanged();
    }

    private void SetAnnotationTool(AnnotationTool tool)
    {
        currentAnnotationTool = tool;
        StateHasChanged();
    }

    private void ClearAnnotations()
    {
        annotations.Clear();
        StateHasChanged();
    }

    private void ShowGallery()
    {
        showGalleryView = true;
        StateHasChanged();
    }

    private void CloseGallery()
    {
        showGalleryView = false;
        StateHasChanged();
    }

    private void PreviewGalleryItem(CapturedMediaInfo media)
    {
        currentPreviewMedia = media;
        isPreviewingCapture = true;
        showGalleryView = false;
        StateHasChanged();
    }

    private void DeleteMediaItem(CapturedMediaInfo media)
    {
        capturedMedia.Remove(media);
        StateHasChanged();
    }

    private void StartQualityMonitoring()
    {
        qualityCheckTimer = new System.Threading.Timer(async _ =>
        {
            try
            {
                // Quality is updated via JS event listener
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in quality monitoring");
            }
        }, null, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
    }

    private void StopQualityMonitoring()
    {
        qualityCheckTimer?.Dispose();
        qualityCheckTimer = null;
    }

    [JSInvokable]
    public void UpdateQuality(QualityMetrics metrics)
    {
        qualityMetrics = metrics;
        InvokeAsync(StateHasChanged);
    }

    private string GetQualityClass(int score)
    {
        return score switch
        {
            >= 80 => "quality-excellent",
            >= 60 => "quality-good",
            >= 40 => "quality-fair",
            _ => "quality-poor"
        };
    }

    public async ValueTask DisposeAsync()
    {
        StopQualityMonitoring();
        recordingTimer?.Dispose();

        if (isCameraActive)
        {
            await StopCamera();
        }

        dotNetReference?.Dispose();

        if (cameraModule != null)
        {
            await cameraModule.DisposeAsync();
        }
    }
}

// Supporting Classes and Enums
public enum CaptureMode
{
    Photo,
    Video
}

public enum MediaType
{
    Photo,
    Video
}

public enum AnnotationTool
{
    Circle,
    Arrow,
    Line
}

public class CapturedMediaInfo
{
    public MediaType Type { get; set; }
    public string DataUrl { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public TimeSpan Duration { get; set; }
    public long Size { get; set; }
}

public class QualityMetrics
{
    public int Brightness { get; set; }
    public int Contrast { get; set; }
    public int Sharpness { get; set; }
    public int QualityScore { get; set; }
    public string Recommendation { get; set; } = string.Empty;
}

public class Annotation
{
    public string Type { get; set; } = string.Empty;
    public string Color { get; set; } = "#ff0000";
    public int LineWidth { get; set; } = 3;
    public double X { get; set; }
    public double Y { get; set; }
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
    public double Radius { get; set; }
}

public class CameraInitResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class PhotoCaptureResult
{
    public bool Success { get; set; }
    public string ImageData { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class RecordingResult
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class VideoStopResult
{
    public bool Success { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string VideoData { get; set; } = string.Empty;
    public double Duration { get; set; }
    public long Size { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class QualityUpdateListener
{
    private readonly ModernCameraCaptureComponent _component;

    public QualityUpdateListener(ModernCameraCaptureComponent component)
    {
        _component = component;
    }

    [JSInvokable]
    public void HandleQualityUpdate(QualityMetrics metrics)
    {
        _component.UpdateQuality(metrics);
    }
}

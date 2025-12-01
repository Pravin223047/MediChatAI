using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Consultation;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Notifications.Hubs;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace MediChatAI_GraphQl.Shared.Services;

public class ConsultationRecordingService : IConsultationRecordingService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConsultationRecordingService> _logger;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationService _notificationService;
    private readonly Cloudinary _cloudinary;

    public ConsultationRecordingService(
        ApplicationDbContext context,
        ILogger<ConsultationRecordingService> logger,
        ICloudinaryService cloudinaryService,
        IHubContext<NotificationHub> hubContext,
        INotificationService notificationService,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _cloudinaryService = cloudinaryService;
        _hubContext = hubContext;
        _notificationService = notificationService;

        // Initialize Cloudinary for video operations
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    #region Recording Management

    public async Task<ConsultationRecordingDto> StartRecordingAsync(int sessionId, string recordedByUserId)
    {
        _logger.LogInformation("Starting recording for consultation session {SessionId}", sessionId);

        var session = await _context.ConsultationSessions.FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Consultation session with ID {sessionId} not found");
        }

        var recording = new ConsultationRecording
        {
            ConsultationSessionId = sessionId,
            RecordedByUserId = recordedByUserId,
            Type = RecordingType.VideoWithAudio,
            Status = RecordingStatus.Recording,
            RecordingUrl = string.Empty, // Will be set after upload
            IsPatientAccessible = true,
            IsDoctorAccessible = true,
            StartedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.ConsultationRecordings.Add(recording);
        await _context.SaveChangesAsync();

        // Broadcast recording started
        await _hubContext.Clients.Group($"consultation_{sessionId}")
            .SendAsync("RecordingStarted", new { recordingId = recording.Id, sessionId });

        _logger.LogInformation("Recording {RecordingId} started for session {SessionId}", recording.Id, sessionId);

        return MapToDto(recording);
    }

    public async Task<ConsultationRecordingDto> StopRecordingAsync(int recordingId)
    {
        _logger.LogInformation("Stopping recording {RecordingId}", recordingId);

        var recording = await _context.ConsultationRecordings
            .Include(r => r.ConsultationSession)
            .FirstOrDefaultAsync(r => r.Id == recordingId);

        if (recording == null)
        {
            throw new ArgumentException($"Recording with ID {recordingId} not found");
        }

        recording.CompletedAt = DateTime.UtcNow;
        recording.Status = RecordingStatus.Processing;
        recording.UpdatedAt = DateTime.UtcNow;

        // Calculate duration
        if (recording.StartedAt != null)
        {
            recording.DurationSeconds = (int)(recording.CompletedAt.Value - recording.StartedAt).TotalSeconds;
        }

        await _context.SaveChangesAsync();

        // Broadcast recording stopped
        await _hubContext.Clients.Group($"consultation_{recording.ConsultationSessionId}")
            .SendAsync("RecordingStopped", new { recordingId = recording.Id });

        _logger.LogInformation("Recording {RecordingId} stopped", recordingId);

        return MapToDto(recording);
    }

    public async Task<ConsultationRecordingDto?> GetRecordingByIdAsync(int recordingId)
    {
        var recording = await _context.ConsultationRecordings
            .Include(r => r.ConsultationSession)
            .Include(r => r.RecordedBy)
            .FirstOrDefaultAsync(r => r.Id == recordingId);

        return recording == null ? null : MapToDto(recording);
    }

    public async Task<List<ConsultationRecordingDto>> GetRecordingsBySessionAsync(int sessionId)
    {
        var recordings = await _context.ConsultationRecordings
            .Include(r => r.RecordedBy)
            .Where(r => r.ConsultationSessionId == sessionId && r.Status != RecordingStatus.Deleted)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync();

        return recordings.Select(MapToDto).ToList();
    }

    public async Task<List<ConsultationRecordingDto>> GetRecordingsByPatientAsync(string patientId)
    {
        var recordings = await _context.ConsultationRecordings
            .Include(r => r.ConsultationSession)
            .Include(r => r.RecordedBy)
            .Where(r => r.ConsultationSession.PatientId == patientId &&
                       r.IsPatientAccessible &&
                       r.Status != RecordingStatus.Deleted)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync();

        return recordings.Select(MapToDto).ToList();
    }

    #endregion

    #region Upload & Processing

    public async Task<ConsultationRecordingDto> UploadRecordingAsync(int recordingId, Stream fileStream, string fileName)
    {
        _logger.LogInformation("Uploading recording {RecordingId} to Cloudinary", recordingId);

        var recording = await _context.ConsultationRecordings
            .Include(r => r.ConsultationSession)
            .FirstOrDefaultAsync(r => r.Id == recordingId);

        if (recording == null)
        {
            throw new ArgumentException($"Recording with ID {recordingId} not found");
        }

        try
        {
            // Reset stream position
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            // Upload to Cloudinary
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = $"medichatai/consultations/{recording.ConsultationSessionId}",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                recording.RecordingUrl = uploadResult.SecureUrl.ToString();
                recording.CloudinaryPublicId = uploadResult.PublicId;
                recording.CloudinaryAssetId = uploadResult.AssetId;
                recording.FileSizeBytes = uploadResult.Bytes;
                recording.Format = uploadResult.Format;
                recording.Status = RecordingStatus.Completed;
                recording.UpdatedAt = DateTime.UtcNow;

                // Extract video metadata
                if (uploadResult.Duration > 0)
                {
                    recording.DurationSeconds = (int)uploadResult.Duration;
                }

                if (uploadResult.Width > 0 && uploadResult.Height > 0)
                {
                    recording.Resolution = $"{uploadResult.Width}x{uploadResult.Height}";
                }

                await _context.SaveChangesAsync();

                // Notify that recording is ready
                await NotifyRecordingReadyAsync(recording);

                _logger.LogInformation("Recording {RecordingId} uploaded successfully", recordingId);

                return MapToDto(recording);
            }
            else
            {
                recording.Status = RecordingStatus.Failed;
                recording.ErrorMessage = uploadResult.Error?.Message ?? "Upload failed";
                recording.ProcessingAttempts++;
                await _context.SaveChangesAsync();

                _logger.LogError("Recording {RecordingId} upload failed: {Error}", recordingId, uploadResult.Error?.Message);

                throw new Exception($"Failed to upload recording: {uploadResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            recording.Status = RecordingStatus.Failed;
            recording.ErrorMessage = ex.Message;
            recording.ProcessingAttempts++;
            await _context.SaveChangesAsync();

            _logger.LogError(ex, "Exception while uploading recording {RecordingId}", recordingId);
            throw;
        }
    }

    public async Task<bool> GenerateThumbnailAsync(int recordingId)
    {
        _logger.LogInformation("Generating thumbnail for recording {RecordingId}", recordingId);

        var recording = await _context.ConsultationRecordings.FirstOrDefaultAsync(r => r.Id == recordingId);

        if (recording == null || string.IsNullOrEmpty(recording.CloudinaryPublicId))
        {
            return false;
        }

        try
        {
            // Generate thumbnail URL from Cloudinary video (first frame at 0 seconds)
            var thumbnailTransform = new Transformation()
                .StartOffset("0")
                .Width(640)
                .Height(360)
                .Crop("fill")
                .Quality("auto:good")
                .FetchFormat("jpg");

            var thumbnailUrl = _cloudinary.Api.UrlVideoUp
                .Transform(thumbnailTransform)
                .BuildUrl(recording.CloudinaryPublicId);

            recording.ThumbnailUrl = thumbnailUrl;
            recording.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Thumbnail generated for recording {RecordingId}", recordingId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate thumbnail for recording {RecordingId}", recordingId);
            return false;
        }
    }

    public async Task<bool> UpdateRecordingMetadataAsync(
        int recordingId,
        long fileSizeBytes,
        int durationSeconds,
        string? format,
        string? resolution)
    {
        var recording = await _context.ConsultationRecordings.FirstOrDefaultAsync(r => r.Id == recordingId);

        if (recording == null)
        {
            return false;
        }

        recording.FileSizeBytes = fileSizeBytes;
        recording.DurationSeconds = durationSeconds;
        recording.Format = format;
        recording.Resolution = resolution;
        recording.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Transcription

    public async Task<bool> UpdateTranscriptAsync(int recordingId, string transcriptText, string? transcriptUrl = null)
    {
        var recording = await _context.ConsultationRecordings.FirstOrDefaultAsync(r => r.Id == recordingId);

        if (recording == null)
        {
            return false;
        }

        recording.TranscriptText = transcriptText;
        recording.TranscriptUrl = transcriptUrl;
        recording.IsTranscribed = true;
        recording.TranscribedAt = DateTime.UtcNow;
        recording.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Transcript updated for recording {RecordingId}", recordingId);

        return true;
    }

    public async Task<bool> UpdateAISummaryAsync(int recordingId, string aiSummary)
    {
        var recording = await _context.ConsultationRecordings
            .Include(r => r.ConsultationSession)
            .FirstOrDefaultAsync(r => r.Id == recordingId);

        if (recording == null)
        {
            return false;
        }

        recording.AISummary = aiSummary;
        recording.IsSummaryGenerated = true;
        recording.SummaryGeneratedAt = DateTime.UtcNow;
        recording.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Notify patient that summary is ready
        if (recording.ConsultationSession != null)
        {
            await _notificationService.CreateNotificationAsync(
                recording.ConsultationSession.PatientId,
                "Recording Summary Ready",
                "AI summary for your consultation recording is now available.",
                NotificationType.Info,
                NotificationCategory.Appointment,
                NotificationPriority.Normal
            );
        }

        _logger.LogInformation("AI summary updated for recording {RecordingId}", recordingId);

        return true;
    }

    #endregion

    #region Access Control

    public async Task<bool> UpdateAccessPermissionsAsync(int recordingId, bool patientAccess, bool doctorAccess)
    {
        var recording = await _context.ConsultationRecordings.FirstOrDefaultAsync(r => r.Id == recordingId);

        if (recording == null)
        {
            return false;
        }

        recording.IsPatientAccessible = patientAccess;
        recording.IsDoctorAccessible = doctorAccess;
        recording.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Access permissions updated for recording {RecordingId}", recordingId);

        return true;
    }

    public async Task<bool> DeleteRecordingAsync(int recordingId)
    {
        _logger.LogInformation("Deleting recording {RecordingId}", recordingId);

        var recording = await _context.ConsultationRecordings.FirstOrDefaultAsync(r => r.Id == recordingId);

        if (recording == null)
        {
            return false;
        }

        // Soft delete
        recording.Status = RecordingStatus.Deleted;
        recording.DeletedAt = DateTime.UtcNow;
        recording.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Optionally delete from Cloudinary (can be done asynchronously later)
        if (!string.IsNullOrEmpty(recording.CloudinaryPublicId))
        {
            try
            {
                await _cloudinary.DestroyAsync(new DeletionParams(recording.CloudinaryPublicId)
                {
                    ResourceType = ResourceType.Video
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete recording from Cloudinary: {PublicId}", recording.CloudinaryPublicId);
            }
        }

        _logger.LogInformation("Recording {RecordingId} deleted", recordingId);

        return true;
    }

    #endregion

    #region Status

    public async Task<bool> UpdateRecordingStatusAsync(int recordingId, RecordingStatus status, string? errorMessage = null)
    {
        var recording = await _context.ConsultationRecordings.FirstOrDefaultAsync(r => r.Id == recordingId);

        if (recording == null)
        {
            return false;
        }

        recording.Status = status;
        recording.ErrorMessage = errorMessage;
        recording.UpdatedAt = DateTime.UtcNow;

        if (status == RecordingStatus.Failed)
        {
            recording.ProcessingAttempts++;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Helper Methods

    private async Task NotifyRecordingReadyAsync(ConsultationRecording recording)
    {
        var session = recording.ConsultationSession ??
                     await _context.ConsultationSessions.FirstOrDefaultAsync(s => s.Id == recording.ConsultationSessionId);

        if (session == null) return;

        // Notify patient
        await _notificationService.CreateNotificationAsync(
            session.PatientId,
            "Recording Ready",
            "Your consultation recording is ready to view.",
            NotificationType.Info,
            NotificationCategory.Appointment,
            NotificationPriority.Normal,
            $"/patient/consultations",
            "View Recording"
        );

        // Broadcast via SignalR
        await _hubContext.Clients.User(session.PatientId)
            .SendAsync("RecordingReady", new { recordingId = recording.Id, sessionId = session.Id });
    }

    private static ConsultationRecordingDto MapToDto(ConsultationRecording recording)
    {
        return new ConsultationRecordingDto
        {
            Id = recording.Id,
            ConsultationSessionId = recording.ConsultationSessionId,
            RecordingUrl = recording.RecordingUrl,
            ThumbnailUrl = recording.ThumbnailUrl,
            Type = recording.Type,
            Status = recording.Status,
            FileSizeBytes = recording.FileSizeBytes,
            DurationSeconds = recording.DurationSeconds,
            Format = recording.Format,
            Resolution = recording.Resolution,
            TranscriptText = recording.TranscriptText,
            TranscriptUrl = recording.TranscriptUrl,
            IsTranscribed = recording.IsTranscribed,
            TranscribedAt = recording.TranscribedAt,
            AISummary = recording.AISummary,
            IsSummaryGenerated = recording.IsSummaryGenerated,
            RecordedByUserId = recording.RecordedByUserId,
            RecordedByUserName = recording.RecordedBy != null
                ? $"{recording.RecordedBy.FirstName} {recording.RecordedBy.LastName}"
                : "Unknown",
            IsPatientAccessible = recording.IsPatientAccessible,
            IsDoctorAccessible = recording.IsDoctorAccessible,
            StartedAt = recording.StartedAt,
            CompletedAt = recording.CompletedAt,
            CreatedAt = recording.CreatedAt
        };
    }

    #endregion
}

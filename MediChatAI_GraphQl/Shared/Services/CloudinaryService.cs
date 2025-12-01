using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using Microsoft.Extensions.Configuration;

namespace MediChatAI_GraphQl.Shared.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger;

        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new InvalidOperationException(
                "Cloudinary configuration is missing. Please add CloudName, ApiKey, and ApiSecret to appsettings.json under the 'Cloudinary' section.");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true; // Use HTTPS URLs
    }

    public async Task<string?> UploadImageAsync(Stream fileStream, string fileName, string folder)
    {
        try
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                _logger.LogWarning("Empty file stream provided for upload");
                return null;
            }

            // Reset stream position to beginning
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = $"medichatai/{folder}",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
                // Add transformations for images
                Transformation = new Transformation()
                    .Quality("auto:good")
                    .FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Image uploaded successfully to Cloudinary: {Url}", uploadResult.SecureUrl);
                return uploadResult.SecureUrl.ToString();
            }

            _logger.LogError("Cloudinary upload failed: {Error}", uploadResult.Error?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while uploading image to Cloudinary: {FileName}", fileName);
            return null;
        }
    }

    public async Task<string?> UploadDocumentAsync(Stream fileStream, string fileName, string folder)
    {
        try
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                _logger.LogWarning("Empty file stream provided for document upload");
                return null;
            }

            // Reset stream position to beginning
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = $"medichatai/{folder}",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Document uploaded successfully to Cloudinary: {Url}", uploadResult.SecureUrl);
                return uploadResult.SecureUrl.ToString();
            }

            _logger.LogError("Cloudinary document upload failed: {Error}", uploadResult.Error?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while uploading document to Cloudinary: {FileName}", fileName);
            return null;
        }
    }

    public async Task<bool> DeleteFileAsync(string publicId)
    {
        try
        {
            if (string.IsNullOrEmpty(publicId))
            {
                _logger.LogWarning("Empty publicId provided for deletion");
                return false;
            }

            var deletionParams = new DeletionParams(publicId);
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("File deleted successfully from Cloudinary: {PublicId}", publicId);
                return true;
            }

            _logger.LogWarning("Cloudinary deletion failed: {Error}", deletionResult.Error?.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while deleting file from Cloudinary: {PublicId}", publicId);
            return false;
        }
    }

    public async Task<string?> UploadVideoAsync(Stream fileStream, string fileName, string folder)
    {
        try
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                _logger.LogWarning("Empty file stream provided for video upload");
                return null;
            }

            // Reset stream position to beginning
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = $"medichatai/{folder}",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false,
                // Add transformations for videos
                Transformation = new Transformation()
                    .Quality("auto:good")
                    .FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Video uploaded successfully to Cloudinary: {Url}", uploadResult.SecureUrl);
                return uploadResult.SecureUrl.ToString();
            }

            _logger.LogError("Cloudinary video upload failed: {Error}", uploadResult.Error?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while uploading video to Cloudinary: {FileName}", fileName);
            return null;
        }
    }

    public async Task<string?> UploadAudioAsync(Stream fileStream, string fileName, string folder)
    {
        try
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                _logger.LogWarning("Empty file stream provided for audio upload");
                return null;
            }

            // Reset stream position to beginning
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            // Use VideoUploadParams for audio files (Cloudinary treats audio as a subtype of video)
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = $"medichatai/{folder}",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Audio uploaded successfully to Cloudinary: {Url}", uploadResult.SecureUrl);
                return uploadResult.SecureUrl.ToString();
            }

            _logger.LogError("Cloudinary audio upload failed: {Error}", uploadResult.Error?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while uploading audio to Cloudinary: {FileName}", fileName);
            return null;
        }
    }
}

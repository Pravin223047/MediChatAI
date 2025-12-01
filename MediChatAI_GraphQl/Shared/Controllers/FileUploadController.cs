using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;

namespace MediChatAI_GraphQl.Shared.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileUploadController : ControllerBase
{
    private readonly ILogger<FileUploadController> _logger;
    private readonly ICloudinaryService _cloudinaryService;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

    public FileUploadController(ILogger<FileUploadController> logger, ICloudinaryService cloudinaryService)
    {
        _logger = logger;
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("aadhaar-card")]
    public async Task<IActionResult> UploadAadhaarCard(IFormFile file)
    {
        try
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            if (file.Length > MaxFileSize)
                return BadRequest(new { error = "File size exceeds 5MB limit" });

            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest(new { error = "Invalid file type. Only JPG, JPEG, PNG, and PDF files are allowed" });

            // Generate unique filename with user ID
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";

            // Upload to Cloudinary
            string? cloudinaryUrl;
            using (var stream = file.OpenReadStream())
            {
                if (extension == ".pdf")
                {
                    cloudinaryUrl = await _cloudinaryService.UploadDocumentAsync(stream, fileName, "aadhaar-cards");
                }
                else
                {
                    cloudinaryUrl = await _cloudinaryService.UploadImageAsync(stream, fileName, "aadhaar-cards");
                }
            }

            if (string.IsNullOrEmpty(cloudinaryUrl))
            {
                _logger.LogError("Cloudinary upload failed for Aadhaar card");
                return StatusCode(500, new { error = "Failed to upload file to cloud storage" });
            }

            _logger.LogInformation("Aadhaar card uploaded successfully to Cloudinary for user {UserId}: {Url}", userId, cloudinaryUrl);

            return Ok(new
            {
                success = true,
                fileName = fileName,
                filePath = cloudinaryUrl,
                message = "Aadhaar card uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading Aadhaar card");
            return StatusCode(500, new { error = "An error occurred while uploading the file" });
        }
    }

    [HttpPost("profile-image")]
    public async Task<IActionResult> UploadProfileImage(IFormFile file)
    {
        try
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            if (file.Length > MaxFileSize)
                return BadRequest(new { error = "File size exceeds 5MB limit" });

            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(extension))
                return BadRequest(new { error = "Invalid file type. Only JPG, JPEG, and PNG files are allowed" });

            // Generate unique filename with user ID
            var fileName = $"{userId}_profile_{Guid.NewGuid()}{extension}";

            // Upload to Cloudinary
            string? cloudinaryUrl;
            using (var stream = file.OpenReadStream())
            {
                cloudinaryUrl = await _cloudinaryService.UploadImageAsync(stream, fileName, "profile-images");
            }

            if (string.IsNullOrEmpty(cloudinaryUrl))
            {
                _logger.LogError("Cloudinary upload failed for profile image");
                return StatusCode(500, new { error = "Failed to upload file to cloud storage" });
            }

            _logger.LogInformation("Profile image uploaded successfully to Cloudinary for user {UserId}: {Url}", userId, cloudinaryUrl);

            return Ok(new
            {
                success = true,
                fileName = fileName,
                filePath = cloudinaryUrl,
                message = "Profile image uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading profile image");
            return StatusCode(500, new { error = "An error occurred while uploading the file" });
        }
    }

    [HttpPost("medical-certificate")]
    public async Task<IActionResult> UploadMedicalCertificate(IFormFile file)
    {
        try
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            if (file.Length > MaxFileSize)
                return BadRequest(new { error = "File size exceeds 5MB limit" });

            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return BadRequest(new { error = "Invalid file type. Only JPG, JPEG, PNG, and PDF files are allowed" });

            // Generate unique filename with user ID
            var fileName = $"{userId}_cert_{Guid.NewGuid()}{extension}";

            // Upload to Cloudinary
            string? cloudinaryUrl;
            using (var stream = file.OpenReadStream())
            {
                if (extension == ".pdf")
                {
                    cloudinaryUrl = await _cloudinaryService.UploadDocumentAsync(stream, fileName, "medical-certificates");
                }
                else
                {
                    cloudinaryUrl = await _cloudinaryService.UploadImageAsync(stream, fileName, "medical-certificates");
                }
            }

            if (string.IsNullOrEmpty(cloudinaryUrl))
            {
                _logger.LogError("Cloudinary upload failed for medical certificate");
                return StatusCode(500, new { error = "Failed to upload file to cloud storage" });
            }

            _logger.LogInformation("Medical certificate uploaded successfully to Cloudinary for user {UserId}: {Url}", userId, cloudinaryUrl);

            return Ok(new
            {
                success = true,
                fileName = fileName,
                filePath = cloudinaryUrl,
                message = "Medical certificate uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading medical certificate");
            return StatusCode(500, new { error = "An error occurred while uploading the file" });
        }
    }

    [HttpPost("patient-document")]
    public async Task<IActionResult> UploadPatientDocument(IFormFile file, [FromForm] string? folder = "patient-documents")
    {
        try
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            if (file.Length > 10 * 1024 * 1024) // 10MB for documents
                return BadRequest(new { error = "File size exceeds 10MB limit" });

            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { error = "Invalid file type" });

            // Generate unique filename with user ID
            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";

            // Upload to Cloudinary
            string? cloudinaryUrl;
            using (var stream = file.OpenReadStream())
            {
                if (new[] { ".jpg", ".jpeg", ".png" }.Contains(extension))
                {
                    cloudinaryUrl = await _cloudinaryService.UploadImageAsync(stream, fileName, folder ?? "patient-documents");
                }
                else
                {
                    cloudinaryUrl = await _cloudinaryService.UploadDocumentAsync(stream, fileName, folder ?? "patient-documents");
                }
            }

            if (string.IsNullOrEmpty(cloudinaryUrl))
            {
                _logger.LogError("Cloudinary upload failed for patient document");
                return StatusCode(500, new { error = "Failed to upload file to cloud storage" });
            }

            _logger.LogInformation("Patient document uploaded successfully to Cloudinary for user {UserId}: {Url}", userId, cloudinaryUrl);

            return Ok(new
            {
                success = true,
                fileName = file.FileName,
                cloudinaryUrl = cloudinaryUrl,
                fileSize = file.Length,
                message = "Document uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading patient document");
            return StatusCode(500, new { error = "An error occurred while uploading the file" });
        }
    }

    [HttpDelete("aadhaar-card/{publicId}")]
    public async Task<IActionResult> DeleteAadhaarCard(string publicId)
    {
        try
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            // Ensure the publicId belongs to the current user
            if (!publicId.Contains(userId))
                return Forbid("You can only delete your own files");

            var deleted = await _cloudinaryService.DeleteFileAsync(publicId);

            if (!deleted)
            {
                _logger.LogWarning("Failed to delete Aadhaar card from Cloudinary for user {UserId}: {PublicId}", userId, publicId);
                return StatusCode(500, new { error = "Failed to delete file from cloud storage" });
            }

            _logger.LogInformation("Aadhaar card deleted successfully for user {UserId}: {PublicId}", userId, publicId);

            return Ok(new { success = true, message = "File deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Aadhaar card file: {PublicId}", publicId);
            return StatusCode(500, new { error = "An error occurred while deleting the file" });
        }
    }
}
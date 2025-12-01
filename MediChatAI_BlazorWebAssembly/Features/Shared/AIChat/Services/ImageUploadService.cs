using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services;

/// <summary>
/// Service for uploading images to the server
/// </summary>
public class ImageUploadService : IImageUploadService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageUploadService> _logger;
    private const string UploadEndpoint = "api/upload/image";
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public ImageUploadService(HttpClient httpClient, ILogger<ImageUploadService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> UploadImageAsync(IBrowserFile file)
    {
        try
        {
            // Validate file size
            if (file.Size > MaxFileSize)
            {
                _logger.LogWarning("File size {Size} exceeds maximum allowed size {MaxSize}", file.Size, MaxFileSize);
                return null;
            }

            // Validate file type
            if (!file.ContentType.StartsWith("image/"))
            {
                _logger.LogWarning("Invalid file type: {ContentType}", file.ContentType);
                return null;
            }

            // Create multipart form data
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream(MaxFileSize);
            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.Name);

            // Upload to server
            var response = await _httpClient.PostAsync(UploadEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Image uploaded successfully: {Result}", result);
                return result;
            }

            _logger.LogError("Failed to upload image: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return null;
        }
    }

    public async Task<string?> UploadImageFromDataUrlAsync(string dataUrl, string fileName)
    {
        try
        {
            // Extract base64 data from data URL
            var base64Data = dataUrl.Contains(",") ? dataUrl.Split(',')[1] : dataUrl;
            var bytes = Convert.FromBase64String(base64Data);

            // Determine content type from data URL
            var contentType = "image/png";
            if (dataUrl.StartsWith("data:"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(dataUrl, @"data:([^;]+);");
                if (match.Success)
                {
                    contentType = match.Groups[1].Value;
                }
            }

            // Create multipart form data
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(new MemoryStream(bytes));

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(streamContent, "file", fileName);

            // Upload to server
            var response = await _httpClient.PostAsync(UploadEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Image uploaded successfully from data URL: {Result}", result);
                return result;
            }

            _logger.LogError("Failed to upload image from data URL: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image from data URL");
            return null;
        }
    }
}

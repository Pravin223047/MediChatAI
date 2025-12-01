using Microsoft.AspNetCore.Components.Forms;

namespace MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services;

/// <summary>
/// Service for uploading images to the server
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Uploads an image file to the server
    /// </summary>
    /// <param name="file">The image file to upload</param>
    /// <returns>The URL of the uploaded image</returns>
    Task<string?> UploadImageAsync(IBrowserFile file);

    /// <summary>
    /// Uploads an image from a data URL
    /// </summary>
    /// <param name="dataUrl">The image data URL</param>
    /// <param name="fileName">The file name</param>
    /// <returns>The URL of the uploaded image</returns>
    Task<string?> UploadImageFromDataUrlAsync(string dataUrl, string fileName);
}

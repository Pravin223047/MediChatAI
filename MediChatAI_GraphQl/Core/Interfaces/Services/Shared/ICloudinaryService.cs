namespace MediChatAI_GraphQl.Core.Interfaces.Services.Shared;

public interface ICloudinaryService
{
    /// <summary>
    /// Uploads an image file to Cloudinary
    /// </summary>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="folder">Folder name in Cloudinary (e.g., "aadhaar", "medical-certificates", "profile-images")</param>
    /// <returns>Cloudinary URL of the uploaded image, or null if upload fails</returns>
    Task<string?> UploadImageAsync(Stream fileStream, string fileName, string folder);

    /// <summary>
    /// Uploads a document (PDF, etc.) to Cloudinary
    /// </summary>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="folder">Folder name in Cloudinary</param>
    /// <returns>Cloudinary URL of the uploaded document, or null if upload fails</returns>
    Task<string?> UploadDocumentAsync(Stream fileStream, string fileName, string folder);

    /// <summary>
    /// Deletes a file from Cloudinary by its public ID
    /// </summary>
    /// <param name="publicId">The public ID of the file in Cloudinary</param>
    /// <returns>True if deletion was successful, false otherwise</returns>
    Task<bool> DeleteFileAsync(string publicId);

    /// <summary>
    /// Uploads a video file to Cloudinary
    /// </summary>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="folder">Folder name in Cloudinary</param>
    /// <returns>Cloudinary URL of the uploaded video, or null if upload fails</returns>
    Task<string?> UploadVideoAsync(Stream fileStream, string fileName, string folder);

    /// <summary>
    /// Uploads an audio file to Cloudinary
    /// </summary>
    /// <param name="fileStream">The file stream to upload</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="folder">Folder name in Cloudinary</param>
    /// <returns>Cloudinary URL of the uploaded audio, or null if upload fails</returns>
    Task<string?> UploadAudioAsync(Stream fileStream, string fileName, string folder);
}

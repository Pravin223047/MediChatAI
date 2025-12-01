namespace MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

public class UploadAttachmentResponseDto
{
    public bool Success { get; set; }
    public string? Url { get; set; }
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public string? MimeType { get; set; }
    public string? ErrorMessage { get; set; }
}

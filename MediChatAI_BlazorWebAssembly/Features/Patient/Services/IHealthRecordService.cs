using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IHealthRecordService
{
    Task<List<PatientDocumentDto>> GetMyDocumentsAsync();
    Task<PatientDocumentDto> UploadDocumentAsync(string fileName, string fileUrl, DocumentType documentType, string mimeType, long fileSizeBytes, string? description = null);
    Task<bool> DeleteDocumentAsync(int documentId);
}

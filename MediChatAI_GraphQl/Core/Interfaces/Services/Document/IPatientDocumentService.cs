using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Document;

public interface IPatientDocumentService
{
    Task<PatientDocumentDto?> GetDocumentByIdAsync(int id);
    Task<List<PatientDocumentDto>> GetDocumentsByPatientIdAsync(string patientId);
    Task<List<PatientDocumentDto>> GetDocumentsByAppointmentIdAsync(int appointmentId);
    Task<List<PatientDocumentDto>> GetDocumentsByAppointmentRequestIdAsync(int requestId);
    Task<List<PatientDocumentDto>> GetDocumentsByTypeAsync(string patientId, DocumentType type);
    Task<PatientDocumentDto> UploadDocumentAsync(UploadPatientDocumentDto dto);
    Task<bool> DeleteDocumentAsync(int id, string userId);
    Task<bool> VerifyDocumentAsync(int id, string doctorId, string? notes);
}

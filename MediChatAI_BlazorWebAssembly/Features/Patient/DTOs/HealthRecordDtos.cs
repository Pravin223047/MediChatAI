using System.Text.Json.Serialization;
using MediChatAI_BlazorWebAssembly.Core.Converters;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

[JsonConverter(typeof(DocumentTypeConverter))]
public enum DocumentType
{
    MedicalReport,
    LabResult,
    Prescription,
    XRay,
    MRI,
    CTScan,
    Ultrasound,
    InsuranceCard,
    IdentityProof,
    Photo,
    Other
}

public class PatientDocumentDto
{
    public int Id { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string? MimeType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedById { get; set; } = string.Empty;
    public string UploadedByName { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public int? AppointmentRequestId { get; set; }
    public int? AppointmentId { get; set; }
}

// GraphQL Response Models
public class MyDocumentsResponse
{
    public List<PatientDocumentDto> MyDocuments { get; set; } = new();
}

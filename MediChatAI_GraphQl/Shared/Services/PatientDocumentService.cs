using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Document;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Shared.Services;

public class PatientDocumentService : IPatientDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PatientDocumentService> _logger;

    public PatientDocumentService(ApplicationDbContext context, ILogger<PatientDocumentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PatientDocumentDto?> GetDocumentByIdAsync(int id)
    {
        return await _context.PatientDocuments
            .Where(d => d.Id == id)
            .Select(d => new PatientDocumentDto
            {
                Id = d.Id,
                PatientId = d.PatientId,
                PatientName = d.Patient != null ? $"{d.Patient.FirstName} {d.Patient.LastName}" : "",
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                DocumentType = d.DocumentType,
                MimeType = d.MimeType,
                FileSizeBytes = d.FileSizeBytes,
                Description = d.Description,
                UploadedAt = d.UploadedAt,
                UploadedById = d.UploadedById,
                UploadedByName = d.UploadedBy != null ? $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}" : "",
                IsVerified = d.IsVerified,
                AppointmentRequestId = d.AppointmentRequestId,
                AppointmentId = d.AppointmentId
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<PatientDocumentDto>> GetDocumentsByPatientIdAsync(string patientId)
    {
        return await _context.PatientDocuments
            .Where(d => d.PatientId == patientId && d.IsVisible)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new PatientDocumentDto
            {
                Id = d.Id,
                PatientId = d.PatientId,
                PatientName = "",  // Simplified - not loading navigation property
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                DocumentType = d.DocumentType,
                MimeType = d.MimeType,
                FileSizeBytes = d.FileSizeBytes,
                Description = d.Description,
                UploadedAt = d.UploadedAt,
                UploadedById = d.UploadedById,
                UploadedByName = "", // Simplified - not loading navigation property
                IsVerified = d.IsVerified,
                AppointmentRequestId = d.AppointmentRequestId,
                AppointmentId = d.AppointmentId
            })
            .ToListAsync();
    }

    public async Task<List<PatientDocumentDto>> GetDocumentsByAppointmentIdAsync(int appointmentId)
    {
        return await _context.PatientDocuments
            .Where(d => d.AppointmentId == appointmentId && d.IsVisible)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new PatientDocumentDto
            {
                Id = d.Id,
                PatientId = d.PatientId,
                PatientName = d.Patient != null ? $"{d.Patient.FirstName} {d.Patient.LastName}" : "",
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                DocumentType = d.DocumentType,
                MimeType = d.MimeType,
                FileSizeBytes = d.FileSizeBytes,
                Description = d.Description,
                UploadedAt = d.UploadedAt,
                UploadedById = d.UploadedById,
                UploadedByName = d.UploadedBy != null ? $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}" : "",
                IsVerified = d.IsVerified,
                AppointmentRequestId = d.AppointmentRequestId,
                AppointmentId = d.AppointmentId
            })
            .ToListAsync();
    }

    public async Task<List<PatientDocumentDto>> GetDocumentsByAppointmentRequestIdAsync(int requestId)
    {
        return await _context.PatientDocuments
            .Where(d => d.AppointmentRequestId == requestId && d.IsVisible)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new PatientDocumentDto
            {
                Id = d.Id,
                PatientId = d.PatientId,
                PatientName = d.Patient != null ? $"{d.Patient.FirstName} {d.Patient.LastName}" : "",
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                DocumentType = d.DocumentType,
                MimeType = d.MimeType,
                FileSizeBytes = d.FileSizeBytes,
                Description = d.Description,
                UploadedAt = d.UploadedAt,
                UploadedById = d.UploadedById,
                UploadedByName = d.UploadedBy != null ? $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}" : "",
                IsVerified = d.IsVerified,
                AppointmentRequestId = d.AppointmentRequestId,
                AppointmentId = d.AppointmentId
            })
            .ToListAsync();
    }

    public async Task<List<PatientDocumentDto>> GetDocumentsByTypeAsync(string patientId, DocumentType type)
    {
        return await _context.PatientDocuments
            .Where(d => d.PatientId == patientId && d.DocumentType == type && d.IsVisible)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => new PatientDocumentDto
            {
                Id = d.Id,
                PatientId = d.PatientId,
                PatientName = d.Patient != null ? $"{d.Patient.FirstName} {d.Patient.LastName}" : "",
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                DocumentType = d.DocumentType,
                MimeType = d.MimeType,
                FileSizeBytes = d.FileSizeBytes,
                Description = d.Description,
                UploadedAt = d.UploadedAt,
                UploadedById = d.UploadedById,
                UploadedByName = d.UploadedBy != null ? $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}" : "",
                IsVerified = d.IsVerified,
                AppointmentRequestId = d.AppointmentRequestId,
                AppointmentId = d.AppointmentId
            })
            .ToListAsync();
    }

    public async Task<PatientDocumentDto> UploadDocumentAsync(UploadPatientDocumentDto dto)
    {
        var document = new PatientDocument
        {
            PatientId = dto.PatientId,
            FileName = dto.FileName,
            FileUrl = dto.FileUrl,
            PublicId = dto.PublicId,
            DocumentType = dto.DocumentType,
            MimeType = dto.MimeType,
            FileSizeBytes = dto.FileSizeBytes,
            Description = dto.Description,
            Tags = dto.Tags,
            UploadedById = dto.UploadedById,
            AppointmentRequestId = dto.AppointmentRequestId,
            AppointmentId = dto.AppointmentId,
            UploadedAt = DateTime.UtcNow,
            IsVisible = true,
            IsVerified = false
        };

        _context.PatientDocuments.Add(document);
        await _context.SaveChangesAsync();

        return (await GetDocumentByIdAsync(document.Id))!;
    }

    public async Task<bool> DeleteDocumentAsync(int id, string userId)
    {
        var document = await _context.PatientDocuments.FindAsync(id);
        if (document == null)
            return false;

        // Only the patient or the uploader can delete
        if (document.PatientId != userId && document.UploadedById != userId)
            return false;

        document.IsVisible = false; // Soft delete
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> VerifyDocumentAsync(int id, string doctorId, string? notes)
    {
        var document = await _context.PatientDocuments.FindAsync(id);
        if (document == null)
            return false;

        document.IsVerified = true;
        document.VerifiedByDoctorId = doctorId;
        document.VerifiedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(notes))
            document.DoctorNotes = notes;

        await _context.SaveChangesAsync();
        return true;
    }

    private PatientDocumentDto MapToDto(PatientDocument d)
    {
        return new PatientDocumentDto
        {
            Id = d.Id,
            PatientId = d.PatientId,
            PatientName = d.Patient != null ? $"{d.Patient.FirstName} {d.Patient.LastName}" : "",
            FileName = d.FileName,
            FileUrl = d.FileUrl,
            DocumentType = d.DocumentType,
            MimeType = d.MimeType,
            FileSizeBytes = d.FileSizeBytes,
            Description = d.Description,
            UploadedAt = d.UploadedAt,
            UploadedById = d.UploadedById,
            UploadedByName = d.UploadedBy != null ? $"{d.UploadedBy.FirstName} {d.UploadedBy.LastName}" : "",
            IsVerified = d.IsVerified,
            AppointmentRequestId = d.AppointmentRequestId,
            AppointmentId = d.AppointmentId
        };
    }
}

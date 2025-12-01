using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Prescription;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Shared.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(ApplicationDbContext context, ILogger<PrescriptionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PrescriptionDto?> GetPrescriptionByIdAsync(int id)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.Id == id)
            .Select(p => MapToDto(p))
            .FirstOrDefaultAsync();
    }

    public async Task<List<PrescriptionDto>> GetPrescriptionsByPatientIdAsync(string patientId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.PatientId == patientId)
            .OrderByDescending(p => p.PrescribedDate)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<List<PrescriptionDto>> GetActivePrescriptionsByPatientIdAsync(string patientId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.PatientId == patientId && p.Status == PrescriptionStatus.Active)
            .OrderByDescending(p => p.PrescribedDate)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<List<PrescriptionDto>> GetPrescriptionsByDoctorIdAsync(string doctorId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.DoctorId == doctorId)
            .OrderByDescending(p => p.PrescribedDate)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<List<PrescriptionDto>> GetPrescriptionsByAppointmentIdAsync(int appointmentId)
    {
        return await _context.Prescriptions
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .Where(p => p.AppointmentId == appointmentId)
            .OrderByDescending(p => p.PrescribedDate)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }

    public async Task<PrescriptionDto> CreatePrescriptionAsync(CreatePrescriptionDto dto)
    {
        // Validate that DoctorId is provided (should be set from authenticated user)
        if (string.IsNullOrEmpty(dto.DoctorId))
            throw new InvalidOperationException("Doctor ID is required to create a prescription");

        // Calculate end date based on the longest duration among items
        var maxDuration = dto.Items.Any() ? dto.Items.Max(i => i.DurationDays) : 7;
        var endDate = dto.StartDate?.AddDays(maxDuration) ??
                     DateTime.UtcNow.AddDays(maxDuration);

        var prescription = new Core.Entities.Prescription
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            AppointmentId = dto.AppointmentId,
            ConsultationSessionId = dto.ConsultationSessionId,
            PrescribedDate = DateTime.UtcNow,
            StartDate = dto.StartDate ?? DateTime.UtcNow,
            EndDate = endDate,
            RefillsAllowed = dto.RefillsAllowed,
            RefillsUsed = 0,
            Diagnosis = dto.Diagnosis,
            DoctorNotes = dto.DoctorNotes,
            DoctorSignature = dto.DoctorSignature,
            IsVerified = !string.IsNullOrEmpty(dto.DoctorSignature),
            Status = PrescriptionStatus.Active,
            CreatedAt = DateTime.UtcNow,
            Items = dto.Items.Select(item => new PrescriptionItem
            {
                MedicationName = item.MedicationName,
                GenericName = item.GenericName,
                Dosage = item.Dosage,
                Frequency = item.Frequency,
                Route = item.Route,
                DurationDays = item.DurationDays,
                Quantity = item.Quantity,
                Form = item.Form,
                Instructions = item.Instructions,
                Warnings = item.Warnings,
                SideEffects = item.SideEffects,
                CreatedAt = DateTime.UtcNow
            }).ToList()
        };

        _context.Prescriptions.Add(prescription);
        await _context.SaveChangesAsync();

        return (await GetPrescriptionByIdAsync(prescription.Id))!;
    }

    public async Task<bool> CancelPrescriptionAsync(int id, string reason)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null)
            return false;

        prescription.Status = PrescriptionStatus.Cancelled;
        prescription.CancellationReason = reason;
        prescription.CancelledAt = DateTime.UtcNow;
        prescription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RefillPrescriptionAsync(int id)
    {
        var prescription = await _context.Prescriptions.FindAsync(id);
        if (prescription == null)
            return false;

        if (prescription.RefillsUsed >= prescription.RefillsAllowed)
            return false;

        prescription.RefillsUsed++;
        prescription.LastRefillDate = DateTime.UtcNow;
        prescription.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static PrescriptionDto MapToDto(Core.Entities.Prescription p)
    {
        return new PrescriptionDto
        {
            Id = p.Id,
            PatientId = p.PatientId,
            PatientName = p.Patient != null ? $"{p.Patient.FirstName} {p.Patient.LastName}" : "",
            DoctorId = p.DoctorId,
            DoctorName = p.Doctor != null ? $"{p.Doctor.FirstName} {p.Doctor.LastName}" : "",
            AppointmentId = p.AppointmentId,
            ConsultationSessionId = p.ConsultationSessionId,
            Items = p.Items.Select(item => new PrescriptionItemDto
            {
                Id = item.Id,
                PrescriptionId = item.PrescriptionId,
                MedicationName = item.MedicationName,
                GenericName = item.GenericName,
                Dosage = item.Dosage,
                Frequency = item.Frequency,
                Route = item.Route,
                DurationDays = item.DurationDays,
                Quantity = item.Quantity,
                Form = item.Form,
                Instructions = item.Instructions,
                Warnings = item.Warnings,
                SideEffects = item.SideEffects
            }).ToList(),
            PrescribedDate = p.PrescribedDate,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Status = p.Status,
            RefillsAllowed = p.RefillsAllowed,
            RefillsUsed = p.RefillsUsed,
            Diagnosis = p.Diagnosis,
            DoctorNotes = p.DoctorNotes,
            DoctorSignature = p.DoctorSignature,
            IsVerified = p.IsVerified
        };
    }
}

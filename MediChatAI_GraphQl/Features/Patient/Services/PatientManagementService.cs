using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Patient;
using MediChatAI_GraphQl.Features.Patient.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediChatAI_GraphQl.Features.Patient.Services;

public class PatientManagementService : IPatientManagementService
{
    private readonly ApplicationDbContext _context;

    public PatientManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PatientListDto>> GetDoctorPatientsAsync(string doctorId)
    {
        // Get all unique patients who have appointments with this doctor
        var patientIds = await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.PatientId)
            .Distinct()
            .ToListAsync();

        var patients = await _context.Users
            .Where(u => patientIds.Contains(u.Id))
            .ToListAsync();

        var patientDtos = new List<PatientListDto>();

        foreach (var patient in patients)
        {
            // Get appointment statistics
            var appointments = await _context.Appointments
                .Where(a => a.PatientId == patient.Id && a.DoctorId == doctorId)
                .ToListAsync();

            var lastVisit = appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .OrderByDescending(a => a.AppointmentDateTime)
                .FirstOrDefault();

            var nextAppointment = appointments
                .Where(a => a.Status == AppointmentStatus.Confirmed && a.AppointmentDateTime > DateTime.UtcNow)
                .OrderBy(a => a.AppointmentDateTime)
                .FirstOrDefault();

            // Get latest vital to determine status
            var latestVital = await _context.PatientVitals
                .Where(v => v.PatientId == patient.Id)
                .OrderByDescending(v => v.RecordedAt)
                .FirstOrDefaultAsync();

            // Determine patient status
            string status = "Active";
            if (latestVital?.Severity == VitalSeverity.Critical)
                status = "Critical";
            else if (nextAppointment != null)
                status = "Follow-up";
            else if (lastVisit != null && (DateTime.UtcNow - lastVisit.AppointmentDateTime).TotalDays <= 30)
                status = "Stable";

            // Get primary chronic condition from medical history
            string? currentCondition = null;
            if (!string.IsNullOrEmpty(patient.MedicalHistory))
            {
                var conditions = patient.MedicalHistory.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                currentCondition = conditions.FirstOrDefault()?.Trim();
            }

            patientDtos.Add(new PatientListDto
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                Email = patient.Email ?? string.Empty,
                PhoneNumber = patient.PhoneNumber,
                ProfileImage = patient.ProfileImage,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                BloodType = patient.BloodType,
                City = patient.City,
                State = patient.State,
                CurrentCondition = currentCondition,
                TotalVisits = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                LastVisitDate = lastVisit?.AppointmentDateTime,
                NextAppointmentDate = nextAppointment?.AppointmentDateTime,
                Status = status,
                HasCriticalVitals = latestVital?.Severity == VitalSeverity.Critical,
                EmergencyContactName = patient.EmergencyContactName,
                EmergencyContactPhone = patient.EmergencyContactPhone
            });
        }

        return patientDtos.OrderByDescending(p => p.LastVisitDate ?? DateTime.MinValue).ToList();
    }

    public async Task<PatientDetailDto?> GetPatientDetailsAsync(string patientId, string? doctorId = null)
    {
        var patient = await _context.Users.FindAsync(patientId);
        if (patient == null)
            return null;

        // Get appointments
        var appointments = await _context.Appointments
            .Where(a => a.PatientId == patientId && (string.IsNullOrEmpty(doctorId) || a.DoctorId == doctorId))
            .Include(a => a.Doctor)
            .OrderByDescending(a => a.AppointmentDateTime)
            .Take(10)
            .ToListAsync();

        // Get prescriptions
        var prescriptions = await _context.Prescriptions
            .Where(p => p.PatientId == patientId && (string.IsNullOrEmpty(doctorId) || p.DoctorId == doctorId))
            .Include(p => p.Doctor)
            .Include(p => p.Items)
            .OrderByDescending(p => p.PrescribedDate)
            .Take(10)
            .ToListAsync();

        // Get vitals
        var vitals = await _context.PatientVitals
            .Where(v => v.PatientId == patientId)
            .OrderByDescending(v => v.RecordedAt)
            .Take(20)
            .ToListAsync();

        // Get documents
        var documents = await _context.PatientDocuments
            .Where(d => d.PatientId == patientId)
            .OrderByDescending(d => d.UploadedAt)
            .Take(10)
            .ToListAsync();

        // Map to DTOs
        var appointmentSummaries = appointments.Select(a => new AppointmentSummaryDto
        {
            Id = a.Id,
            AppointmentDateTime = a.AppointmentDateTime,
            Status = a.Status.ToString(),
            Type = a.Type.ToString(),
            ReasonForVisit = a.ReasonForVisit,
            DoctorNotes = a.DoctorNotes,
            DoctorName = a.Doctor != null ? $"{a.Doctor.FirstName} {a.Doctor.LastName}" : string.Empty,
            IsVirtual = a.IsVirtual
        }).ToList();

        var prescriptionSummaries = prescriptions.Select(p => new PrescriptionSummaryDto
        {
            Id = p.Id,
            MedicationName = p.Items.Any() ?
                (p.Items.Count == 1 ? p.Items.First().MedicationName : $"{p.Items.Count} medications") :
                "No medications",
            Dosage = p.Items.Any() && p.Items.Count == 1 ? p.Items.First().Dosage : "",
            Frequency = p.Items.Any() && p.Items.Count == 1 ? p.Items.First().Frequency : "",
            PrescribedDate = p.PrescribedDate,
            Status = p.Status.ToString(),
            Diagnosis = p.Diagnosis,
            DurationDays = p.Items.Any() ? p.Items.Max(i => i.DurationDays) : 0
        }).ToList();

        var vitalSummaries = vitals.Select(v => new VitalSummaryDto
        {
            Id = v.Id,
            VitalType = v.VitalType.ToString(),
            Value = v.Value,
            Unit = v.Unit,
            Severity = v.Severity.ToString(),
            RecordedAt = v.RecordedAt,
            Notes = v.Notes,
            SystolicValue = v.SystolicValue,
            DiastolicValue = v.DiastolicValue
        }).ToList();

        var documentSummaries = documents.Select(d => new DocumentSummaryDto
        {
            Id = d.Id,
            DocumentName = d.FileName,
            DocumentType = d.DocumentType.ToString(),
            UploadedAt = d.UploadedAt,
            Description = d.Description,
            FileSize = d.FileSizeBytes
        }).ToList();

        var latestVital = vitals.FirstOrDefault();
        var lastVisit = appointments.FirstOrDefault(a => a.Status == AppointmentStatus.Completed);
        var nextAppointment = appointments.FirstOrDefault(a => a.Status == AppointmentStatus.Confirmed && a.AppointmentDateTime > DateTime.UtcNow);

        return new PatientDetailDto
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Email = patient.Email ?? string.Empty,
            PhoneNumber = patient.PhoneNumber,
            ProfileImage = patient.ProfileImage,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            BloodType = patient.BloodType,
            Address = patient.Address,
            City = patient.City,
            State = patient.State,
            ZipCode = patient.ZipCode,
            Country = patient.Country,
            EmergencyContactName = patient.EmergencyContactName,
            EmergencyContactPhone = patient.EmergencyContactPhone,
            Allergies = patient.Allergies,
            MedicalHistory = patient.MedicalHistory,
            InsuranceProvider = patient.InsuranceProvider,
            InsurancePolicyNumber = patient.InsurancePolicyNumber,
            InsuranceGroupNumber = patient.InsuranceGroupNumber,
            InsuranceExpiryDate = patient.InsuranceExpiryDate,
            CreatedAt = patient.CreatedAt,
            LastLoginAt = patient.LastLoginAt,
            LastProfileUpdate = patient.LastProfileUpdate,
            RecentAppointments = appointmentSummaries,
            RecentPrescriptions = prescriptionSummaries,
            RecentVitals = vitalSummaries,
            RecentDocuments = documentSummaries,
            TotalAppointments = await _context.Appointments.CountAsync(a => a.PatientId == patientId),
            TotalPrescriptions = await _context.Prescriptions.CountAsync(p => p.PatientId == patientId),
            TotalDocuments = await _context.PatientDocuments.CountAsync(d => d.PatientId == patientId),
            LastVisitDate = lastVisit?.AppointmentDateTime,
            NextAppointmentDate = nextAppointment?.AppointmentDateTime,
            HasCriticalVitals = latestVital?.Severity == VitalSeverity.Critical
        };
    }

    public async Task<List<PatientListDto>> SearchPatientsAsync(string doctorId, string searchTerm)
    {
        var allPatients = await GetDoctorPatientsAsync(doctorId);

        if (string.IsNullOrWhiteSpace(searchTerm))
            return allPatients;

        searchTerm = searchTerm.ToLower();

        return allPatients
            .Where(p =>
                p.FirstName.ToLower().Contains(searchTerm) ||
                p.LastName.ToLower().Contains(searchTerm) ||
                p.Email.ToLower().Contains(searchTerm) ||
                (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm)))
            .ToList();
    }
}

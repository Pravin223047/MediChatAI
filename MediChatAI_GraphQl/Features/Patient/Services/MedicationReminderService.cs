using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Patient;
using MediChatAI_GraphQl.Features.Patient.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Features.Patient.Services;

public class MedicationReminderService : IMedicationReminderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<MedicationReminderService> _logger;

    public MedicationReminderService(ApplicationDbContext context, ILogger<MedicationReminderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MedicationReminder>> GetPatientRemindersAsync(string patientId)
    {
        return await _context.MedicationReminders
            .Where(mr => mr.PatientId == patientId && mr.IsActive)
            .Include(mr => mr.AdherenceLogs.OrderByDescending(a => a.ScheduledTime).Take(7))
            .OrderBy(mr => mr.NextReminderAt)
            .ToListAsync();
    }

    public Task<MedicationReminder> CreateReminderAsync(string patientId, CreateMedicationReminderInput input)
    {
        // TODO: Implement when CreateMedicationReminderInput DTO is properly defined
        throw new NotImplementedException("Medication reminder creation not yet implemented");
    }

    public async Task<bool> UpdateAdherenceAsync(Guid reminderId, bool taken, string? notes = null)
    {
        var reminder = await _context.MedicationReminders.FindAsync(reminderId);
        if (reminder == null) return false;

        var adherence = new MedicationAdherence
        {
            ReminderId = reminderId,
            ScheduledTime = DateTime.UtcNow,
            ActualTime = taken ? DateTime.UtcNow : null,
            Status = taken ? AdherenceStatus.Taken : AdherenceStatus.Missed,
            Notes = notes
        };

        _context.MedicationAdherences.Add(adherence);

        if (taken)
            reminder.TotalDosesTaken++;
        else
            reminder.TotalDosesMissed++;

        reminder.AdherenceRate = (decimal)reminder.TotalDosesTaken / (reminder.TotalDosesTaken + reminder.TotalDosesMissed) * 100;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReminderAsync(Guid reminderId, string patientId)
    {
        var reminder = await _context.MedicationReminders
            .FirstOrDefaultAsync(mr => mr.Id == reminderId && mr.PatientId == patientId);

        if (reminder == null) return false;

        reminder.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }
}

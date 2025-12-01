using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Features.Patient.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Patient;

public interface IMedicationReminderService
{
    Task<List<MedicationReminder>> GetPatientRemindersAsync(string patientId);
    Task<MedicationReminder> CreateReminderAsync(string patientId, CreateMedicationReminderInput input);
    Task<bool> UpdateAdherenceAsync(Guid reminderId, bool taken, string? notes = null);
    Task<bool> DeleteReminderAsync(Guid reminderId, string patientId);
}

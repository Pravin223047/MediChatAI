using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Features.Doctor.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;

public interface IEmergencyAlertService
{
    Task<EmergencyAlert> CreateAlertAsync(CreateEmergencyAlertInput input);
    Task<IEnumerable<EmergencyAlert>> GetAlertsForDoctorAsync(string doctorId, AlertStatus? status = null);
    Task<bool> AcknowledgeAlertAsync(Guid alertId, string userId);
    Task<bool> ResolveAlertAsync(Guid alertId, string userId, string? resolutionNotes = null);
    Task<bool> DismissAlertAsync(Guid alertId, string userId);
    Task<int> GetActiveAlertCountAsync(string doctorId);
}

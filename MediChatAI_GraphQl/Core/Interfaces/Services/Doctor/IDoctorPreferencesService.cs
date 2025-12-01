using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Features.Doctor.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;

public interface IDoctorPreferencesService
{
    Task<DoctorPreference> GetPreferencesAsync(string doctorId);
    Task<DoctorPreference> UpdatePreferencesAsync(string doctorId, UpdateDoctorPreferencesInput input);
    Task<DoctorPreference> GetOrCreateDefaultPreferencesAsync(string doctorId);
}

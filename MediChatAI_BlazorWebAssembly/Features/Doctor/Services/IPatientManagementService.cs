using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public interface IPatientManagementService
{
    Task<PatientListResponse> GetPatientsAsync(PatientSearchFilter filter);
    Task<PatientDetailModel?> GetPatientByIdAsync(string patientId);
    Task<bool> AddPatientAsync(AddPatientModel model);
    Task<bool> UpdatePatientAsync(PatientDetailModel model);
    Task<bool> DeletePatientAsync(string patientId);
    Task<List<string>> GetAvailableTagsAsync();
    Task<bool> AddPatientTagAsync(string patientId, string tag);
    Task<bool> RemovePatientTagAsync(string patientId, string tag);
    Task<VitalSignsModel?> AddVitalSignsAsync(string patientId, VitalSignsModel vitalSigns);
    Task<List<VitalSignsModel>> GetVitalSignsHistoryAsync(string patientId, int limit = 10);
}

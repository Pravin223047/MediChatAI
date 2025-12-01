using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Features.Doctor.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;

public interface IPatientVitalsService
{
    Task<PatientVital> RecordVitalsAsync(RecordVitalsInput input);
    Task<IEnumerable<PatientVital>> GetPatientVitalsAsync(string patientId, DateTime? startDate = null, DateTime? endDate = null);
    Task<PatientVitalsData> GetLatestVitalsAsync(string patientId);
    Task<IEnumerable<PatientVital>> GetCriticalVitalsForDoctorAsync(string doctorId);
    Task CheckAndCreateAlertsAsync(Guid vitalId);
}

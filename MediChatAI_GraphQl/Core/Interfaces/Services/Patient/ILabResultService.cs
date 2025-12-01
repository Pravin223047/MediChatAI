using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Features.Patient.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Patient;

public interface ILabResultService
{
    Task<List<LabResult>> GetPatientLabResultsAsync(string patientId, DateTime? startDate = null, DateTime? endDate = null);
    Task<LabResult> UploadLabResultAsync(string patientId, UploadLabResultInput input);
    Task<bool> DeleteLabResultAsync(Guid labResultId, string patientId);
    Task<LabResult?> GetLabResultByIdAsync(Guid labResultId);
}

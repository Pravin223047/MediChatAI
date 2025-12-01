using MediChatAI_GraphQl.Features.Patient.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Patient;

public interface IPatientManagementService
{
    /// <summary>
    /// Get all patients assigned to a specific doctor
    /// </summary>
    Task<List<PatientListDto>> GetDoctorPatientsAsync(string doctorId);

    /// <summary>
    /// Get detailed information about a specific patient
    /// </summary>
    Task<PatientDetailDto?> GetPatientDetailsAsync(string patientId, string? doctorId = null);

    /// <summary>
    /// Search patients by name, email, or phone
    /// </summary>
    Task<List<PatientListDto>> SearchPatientsAsync(string doctorId, string searchTerm);
}

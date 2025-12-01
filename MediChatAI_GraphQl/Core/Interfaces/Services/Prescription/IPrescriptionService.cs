using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Shared.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Prescription;

public interface IPrescriptionService
{
    Task<PrescriptionDto?> GetPrescriptionByIdAsync(int id);
    Task<List<PrescriptionDto>> GetPrescriptionsByPatientIdAsync(string patientId);
    Task<List<PrescriptionDto>> GetActivePrescriptionsByPatientIdAsync(string patientId);
    Task<List<PrescriptionDto>> GetPrescriptionsByDoctorIdAsync(string doctorId);
    Task<List<PrescriptionDto>> GetPrescriptionsByAppointmentIdAsync(int appointmentId);
    Task<PrescriptionDto> CreatePrescriptionAsync(CreatePrescriptionDto dto);
    Task<bool> CancelPrescriptionAsync(int id, string reason);
    Task<bool> RefillPrescriptionAsync(int id);
}

using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public interface IPrescriptionService
{
    // Prescription CRUD
    Task<PrescriptionModel?> CreatePrescriptionAsync(CreatePrescriptionModel model);
    Task<PrescriptionModel?> GetPrescriptionByIdAsync(string prescriptionId);
    Task<List<PrescriptionModel>> GetPrescriptionsAsync(PrescriptionSearchFilter filter);
    Task<bool> UpdatePrescriptionAsync(PrescriptionModel prescription);
    Task<bool> CancelPrescriptionAsync(string prescriptionId, string reason);
    Task<bool> DeletePrescriptionAsync(string prescriptionId);

    // Drug Database
    Task<DrugSearchResult> SearchDrugsAsync(string searchTerm, int limit = 20);
    Task<DrugModel?> GetDrugByIdAsync(string drugId);
    Task<List<DrugModel>> GetDrugsByCategoryAsync(string category);
    Task<List<string>> GetDrugCategoriesAsync();

    // Drug Interactions
    Task<DrugInteractionCheckResult> CheckDrugInteractionsAsync(List<string> drugIds);
    Task<AllergyCheckResult> CheckAllergiesAsync(string patientId, List<string> drugIds);

    // Dosage Calculations
    Task<DosageRecommendation> CalculateRecommendedDosageAsync(DosageCalculationModel model);

    // Templates
    Task<List<PrescriptionTemplateModel>> GetTemplatesAsync();
    Task<PrescriptionTemplateModel?> GetTemplateByIdAsync(string templateId);
    Task<bool> SaveTemplateAsync(PrescriptionTemplateModel template);
    Task<bool> DeleteTemplateAsync(string templateId);

    // Statistics
    Task<PrescriptionStatisticsModel> GetStatisticsAsync();

    // Patient Prescriptions
    Task<List<PrescriptionModel>> GetPatientPrescriptionsAsync(string patientId);
    Task<List<PrescriptionModel>> GetActivePrescriptionsForPatientAsync(string patientId);

    // Digital Signature
    Task<string> GenerateDigitalSignatureAsync(string prescriptionId);

    // Consultation Integration
    Task<PrescriptionModel?> CreatePrescriptionDuringConsultationAsync(int consultationSessionId, CreatePrescriptionModel model);
    Task<bool> LinkPrescriptionToConsultationAsync(string prescriptionId, int consultationSessionId);
    Task<List<PrescriptionModel>> GetPendingPrescriptionsAsync(string doctorId);
    Task<bool> SignPrescriptionAsync(string prescriptionId, string signatureBase64);
    Task<List<PrescriptionModel>> GetConsultationPrescriptionsAsync(int consultationSessionId);
}

using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public class PrescriptionService : IPrescriptionService
{
    private static List<DrugModel> _drugDatabase = InitializeDrugDatabase();
    private static List<PrescriptionTemplateModel> _templates = InitializeTemplates();

    private readonly IPatientManagementService _patientService;
    private readonly IGraphQLService _graphQLService;

    public PrescriptionService(IPatientManagementService patientService, IGraphQLService graphQLService)
    {
        _patientService = patientService;
        _graphQLService = graphQLService;
    }

    public async Task<PrescriptionModel?> CreatePrescriptionAsync(CreatePrescriptionModel model)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(model.PatientId);
            if (patient == null) return null;

            // Map frontend model to GraphQL input
            var items = model.Items.Select(item => new
            {
                medicationName = item.DrugName,
                genericName = item.GenericName,
                dosage = item.Dosage,
                frequency = item.Frequency,
                route = item.Route,
                durationDays = item.DurationDays,
                quantity = item.Quantity,
                form = item.Form,
                instructions = item.Instructions,
                warnings = item.Warnings,
                sideEffects = item.SideEffects
            }).ToList();

            var mutation = @"
                mutation CreatePrescription($input: CreatePrescriptionDtoInput!) {
                    createPrescription(input: $input) {
                        id
                        patientId
                        patientName
                        doctorId
                        doctorName
                        prescribedDate
                        startDate
                        endDate
                        status
                        refillsAllowed
                        refillsUsed
                        diagnosis
                        doctorNotes
                        doctorSignature
                        isVerified
                        items {
                            id
                            medicationName
                            genericName
                            dosage
                            frequency
                            route
                            durationDays
                            quantity
                            form
                            instructions
                            warnings
                            sideEffects
                        }
                    }
                }";

            var variables = new
            {
                input = new
                {
                    patientId = model.PatientId,
                    items = items,
                    diagnosis = model.Diagnosis,
                    doctorNotes = model.Notes,
                    refillsAllowed = model.RefillsAllowed,
                    startDate = DateTime.UtcNow
                }
            };

            var response = await _graphQLService.SendQueryAsync<CreatePrescriptionResponse>(mutation, variables);

            if (response?.CreatePrescription != null)
            {
                return MapToPrescriptionModel(response.CreatePrescription);
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating prescription: {ex.Message}");
            return null;
        }
    }

    public async Task<PrescriptionModel?> GetPrescriptionByIdAsync(string prescriptionId)
    {
        try
        {
            var query = @"
                query GetPrescription($id: Int!) {
                    prescription(id: $id) {
                        id
                        patientId
                        patientName
                        doctorId
                        doctorName
                        prescribedDate
                        startDate
                        endDate
                        status
                        refillsAllowed
                        refillsUsed
                        diagnosis
                        doctorNotes
                        doctorSignature
                        isVerified
                        items {
                            id
                            medicationName
                            genericName
                            dosage
                            frequency
                            route
                            durationDays
                            quantity
                            form
                            instructions
                            warnings
                            sideEffects
                        }
                    }
                }";

            var variables = new { id = int.Parse(prescriptionId) };

            var response = await _graphQLService.SendQueryAsync<GetPrescriptionResponse>(query, variables);

            if (response?.Prescription != null)
            {
                return MapToPrescriptionModel(response.Prescription);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<PrescriptionModel>> GetPrescriptionsAsync(PrescriptionSearchFilter filter)
    {
        try
        {
            var query = @"
                query GetMyPrescribedMedications {
                    myPrescribedMedications {
                        id
                        patientId
                        patientName
                        doctorId
                        doctorName
                        prescribedDate
                        startDate
                        endDate
                        status
                        refillsAllowed
                        refillsUsed
                        diagnosis
                        doctorNotes
                        items {
                            id
                            medicationName
                            genericName
                            dosage
                            frequency
                            route
                            durationDays
                            quantity
                            form
                            instructions
                            warnings
                        }
                    }
                }";

            var response = await _graphQLService.SendQueryAsync<GetMyPrescriptionsResponse>(query);

            if (response?.MyPrescribedMedications != null)
            {
                var prescriptions = response.MyPrescribedMedications
                    .Select(MapToPrescriptionModel)
                    .ToList();

                // Apply filters
                var filteredQuery = prescriptions.AsQueryable();

                if (!string.IsNullOrEmpty(filter.PatientId))
                    filteredQuery = filteredQuery.Where(p => p.PatientId == filter.PatientId);

                if (filter.StartDate.HasValue)
                    filteredQuery = filteredQuery.Where(p => p.PrescriptionDate >= filter.StartDate.Value);

                if (filter.EndDate.HasValue)
                    filteredQuery = filteredQuery.Where(p => p.PrescriptionDate <= filter.EndDate.Value);

                if (!string.IsNullOrEmpty(filter.Status))
                    filteredQuery = filteredQuery.Where(p => p.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.DrugName))
                    filteredQuery = filteredQuery.Where(p => p.Items.Any(i => i.DrugName.Contains(filter.DrugName, StringComparison.OrdinalIgnoreCase)));

                if (filter.ActiveOnly)
                    filteredQuery = filteredQuery.Where(p => p.IsActive && p.Status == "Active");

                return filteredQuery.OrderByDescending(p => p.PrescriptionDate).ToList();
            }

            return new List<PrescriptionModel>();
        }
        catch
        {
            return new List<PrescriptionModel>();
        }
    }

    public Task<bool> UpdatePrescriptionAsync(PrescriptionModel prescription)
    {
        // TODO: Implement update mutation if needed
        return Task.FromResult(false);
    }

    public async Task<bool> CancelPrescriptionAsync(string prescriptionId, string reason)
    {
        try
        {
            var mutation = @"
                mutation CancelPrescription($prescriptionId: Int!, $reason: String!) {
                    cancelPrescription(prescriptionId: $prescriptionId, reason: $reason)
                }";

            var variables = new
            {
                prescriptionId = int.Parse(prescriptionId),
                reason = reason
            };

            var response = await _graphQLService.SendQueryAsync<CancelPrescriptionResponse>(mutation, variables);

            return response?.CancelPrescription ?? false;
        }
        catch
        {
            return false;
        }
    }

    public Task<bool> DeletePrescriptionAsync(string prescriptionId)
    {
        // Prescriptions shouldn't be deleted, only cancelled
        return CancelPrescriptionAsync(prescriptionId, "Deleted by doctor");
    }

    public async Task<List<PrescriptionModel>> GetPatientPrescriptionsAsync(string patientId)
    {
        try
        {
            var query = @"
                query GetPatientPrescriptions($patientId: String!) {
                    patientPrescriptions(patientId: $patientId) {
                        id
                        patientId
                        patientName
                        doctorId
                        doctorName
                        prescribedDate
                        startDate
                        endDate
                        status
                        refillsAllowed
                        refillsUsed
                        diagnosis
                        doctorNotes
                        items {
                            id
                            medicationName
                            genericName
                            dosage
                            frequency
                            route
                            durationDays
                            quantity
                            form
                            instructions
                            warnings
                        }
                    }
                }";

            var variables = new { patientId = patientId };

            var response = await _graphQLService.SendQueryAsync<GetPatientPrescriptionsResponse>(query, variables);

            if (response?.PatientPrescriptions != null)
            {
                return response.PatientPrescriptions
                    .Select(MapToPrescriptionModel)
                    .OrderByDescending(p => p.PrescriptionDate)
                    .ToList();
            }

            return new List<PrescriptionModel>();
        }
        catch
        {
            return new List<PrescriptionModel>();
        }
    }

    public async Task<List<PrescriptionModel>> GetActivePrescriptionsForPatientAsync(string patientId)
    {
        var allPrescriptions = await GetPatientPrescriptionsAsync(patientId);
        return allPrescriptions
            .Where(p => p.IsActive && p.Status == "Active")
            .ToList();
    }

    public async Task<PrescriptionModel?> CreatePrescriptionDuringConsultationAsync(int consultationSessionId, CreatePrescriptionModel model)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(model.PatientId);
            if (patient == null) return null;

            var items = model.Items.Select(item => new
            {
                medicationName = item.DrugName,
                genericName = item.GenericName,
                dosage = item.Dosage,
                frequency = item.Frequency,
                route = item.Route,
                durationDays = item.DurationDays,
                quantity = item.Quantity,
                form = item.Form,
                instructions = item.Instructions,
                warnings = item.Warnings,
                sideEffects = item.SideEffects
            }).ToList();

            var mutation = @"
                mutation CreatePrescription($input: CreatePrescriptionDtoInput!) {
                    createPrescription(input: $input) {
                        id
                        patientId
                        patientName
                        doctorId
                        doctorName
                        prescribedDate
                        startDate
                        endDate
                        status
                        refillsAllowed
                        refillsUsed
                        diagnosis
                        doctorNotes
                        consultationSessionId
                        isVerified
                        items {
                            id
                            medicationName
                            genericName
                            dosage
                            frequency
                            route
                            durationDays
                            quantity
                            form
                            instructions
                            warnings
                            sideEffects
                        }
                    }
                }";

            var variables = new
            {
                input = new
                {
                    patientId = model.PatientId,
                    consultationSessionId = consultationSessionId,
                    items = items,
                    diagnosis = model.Diagnosis,
                    doctorNotes = model.Notes,
                    refillsAllowed = model.RefillsAllowed,
                    startDate = DateTime.UtcNow
                }
            };

            var response = await _graphQLService.SendQueryAsync<CreatePrescriptionResponse>(mutation, variables);

            if (response?.CreatePrescription != null)
            {
                var prescription = MapToPrescriptionModel(response.CreatePrescription);
                prescription.ConsultationSessionId = consultationSessionId;
                return prescription;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    // Local features (drug database, templates) - kept as-is
    public Task<DrugSearchResult> SearchDrugsAsync(string searchTerm, int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Task.FromResult(new DrugSearchResult
            {
                Drugs = _drugDatabase.Take(limit).ToList(),
                TotalCount = _drugDatabase.Count
            });
        }

        var searchLower = searchTerm.ToLower();
        var results = _drugDatabase
            .Where(d =>
                d.Name.ToLower().Contains(searchLower) ||
                d.GenericName.ToLower().Contains(searchLower) ||
                d.Category.ToLower().Contains(searchLower))
            .Take(limit)
            .ToList();

        return Task.FromResult(new DrugSearchResult
        {
            Drugs = results,
            TotalCount = results.Count
        });
    }

    public Task<DrugModel?> GetDrugByIdAsync(string drugId)
    {
        var drug = _drugDatabase.FirstOrDefault(d => d.DrugId == drugId);
        return Task.FromResult(drug);
    }

    public Task<List<DrugModel>> GetDrugsByCategoryAsync(string category)
    {
        var drugs = _drugDatabase.Where(d => d.Category == category).ToList();
        return Task.FromResult(drugs);
    }

    public Task<List<string>> GetDrugCategoriesAsync()
    {
        var categories = _drugDatabase.Select(d => d.Category).Distinct().OrderBy(c => c).ToList();
        return Task.FromResult(categories);
    }

    public Task<DrugInteractionCheckResult> CheckDrugInteractionsAsync(List<string> drugIds)
    {
        var interactions = new List<DrugInteractionModel>();

        for (int i = 0; i < drugIds.Count; i++)
        {
            for (int j = i + 1; j < drugIds.Count; j++)
            {
                var drug1 = _drugDatabase.FirstOrDefault(d => d.DrugId == drugIds[i]);
                var drug2 = _drugDatabase.FirstOrDefault(d => d.DrugId == drugIds[j]);

                if (drug1 != null && drug2 != null &&
                    (drug1.InteractsWith.Contains(drugIds[j]) || drug2.InteractsWith.Contains(drugIds[i])))
                {
                    interactions.Add(new DrugInteractionModel
                    {
                        Drug1Id = drug1.DrugId,
                        Drug1Name = drug1.Name,
                        Drug2Id = drug2.DrugId,
                        Drug2Name = drug2.Name,
                        InteractionLevel = DetermineInteractionLevel(drug1, drug2),
                        Description = $"{drug1.Name} may interact with {drug2.Name}",
                        Recommendation = "Monitor patient closely. Consider alternative medications."
                    });
                }
            }
        }

        var result = new DrugInteractionCheckResult
        {
            HasInteractions = interactions.Any(),
            Interactions = interactions,
            SevereCount = interactions.Count(i => i.InteractionLevel == "Severe"),
            ModerateCount = interactions.Count(i => i.InteractionLevel == "Moderate"),
            MildCount = interactions.Count(i => i.InteractionLevel == "Mild")
        };

        return Task.FromResult(result);
    }

    public async Task<AllergyCheckResult> CheckAllergiesAsync(string patientId, List<string> drugIds)
    {
        var patient = await _patientService.GetPatientByIdAsync(patientId);
        if (patient == null || !patient.Allergies.Any())
        {
            return new AllergyCheckResult { HasAllergyConflict = false };
        }

        var conflictingDrugs = new List<string>();
        foreach (var drugId in drugIds)
        {
            var drug = await GetDrugByIdAsync(drugId);
            if (drug != null && patient.Allergies.Any(a =>
                drug.Name.Contains(a, StringComparison.OrdinalIgnoreCase) ||
                drug.GenericName.Contains(a, StringComparison.OrdinalIgnoreCase)))
            {
                conflictingDrugs.Add(drug.Name);
            }
        }

        return new AllergyCheckResult
        {
            HasAllergyConflict = conflictingDrugs.Any(),
            ConflictingDrugs = conflictingDrugs,
            Warning = conflictingDrugs.Any()
                ? $"Patient is allergic to: {string.Join(", ", conflictingDrugs)}"
                : null
        };
    }

    public Task<DosageRecommendation> CalculateRecommendedDosageAsync(DosageCalculationModel model)
    {
        var drug = _drugDatabase.FirstOrDefault(d => d.DrugId == model.DrugId);
        if (drug == null)
        {
            return Task.FromResult(new DosageRecommendation
            {
                RecommendedDosage = "Unknown",
                Frequency = "Unknown"
            });
        }

        var recommendation = new DosageRecommendation
        {
            RecommendedDosage = drug.AvailableDosages.FirstOrDefault() ?? "500mg",
            Frequency = "Twice daily",
            MaxDailyDose = "2000mg",
            Warnings = new List<string>()
        };

        // Age-based adjustments
        if (model.PatientAge.HasValue)
        {
            if (model.PatientAge < 12)
            {
                recommendation.Warnings.Add("Pediatric dosing required. Reduce dose by 50%.");
                recommendation.AdjustmentReason = "Pediatric patient";
            }
            else if (model.PatientAge > 65)
            {
                recommendation.Warnings.Add("Geriatric dosing. Consider reduced dose.");
                recommendation.AdjustmentReason = "Elderly patient";
            }
        }

        // Weight-based adjustments
        if (model.PatientWeight.HasValue)
        {
            if (model.PatientWeight < 50)
            {
                recommendation.Warnings.Add("Low body weight. Consider dose reduction.");
            }
        }

        // Renal function adjustments
        if (!string.IsNullOrEmpty(model.RenalFunction) && model.RenalFunction != "Normal")
        {
            recommendation.Warnings.Add($"Renal impairment ({model.RenalFunction}). Dose adjustment may be required.");
            recommendation.AdjustmentReason = "Renal impairment";
        }

        return Task.FromResult(recommendation);
    }

    public Task<List<PrescriptionTemplateModel>> GetTemplatesAsync()
    {
        return Task.FromResult(_templates.OrderByDescending(t => t.UsageCount).ToList());
    }

    public Task<PrescriptionTemplateModel?> GetTemplateByIdAsync(string templateId)
    {
        var template = _templates.FirstOrDefault(t => t.TemplateId == templateId);
        return Task.FromResult(template);
    }

    public Task<bool> SaveTemplateAsync(PrescriptionTemplateModel template)
    {
        template.TemplateId = $"TPL{_templates.Count + 1}";
        template.CreatedAt = DateTime.Now;
        _templates.Add(template);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteTemplateAsync(string templateId)
    {
        var template = _templates.FirstOrDefault(t => t.TemplateId == templateId);
        if (template == null || !template.IsCustom) return Task.FromResult(false);

        _templates.Remove(template);
        return Task.FromResult(true);
    }

    public async Task<PrescriptionStatisticsModel> GetStatisticsAsync()
    {
        var prescriptions = await GetPrescriptionsAsync(new PrescriptionSearchFilter());

        var stats = new PrescriptionStatisticsModel
        {
            TotalPrescriptions = prescriptions.Count,
            ActivePrescriptions = prescriptions.Count(p => p.IsActive && p.Status == "Active"),
            ExpiredPrescriptions = prescriptions.Count(p => p.ExpiryDate < DateTime.Now || p.Status == "Expired"),
            PrescriptionsByCategory = prescriptions
                .SelectMany(p => p.Items)
                .GroupBy(i => _drugDatabase.FirstOrDefault(d => d.DrugId == i.DrugId)?.Category ?? "Other")
                .ToDictionary(g => g.Key, g => g.Count()),
            MostPrescribedDrugs = prescriptions
                .SelectMany(p => p.Items)
                .GroupBy(i => i.DrugName)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageItemsPerPrescription = prescriptions.Any()
                ? (int)prescriptions.Average(p => p.Items.Count)
                : 0,
            PrescriptionsThisMonth = prescriptions.Count(p =>
                p.PrescriptionDate.Year == DateTime.Now.Year &&
                p.PrescriptionDate.Month == DateTime.Now.Month),
            PrescriptionsThisWeek = prescriptions.Count(p =>
                p.PrescriptionDate >= DateTime.Now.AddDays(-7))
        };

        return stats;
    }

    public Task<string> GenerateDigitalSignatureAsync(string prescriptionId)
    {
        // Digital signature is handled by backend now
        var signature = $"DS-{prescriptionId}-{DateTime.Now.Ticks}";
        return Task.FromResult(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(signature)));
    }

    public Task<bool> LinkPrescriptionToConsultationAsync(string prescriptionId, int consultationSessionId)
    {
        // This is now handled during prescription creation
        return Task.FromResult(true);
    }

    public Task<List<PrescriptionModel>> GetPendingPrescriptionsAsync(string doctorId)
    {
        // TODO: Implement if backend supports pending status query
        return Task.FromResult(new List<PrescriptionModel>());
    }

    public Task<bool> SignPrescriptionAsync(string prescriptionId, string signatureBase64)
    {
        // TODO: Implement signature mutation if needed
        return Task.FromResult(true);
    }

    public Task<List<PrescriptionModel>> GetConsultationPrescriptionsAsync(int consultationSessionId)
    {
        // TODO: Implement query for consultation prescriptions
        return Task.FromResult(new List<PrescriptionModel>());
    }

    // Helper methods
    private PrescriptionModel MapToPrescriptionModel(PrescriptionDto dto)
    {
        return new PrescriptionModel
        {
            PrescriptionId = dto.Id.ToString(),
            PatientId = dto.PatientId,
            PatientName = dto.PatientName,
            DoctorId = dto.DoctorId,
            DoctorName = dto.DoctorName,
            PrescriptionDate = dto.PrescribedDate,
            ExpiryDate = dto.EndDate ?? DateTime.Now.AddMonths(6),
            Items = dto.Items.Select(item => new PrescriptionItemModel
            {
                DrugId = item.Id.ToString(),
                DrugName = item.MedicationName,
                GenericName = item.GenericName,
                Dosage = item.Dosage,
                Frequency = item.Frequency,
                Route = item.Route,
                DurationDays = item.DurationDays,
                Quantity = item.Quantity,
                Form = item.Form,
                Instructions = item.Instructions,
                Warnings = item.Warnings,
                SideEffects = item.SideEffects
            }).ToList(),
            Diagnosis = dto.Diagnosis,
            Notes = dto.DoctorNotes,
            SpecialInstructions = "",
            IsActive = dto.Status.ToString() == "Active",
            Status = dto.Status.ToString(),
            RefillsAllowed = dto.RefillsAllowed,
            RefillsUsed = dto.RefillsUsed,
            DigitalSignature = dto.DoctorSignature,
            SignedAt = dto.IsVerified ? dto.PrescribedDate : null,
            ConsultationSessionId = dto.ConsultationSessionId
        };
    }

    private string DetermineInteractionLevel(DrugModel drug1, DrugModel drug2)
    {
        if (drug1.Category == drug2.Category)
            return "Moderate";
        if (drug1.IsControlledSubstance || drug2.IsControlledSubstance)
            return "Severe";
        return "Mild";
    }

    // GraphQL Response DTOs
    private class CreatePrescriptionResponse
    {
        public PrescriptionDto? CreatePrescription { get; set; }
    }

    private class GetPrescriptionResponse
    {
        public PrescriptionDto? Prescription { get; set; }
    }

    private class GetMyPrescriptionsResponse
    {
        public List<PrescriptionDto>? MyPrescribedMedications { get; set; }
    }

    private class GetPatientPrescriptionsResponse
    {
        public List<PrescriptionDto>? PatientPrescriptions { get; set; }
    }

    private class CancelPrescriptionResponse
    {
        public bool CancelPrescription { get; set; }
    }

    private class PrescriptionDto
    {
        public int Id { get; set; }
        public string PatientId { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string DoctorId { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public DateTime PrescribedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PrescriptionStatus Status { get; set; }
        public int RefillsAllowed { get; set; }
        public int RefillsUsed { get; set; }
        public string? Diagnosis { get; set; }
        public string? DoctorNotes { get; set; }
        public string? DoctorSignature { get; set; }
        public bool IsVerified { get; set; }
        public int? ConsultationSessionId { get; set; }
        public List<PrescriptionItemDto> Items { get; set; } = new();
    }

    private class PrescriptionItemDto
    {
        public int Id { get; set; }
        public string MedicationName { get; set; } = "";
        public string? GenericName { get; set; }
        public string Dosage { get; set; } = "";
        public string Frequency { get; set; } = "";
        public string? Route { get; set; }
        public int DurationDays { get; set; }
        public int Quantity { get; set; }
        public string? Form { get; set; }
        public string? Instructions { get; set; }
        public string? Warnings { get; set; }
        public string? SideEffects { get; set; }
    }

    private enum PrescriptionStatus
    {
        Active,
        Completed,
        Cancelled,
        Expired
    }

    // Keep the drug database initialization (too long to include here - keeping from original file)
    private static List<DrugModel> InitializeDrugDatabase()
    {
        // [DRUG DATABASE CODE - SAME AS ORIGINAL FILE - Lines 484-782]
        var drugs = new List<DrugModel>();
        int drugId = 1;

        // Analgesics & Pain Relief
        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Paracetamol",
            GenericName = "Acetaminophen",
            Category = "Analgesic",
            AvailableDosages = new List<string> { "500mg", "650mg", "1000mg" },
            DefaultDosageUnit = "mg",
            CommonSideEffects = new List<string> { "Nausea", "Allergic reactions" },
            Contraindications = new List<string> { "Severe liver disease" },
            InteractsWith = new List<string> { "DRG7" },
            RequiresPrescription = false,
            Manufacturer = "Generic",
            AverageCost = 5.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Ibuprofen",
            GenericName = "Ibuprofen",
            Category = "NSAID",
            AvailableDosages = new List<string> { "200mg", "400mg", "600mg" },
            CommonSideEffects = new List<string> { "Stomach upset", "Nausea", "Heartburn" },
            Contraindications = new List<string> { "Active peptic ulcer", "Severe heart failure" },
            InteractsWith = new List<string> { "DRG7", "DRG15" },
            RequiresPrescription = false,
            AverageCost = 8.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Tramadol",
            GenericName = "Tramadol HCl",
            Category = "Opioid Analgesic",
            AvailableDosages = new List<string> { "50mg", "100mg" },
            CommonSideEffects = new List<string> { "Dizziness", "Nausea", "Constipation" },
            Contraindications = new List<string> { "Acute intoxication", "Respiratory depression" },
            InteractsWith = new List<string> { "DRG20" },
            IsControlledSubstance = true,
            ControlledSubstanceSchedule = "Schedule IV",
            RequiresPrescription = true,
            AverageCost = 25.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Amoxicillin",
            GenericName = "Amoxicillin",
            Category = "Antibiotic",
            AvailableDosages = new List<string> { "250mg", "500mg", "875mg" },
            CommonSideEffects = new List<string> { "Diarrhea", "Nausea", "Rash" },
            Contraindications = new List<string> { "Penicillin allergy" },
            InteractsWith = new List<string> { "DRG7" },
            RequiresPrescription = true,
            AverageCost = 15.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Azithromycin",
            GenericName = "Azithromycin",
            Category = "Antibiotic",
            AvailableDosages = new List<string> { "250mg", "500mg" },
            CommonSideEffects = new List<string> { "Diarrhea", "Nausea", "Abdominal pain" },
            Contraindications = new List<string> { "Liver disease" },
            InteractsWith = new List<string> { "DRG7" },
            RequiresPrescription = true,
            AverageCost = 20.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Ciprofloxacin",
            GenericName = "Ciprofloxacin",
            Category = "Antibiotic",
            AvailableDosages = new List<string> { "250mg", "500mg", "750mg" },
            CommonSideEffects = new List<string> { "Nausea", "Diarrhea", "Dizziness" },
            Contraindications = new List<string> { "Tendon disorders" },
            InteractsWith = new List<string> { "DRG7", "DRG1" },
            RequiresPrescription = true,
            AverageCost = 30.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Warfarin",
            GenericName = "Warfarin Sodium",
            Category = "Anticoagulant",
            AvailableDosages = new List<string> { "1mg", "2mg", "5mg" },
            CommonSideEffects = new List<string> { "Bleeding", "Bruising" },
            Contraindications = new List<string> { "Active bleeding", "Pregnancy" },
            InteractsWith = new List<string> { "DRG1", "DRG2", "DRG4", "DRG15" },
            RequiresPrescription = true,
            AverageCost = 10.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Atorvastatin",
            GenericName = "Atorvastatin Calcium",
            Category = "Statin",
            AvailableDosages = new List<string> { "10mg", "20mg", "40mg", "80mg" },
            CommonSideEffects = new List<string> { "Muscle pain", "Headache", "Nausea" },
            Contraindications = new List<string> { "Active liver disease", "Pregnancy" },
            InteractsWith = new List<string> { "DRG6" },
            RequiresPrescription = true,
            AverageCost = 15.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Amlodipine",
            GenericName = "Amlodipine Besylate",
            Category = "Antihypertensive",
            AvailableDosages = new List<string> { "2.5mg", "5mg", "10mg" },
            CommonSideEffects = new List<string> { "Swelling", "Fatigue", "Dizziness" },
            Contraindications = new List<string> { "Severe hypotension" },
            InteractsWith = new List<string>(),
            RequiresPrescription = true,
            AverageCost = 12.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Lisinopril",
            GenericName = "Lisinopril",
            Category = "ACE Inhibitor",
            AvailableDosages = new List<string> { "5mg", "10mg", "20mg", "40mg" },
            CommonSideEffects = new List<string> { "Dry cough", "Dizziness", "Headache" },
            Contraindications = new List<string> { "Pregnancy", "Angioedema" },
            InteractsWith = new List<string> { "DRG2" },
            RequiresPrescription = true,
            AverageCost = 10.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Metformin",
            GenericName = "Metformin HCl",
            Category = "Antidiabetic",
            AvailableDosages = new List<string> { "500mg", "850mg", "1000mg" },
            CommonSideEffects = new List<string> { "Nausea", "Diarrhea", "Stomach upset" },
            Contraindications = new List<string> { "Severe renal impairment", "Metabolic acidosis" },
            InteractsWith = new List<string>(),
            RequiresPrescription = true,
            AverageCost = 8.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Glimepiride",
            GenericName = "Glimepiride",
            Category = "Antidiabetic",
            AvailableDosages = new List<string> { "1mg", "2mg", "4mg" },
            CommonSideEffects = new List<string> { "Hypoglycemia", "Nausea", "Dizziness" },
            Contraindications = new List<string> { "Type 1 diabetes", "Severe renal impairment" },
            InteractsWith = new List<string> { "DRG2" },
            RequiresPrescription = true,
            AverageCost = 12.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Salbutamol",
            GenericName = "Salbutamol Sulfate",
            Category = "Bronchodilator",
            AvailableDosages = new List<string> { "2mg", "4mg", "100mcg inhaler" },
            CommonSideEffects = new List<string> { "Tremor", "Headache", "Tachycardia" },
            Contraindications = new List<string> { "Severe cardiovascular disease" },
            InteractsWith = new List<string>(),
            RequiresPrescription = true,
            AverageCost = 20.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Montelukast",
            GenericName = "Montelukast Sodium",
            Category = "Antiasthmatic",
            AvailableDosages = new List<string> { "4mg", "5mg", "10mg" },
            CommonSideEffects = new List<string> { "Headache", "Abdominal pain" },
            Contraindications = new List<string> { "Hypersensitivity" },
            InteractsWith = new List<string>(),
            RequiresPrescription = true,
            AverageCost = 25.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Aspirin",
            GenericName = "Acetylsalicylic Acid",
            Category = "Antiplatelet",
            AvailableDosages = new List<string> { "75mg", "150mg", "300mg" },
            CommonSideEffects = new List<string> { "Stomach upset", "Bleeding" },
            Contraindications = new List<string> { "Active bleeding", "Children with viral infections" },
            InteractsWith = new List<string> { "DRG2", "DRG7" },
            RequiresPrescription = false,
            AverageCost = 3.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Omeprazole",
            GenericName = "Omeprazole",
            Category = "Proton Pump Inhibitor",
            AvailableDosages = new List<string> { "10mg", "20mg", "40mg" },
            CommonSideEffects = new List<string> { "Headache", "Diarrhea", "Nausea" },
            Contraindications = new List<string> { "Hypersensitivity" },
            InteractsWith = new List<string> { "DRG7" },
            RequiresPrescription = true,
            AverageCost = 10.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Levothyroxine",
            GenericName = "Levothyroxine Sodium",
            Category = "Thyroid Hormone",
            AvailableDosages = new List<string> { "25mcg", "50mcg", "75mcg", "100mcg" },
            CommonSideEffects = new List<string> { "Weight loss", "Tremor", "Headache" },
            Contraindications = new List<string> { "Untreated adrenal insufficiency" },
            InteractsWith = new List<string>(),
            RequiresPrescription = true,
            AverageCost = 15.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Cetirizine",
            GenericName = "Cetirizine HCl",
            Category = "Antihistamine",
            AvailableDosages = new List<string> { "5mg", "10mg" },
            CommonSideEffects = new List<string> { "Drowsiness", "Dry mouth" },
            Contraindications = new List<string> { "Severe renal impairment" },
            InteractsWith = new List<string>(),
            RequiresPrescription = false,
            AverageCost = 5.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Diazepam",
            GenericName = "Diazepam",
            Category = "Benzodiazepine",
            AvailableDosages = new List<string> { "2mg", "5mg", "10mg" },
            CommonSideEffects = new List<string> { "Drowsiness", "Fatigue", "Confusion" },
            Contraindications = new List<string> { "Severe respiratory insufficiency", "Sleep apnea" },
            InteractsWith = new List<string> { "DRG3" },
            IsControlledSubstance = true,
            ControlledSubstanceSchedule = "Schedule IV",
            RequiresPrescription = true,
            AverageCost = 15.00
        });

        drugs.Add(new DrugModel
        {
            DrugId = $"DRG{drugId++}",
            Name = "Fluoxetine",
            GenericName = "Fluoxetine HCl",
            Category = "SSRI Antidepressant",
            AvailableDosages = new List<string> { "10mg", "20mg", "40mg" },
            CommonSideEffects = new List<string> { "Nausea", "Insomnia", "Anxiety" },
            Contraindications = new List<string> { "Use of MAO inhibitors" },
            InteractsWith = new List<string> { "DRG3", "DRG7" },
            RequiresPrescription = true,
            AverageCost = 20.00
        });

        return drugs;
    }

    private static List<PrescriptionTemplateModel> InitializeTemplates()
    {
        // [TEMPLATE CODE - SAME AS ORIGINAL FILE - Lines 784-960]
        var templates = new List<PrescriptionTemplateModel>();

        templates.Add(new PrescriptionTemplateModel
        {
            TemplateId = "TPL001",
            Name = "Common Cold",
            Diagnosis = "Upper Respiratory Tract Infection",
            Items = new List<PrescriptionItemModel>
            {
                new PrescriptionItemModel
                {
                    DrugId = "DRG1",
                    DrugName = "Paracetamol",
                    Dosage = "500mg",
                    Frequency = "Three times daily",
                    Route = "Oral",
                    DurationDays = 5,
                    Quantity = 15,
                    Instructions = "Take with water",
                    TimingInstructions = "After meals"
                },
                new PrescriptionItemModel
                {
                    DrugId = "DRG18",
                    DrugName = "Cetirizine",
                    Dosage = "10mg",
                    Frequency = "Once daily",
                    Route = "Oral",
                    DurationDays = 5,
                    Quantity = 5,
                    Instructions = "Take at bedtime",
                    TimingInstructions = "Before sleep"
                }
            },
            Notes = "Rest, increase fluid intake",
            UsageCount = 45,
            IsCustom = false,
            CreatedAt = DateTime.Now.AddMonths(-6)
        });

        templates.Add(new PrescriptionTemplateModel
        {
            TemplateId = "TPL002",
            Name = "Hypertension Management",
            Diagnosis = "Essential Hypertension",
            Items = new List<PrescriptionItemModel>
            {
                new PrescriptionItemModel
                {
                    DrugId = "DRG9",
                    DrugName = "Amlodipine",
                    Dosage = "5mg",
                    Frequency = "Once daily",
                    Route = "Oral",
                    DurationDays = 30,
                    Quantity = 30,
                    Instructions = "Take regularly",
                    TimingInstructions = "Morning"
                },
                new PrescriptionItemModel
                {
                    DrugId = "DRG8",
                    DrugName = "Atorvastatin",
                    Dosage = "10mg",
                    Frequency = "Once daily",
                    Route = "Oral",
                    DurationDays = 30,
                    Quantity = 30,
                    Instructions = "For cholesterol management",
                    TimingInstructions = "Evening"
                }
            },
            Notes = "Monitor blood pressure regularly. Lifestyle modifications recommended.",
            UsageCount = 38,
            IsCustom = false,
            CreatedAt = DateTime.Now.AddMonths(-5)
        });

        templates.Add(new PrescriptionTemplateModel
        {
            TemplateId = "TPL003",
            Name = "Type 2 Diabetes",
            Diagnosis = "Type 2 Diabetes Mellitus",
            Items = new List<PrescriptionItemModel>
            {
                new PrescriptionItemModel
                {
                    DrugId = "DRG11",
                    DrugName = "Metformin",
                    Dosage = "500mg",
                    Frequency = "Twice daily",
                    Route = "Oral",
                    DurationDays = 30,
                    Quantity = 60,
                    Instructions = "Take with meals",
                    TimingInstructions = "With breakfast and dinner"
                }
            },
            Notes = "Monitor blood sugar levels. Diet and exercise important.",
            UsageCount = 32,
            IsCustom = false,
            CreatedAt = DateTime.Now.AddMonths(-4)
        });

        templates.Add(new PrescriptionTemplateModel
        {
            TemplateId = "TPL004",
            Name = "Bacterial Infection",
            Diagnosis = "Acute Bacterial Infection",
            Items = new List<PrescriptionItemModel>
            {
                new PrescriptionItemModel
                {
                    DrugId = "DRG4",
                    DrugName = "Amoxicillin",
                    Dosage = "500mg",
                    Frequency = "Three times daily",
                    Route = "Oral",
                    DurationDays = 7,
                    Quantity = 21,
                    Instructions = "Complete the full course",
                    TimingInstructions = "Every 8 hours"
                }
            },
            Notes = "Complete antibiotic course even if symptoms improve",
            UsageCount = 28,
            IsCustom = false,
            CreatedAt = DateTime.Now.AddMonths(-3)
        });

        templates.Add(new PrescriptionTemplateModel
        {
            TemplateId = "TPL005",
            Name = "Asthma Management",
            Diagnosis = "Bronchial Asthma",
            Items = new List<PrescriptionItemModel>
            {
                new PrescriptionItemModel
                {
                    DrugId = "DRG13",
                    DrugName = "Salbutamol",
                    Dosage = "100mcg",
                    Frequency = "As needed",
                    Route = "Inhalation",
                    DurationDays = 30,
                    Quantity = 1,
                    Instructions = "Use inhaler as directed",
                    TimingInstructions = "When experiencing symptoms"
                },
                new PrescriptionItemModel
                {
                    DrugId = "DRG14",
                    DrugName = "Montelukast",
                    Dosage = "10mg",
                    Frequency = "Once daily",
                    Route = "Oral",
                    DurationDays = 30,
                    Quantity = 30,
                    Instructions = "Take regularly",
                    TimingInstructions = "Evening"
                }
            },
            Notes = "Avoid triggers. Use peak flow meter regularly.",
            UsageCount = 22,
            IsCustom = false,
            CreatedAt = DateTime.Now.AddMonths(-2)
        });

        return templates;
    }
}

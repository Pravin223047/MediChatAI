using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public class PatientManagementService : IPatientManagementService
{
    private readonly IGraphQLService _graphQLService;

    public PatientManagementService(IGraphQLService graphQLService)
    {
        _graphQLService = graphQLService;
    }

    public async Task<PatientListResponse> GetPatientsAsync(PatientSearchFilter filter)
    {
        var query = @"
            query {
                doctorPatients {
                    id
                    firstName
                    lastName
                    email
                    phoneNumber
                    profileImage
                    dateOfBirth
                    age
                    gender
                    bloodType
                    city
                    state
                    currentCondition
                    totalVisits
                    lastVisitDate
                    nextAppointmentDate
                    status
                    hasCriticalVitals
                    emergencyContactName
                    emergencyContactPhone
                }
            }";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("doctorPatients"))
            {
                var json = JsonSerializer.Serialize(response["doctorPatients"]);
                var backendPatients = JsonSerializer.Deserialize<List<PatientListDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PatientListDto>();

                // Convert backend DTOs to frontend models
                var patients = backendPatients.Select(p => new PatientListItemModel
                {
                    PatientId = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber ?? string.Empty,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender ?? string.Empty,
                    BloodGroup = p.BloodType,
                    LastVisitDate = p.LastVisitDate ?? DateTime.Now.AddMonths(-1),
                    NextAppointmentDate = p.NextAppointmentDate,
                    Status = p.Status,
                    CurrentCondition = p.CurrentCondition,
                    TotalVisits = p.TotalVisits
                }).ToList();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    patients = patients.Where(p =>
                        p.FirstName.ToLower().Contains(searchLower) ||
                        p.LastName.ToLower().Contains(searchLower) ||
                        p.Email.ToLower().Contains(searchLower) ||
                        p.PhoneNumber.Contains(searchLower)).ToList();
                }

                if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "All")
                {
                    patients = patients.Where(p => p.Status == filter.Status).ToList();
                }

                if (!string.IsNullOrWhiteSpace(filter.Gender) && filter.Gender != "All")
                {
                    patients = patients.Where(p => p.Gender == filter.Gender).ToList();
                }

                if (filter.AgeFrom.HasValue)
                {
                    patients = patients.Where(p => p.Age >= filter.AgeFrom.Value).ToList();
                }

                if (filter.AgeTo.HasValue)
                {
                    patients = patients.Where(p => p.Age <= filter.AgeTo.Value).ToList();
                }

                if (filter.LastVisitFrom.HasValue)
                {
                    patients = patients.Where(p => p.LastVisitDate >= filter.LastVisitFrom.Value).ToList();
                }

                if (filter.LastVisitTo.HasValue)
                {
                    patients = patients.Where(p => p.LastVisitDate <= filter.LastVisitTo.Value).ToList();
                }

                // Apply sorting
                patients = filter.SortBy switch
                {
                    "Name" => filter.SortDescending
                        ? patients.OrderByDescending(p => p.LastName).ToList()
                        : patients.OrderBy(p => p.LastName).ToList(),
                    "Age" => filter.SortDescending
                        ? patients.OrderByDescending(p => p.Age).ToList()
                        : patients.OrderBy(p => p.Age).ToList(),
                    _ => filter.SortDescending
                        ? patients.OrderByDescending(p => p.LastVisitDate).ToList()
                        : patients.OrderBy(p => p.LastVisitDate).ToList()
                };

                var totalCount = patients.Count;

                // Apply pagination
                var paginatedPatients = patients
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                return new PatientListResponse
                {
                    Patients = paginatedPatients,
                    TotalCount = totalCount,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching patients: {ex.Message}");
        }

        return new PatientListResponse
        {
            Patients = new List<PatientListItemModel>(),
            TotalCount = 0,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<PatientDetailModel?> GetPatientByIdAsync(string patientId)
    {
        var query = @"
            query($patientId: String!) {
                patientDetails(patientId: $patientId) {
                    id
                    firstName
                    lastName
                    email
                    phoneNumber
                    profileImage
                    dateOfBirth
                    age
                    gender
                    bloodType
                    address
                    city
                    state
                    zipCode
                    country
                    emergencyContactName
                    emergencyContactPhone
                    allergies
                    medicalHistory
                    insuranceProvider
                    insurancePolicyNumber
                    insuranceGroupNumber
                    insuranceExpiryDate
                    createdAt
                    lastLoginAt
                    lastProfileUpdate
                    totalAppointments
                    totalPrescriptions
                    totalDocuments
                    lastVisitDate
                    nextAppointmentDate
                    hasCriticalVitals
                    recentAppointments {
                        id
                        appointmentDateTime
                        status
                        type
                        reasonForVisit
                        doctorNotes
                        doctorName
                        isVirtual
                    }
                    recentPrescriptions {
                        id
                        medicationName
                        dosage
                        frequency
                        prescribedDate
                        status
                        diagnosis
                        durationDays
                    }
                    recentVitals {
                        id
                        vitalType
                        value
                        unit
                        severity
                        recordedAt
                        notes
                        systolicValue
                        diastolicValue
                    }
                    recentDocuments {
                        id
                        documentName
                        documentType
                        uploadedAt
                        description
                        fileSize
                    }
                }
            }";

        var variables = new { patientId };

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query, variables);
            if (response != null && response.ContainsKey("patientDetails"))
            {
                var json = JsonSerializer.Serialize(response["patientDetails"]);
                var backendPatient = JsonSerializer.Deserialize<PatientDetailDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (backendPatient != null)
                {
                    // Convert backend DTO to frontend model
                    var patientDetail = new PatientDetailModel
                    {
                        PatientId = backendPatient.Id,
                        FirstName = backendPatient.FirstName,
                        LastName = backendPatient.LastName,
                        Email = backendPatient.Email,
                        PhoneNumber = backendPatient.PhoneNumber ?? string.Empty,
                        DateOfBirth = backendPatient.DateOfBirth,
                        Gender = backendPatient.Gender ?? string.Empty,
                        Address = backendPatient.Address ?? string.Empty,
                        City = backendPatient.City ?? string.Empty,
                        State = backendPatient.State ?? string.Empty,
                        ZipCode = backendPatient.ZipCode ?? string.Empty,
                        BloodGroup = backendPatient.BloodType,
                        EmergencyContactName = backendPatient.EmergencyContactName,
                        EmergencyContactPhone = backendPatient.EmergencyContactPhone,
                        RegistrationDate = backendPatient.CreatedAt,
                        LastVisitDate = backendPatient.LastVisitDate ?? DateTime.Now.AddMonths(-1),
                        NextAppointmentDate = backendPatient.NextAppointmentDate,
                        TotalVisits = backendPatient.TotalAppointments,
                        Status = backendPatient.HasCriticalVitals ? "Critical" : "Active"
                    };

                    // Parse allergies from comma-separated string
                    if (!string.IsNullOrEmpty(backendPatient.Allergies))
                    {
                        patientDetail.Allergies = backendPatient.Allergies.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(a => a.Trim())
                            .ToList();
                    }

                    // Parse chronic conditions from medical history
                    if (!string.IsNullOrEmpty(backendPatient.MedicalHistory))
                    {
                        patientDetail.ChronicConditions = backendPatient.MedicalHistory.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .ToList();
                    }

                    // Convert vitals to VitalSignsModel
                    if (backendPatient.RecentVitals != null && backendPatient.RecentVitals.Any())
                    {
                        var vitalsByDate = backendPatient.RecentVitals
                            .GroupBy(v => v.RecordedAt.Date)
                            .Select(g => new VitalSignsModel
                            {
                                RecordedDate = g.Key,
                                BloodPressureSystolic = g.FirstOrDefault(v => v.VitalType == "BloodPressure")?.SystolicValue,
                                BloodPressureDiastolic = g.FirstOrDefault(v => v.VitalType == "BloodPressure")?.DiastolicValue,
                                HeartRate = double.TryParse(g.FirstOrDefault(v => v.VitalType == "HeartRate")?.Value, out var hr) ? hr : null,
                                Temperature = double.TryParse(g.FirstOrDefault(v => v.VitalType == "Temperature")?.Value, out var temp) ? temp : null,
                                OxygenSaturation = double.TryParse(g.FirstOrDefault(v => v.VitalType == "OxygenSaturation")?.Value, out var o2) ? o2 : null,
                                RespiratoryRate = double.TryParse(g.FirstOrDefault(v => v.VitalType == "RespiratoryRate")?.Value, out var rr) ? rr : null,
                                BloodSugar = double.TryParse(g.FirstOrDefault(v => v.VitalType == "BloodGlucose")?.Value, out var bs) ? bs : null,
                                Notes = string.Join("; ", g.Where(v => !string.IsNullOrEmpty(v.Notes)).Select(v => v.Notes))
                            })
                            .OrderByDescending(v => v.RecordedDate)
                            .ToList();

                        patientDetail.VitalSignsHistory = vitalsByDate;
                    }

                    // Convert appointments to medical history
                    if (backendPatient.RecentAppointments != null && backendPatient.RecentAppointments.Any())
                    {
                        patientDetail.MedicalHistory = backendPatient.RecentAppointments
                            .Where(a => !string.IsNullOrEmpty(a.DoctorNotes) || !string.IsNullOrEmpty(a.ReasonForVisit))
                            .Select(a => new MedicalHistoryItemModel
                            {
                                VisitDate = a.AppointmentDateTime,
                                Diagnosis = a.DoctorNotes ?? string.Empty,
                                Symptoms = a.ReasonForVisit ?? string.Empty,
                                Treatment = string.Empty,
                                DoctorName = a.DoctorName,
                                Notes = $"Status: {a.Status}, Type: {a.Type}"
                            })
                            .OrderByDescending(m => m.VisitDate)
                            .ToList();
                    }

                    // Get current medications from recent prescriptions
                    if (backendPatient.RecentPrescriptions != null && backendPatient.RecentPrescriptions.Any())
                    {
                        patientDetail.CurrentMedications = backendPatient.RecentPrescriptions
                            .Where(p => p.Status == "Active")
                            .Select(p => $"{p.MedicationName} - {p.Dosage} ({p.Frequency})")
                            .ToList();
                    }

                    return patientDetail;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching patient details: {ex.Message}");
        }

        return null;
    }

    public Task<bool> AddPatientAsync(AddPatientModel model)
    {
        // TODO: Implement add patient via GraphQL mutation
        throw new NotImplementedException("Adding patients is not yet implemented. Patients are added through the registration system.");
    }

    public Task<bool> UpdatePatientAsync(PatientDetailModel model)
    {
        // TODO: Implement update patient via GraphQL mutation
        throw new NotImplementedException("Updating patient details is not yet implemented.");
    }

    public Task<bool> DeletePatientAsync(string patientId)
    {
        // TODO: Implement delete patient via GraphQL mutation
        throw new NotImplementedException("Deleting patients is not yet implemented.");
    }

    public Task<List<string>> GetAvailableTagsAsync()
    {
        var tags = new List<string>
        {
            "VIP",
            "Regular Follow-up",
            "High Risk",
            "Diabetic",
            "Cardiac",
            "Hypertensive",
            "Geriatric",
            "Pediatric"
        };
        return Task.FromResult(tags);
    }

    public Task<bool> AddPatientTagAsync(string patientId, string tag)
    {
        // TODO: Implement tag functionality
        return Task.FromResult(true);
    }

    public Task<bool> RemovePatientTagAsync(string patientId, string tag)
    {
        // TODO: Implement tag functionality
        return Task.FromResult(true);
    }

    public Task<VitalSignsModel?> AddVitalSignsAsync(string patientId, VitalSignsModel vitalSigns)
    {
        // TODO: Implement add vital signs via GraphQL mutation
        throw new NotImplementedException("Adding vital signs is not yet implemented.");
    }

    public async Task<List<VitalSignsModel>> GetVitalSignsHistoryAsync(string patientId, int limit = 10)
    {
        // Get patient details which include recent vitals
        var patient = await GetPatientByIdAsync(patientId);
        if (patient == null)
            return new List<VitalSignsModel>();

        return patient.VitalSignsHistory.Take(limit).ToList();
    }
}

// DTO classes matching the backend GraphQL response
public class PatientListDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? BloodType { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? CurrentCondition { get; set; }
    public int TotalVisits { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public DateTime? NextAppointmentDate { get; set; }
    public string Status { get; set; } = "Active";
    public bool HasCriticalVitals { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
}

public class PatientDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? BloodType { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceGroupNumber { get; set; }
    public DateTime? InsuranceExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public DateTime? LastProfileUpdate { get; set; }
    public List<AppointmentSummaryDto> RecentAppointments { get; set; } = new();
    public List<PrescriptionSummaryDto> RecentPrescriptions { get; set; } = new();
    public List<VitalSummaryDto> RecentVitals { get; set; } = new();
    public List<DocumentSummaryDto> RecentDocuments { get; set; } = new();
    public int TotalAppointments { get; set; }
    public int TotalPrescriptions { get; set; }
    public int TotalDocuments { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public DateTime? NextAppointmentDate { get; set; }
    public bool HasCriticalVitals { get; set; }
}

public class AppointmentSummaryDto
{
    public int Id { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? ReasonForVisit { get; set; }
    public string? DoctorNotes { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public bool IsVirtual { get; set; }
}

public class PrescriptionSummaryDto
{
    public int Id { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime PrescribedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Diagnosis { get; set; }
    public int DurationDays { get; set; }
}

public class VitalSummaryDto
{
    public Guid Id { get; set; }
    public string VitalType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public string? Notes { get; set; }
    public int? SystolicValue { get; set; }
    public int? DiastolicValue { get; set; }
}

public class DocumentSummaryDto
{
    public int Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? Description { get; set; }
    public long FileSize { get; set; }
}

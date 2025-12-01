using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public class DoctorOnboardingService : IDoctorOnboardingService
{
    private readonly IGraphQLService _graphQLService;
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public DoctorOnboardingService(IGraphQLService graphQLService, HttpClient httpClient, ILocalStorageService localStorage)
    {
        _graphQLService = graphQLService;
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<DoctorProfileResult> CompleteDoctorProfileAsync(DoctorProfileCompletionModel model, string aadhaarImagePath, string? medicalCertificateFilePath = null)
    {
        var mutation = @"
            mutation CompleteDoctorProfile($input: DoctorProfileCompletionInput!, $aadhaarImagePath: String!, $medicalCertificateFilePath: String) {
                completeDoctorProfile(input: $input, aadhaarImagePath: $aadhaarImagePath, medicalCertificateFilePath: $medicalCertificateFilePath) {
                    success
                    message
                    errors
                }
            }";

        var variables = new
        {
            input = new
            {
                firstName = model.FirstName,
                lastName = model.LastName,
                gender = model.Gender,
                dateOfBirth = model.DateOfBirth?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                phoneNumber = model.PhoneNumber,
                address = model.Address,
                city = model.City,
                state = model.State,
                zipCode = model.ZipCode,
                specialization = model.Specialization,
                yearsOfExperience = model.YearsOfExperience,
                medicalRegistrationNumber = model.MedicalRegistrationNumber,
                educationHistory = model.EducationHistory,
                affiliatedHospitals = model.AffiliatedHospitals,
                consultationHours = model.ConsultationHours,
                aadhaarNumber = model.AadhaarNumber,
                medicalCertificateUrl = model.MedicalCertificateUrl
            },
            aadhaarImagePath = aadhaarImagePath,
            medicalCertificateFilePath = medicalCertificateFilePath
        };

        var response = await _graphQLService.SendQueryAsync<JsonElement>(mutation, variables);

        if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("completeDoctorProfile", out var completeDoctorProfile))
        {
            return new DoctorProfileResult
            {
                Success = completeDoctorProfile.TryGetProperty("success", out var success) && success.GetBoolean(),
                Message = completeDoctorProfile.TryGetProperty("message", out var message) ? message.GetString() ?? "" : "",
                Errors = completeDoctorProfile.TryGetProperty("errors", out var errors)
                    ? JsonSerializer.Deserialize<List<string>>(errors.GetRawText()) ?? new List<string>()
                    : new List<string>()
            };
        }

        return new DoctorProfileResult
        {
            Success = false,
            Message = "Failed to complete profile",
            Errors = new List<string> { "Unknown error occurred" }
        };
    }

    public async Task<DoctorOnboardingStatusModel> GetOnboardingStatusAsync()
    {
        var query = @"
            query GetDoctorOnboardingStatus {
                doctorOnboardingStatus {
                    isProfileCompleted
                    isAadhaarVerified
                    isMedicalCertificateVerified
                    isApprovedByAdmin
                    adminRejectionReason
                    aadhaarVerificationFailureReason
                    medicalCertificateVerificationFailureReason
                    profileSubmissionDate
                    adminApprovalDate
                    status
                }
            }";

        var response = await _graphQLService.SendQueryAsync<JsonElement>(query);

        if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("doctorOnboardingStatus", out var status))
        {
            return new DoctorOnboardingStatusModel
            {
                IsProfileCompleted = status.TryGetProperty("isProfileCompleted", out var isProfileCompleted) && isProfileCompleted.GetBoolean(),
                IsAadhaarVerified = status.TryGetProperty("isAadhaarVerified", out var isAadhaarVerified) && isAadhaarVerified.GetBoolean(),
                IsMedicalCertificateVerified = status.TryGetProperty("isMedicalCertificateVerified", out var isMedicalCertificateVerified) && isMedicalCertificateVerified.GetBoolean(),
                IsApprovedByAdmin = status.TryGetProperty("isApprovedByAdmin", out var isApprovedByAdmin) && isApprovedByAdmin.GetBoolean(),
                AdminRejectionReason = status.TryGetProperty("adminRejectionReason", out var adminRejectionReason) && adminRejectionReason.ValueKind != JsonValueKind.Null ? adminRejectionReason.GetString() : null,
                AadhaarVerificationFailureReason = status.TryGetProperty("aadhaarVerificationFailureReason", out var aadhaarFailureReason) && aadhaarFailureReason.ValueKind != JsonValueKind.Null ? aadhaarFailureReason.GetString() : null,
                MedicalCertificateVerificationFailureReason = status.TryGetProperty("medicalCertificateVerificationFailureReason", out var certFailureReason) && certFailureReason.ValueKind != JsonValueKind.Null ? certFailureReason.GetString() : null,
                ProfileSubmissionDate = status.TryGetProperty("profileSubmissionDate", out var profileSubmissionDate) && profileSubmissionDate.ValueKind != JsonValueKind.Null
                    ? DateTime.Parse(profileSubmissionDate.GetString()!)
                    : null,
                AdminApprovalDate = status.TryGetProperty("adminApprovalDate", out var adminApprovalDate) && adminApprovalDate.ValueKind != JsonValueKind.Null
                    ? DateTime.Parse(adminApprovalDate.GetString()!)
                    : null,
                Status = status.TryGetProperty("status", out var statusVal) ? statusVal.GetString() ?? "" : ""
            };
        }

        return new DoctorOnboardingStatusModel();
    }

    public async Task<List<PendingDoctorApprovalModel>> GetPendingApprovalsAsync()
    {
        var query = @"
            query GetPendingDoctorApprovals {
                pendingDoctorApprovals {
                    userId
                    email
                    fullName
                    specialization
                    medicalRegistrationNumber
                    yearsOfExperience
                    isAadhaarVerified
                    isMedicalCertificateVerified
                    aiVerificationNotes
                    aadhaarVerificationFailureReason
                    medicalCertificateVerificationFailureReason
                    profileSubmissionDate
                    aadhaarCardImagePath
                    medicalCertificateFilePath
                }
            }";

        var response = await _graphQLService.SendQueryAsync<JsonElement>(query);

        if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("pendingDoctorApprovals", out var approvalsArray))
        {
            var approvals = new List<PendingDoctorApprovalModel>();
            foreach (var approval in approvalsArray.EnumerateArray())
            {
                approvals.Add(new PendingDoctorApprovalModel
                {
                    UserId = approval.TryGetProperty("userId", out var userId) ? userId.GetString() ?? "" : "",
                    Email = approval.TryGetProperty("email", out var email) ? email.GetString() ?? "" : "",
                    FullName = approval.TryGetProperty("fullName", out var fullName) ? fullName.GetString() ?? "" : "",
                    Specialization = approval.TryGetProperty("specialization", out var specialization) && specialization.ValueKind != JsonValueKind.Null ? specialization.GetString() : null,
                    MedicalRegistrationNumber = approval.TryGetProperty("medicalRegistrationNumber", out var medicalRegistrationNumber) && medicalRegistrationNumber.ValueKind != JsonValueKind.Null ? medicalRegistrationNumber.GetString() : null,
                    YearsOfExperience = approval.TryGetProperty("yearsOfExperience", out var yearsOfExperience) && yearsOfExperience.ValueKind != JsonValueKind.Null ? yearsOfExperience.GetInt32() : (int?)null,
                    IsAadhaarVerified = approval.TryGetProperty("isAadhaarVerified", out var isAadhaarVerified) && isAadhaarVerified.GetBoolean(),
                    IsMedicalCertificateVerified = approval.TryGetProperty("isMedicalCertificateVerified", out var isMedicalCertificateVerified) && isMedicalCertificateVerified.GetBoolean(),
                    AiVerificationNotes = approval.TryGetProperty("aiVerificationNotes", out var aiVerificationNotes) && aiVerificationNotes.ValueKind != JsonValueKind.Null ? aiVerificationNotes.GetString() : null,
                    ProfileSubmissionDate = approval.TryGetProperty("profileSubmissionDate", out var profileSubmissionDate) && profileSubmissionDate.ValueKind != JsonValueKind.Null
                        ? DateTime.Parse(profileSubmissionDate.GetString()!)
                        : null,
                    AadhaarCardImagePath = approval.TryGetProperty("aadhaarCardImagePath", out var aadhaarCardImagePath) && aadhaarCardImagePath.ValueKind != JsonValueKind.Null ? aadhaarCardImagePath.GetString() : null,
                    MedicalCertificateFilePath = approval.TryGetProperty("medicalCertificateFilePath", out var medicalCertificateFilePath) && medicalCertificateFilePath.ValueKind != JsonValueKind.Null ? medicalCertificateFilePath.GetString() : null
                });
            }
            return approvals;
        }

        return new List<PendingDoctorApprovalModel>();
    }

    public async Task<AdminApprovalResult> ApproveOrRejectDoctorAsync(AdminApprovalModel model)
    {
        var mutation = @"
            mutation ApproveOrRejectDoctor($input: AdminApprovalInput!) {
                approveOrRejectDoctor(input: $input) {
                    success
                    message
                    errors
                }
            }";

        var variables = new
        {
            input = new
            {
                doctorUserId = model.DoctorUserId,
                isApproved = model.IsApproved,
                rejectionReason = model.RejectionReason
            }
        };

        var response = await _graphQLService.SendQueryAsync<JsonElement>(mutation, variables);

        if (response.ValueKind != JsonValueKind.Undefined && response.TryGetProperty("approveOrRejectDoctor", out var approveOrRejectDoctor))
        {
            return new AdminApprovalResult
            {
                Success = approveOrRejectDoctor.TryGetProperty("success", out var success) && success.GetBoolean(),
                Message = approveOrRejectDoctor.TryGetProperty("message", out var message) ? message.GetString() ?? "" : "",
                Errors = approveOrRejectDoctor.TryGetProperty("errors", out var errors)
                    ? JsonSerializer.Deserialize<List<string>>(errors.GetRawText()) ?? new List<string>()
                    : new List<string>()
            };
        }

        return new AdminApprovalResult
        {
            Success = false,
            Message = "Failed to process approval",
            Errors = new List<string> { "Unknown error occurred" }
        };
    }

    public async Task<string?> UploadAadhaarCardAsync(Stream fileStream, string fileName)
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Copy Blazor file stream to byte array to completely avoid _blazorFilesById error
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Create content from byte array (not from stream)
            using var content = new MultipartFormDataContent();
            var byteArrayContent = new ByteArrayContent(fileBytes);
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(byteArrayContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/FileUpload/aadhaar-card", content);

            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FileUploadResponse>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.filePath;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Aadhaar upload failed: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Aadhaar upload exception: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> UploadMedicalCertificateAsync(Stream fileStream, string fileName)
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Copy Blazor file stream to byte array to completely avoid _blazorFilesById error
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Create content from byte array (not from stream)
            using var content = new MultipartFormDataContent();
            var byteArrayContent = new ByteArrayContent(fileBytes);
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(byteArrayContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/FileUpload/medical-certificate", content);

            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FileUploadResponse>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.filePath;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Medical certificate upload failed: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Medical certificate upload exception: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> UploadProfileImageAsync(Stream fileStream, string fileName)
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Copy Blazor file stream to byte array to completely avoid _blazorFilesById error
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Create content from byte array (not from stream)
            using var content = new MultipartFormDataContent();
            var byteArrayContent = new ByteArrayContent(fileBytes);
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(byteArrayContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/FileUpload/profile-image", content);

            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FileUploadResponse>(responseText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result?.filePath;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Profile image upload failed: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Profile image upload exception: {ex.Message}");
            return null;
        }
    }
}

public class FileUploadResponse
{
    public bool success { get; set; }
    public string? fileName { get; set; }
    public string? filePath { get; set; }
    public string? message { get; set; }
}
namespace MediChatAI_BlazorWebAssembly.Features.Profile.Models;

// Profile Domain Models
public record UserProfile(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool EmailConfirmed,
    bool TwoFactorEnabled = false,
    string? ProfileImage = null,
    DateTime? DateOfBirth = null,
    string? Gender = null,
    string? Address = null,
    string? City = null,
    string? State = null,
    string? ZipCode = null,
    string? Country = null,
    string? PhoneNumber = null,
    string? Specialization = null,
    string? LicenseNumber = null,
    string? Department = null,
    string? EmergencyContactName = null,
    string? EmergencyContactPhone = null,
    string? BloodType = null,
    string? Allergies = null,
    string? MedicalHistory = null,
    DateTime? CreatedAt = null,
    DateTime? LastLoginAt = null,
    DateTime? LastProfileUpdate = null);

// Profile Request/Response Models
public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateTime? DateOfBirth,
    string? Gender,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Country,
    string? Specialization,
    string? LicenseNumber,
    string? Department,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? BloodType,
    string? Allergies,
    string? MedicalHistory);

public record ProfileResponse(
    bool Success,
    UserProfile? Profile,
    string[] Errors);

// GraphQL Response Wrappers for Profile Operations
public record GetUserProfileResponse(UserProfile? UserProfile);

public record UpdateProfileResponse(ProfileResponse UpdateProfile);
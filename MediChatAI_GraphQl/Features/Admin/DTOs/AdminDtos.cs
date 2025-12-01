namespace MediChatAI_GraphQl.Features.Admin.DTOs;

// User Management DTOs
public record UserListItem(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool EmailConfirmed,
    DateTime CreatedAt,
    DateTime LastLoginAt,
    bool IsActive
);

public record UserDetailsDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool EmailConfirmed,
    DateTime CreatedAt,
    DateTime LastLoginAt,
    DateTime? LastProfileUpdate,
    string? ProfileImage,
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
    string? MedicalHistory,
    bool IsActive
);

public record UserActivityDto(
    string Id,
    string? UserId,
    string UserEmail,
    string UserName,
    string UserRole,
    string ActivityType,
    string Description,
    string? Details,
    string IpAddress,
    DateTime Timestamp,
    string? AdditionalData
);

public record AdminStatsDto(
    int TotalUsers,
    int TotalPatients,
    int TotalDoctors,
    int TotalAdmins,
    int ActiveUsersToday,
    int NewUsersThisWeek,
    int TotalActivities,
    DateTime LastSystemBackup
);

// Input DTOs
public record GetUsersInput(
    int Skip = 0,
    int Take = 50,
    string? SearchTerm = null,
    string? Role = null,
    bool? IsActive = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
);

public record GetUserActivitiesInput(
    string? UserId = null,
    string? ActivityType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Skip = 0,
    int Take = 100,
    string? SortBy = "Timestamp",
    bool SortDescending = true
);

public record UpdateUserStatusInput(
    string UserId,
    bool IsActive
);

public record UpdateUserRoleInput(
    string UserId,
    string NewRole
);

// Result DTOs
public record UsersResult(
    List<UserListItem> Users,
    int TotalCount,
    bool Success,
    string[] Errors
);

public record UserDetailsResult(
    UserDetailsDto? User,
    bool Success,
    string[] Errors
);

public record UserActivitiesResult(
    List<UserActivityDto> Activities,
    int TotalCount,
    bool Success,
    string[] Errors
);

public record AdminOperationResult(
    bool Success,
    string[] Errors,
    string? Message = null
);
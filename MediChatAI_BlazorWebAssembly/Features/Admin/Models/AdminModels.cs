namespace MediChatAI_BlazorWebAssembly.Features.Admin.Models;

// User Management Models
public record UserListItem(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool EmailConfirmed,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
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

// Input Models
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

// Result Models
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

// Report Models
public record ReportTemplateDto(
    string Id,
    string Name,
    string Description,
    string Category,
    string VisualizationType,
    List<string> Metrics,
    string Dimension,
    DateTime CreatedAt,
    DateTime? LastUsed,
    bool IsDefault
);

public record CustomReportDto(
    string Id,
    string Name,
    string Description,
    string VisualizationType,
    List<ReportMetricDto> Metrics,
    ReportDimensionDto? Dimension,
    ReportFilterDto? Filters,
    DateTime CreatedAt,
    string CreatedBy
);

public record ReportMetricDto(
    string Id,
    string Name,
    string Type,
    string AggregationType
);

public record ReportDimensionDto(
    string Id,
    string Name,
    string Type
);

public record ReportFilterDto(
    DateTime? FromDate,
    DateTime? ToDate,
    string? Role,
    string? ActivityType,
    string? Status,
    string? GroupBy,
    string? SortBy,
    string? SortOrder
);

public record ScheduledReportDto(
    string Id,
    string ReportId,
    string ReportName,
    string Schedule,
    string Frequency,
    List<string> Recipients,
    string Format,
    bool IsActive,
    DateTime? NextRun,
    DateTime? LastRun,
    DateTime CreatedAt
);

public record ReportDataDto(
    string ReportId,
    string ReportName,
    Dictionary<string, object> Data,
    DateTime GeneratedAt,
    ReportFilterDto? Filters
);

public record ReportHistoryDto(
    string Id,
    string ReportName,
    string ReportType,
    string GeneratedBy,
    DateTime GeneratedAt,
    string Format,
    long FileSize,
    string? ReportTypeId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? ConfigJson = null
);

// GraphQL Response Wrappers
public record GetUsersResponse(UsersResult Users);
public record GetUserDetailsResponse(UserDetailsResult UserDetails);
public record GetUserActivitiesResponse(UserActivitiesResult UserActivities);
public record GetAdminStatsResponse(AdminStatsDto AdminStats);
public record UpdateUserStatusResponse(AdminOperationResult UpdateUserStatus);
public record UpdateUserRoleResponse(AdminOperationResult UpdateUserRole);
public record DeleteUserResponse(AdminOperationResult DeleteUser);
public record ExportUserDataResponse(AdminOperationResult ExportUserData);
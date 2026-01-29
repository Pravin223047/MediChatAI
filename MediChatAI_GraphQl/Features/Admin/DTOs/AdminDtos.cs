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
    int TotalAppointments,
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
    DateTime? FromDate = null,
    DateTime? ToDate = null,
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

public record GetAllAppointmentsInput(
    int Skip = 0,
    int Take = 20,
    string? SearchTerm = null,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
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

public record AdminAppointmentDto(
    string Id,
    string PatientId,
    string PatientName,
    string PatientEmail,
    string DoctorId,
    string DoctorName,
    string Specialization,
    DateTime AppointmentDate,
    string AppointmentTime,
    string AppointmentType,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    bool IsRequest = false  // True if this is an AppointmentRequest, false if Appointment
);

public record AllAppointmentsResult(
    List<AdminAppointmentDto> Appointments,
    int TotalCount,
    int PendingCount,
    int ConfirmedCount,
    int CompletedTodayCount,
    int RequestCount,  // Count of pending appointment requests
    bool Success,
    string[] Errors
);
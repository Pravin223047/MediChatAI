using HotChocolate.Authorization;
using HotChocolate;
using System.Security.Claims;
using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Admin.DTOs;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Features.Authentication.DTOs;
using MediChatAI_GraphQl.Features.Notifications.DTOs;
using MediChatAI_GraphQl.Features.Emergency.DTOs;
using MediChatAI_GraphQl.Features.Patient.DTOs;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Infrastructure.Data;
using System.Threading.Tasks;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Emergency;
using MediChatAI_GraphQl.Core.Interfaces.Services.Appointment;
using MediChatAI_GraphQl.Core.Interfaces.Services.Document;
using MediChatAI_GraphQl.Core.Interfaces.Services.Prescription;
using MediChatAI_GraphQl.Core.Interfaces.Services.Patient;
using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Features.Emergency.Services;
using MediChatAI_GraphQl.Shared.Services;

namespace MediChatAI_GraphQl.GraphQL.Queries
{
    public class Query
    {
        /// <summary>
        /// Retrieves the currently authenticated user's information.
        /// </summary>
        [Authorize]
        public async Task<UserInfo?> GetCurrentUserAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAuthService authService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await authService.GetCurrentUserAsync(userId);
        }

        /// <summary>
        /// Retrieves the currently authenticated user's full profile.
        /// </summary>
        [Authorize]
        public async Task<UserProfile?> GetUserProfileAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAuthService authService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await authService.GetUserProfileAsync(userId);
        }

        [Authorize(Roles = new[] { "Admin" })]
        public string GetAdminData() => "This is admin-only data";

        [Authorize(Roles = new[] { "Doctor" })]
        public string GetDoctorData() => "This is doctor-only data";

        [Authorize(Roles = new[] { "Patient" })]
        public string GetPatientData() => "This is patient-only data";

        // Admin-specific queries
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<UsersResult> GetUsersAsync(
            GetUsersInput input,
            [Service] IAdminService adminService)
        {
            return await adminService.GetUsersAsync(input);
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<UserDetailsResult> GetUserDetailsAsync(
            string userId,
            [Service] IAdminService adminService)
        {
            return await adminService.GetUserDetailsAsync(userId);
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<UserActivitiesResult> GetUserActivitiesAsync(
            GetUserActivitiesInput input,
            [Service] IAdminService adminService)
        {
            return await adminService.GetUserActivitiesAsync(input);
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<AdminStatsDto> GetAdminStatsAsync(
            [Service] IAdminService adminService)
        {
            return await adminService.GetAdminStatsAsync();
        }

        // Doctor onboarding queries
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<DoctorOnboardingStatus> GetDoctorOnboardingStatusAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorOnboardingService doctorOnboardingService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            return await doctorOnboardingService.GetOnboardingStatusAsync(userId);
        }

        [Authorize(Roles = new[] { "Admin" })]
        public async Task<IEnumerable<PendingDoctorApproval>> GetPendingDoctorApprovalsAsync(
            [Service] IDoctorOnboardingService doctorOnboardingService)
        {
            return await doctorOnboardingService.GetPendingApprovalsAsync();
        }

        // System Settings queries
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<SystemSettingsData> GetSystemSettings([Service] ISystemSettingsService settingsService)
        {
            return await settingsService.GetSystemSettingsDataAsync();
        }

        /// <summary>
        /// Public endpoint to get general/public settings - accessible to all users (including non-authenticated)
        /// Returns site name, description, contact info, and format preferences
        /// </summary>
        public async Task<GeneralSettingsData> GetPublicSettings([Service] ISystemSettingsService settingsService)
        {
            var settings = await settingsService.GetSystemSettingsAsync();

            return new GeneralSettingsData
            {
                SiteName = settings.SiteName,
                SiteDescription = settings.SiteDescription,
                ContactEmail = settings.ContactEmail,
                ContactPhone = settings.ContactPhone,
                Timezone = settings.Timezone,
                DateFormat = settings.DateFormat,
                TimeFormat = settings.TimeFormat
            };
        }

        /// <summary>
        /// Public endpoint to get appearance/theme settings - accessible to all users (including non-authenticated)
        /// </summary>
        public async Task<AppearanceSettingsData> GetAppearanceSettings([Service] ISystemSettingsService settingsService)
        {
            var settings = await settingsService.GetSystemSettingsAsync();

            return new AppearanceSettingsData
            {
                ThemeMode = settings.ThemeMode,
                ThemePrimaryColor = settings.ThemePrimaryColor,
                ThemeAccentColor = settings.ThemeAccentColor,
                ThemeBackgroundColor = settings.ThemeBackgroundColor,
                ThemeTextColor = settings.ThemeTextColor,
                ThemeSidebarStyle = settings.ThemeSidebarStyle,
                ThemeFontSize = settings.ThemeFontSize,
                ThemeEnableAnimations = settings.ThemeEnableAnimations,
                ThemeCompactMode = settings.ThemeCompactMode,
                ThemePreset = settings.ThemePreset,
                ThemeHighContrast = settings.ThemeHighContrast,
                ThemeBorderRadius = settings.ThemeBorderRadius
            };
        }

        // Notification queries
        [Authorize]
        public async Task<NotificationsResult> GetNotificationsAsync(
            GetNotificationsInput input,
            ClaimsPrincipal claimsPrincipal,
            [Service] INotificationService notificationService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var (notifications, totalCount) = await notificationService.GetUserNotificationsAsync(
                userId,
                input.Skip ?? 0,
                input.Take ?? 20,
                input.IsRead,
                input.Category,
                input.Priority);

            return new NotificationsResult
            {
                Notifications = notifications,
                TotalCount = totalCount
            };
        }

        [Authorize]
        public async Task<int> GetUnreadNotificationCountAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] INotificationService notificationService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return 0;

            return await notificationService.GetUnreadCountAsync(userId);
        }

        [Authorize]
        public async Task<NotificationPreference> GetNotificationPreferencesAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] INotificationService notificationService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            return await notificationService.GetOrCreatePreferencesAsync(userId);
        }

        // Admin-only: Get all notifications from all users
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<AllNotificationsResult> GetAllNotificationsAsync(
            GetAllNotificationsInput input,
            [Service] INotificationService notificationService,
            [Service] Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
            [Service] ApplicationDbContext context)
        {
            var query = notificationService.GetAllNotificationsQuery();

            // Filter by userId if specified
            if (!string.IsNullOrEmpty(input.UserId))
            {
                query = query.Where(n => n.UserId == input.UserId);
            }

            // Filter by category if specified
            if (input.Category.HasValue)
            {
                query = query.Where(n => n.Category == input.Category.Value);
            }

            // Filter by type if specified
            if (input.Type.HasValue)
            {
                query = query.Where(n => n.Type == input.Type.Value);
            }

            // Filter by date range
            if (input.StartDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= input.StartDate.Value);
            }

            if (input.EndDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= input.EndDate.Value);
            }

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip(input.Skip ?? 0)
                .Take(input.Take ?? 50)
                .Include(n => n.User)
                .ToListAsync();

            // Pre-fetch all user roles in a single query to avoid N+1 problem
            var userIds = notifications.Where(n => n.User != null).Select(n => n.UserId).Distinct().ToList();
            var userRoles = await context.UserRoles
                .AsNoTracking()
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(context.Roles.AsNoTracking(),
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new { ur.UserId, RoleName = r.Name })
                .ToListAsync();

            var userRoleMap = userRoles
                .GroupBy(ur => ur.UserId)
                .ToDictionary(g => g.Key, g => g.First().RoleName ?? "Unknown");

            var results = new List<AdminNotificationView>();
            foreach (var notification in notifications)
            {
                var user = notification.User;
                var role = user != null && userRoleMap.ContainsKey(user.Id)
                    ? userRoleMap[user.Id]
                    : "Unknown";

                results.Add(new AdminNotificationView
                {
                    Notification = notification,
                    UserEmail = user?.Email ?? "Unknown",
                    UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown",
                    UserRole = role
                });
            }

            return new AllNotificationsResult
            {
                Notifications = results,
                TotalCount = totalCount
            };
        }

        // Emergency Hospital Search - Public endpoint (no authorization required)
        /// <summary>
        /// Search for nearby hospitals based on user's location. Public endpoint accessible to all users.
        /// </summary>
        public async Task<HospitalSearchResult> SearchNearbyHospitalsAsync(
            HospitalSearchInput input,
            [Service] IHospitalLocationService hospitalLocationService)
        {
            return await hospitalLocationService.SearchNearbyHospitalsAsync(input);
        }

        // Emergency Chat Queries
        /// <summary>
        /// Get chat history for a session (emergency chat)
        /// </summary>
        [AllowAnonymous]
        public async Task<ChatHistoryResponse> GetChatHistoryAsync(
            string? sessionId,
            string? userId,
            int limit,
            DateTime? since,
            [Service] IGeminiEmergencyChatService chatService)
        {
            var request = new ChatHistoryRequest
            {
                SessionId = sessionId,
                UserId = userId,
                Limit = limit,
                Since = since
            };

            return await chatService.GetChatHistoryAsync(request);
        }

        // Geoapify Routing Queries - Public endpoints for emergency navigation
        /// <summary>
        /// Calculate real driving route with turn-by-turn directions and static map
        /// </summary>
        [AllowAnonymous]
        public async Task<GeoapifyRouteResponse> GetRouteAsync(
            GeoapifyRouteRequest request,
            [Service] IGeoapifyService geoapifyService)
        {
            return await geoapifyService.GetRouteAsync(request);
        }

        /// <summary>
        /// Get isochrone (reachability zones) for a location
        /// </summary>
        [AllowAnonymous]
        public async Task<GeoapifyIsochroneResponse> GetIsochroneAsync(
            GeoapifyIsochroneRequest request,
            [Service] IGeoapifyService geoapifyService)
        {
            return await geoapifyService.GetIsochroneAsync(request);
        }

        /// <summary>
        /// Calculate distances from one origin to multiple destinations
        /// </summary>
        [AllowAnonymous]
        public async Task<GeoapifyMatrixResponse> GetDistanceMatrixAsync(
            GeoapifyMatrixRequest request,
            [Service] IGeoapifyService geoapifyService)
        {
            return await geoapifyService.GetDistanceMatrixAsync(request);
        }

        // Doctor Dashboard Queries
        /// <summary>
        /// Get complete dashboard data for the authenticated doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<DoctorDashboardData> GetDoctorDashboardDataAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorDashboardService dashboardService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await dashboardService.GetDashboardDataAsync(doctorId);
        }

        /// <summary>
        /// Get overview statistics for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<DoctorOverviewStats> GetDoctorOverviewStatsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorDashboardService dashboardService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await dashboardService.GetOverviewStatsAsync(doctorId);
        }

        /// <summary>
        /// Get today's appointments for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<TodayAppointment>> GetTodayAppointmentsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorDashboardService dashboardService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await dashboardService.GetTodayAppointmentsAsync(doctorId);
        }

        /// <summary>
        /// Get past appointments for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<TodayAppointment>> GetPastAppointmentsAsync(
            int days,
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorDashboardService dashboardService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await dashboardService.GetPastAppointmentsAsync(doctorId, days);
        }

        /// <summary>
        /// Get upcoming appointments for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<TodayAppointment>> GetUpcomingAppointmentsAsync(
            int days,
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorDashboardService dashboardService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await dashboardService.GetUpcomingAppointmentsAsync(doctorId, days);
        }

        /// <summary>
        /// Get recent activities for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<RecentActivity>> GetRecentActivitiesAsync(
            int limit,
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorDashboardService dashboardService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await dashboardService.GetRecentActivitiesAsync(doctorId, limit);
        }

        /// <summary>
        /// Get doctor analytics for a specific date range
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<DoctorAnalyticsData> GetDoctorAnalyticsAsync(
            DateTime? startDate,
            DateTime? endDate,
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorAnalyticsService analyticsService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await analyticsService.GetAnalyticsAsync(doctorId, startDate, endDate);
        }

        /// <summary>
        /// Get patient vitals for a specific patient
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<PatientVital>> GetPatientVitalsAsync(
            string patientId,
            DateTime? startDate,
            DateTime? endDate,
            ClaimsPrincipal claimsPrincipal,
            [Service] IPatientVitalsService vitalsService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await vitalsService.GetPatientVitalsAsync(patientId, startDate, endDate);
        }

        /// <summary>
        /// Get latest vitals for a patient
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<PatientVitalsData> GetLatestPatientVitalsAsync(
            string patientId,
            ClaimsPrincipal claimsPrincipal,
            [Service] IPatientVitalsService vitalsService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await vitalsService.GetLatestVitalsAsync(patientId);
        }

        /// <summary>
        /// Get critical vitals requiring immediate attention
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<PatientVital>> GetCriticalVitalsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IPatientVitalsService vitalsService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await vitalsService.GetCriticalVitalsForDoctorAsync(doctorId);
        }

        /// <summary>
        /// Get emergency alerts for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<EmergencyAlert>> GetEmergencyAlertsAsync(
            AlertStatus? status,
            ClaimsPrincipal claimsPrincipal,
            [Service] IEmergencyAlertService alertService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await alertService.GetAlertsForDoctorAsync(doctorId, status);
        }

        /// <summary>
        /// Get active emergency alerts for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<EmergencyAlert>> GetActiveEmergencyAlertsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IEmergencyAlertService alertService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await alertService.GetAlertsForDoctorAsync(doctorId, AlertStatus.Active);
        }

        /// <summary>
        /// Get active alert count for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<int> GetActiveAlertCountAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IEmergencyAlertService alertService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                return 0;

            return await alertService.GetActiveAlertCountAsync(doctorId);
        }

        /// <summary>
        /// Get conversation between doctor and another user
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<DoctorMessage>> GetConversationAsync(
            string receiverId,
            int limit,
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorChatService chatService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await chatService.GetConversationAsync(doctorId, receiverId, limit);
        }

        /// <summary>
        /// Get all conversations for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<IEnumerable<ConversationDto>> GetDoctorConversationsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorChatService chatService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            var conversations = await chatService.GetConversationsAsync(doctorId);
            return conversations.Select(c => new ConversationDto
            {
                Id = c.ConversationId ?? Guid.NewGuid(),
                PartnerId = c.PartnerId,
                PartnerName = c.PartnerName,
                PartnerRole = c.PartnerRole,
                PartnerProfileImage = c.PartnerProfileImage,
                LastMessage = c.LastMessage,
                LastMessageTime = c.LastMessageTime,
                UnreadCount = c.UnreadCount,
                IsOnline = c.IsOnline
            });
        }

        /// <summary>
        /// Get unread message count for the doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<int> GetUnreadMessageCountAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorChatService chatService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                return 0;

            return await chatService.GetUnreadCountAsync(doctorId);
        }

        /// <summary>
        /// Get doctor preferences (theme, dashboard settings, etc.)
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<DoctorPreference?> GetDoctorPreferencesAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IDoctorPreferencesService preferencesService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await preferencesService.GetPreferencesAsync(doctorId);
        }

        // ============================================
        // APPOINTMENT SYSTEM QUERIES
        // ============================================

        /// <summary>
        /// Get appointment by ID
        /// </summary>
        [Authorize]
        public async Task<AppointmentDto?> GetAppointmentAsync(
            int id,
            [Service] IAppointmentService appointmentService)
        {
            return await appointmentService.GetAppointmentByIdAsync(id);
        }

        /// <summary>
        /// Get all appointments for the current patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<List<AppointmentDto>> GetMyAppointmentsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAppointmentService appointmentService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            return await appointmentService.GetAppointmentsByPatientIdAsync(patientId);
        }

        /// <summary>
        /// Get upcoming appointments for the current patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<List<AppointmentDto>> GetMyUpcomingAppointmentsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAppointmentService appointmentService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            return await appointmentService.GetUpcomingAppointmentsByPatientIdAsync(patientId);
        }

        /// <summary>
        /// Get all appointments for a doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<AppointmentDto>> GetDoctorAppointmentsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAppointmentService appointmentService,
            [Service] ILogger<Query> logger)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            logger.LogInformation("GetDoctorAppointmentsAsync called. DoctorId from claims: {DoctorId}", doctorId);
            logger.LogInformation("All claims: {Claims}", string.Join(", ", claimsPrincipal.Claims.Select(c => $"{c.Type}={c.Value}")));

            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            var appointments = await appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);
            logger.LogInformation("Found {Count} appointments for doctor {DoctorId}", appointments.Count, doctorId);

            return appointments;
        }

        /// <summary>
        /// Get upcoming appointments for a doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<AppointmentDto>> GetDoctorUpcomingAppointmentsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAppointmentService appointmentService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await appointmentService.GetUpcomingAppointmentsByDoctorIdAsync(doctorId);
        }

        /// <summary>
        /// Get appointment request by ID
        /// </summary>
        [Authorize]
        public async Task<AppointmentRequestDto?> GetAppointmentRequestAsync(
            int id,
            [Service] IAppointmentService appointmentService)
        {
            return await appointmentService.GetAppointmentRequestByIdAsync(id);
        }

        /// <summary>
        /// Get all appointment requests for the current patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<List<AppointmentRequestDto>> GetMyAppointmentRequestsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAppointmentService appointmentService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            return await appointmentService.GetRequestsByPatientIdAsync(patientId);
        }

        /// <summary>
        /// Get pending appointment requests for a doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<AppointmentRequestDto>> GetPendingAppointmentRequestsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAppointmentService appointmentService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            Console.WriteLine($"Fetching pending appointment requests for doctor {doctorId}");

            return await appointmentService.GetPendingRequestsByDoctorIdAsync(doctorId);
        }

        /// <summary>
        /// Debug endpoint to get all appointment requests (Admin only)
        /// </summary>
        [Authorize(Roles = new[] { "Admin" })]
        public async Task<List<AppointmentRequestDto>> GetAllAppointmentRequestsAsync(
            [Service] ApplicationDbContext context)
        {
            return await context.AppointmentRequests
                .Include(r => r.PreferredDoctor)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new AppointmentRequestDto
                {
                    Id = r.Id,
                    PatientId = r.PatientId,
                    PreferredDoctorId = r.PreferredDoctorId,
                    PreferredDoctorName = r.PreferredDoctor != null ? $"{r.PreferredDoctor.FirstName} {r.PreferredDoctor.LastName}" : null,
                    FullName = r.FullName,
                    Age = r.Age,
                    Gender = r.Gender,
                    Email = r.Email,
                    PhoneNumber = r.PhoneNumber,
                    BloodType = r.BloodType,
                    Allergies = r.Allergies,
                    SymptomDescription = r.SymptomDescription,
                    SymptomSeverity = r.SymptomSeverity,
                    ReasonForVisit = r.ReasonForVisit ?? r.SymptomDescription,
                    InsuranceProvider = r.InsuranceProvider,
                    PreferredDate = r.PreferredDate,
                    PreferredTimeSlot = r.PreferredTimeSlot,
                    IsUrgent = r.IsUrgent,
                    Status = r.Status,
                    RequestedAt = r.RequestedAt,
                    AppointmentId = r.AppointmentId
                })
                .ToListAsync();
        }

        /// <summary>
        /// Get doctor availability for a date range
        /// </summary>
        [Authorize]
        public async Task<List<DoctorAvailabilityDto>> GetDoctorAvailabilityAsync(
            string doctorId,
            DateTime startDate,
            DateTime endDate,
            [Service] IAppointmentService appointmentService)
        {
            return await appointmentService.GetDoctorAvailabilityAsync(doctorId, startDate, endDate);
        }

        /// <summary>
        /// Get available time slots for a doctor on a specific date
        /// </summary>
        [Authorize]
        public async Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(
            string doctorId,
            DateTime date,
            [Service] IAppointmentService appointmentService)
        {
            return await appointmentService.GetAvailableTimeSlotsAsync(doctorId, date);
        }

        /// <summary>
        /// Get all available doctors for appointment booking
        /// </summary>
        [Authorize]
        public async Task<List<AvailableDoctorDto>> GetAvailableDoctorsAsync(
            string? specialization,
            [Service] ApplicationDbContext context,
            [Service] Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole> roleManager)
        {
            // Get the Doctor role ID
            var doctorRole = await roleManager.FindByNameAsync("Doctor");
            if (doctorRole == null)
                return new List<AvailableDoctorDto>();

            // Get all users with Doctor role who are approved
            var doctorUserIds = await context.UserRoles
                .Where(ur => ur.RoleId == doctorRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            var query = context.Users
                .Where(u => doctorUserIds.Contains(u.Id) && u.IsApprovedByAdmin);

            // Filter by specialization if provided
            if (!string.IsNullOrWhiteSpace(specialization))
            {
                query = query.Where(u => u.Specialization == specialization);
            }

            var doctors = await query
                .Select(u => new AvailableDoctorDto
                {
                    Id = u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    Specialization = u.Specialization,
                    ProfileImage = u.ProfileImage,
                    YearsOfExperience = u.YearsOfExperience,
                    Department = u.Department,
                    Rating = 4.5m, // TODO: Calculate from actual reviews
                    IsAvailable = true
                })
                .ToListAsync();

            return doctors;
        }

        // ============================================
        // TIMEBLOCK QUERIES
        // ============================================

        /// <summary>
        /// Get a time block by ID for the authenticated doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<TimeBlockDto?> GetTimeBlockByIdAsync(
            Guid timeBlockId,
            ClaimsPrincipal claimsPrincipal,
            [Service] ITimeBlockService timeBlockService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            var timeBlock = await timeBlockService.GetTimeBlockByIdAsync(timeBlockId);
            if (timeBlock == null || timeBlock.DoctorId != doctorId)
                return null;

            return timeBlock;
        }

        /// <summary>
        /// Get all time blocks for the authenticated doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<TimeBlockDto>> GetMyTimeBlocksAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] ITimeBlockService timeBlockService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await timeBlockService.GetTimeBlocksByDoctorIdAsync(doctorId);
        }

        /// <summary>
        /// Get time blocks for the authenticated doctor within a date range
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<TimeBlockDto>> GetMyTimeBlocksByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            ClaimsPrincipal claimsPrincipal,
            [Service] ITimeBlockService timeBlockService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await timeBlockService.GetTimeBlocksByDoctorIdAndDateRangeAsync(doctorId, startDate, endDate);
        }

        /// <summary>
        /// Get active time blocks for the authenticated doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<TimeBlockDto>> GetMyActiveTimeBlocksAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] ITimeBlockService timeBlockService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await timeBlockService.GetActiveTimeBlocksByDoctorIdAsync(doctorId);
        }

        /// <summary>
        /// Check if a time slot has conflicts for the authenticated doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<bool> CheckTimeSlotConflictAsync(
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime,
            Guid? excludeTimeBlockId,
            ClaimsPrincipal claimsPrincipal,
            [Service] ITimeBlockService timeBlockService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await timeBlockService.HasConflictAsync(doctorId, date, startTime, endTime, excludeTimeBlockId);
        }

        /// <summary>
        /// Get complete week schedule including appointments and time blocks for the authenticated doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<DoctorWeekScheduleDto> GetMyWeekScheduleAsync(
            DateTime weekStartDate,
            ClaimsPrincipal claimsPrincipal,
            [Service] IAppointmentService appointmentService,
            [Service] ITimeBlockService timeBlockService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            var weekEndDate = weekStartDate.AddDays(7);

            // Get appointments for the week
            var appointments = await appointmentService.GetAppointmentsByDoctorIdAsync(doctorId);
            var weekAppointments = appointments
                .Where(a => a.AppointmentDateTime >= weekStartDate && a.AppointmentDateTime < weekEndDate)
                .ToList();

            // Get time blocks for the week
            var timeBlocks = await timeBlockService.GetTimeBlocksByDoctorIdAndDateRangeAsync(
                doctorId,
                weekStartDate,
                weekEndDate);

            return new DoctorWeekScheduleDto
            {
                WeekStartDate = weekStartDate,
                WeekEndDate = weekEndDate,
                Appointments = weekAppointments,
                TimeBlocks = timeBlocks
            };
        }

        // ============================================
        // PATIENT DOCUMENT QUERIES
        // ============================================

        /// <summary>
        /// Get document by ID
        /// </summary>
        [Authorize]
        public async Task<PatientDocumentDto?> GetPatientDocumentAsync(
            int id,
            [Service] IPatientDocumentService documentService)
        {
            return await documentService.GetDocumentByIdAsync(id);
        }

        /// <summary>
        /// Get all documents for the current patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<List<PatientDocumentDto>> GetMyDocumentsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IPatientDocumentService documentService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            return await documentService.GetDocumentsByPatientIdAsync(patientId);
        }

        /// <summary>
        /// Get documents by patient ID (for doctors)
        /// </summary>
        [Authorize(Roles = new[] { "Doctor", "Admin" })]
        public async Task<List<PatientDocumentDto>> GetPatientDocumentsAsync(
            string patientId,
            [Service] IPatientDocumentService documentService)
        {
            return await documentService.GetDocumentsByPatientIdAsync(patientId);
        }

        /// <summary>
        /// Get documents for an appointment
        /// </summary>
        [Authorize]
        public async Task<List<PatientDocumentDto>> GetAppointmentDocumentsAsync(
            int appointmentId,
            [Service] IPatientDocumentService documentService)
        {
            return await documentService.GetDocumentsByAppointmentIdAsync(appointmentId);
        }

        /// <summary>
        /// Get documents for an appointment request
        /// </summary>
        [Authorize]
        public async Task<List<PatientDocumentDto>> GetAppointmentRequestDocumentsAsync(
            int requestId,
            [Service] IPatientDocumentService documentService)
        {
            return await documentService.GetDocumentsByAppointmentRequestIdAsync(requestId);
        }

        // ============================================
        // PRESCRIPTION QUERIES
        // ============================================

        /// <summary>
        /// Get prescription by ID
        /// </summary>
        [Authorize]
        public async Task<PrescriptionDto?> GetPrescriptionAsync(
            int id,
            [Service] IPrescriptionService prescriptionService)
        {
            return await prescriptionService.GetPrescriptionByIdAsync(id);
        }

        /// <summary>
        /// Get all prescriptions for the current patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<List<PrescriptionDto>> GetMyPrescriptionsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IPrescriptionService prescriptionService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            return await prescriptionService.GetPrescriptionsByPatientIdAsync(patientId);
        }

        /// <summary>
        /// Get active prescriptions for the current patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<List<PrescriptionDto>> GetMyActivePrescriptionsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IPrescriptionService prescriptionService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            return await prescriptionService.GetActivePrescriptionsByPatientIdAsync(patientId);
        }

        /// <summary>
        /// Get prescriptions by patient ID (for doctors)
        /// </summary>
        [Authorize(Roles = new[] { "Doctor", "Admin" })]
        public async Task<List<PrescriptionDto>> GetPatientPrescriptionsAsync(
            string patientId,
            [Service] IPrescriptionService prescriptionService)
        {
            return await prescriptionService.GetPrescriptionsByPatientIdAsync(patientId);
        }

        /// <summary>
        /// Get prescriptions created by the current doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<PrescriptionDto>> GetMyPrescribedMedicationsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IPrescriptionService prescriptionService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await prescriptionService.GetPrescriptionsByDoctorIdAsync(doctorId);
        }

        /// <summary>
        /// Get prescriptions for an appointment
        /// </summary>
        [Authorize]
        public async Task<List<PrescriptionDto>> GetAppointmentPrescriptionsAsync(
            int appointmentId,
            [Service] IPrescriptionService prescriptionService)
        {
            return await prescriptionService.GetPrescriptionsByAppointmentIdAsync(appointmentId);
        }

        /// <summary>
        /// Get prescription as HTML for PDF generation
        /// </summary>
        [Authorize]
        public async Task<string> GetPrescriptionHtmlAsync(
            int prescriptionId,
            [Service] IPrescriptionService prescriptionService,
            [Service] IPrescriptionPdfService pdfService)
        {
            var prescription = await prescriptionService.GetPrescriptionByIdAsync(prescriptionId);
            if (prescription == null)
                throw new ArgumentException("Prescription not found");

            return await pdfService.GeneratePrescriptionHtmlAsync(prescription);
        }

        // ============================================
        // PATIENT MODULE QUERIES
        // ============================================

        /// <summary>
        /// Get complete dashboard data for the authenticated patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<PatientDashboardDto> GetPatientDashboardDataAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IAppointmentService appointmentService,
            [Service] IPrescriptionService prescriptionService,
            [Service] IPatientDocumentService documentService,
            [Service] INotificationService notificationService,
            [Service] ApplicationDbContext context,
            [Service] ILogger<Query> logger)
        {
            try
            {
                logger.LogInformation("GetPatientDashboardDataAsync called");

                var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(patientId))
                {
                    logger.LogWarning("Patient not authenticated - no user ID in claims");
                    throw new UnauthorizedAccessException("Patient not authenticated");
                }

                logger.LogInformation("Fetching dashboard data for patient: {PatientId}", patientId);

                var dashboard = new PatientDashboardDto();

                // Get appointments
                logger.LogInformation("Fetching appointments for patient: {PatientId}", patientId);
                var allAppointments = await appointmentService.GetAppointmentsByPatientIdAsync(patientId);
                logger.LogInformation("Found {Count} total appointments", allAppointments.Count);

                var upcomingAppointments = await appointmentService.GetUpcomingAppointmentsByPatientIdAsync(patientId);
                logger.LogInformation("Found {Count} upcoming appointments", upcomingAppointments.Count);

                dashboard.UpcomingAppointments = upcomingAppointments.Take(5).Select(a => new UpcomingAppointmentSummary
            {
                Id = a.Id,
                DoctorName = a.DoctorName ?? "",
                Specialization = a.DoctorSpecialization ?? "",
                DoctorProfileImage = "", // TODO: Add to AppointmentDto
                AppointmentDate = a.AppointmentDateTime.Date,
                AppointmentTime = a.AppointmentDateTime.ToString("HH:mm"),
                AppointmentType = a.Type.ToString(),
                Status = a.Status.ToString(),
                IsVirtual = a.IsVirtual,
                MeetingLink = a.MeetingLink,
                ReasonForVisit = a.ReasonForVisit
            }).ToList();

                // Get prescriptions
                logger.LogInformation("Fetching prescriptions for patient: {PatientId}", patientId);
                var activePrescriptions = await prescriptionService.GetActivePrescriptionsByPatientIdAsync(patientId);
                logger.LogInformation("Found {Count} active prescriptions", activePrescriptions.Count);

                dashboard.ActivePrescriptions = activePrescriptions.Take(5).Select(p => new ActivePrescriptionSummary
            {
                Id = p.Id,
                MedicationName = p.Items.Any() ?
                    (p.Items.Count == 1 ? p.Items.First().MedicationName : $"{p.Items.Count} medications") :
                    "No medications",
                Dosage = p.Items.Any() && p.Items.Count == 1 ? p.Items.First().Dosage ?? "" : "",
                Frequency = p.Items.Any() && p.Items.Count == 1 ? p.Items.First().Frequency ?? "" : "",
                StartDate = p.StartDate ?? p.PrescribedDate,
                EndDate = p.EndDate,
                RefillsRemaining = p.RefillsAllowed - p.RefillsUsed,
                DoctorName = p.DoctorName ?? "",
                IsActive = p.Status == PrescriptionStatus.Active,
                Instructions = p.Items.Any() && p.Items.Count == 1 ? p.Items.First().Instructions ?? "" : ""
            }).ToList();

                // Get documents
                logger.LogInformation("Fetching documents for patient: {PatientId}", patientId);
                var documents = await documentService.GetDocumentsByPatientIdAsync(patientId);
                logger.LogInformation("Found {Count} documents", documents.Count);

                dashboard.RecentDocuments = documents.OrderByDescending(d => d.UploadedAt).Take(5).Select(d => new RecentDocumentSummary
            {
                Id = d.Id,
                DocumentName = d.FileName ?? "",
                DocumentType = d.DocumentType.ToString(),
                DocumentCategory = d.DocumentType.ToString(), // Using DocumentType as category
                UploadedDate = d.UploadedAt,
                FileUrl = d.FileUrl ?? "",
                FileSizeBytes = d.FileSizeBytes
            }).ToList();

                // Get notifications
                logger.LogInformation("Fetching notifications for patient: {PatientId}", patientId);
                var (notifications, _) = await notificationService.GetUserNotificationsAsync(patientId, 0, 5, null, null, null);
                logger.LogInformation("Found {Count} notifications", notifications.Count());

                dashboard.RecentNotifications = notifications.Select(n => new PatientNotificationSummary
            {
                Id = n.Id,
                Title = n.Title ?? "",
                Message = n.Message ?? "",
                Category = n.Category.ToString(),
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            }).ToList();

                // Calculate stats
                logger.LogInformation("Calculating overview stats for patient: {PatientId}", patientId);
                dashboard.OverviewStats = new PatientOverviewStats
            {
                TotalAppointments = allAppointments.Count,
                UpcomingAppointments = upcomingAppointments.Count,
                CompletedAppointments = allAppointments.Count(a => a.Status == AppointmentStatus.Completed),
                ActivePrescriptions = activePrescriptions.Count,
                TotalDocuments = documents.Count,
                UnreadMessages = 0, // TODO: Implement messaging service
                PendingRefills = activePrescriptions.Count(p => (p.RefillsAllowed - p.RefillsUsed) <= 1)
            };

                // TODO: Get latest vitals from health metrics service when implemented
                dashboard.LatestVitals = null;

                logger.LogInformation("Successfully built dashboard data for patient: {PatientId}", patientId);
                return dashboard;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching dashboard data for patient");
                throw;
            }
        }

        /// <summary>
        /// Get patient health metrics/vitals
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<HealthMetricsResponse> GetPatientHealthMetricsAsync(
            GetHealthMetricsInput input,
            ClaimsPrincipal claimsPrincipal,
            [Service] IPatientVitalsService vitalsService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            var vitals = await vitalsService.GetPatientVitalsAsync(patientId, input.StartDate, input.EndDate);

            var metrics = vitals.Select(v => new HealthMetricDto
            {
                Id = v.Id,
                PatientId = v.PatientId,
                MetricType = v.VitalType.ToString(),
                Value = double.TryParse(v.Value, out var val) ? val : 0,
                Unit = v.Unit ?? "",
                RecordedDate = v.RecordedAt,
                Notes = v.Notes,
                SystolicValue = v.SystolicValue,
                DiastolicValue = v.DiastolicValue,
                Status = v.Severity.ToString()
            }).ToList();

            return new HealthMetricsResponse
            {
                Metrics = metrics,
                TotalCount = metrics.Count,
                Summary = new HealthMetricsSummary()
            };
        }

        /// <summary>
        /// Get patient consultation history
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        [GraphQLName("getPatientConsultations")]
        public async Task<List<ConsultationHistoryDto>> GetPatientConsultationsAsync(
            GetConsultationsInput input,
            ClaimsPrincipal claimsPrincipal,
            [Service] IAppointmentService appointmentService,
            [Service] IPrescriptionService prescriptionService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                return new List<ConsultationHistoryDto>();

            try
            {
                // Get appointments that can be considered consultations
                var appointments = await appointmentService.GetAppointmentsByPatientIdAsync(patientId);
                var consultationAppointments = appointments.Where(a => 
                    a.Status == AppointmentStatus.Completed || 
                    a.Status == AppointmentStatus.InProgress ||
                    (a.Status == AppointmentStatus.Confirmed && a.AppointmentDateTime <= DateTime.UtcNow));

                if (input.StartDate.HasValue)
                    consultationAppointments = consultationAppointments.Where(a => a.AppointmentDateTime >= input.StartDate.Value);

                if (input.EndDate.HasValue)
                    consultationAppointments = consultationAppointments.Where(a => a.AppointmentDateTime <= input.EndDate.Value);

                if (!string.IsNullOrEmpty(input.DoctorId))
                    consultationAppointments = consultationAppointments.Where(a => a.DoctorId == input.DoctorId);

                var consultations = new List<ConsultationHistoryDto>();

                foreach (var appointment in consultationAppointments.OrderByDescending(a => a.AppointmentDateTime).Skip(input.Skip ?? 0).Take(input.Limit ?? 50))
                {
                    consultations.Add(new ConsultationHistoryDto
                    {
                        Id = appointment.Id,
                        DoctorId = appointment.DoctorId ?? "",
                        DoctorName = appointment.DoctorName ?? "",
                        Specialization = appointment.DoctorSpecialization ?? "",
                        DoctorProfileImage = "",
                        ConsultationDate = appointment.AppointmentDateTime,
                        ChiefComplaint = appointment.ReasonForVisit ?? "",
                        Diagnosis = appointment.DoctorNotes ?? "",
                        Observations = "",
                        TreatmentPlan = new List<string>(),
                        Prescriptions = new List<ConsultationPrescriptionDto>(),
                        FollowUpInstructions = null,
                        NextFollowUpDate = null,
                        IsRated = false,
                        Rating = null,
                        PatientFeedback = null,
                        ConsultationType = appointment.Type.ToString()
                    });
                }

                return consultations;
            }
            catch
            {
                return new List<ConsultationHistoryDto>();
            }
        }

        /// <summary>
        /// Get patient lab results
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public Task<List<LabResultDto>> GetPatientLabResultsAsync(
            GetLabResultsInput input,
            ClaimsPrincipal claimsPrincipal,
            [Service] ApplicationDbContext context)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            // TODO: Implement LabResult entity and service
            // For now, returning empty list
            return Task.FromResult(new List<LabResultDto>());
        }

        /// <summary>
        /// Get medication reminders for the patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public Task<List<MedicationReminderDto>> GetMedicationRemindersAsync(
            GetMedicationRemindersInput input,
            ClaimsPrincipal claimsPrincipal,
            [Service] ApplicationDbContext context)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            // TODO: Implement MedicationReminder entity and service
            // For now, returning empty list
            return Task.FromResult(new List<MedicationReminderDto>());
        }

        /// <summary>
        /// Get all conversations for the current user
        /// </summary>
        [Authorize]
        public async Task<List<ConversationDto>> GetConversationsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IMessagingService messagingService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            return await messagingService.GetUserConversationsAsync(userId);
        }

        /// <summary>
        /// Get messages in a specific conversation
        /// </summary>
        [Authorize]
        public async Task<List<MessageDto>> GetConversationMessagesAsync(
            Guid conversationId,
            ClaimsPrincipal claimsPrincipal,
            [Service] IMessagingService messagingService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            return await messagingService.GetConversationMessagesAsync(conversationId, userId);
        }

        /// <summary>
        /// Get unread message count for current user
        /// </summary>
        [Authorize]
        public async Task<int> GetUnreadMessageCountAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IMessagingService messagingService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return 0;

            return await messagingService.GetUnreadCountAsync(userId);
        }

        /// <summary>
        /// Search messages with filters (text search, date range, message type, etc.)
        /// </summary>
        [Authorize]
        public async Task<SearchMessagesResponse> SearchMessagesAsync(
            SearchMessagesInput input,
            ClaimsPrincipal claimsPrincipal,
            [Service] IMessagingService messagingService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            return await messagingService.SearchMessagesAsync(userId, input);
        }

        /// <summary>
        /// Debug endpoint to check appointment request counts
        /// </summary>
        [Authorize]
        public async Task<AppointmentRequestDebugInfo> GetAppointmentRequestDebugInfoAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] ApplicationDbContext context)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;
            
            var totalRequests = await context.AppointmentRequests.CountAsync();
            var pendingRequests = await context.AppointmentRequests.CountAsync(r => r.Status == RequestStatus.Pending);
            var doctorRequests = 0;
            
            if (userRole == "Doctor" && !string.IsNullOrEmpty(userId))
            {
                doctorRequests = await context.AppointmentRequests
                    .CountAsync(r => (r.PreferredDoctorId == userId || r.PreferredDoctorId == null) && r.Status == RequestStatus.Pending);
            }
            
            return new AppointmentRequestDebugInfo
            {
                UserId = userId ?? "Unknown",
                UserRole = userRole ?? "Unknown",
                TotalRequests = totalRequests,
                PendingRequests = pendingRequests,
                DoctorPendingRequests = doctorRequests
            };
        }

        /// <summary>
        /// Debug endpoint to check messaging system status
        /// </summary>
        [Authorize]
        public async Task<MessagingDebugInfo> GetMessagingDebugInfoAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] ApplicationDbContext context)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            var totalMessages = await context.DoctorMessages.CountAsync();
            var userSentMessages = await context.DoctorMessages.CountAsync(m => m.SenderId == userId);
            var userReceivedMessages = await context.DoctorMessages.CountAsync(m => m.ReceiverId == userId);
            var unreadMessages = await context.DoctorMessages.CountAsync(m => m.ReceiverId == userId && m.Status != MessageStatus.Read);

            // Get user's appointments
            var userAppointments = 0;
            if (userRole == "Patient")
            {
                userAppointments = await context.Appointments.CountAsync(a => a.PatientId == userId);
            }
            else if (userRole == "Doctor")
            {
                userAppointments = await context.Appointments.CountAsync(a => a.DoctorId == userId);
            }

            // Get conversation partners
            var sentToUsers = await context.DoctorMessages
                .Where(m => m.SenderId == userId)
                .Select(m => m.ReceiverId)
                .Distinct()
                .CountAsync();

            var receivedFromUsers = await context.DoctorMessages
                .Where(m => m.ReceiverId == userId)
                .Select(m => m.SenderId)
                .Distinct()
                .CountAsync();

            return new MessagingDebugInfo
            {
                UserId = userId,
                UserRole = userRole ?? "Unknown",
                TotalMessages = totalMessages,
                UserSentMessages = userSentMessages,
                UserReceivedMessages = userReceivedMessages,
                UnreadMessages = unreadMessages,
                UserAppointments = userAppointments,
                ConversationPartners = Math.Max(sentToUsers, receivedFromUsers)
            };
        }

        // ============================================
        // CONSULTATION SYSTEM QUERIES
        // ============================================

        /// <summary>
        /// Get consultation session by ID
        /// </summary>
        [Authorize]
        public async Task<ConsultationSessionDto?> GetConsultationSessionAsync(
            int sessionId,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            return await consultationService.GetConsultationSessionByIdAsync(sessionId);
        }

        /// <summary>
        /// Get consultation sessions for a doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<ConsultationSessionDto>> GetDoctorConsultationSessionsAsync(
            DateTime? startDate,
            DateTime? endDate,
            ClaimsPrincipal claimsPrincipal,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await consultationService.GetSessionsByDoctorIdAsync(doctorId, startDate, endDate);
        }

        /// <summary>
        /// Get consultation sessions for a patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<List<ConsultationSessionDto>> GetPatientConsultationSessionsAsync(
            DateTime? startDate,
            DateTime? endDate,
            ClaimsPrincipal claimsPrincipal,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            return await consultationService.GetSessionsByPatientIdAsync(patientId, startDate, endDate);
        }

        /// <summary>
        /// Get active consultation session for current user
        /// </summary>
        [Authorize]
        public async Task<ConsultationSessionDto?> GetActiveConsultationSessionAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService,
            [Service] ApplicationDbContext context)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            // Find active session where user is doctor, patient, or participant
            var activeSession = await context.ConsultationSessions
                .Include(s => s.Participants)
                .Where(s => s.Status == ConsultationStatus.Active &&
                           (s.DoctorId == userId ||
                            s.PatientId == userId ||
                            s.Participants.Any(p => p.UserId == userId && !p.LeftAt.HasValue)))
                .FirstOrDefaultAsync();

            if (activeSession == null)
                return null;

            return await consultationService.GetConsultationSessionByIdAsync(activeSession.Id);
        }

        /// <summary>
        /// Get consultation participants for a session
        /// </summary>
        [Authorize]
        public async Task<List<ConsultationParticipantDto>> GetConsultationParticipantsAsync(
            int sessionId,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            return await consultationService.GetSessionParticipantsAsync(sessionId);
        }

        /// <summary>
        /// Get consultation notes for a session
        /// </summary>
        [Authorize]
        public async Task<List<ConsultationNoteDto>> GetConsultationNotesAsync(
            int sessionId,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            return await consultationService.GetSessionNotesAsync(sessionId);
        }

        /// <summary>
        /// Get consultation recordings for a session
        /// </summary>
        [Authorize]
        public async Task<List<ConsultationRecordingDto>> GetConsultationRecordingsAsync(
            int sessionId,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationRecordingService recordingService)
        {
            return await recordingService.GetRecordingsBySessionAsync(sessionId);
        }

        /// <summary>
        /// Get consultation recording by ID
        /// </summary>
        [Authorize]
        public async Task<ConsultationRecordingDto?> GetConsultationRecordingAsync(
            int recordingId,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationRecordingService recordingService)
        {
            return await recordingService.GetRecordingByIdAsync(recordingId);
        }

        /// <summary>
        /// Get all recordings for a patient
        /// </summary>
        [Authorize(Roles = new[] { "Patient" })]
        public async Task<List<ConsultationRecordingDto>> GetMyConsultationRecordingsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationRecordingService recordingService)
        {
            var patientId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(patientId))
                throw new UnauthorizedAccessException("Patient not authenticated");

            return await recordingService.GetRecordingsByPatientAsync(patientId);
        }

        /// <summary>
        /// Verify participant invitation token
        /// </summary>
        [AllowAnonymous]
        public async Task<ConsultationParticipantDto?> VerifyParticipantTokenAsync(
            string token,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            return await consultationService.VerifyParticipantTokenAsync(token);
        }

        /// <summary>
        /// Get consultation session by room ID
        /// </summary>
        [Authorize]
        public async Task<ConsultationSessionDto?> GetConsultationSessionByRoomIdAsync(
            string roomId,
            [Service] ApplicationDbContext context,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            var session = await context.ConsultationSessions
                .FirstOrDefaultAsync(s => s.RoomId == roomId);

            if (session == null)
                return null;

            return await consultationService.GetConsultationSessionByIdAsync(session.Id);
        }

        /// <summary>
        /// Get active consultation sessions for a specific doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<ConsultationSessionDto>> GetDoctorActiveConsultationsAsync(
            string doctorId,
            [Service] ApplicationDbContext context,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            var activeSessions = await context.ConsultationSessions
                .Include(s => s.Participants)
                .Where(s => s.DoctorId == doctorId &&
                           (s.Status == ConsultationStatus.Active || s.Status == ConsultationStatus.WaitingForDoctor))
                .OrderByDescending(s => s.ActualStartTime ?? s.UpdatedAt)
                .ToListAsync();

            var result = new List<ConsultationSessionDto>();
            foreach (var session in activeSessions)
            {
                var dto = await consultationService.GetConsultationSessionByIdAsync(session.Id);
                if (dto != null)
                    result.Add(dto);
            }
            return result;
        }

        /// <summary>
        /// Get recent completed consultation sessions for a specific doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<ConsultationSessionDto>> GetDoctorRecentConsultationsAsync(
            string doctorId,
            int count,
            [Service] ApplicationDbContext context,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            var completedSessions = await context.ConsultationSessions
                .Where(s => s.DoctorId == doctorId && s.Status == ConsultationStatus.Completed)
                .OrderByDescending(s => s.ActualEndTime ?? s.UpdatedAt)
                .Take(count)
                .ToListAsync();

            var result = new List<ConsultationSessionDto>();
            foreach (var session in completedSessions)
            {
                var dto = await consultationService.GetConsultationSessionByIdAsync(session.Id);
                if (dto != null)
                    result.Add(dto);
            }
            return result;
        }

        /// <summary>
        /// Get active consultation session for a specific appointment
        /// </summary>
        [Authorize]
        public async Task<ConsultationSessionDto?> GetActiveConsultationByAppointmentAsync(
            int appointmentId,
            [Service] ApplicationDbContext context,
            [Service] MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService consultationService)
        {
            var session = await context.ConsultationSessions
                .Where(s => s.AppointmentId == appointmentId &&
                           (s.Status == ConsultationStatus.Active ||
                            s.Status == ConsultationStatus.WaitingForDoctor ||
                            s.Status == ConsultationStatus.Scheduled))
                .FirstOrDefaultAsync();

            if (session == null)
                return null;

            return await consultationService.GetConsultationSessionByIdAsync(session.Id);
        }

        // ============================================
        // PATIENT MANAGEMENT QUERIES (FOR DOCTORS)
        // ============================================

        /// <summary>
        /// Get all patients assigned to the authenticated doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<PatientListDto>> GetDoctorPatientsAsync(
            ClaimsPrincipal claimsPrincipal,
            [Service] IPatientManagementService patientManagementService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await patientManagementService.GetDoctorPatientsAsync(doctorId);
        }

        /// <summary>
        /// Get detailed information about a specific patient
        /// </summary>
        [Authorize(Roles = new[] { "Doctor", "Admin" })]
        public async Task<PatientDetailDto?> GetPatientDetailsAsync(
            string patientId,
            ClaimsPrincipal claimsPrincipal,
            [Service] IPatientManagementService patientManagementService)
        {
            var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated");

            // For doctors, pass their ID to filter data; for admins, pass null to get all data
            var doctorId = userRole == "Doctor" ? userId : null;

            return await patientManagementService.GetPatientDetailsAsync(patientId, doctorId);
        }

        /// <summary>
        /// Search patients by name, email, or phone for the authenticated doctor
        /// </summary>
        [Authorize(Roles = new[] { "Doctor" })]
        public async Task<List<PatientListDto>> SearchDoctorPatientsAsync(
            string searchTerm,
            ClaimsPrincipal claimsPrincipal,
            [Service] IPatientManagementService patientManagementService)
        {
            var doctorId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(doctorId))
                throw new UnauthorizedAccessException("Doctor not authenticated");

            return await patientManagementService.SearchPatientsAsync(doctorId, searchTerm);
        }
    }

    public class AppointmentRequestDebugInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int DoctorPendingRequests { get; set; }
    }

    public class MessagingDebugInfo
    {
        public string UserId { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public int TotalMessages { get; set; }
        public int UserSentMessages { get; set; }
        public int UserReceivedMessages { get; set; }
        public int UnreadMessages { get; set; }
        public int UserAppointments { get; set; }
        public int ConversationPartners { get; set; }
    }
}

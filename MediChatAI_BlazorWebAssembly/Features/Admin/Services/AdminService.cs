using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Admin.Services;

public class AdminService : IAdminService
{
    private readonly IGraphQLService _graphQLService;

    public AdminService(IGraphQLService graphQLService)
    {
        _graphQLService = graphQLService;
    }

    public async Task<UsersResult?> GetUsersAsync(GetUsersInput input)
    {
        var query = @"
            query GetUsers($input: GetUsersInput!) {
                users(input: $input) {
                    users {
                        id
                        email
                        firstName
                        lastName
                        role
                        emailConfirmed
                        createdAt
                        lastLoginAt
                        isActive
                    }
                    totalCount
                    success
                    errors
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<GetUsersResponse>(query, variables);
        return response?.Users;
    }

    public async Task<UserDetailsResult?> GetUserDetailsAsync(string userId)
    {
        var query = @"
            query GetUserDetails($userId: String!) {
                userDetails(userId: $userId) {
                    user {
                        id
                        email
                        firstName
                        lastName
                        role
                        emailConfirmed
                        createdAt
                        lastLoginAt
                        lastProfileUpdate
                        profileImage
                        dateOfBirth
                        gender
                        address
                        city
                        state
                        zipCode
                        country
                        specialization
                        licenseNumber
                        department
                        emergencyContactName
                        emergencyContactPhone
                        bloodType
                        allergies
                        medicalHistory
                        isActive
                    }
                    success
                    errors
                }
            }";

        var variables = new { userId };
        var response = await _graphQLService.SendQueryAsync<GetUserDetailsResponse>(query, variables);
        return response?.UserDetails;
    }

    public async Task<AdminOperationResult?> UpdateUserStatusAsync(UpdateUserStatusInput input)
    {
        var mutation = @"
            mutation UpdateUserStatus($input: UpdateUserStatusInput!) {
                updateUserStatus(input: $input) {
                    success
                    errors
                    message
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<UpdateUserStatusResponse>(mutation, variables);
        return response?.UpdateUserStatus;
    }

    public async Task<AdminOperationResult?> UpdateUserRoleAsync(UpdateUserRoleInput input)
    {
        var mutation = @"
            mutation UpdateUserRole($input: UpdateUserRoleInput!) {
                updateUserRole(input: $input) {
                    success
                    errors
                    message
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<UpdateUserRoleResponse>(mutation, variables);
        return response?.UpdateUserRole;
    }

    public async Task<AdminOperationResult?> DeleteUserAsync(string userId)
    {
        var mutation = @"
            mutation DeleteUser($userId: String!) {
                deleteUser(userId: $userId) {
                    success
                    errors
                    message
                }
            }";

        var variables = new { userId };
        var response = await _graphQLService.SendQueryAsync<DeleteUserResponse>(mutation, variables);
        return response?.DeleteUser;
    }

    public async Task<UserActivitiesResult?> GetUserActivitiesAsync(GetUserActivitiesInput input)
    {
        var query = @"
            query GetUserActivities($input: GetUserActivitiesInput!) {
                userActivities(input: $input) {
                    activities {
                        id
                        userId
                        userEmail
                        userName
                        userRole
                        activityType
                        description
                        details
                        ipAddress
                        timestamp
                        additionalData
                    }
                    totalCount
                    success
                    errors
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<GetUserActivitiesResponse>(query, variables);
        return response?.UserActivities;
    }

    public async Task<AdminStatsDto?> GetAdminStatsAsync()
    {
        var query = @"
            query GetAdminStats {
                adminStats {
                    totalUsers
                    totalPatients
                    totalDoctors
                    totalAdmins
                    activeUsersToday
                    newUsersThisWeek
                    totalActivities
                    lastSystemBackup
                }
            }";

        var response = await _graphQLService.SendQueryAsync<GetAdminStatsResponse>(query);
        return response?.AdminStats;
    }

    public async Task<AdminOperationResult?> SendSystemNotificationAsync(string title, string message, string? targetRole = null)
    {
        var mutation = @"
            mutation SendSystemNotification($title: String!, $message: String!, $targetRole: String) {
                sendSystemNotification(title: $title, message: $message, targetRole: $targetRole) {
                    success
                    errors
                    message
                }
            }";

        var variables = new { title, message, targetRole };
        var response = await _graphQLService.SendQueryAsync<dynamic>(mutation, variables);

        if (response != null)
        {
            var element = ((JsonElement)response).GetProperty("sendSystemNotification");
            return JsonSerializer.Deserialize<AdminOperationResult>(element.GetRawText(),
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }

        return null;
    }

    public async Task<AdminOperationResult?> ExportUserDataAsync(string? userId = null)
    {
        var mutation = @"
            mutation ExportUserData($userId: String) {
                exportUserData(userId: $userId) {
                    success
                    errors
                    message
                }
            }";

        var variables = new { userId = userId ?? (object?)null };

        try
        {
            var response = await _graphQLService.SendQueryAsync<ExportUserDataResponse>(mutation, variables);
            return response?.ExportUserData;
        }
        catch (Exception)
        {
            // Fallback to dynamic parsing if typed response fails
            var response = await _graphQLService.SendQueryAsync<dynamic>(mutation, variables);

            if (response != null)
            {
                var element = ((JsonElement)response).GetProperty("exportUserData");
                return JsonSerializer.Deserialize<AdminOperationResult>(element.GetRawText(),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }

            return null;
        }
    }
}
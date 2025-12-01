using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

namespace MediChatAI_BlazorWebAssembly.Features.Profile.Services;

public interface IUserProfileService
{
    Task<UserProfile?> GetUserProfileAsync();
    Task<ProfileResponse> UpdateProfileAsync(UpdateProfileRequest request);
}

public class UserProfileService : IUserProfileService
{
    private readonly IGraphQLService _graphQLService;

    public UserProfileService(IGraphQLService graphQLService)
    {
        _graphQLService = graphQLService;
    }

    public async Task<UserProfile?> GetUserProfileAsync()
    {
        var query = """
            query GetUserProfile {
              userProfile {
                id
                email
                firstName
                lastName
                role
                emailConfirmed
                twoFactorEnabled
                profileImage
                dateOfBirth
                gender
                address
                city
                state
                zipCode
                country
                phoneNumber
                specialization
                licenseNumber
                department
                emergencyContactName
                emergencyContactPhone
                bloodType
                allergies
                medicalHistory
                createdAt
                lastLoginAt
                lastProfileUpdate
              }
            }
            """;

        var response = await _graphQLService.SendQueryAsync<GetUserProfileResponse>(query);
        return response?.UserProfile;
    }

    public async Task<ProfileResponse> UpdateProfileAsync(UpdateProfileRequest request)
    {
        var query = """
            mutation UpdateProfile($input: UpdateProfileInput!) {
              updateProfile(input: $input) {
                success
                profile {
                  id
                  email
                  firstName
                  lastName
                  role
                  emailConfirmed
                  profileImage
                  dateOfBirth
                  gender
                  address
                  city
                  state
                  zipCode
                  country
                  phoneNumber
                  specialization
                  licenseNumber
                  department
                  emergencyContactName
                  emergencyContactPhone
                  bloodType
                  allergies
                  medicalHistory
                  createdAt
                  lastLoginAt
                  lastProfileUpdate
                }
                errors
              }
            }
            """;

        var variables = new { input = request };
        var response = await _graphQLService.SendQueryAsync<UpdateProfileResponse>(query, variables);

        return response?.UpdateProfile ?? new ProfileResponse(false, null, new[] { "Network error occurred" });
    }
}
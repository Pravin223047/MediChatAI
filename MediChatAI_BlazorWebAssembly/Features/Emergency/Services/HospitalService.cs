using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Services;

public class HospitalService : IHospitalService
{
    private readonly IGraphQLService _graphQLService;

    public HospitalService(IGraphQLService graphQLService)
    {
        _graphQLService = graphQLService;
    }

    public async Task<HospitalSearchResultDto?> SearchNearbyHospitalsAsync(HospitalSearchInputDto input)
    {
        var query = @"
            query SearchNearbyHospitals($input: HospitalSearchInput!) {
                searchNearbyHospitals(input: $input) {
                    hospitals {
                        name
                        rating
                        phoneNumber
                        fullAddress
                        latitude
                        longitude
                        website
                        photo
                        photos
                        openHours
                        type
                        reviews
                        distanceInKm
                        isOpen
                    }
                    totalCount
                    userLatitude
                    userLongitude
                    errorMessage
                    success
                }
            }";

        var variables = new { input };
        var response = await _graphQLService.SendQueryAsync<SearchNearbyHospitalsResponse>(query, variables);
        return response?.SearchNearbyHospitals;
    }

    // Response wrapper class
    private class SearchNearbyHospitalsResponse
    {
        public HospitalSearchResultDto? SearchNearbyHospitals { get; set; }
    }
}

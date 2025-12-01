using MediChatAI_GraphQl.Features.Emergency.DTOs;

namespace MediChatAI_GraphQl.Core.Interfaces.Services.Emergency;

public interface IHospitalLocationService
{
    Task<HospitalSearchResult> SearchNearbyHospitalsAsync(HospitalSearchInput input);
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
}

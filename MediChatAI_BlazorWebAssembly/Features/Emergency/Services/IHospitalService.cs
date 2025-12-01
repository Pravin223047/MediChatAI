using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Services;

public interface IHospitalService
{
    /// <summary>
    /// Search for nearby hospitals based on user's location
    /// </summary>
    Task<HospitalSearchResultDto?> SearchNearbyHospitalsAsync(HospitalSearchInputDto input);
}

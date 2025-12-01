namespace MediChatAI_GraphQl.Features.Emergency.Services;

public class GeoapifySettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.geoapify.com/v1";
    public int MaxConcurrentRequests { get; set; } = 5;
    public int TimeoutSeconds { get; set; } = 10;
}

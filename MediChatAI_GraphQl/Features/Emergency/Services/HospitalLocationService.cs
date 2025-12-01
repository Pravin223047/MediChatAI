using System.Text.Json;
using MediChatAI_GraphQl.Core.Interfaces.Services.Emergency;
using MediChatAI_GraphQl.Features.Emergency.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MediChatAI_GraphQl.Features.Emergency.Services;

public class HospitalLocationService : IHospitalLocationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HospitalLocationService> _logger;

    public HospitalLocationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<HospitalLocationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HospitalSearchResult> SearchNearbyHospitalsAsync(HospitalSearchInput input)
    {
        var result = new HospitalSearchResult
        {
            UserLatitude = input.Latitude,
            UserLongitude = input.Longitude,
            Success = false
        };

        try
        {
            // Get API key from configuration
            var apiKey = _configuration["RapidAPI:MapsData:ApiKey"];
            var apiHost = _configuration["RapidAPI:MapsData:Host"] ?? "maps-data.p.rapidapi.com";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("RapidAPI key is not configured");
                result.ErrorMessage = "Hospital search service is not configured";
                return result;
            }

            // Build the API URL
            var zoom = input.Radius switch
            {
                <= 1000 => 14,
                <= 5000 => 12,
                <= 10000 => 11,
                _ => 10
            };

            var url = $"https://{apiHost}/nearby.php?query=Hospital&lat={input.Latitude}&lng={input.Longitude}&limit={input.Limit}&country={input.Country}&lang={input.Language}&offset=0&zoom={zoom}";

            // Configure request headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", apiHost);

            _logger.LogInformation("Fetching hospitals near {Latitude}, {Longitude}", input.Latitude, input.Longitude);

            // Make the API request
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            // Parse the response
            var apiResponse = JsonSerializer.Deserialize<RapidApiHospitalResponse>(responseBody);

            if (apiResponse?.Data == null || apiResponse.Data.Count == 0)
            {
                _logger.LogWarning("No hospitals found near {Latitude}, {Longitude}", input.Latitude, input.Longitude);
                result.ErrorMessage = "No hospitals found in your area";
                result.Success = true;
                return result;
            }

            // Filter and map hospitals
            var hospitals = new List<Hospital>();

            foreach (var data in apiResponse.Data)
            {
                // Filter: Only include hospitals with rating and phone number
                if (data.Rating.HasValue && !string.IsNullOrEmpty(data.PhoneNumber))
                {
                    var hospital = new Hospital
                    {
                        Name = data.Name ?? "Unnamed Hospital",
                        Rating = data.Rating,
                        PhoneNumber = data.PhoneNumber,
                        FullAddress = data.FullAddress,
                        Latitude = data.Latitude,
                        Longitude = data.Longitude,
                        Website = data.Website,
                        Photo = data.Photo,
                        Photos = data.Photos?
                            .Where(p => !string.IsNullOrEmpty(p.Photo))
                            .Select(p => p.Photo!)
                            .ToList() ?? new List<string>(),
                        OpenHours = data.OpenHours,
                        Type = data.Type,
                        Reviews = data.Reviews
                    };

                    // Calculate distance if coordinates are available
                    if (data.Latitude.HasValue && data.Longitude.HasValue)
                    {
                        hospital.DistanceInKm = CalculateDistance(
                            input.Latitude,
                            input.Longitude,
                            data.Latitude.Value,
                            data.Longitude.Value
                        );
                    }

                    // Determine if hospital is open (simple heuristic)
                    hospital.IsOpen = IsHospitalOpen(data.OpenHours);

                    hospitals.Add(hospital);
                }
            }

            // Sort by distance
            hospitals = hospitals.OrderBy(h => h.DistanceInKm ?? double.MaxValue).ToList();

            result.Hospitals = hospitals;
            result.TotalCount = hospitals.Count;
            result.Success = true;

            _logger.LogInformation("Successfully fetched {Count} hospitals", hospitals.Count);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while fetching hospitals");
            result.ErrorMessage = "Failed to connect to hospital search service";
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching hospitals");
            result.ErrorMessage = "An unexpected error occurred while searching for hospitals";
            return result;
        }
    }

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula to calculate distance between two points on Earth
        const double earthRadiusKm = 6371.0;

        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    private bool IsHospitalOpen(string? openHours)
    {
        if (string.IsNullOrEmpty(openHours))
        {
            // Assume 24/7 if no hours specified (common for hospitals)
            return true;
        }

        // Check for 24/7 or "Open 24 hours" patterns
        if (openHours.Contains("24", StringComparison.OrdinalIgnoreCase) ||
            openHours.Contains("24/7", StringComparison.OrdinalIgnoreCase) ||
            openHours.Contains("Open 24 hours", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // For more complex parsing, you would need to parse the actual hours
        // For now, assume open if we can't determine
        return true;
    }
}

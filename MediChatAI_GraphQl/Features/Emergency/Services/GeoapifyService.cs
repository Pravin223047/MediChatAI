using System.Text.Json;
using Microsoft.Extensions.Options;
using MediChatAI_GraphQl.Features.Emergency.DTOs;

namespace MediChatAI_GraphQl.Features.Emergency.Services;

public interface IGeoapifyService
{
    Task<GeoapifyRouteResponse> GetRouteAsync(GeoapifyRouteRequest request);
    Task<GeoapifyIsochroneResponse> GetIsochroneAsync(GeoapifyIsochroneRequest request);
    Task<GeoapifyMatrixResponse> GetDistanceMatrixAsync(GeoapifyMatrixRequest request);
    string GetStaticMapUrl(double originLat, double originLng, double destLat, double destLng, int width = 800, int height = 400);
}

public class GeoapifyService : IGeoapifyService
{
    private readonly HttpClient _httpClient;
    private readonly GeoapifySettings _settings;
    private readonly ILogger<GeoapifyService> _logger;
    private readonly SemaphoreSlim _rateLimiter;

    public GeoapifyService(
        HttpClient httpClient,
        IOptions<GeoapifySettings> settings,
        ILogger<GeoapifyService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _rateLimiter = new SemaphoreSlim(_settings.MaxConcurrentRequests, _settings.MaxConcurrentRequests);

        // Ensure BaseUrl has trailing slash for correct relative URL resolution
        var baseUrl = _settings.BaseUrl.EndsWith("/") ? _settings.BaseUrl : _settings.BaseUrl + "/";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<GeoapifyRouteResponse> GetRouteAsync(GeoapifyRouteRequest request)
    {
        await _rateLimiter.WaitAsync();
        try
        {
            var waypoints = $"{request.OriginLat},{request.OriginLng}|{request.DestLat},{request.DestLng}";
            var url = $"routing?waypoints={waypoints}&mode={request.Mode}&apiKey={_settings.ApiKey}";

            _logger.LogInformation("Geoapify: Fetching route from {Origin} to {Dest}",
                $"{request.OriginLat},{request.OriginLng}", $"{request.DestLat},{request.DestLng}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Geoapify API error: {StatusCode} - {Content}", response.StatusCode, errorContent);

                return new GeoapifyRouteResponse
                {
                    Success = false,
                    ErrorMessage = $"API request failed: {response.StatusCode}"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            if (jsonDoc.RootElement.TryGetProperty("features", out var features) && features.GetArrayLength() > 0)
            {
                var feature = features[0];
                var properties = feature.GetProperty("properties");

                var distanceMeters = properties.GetProperty("distance").GetDouble();
                var timeSeconds = properties.GetProperty("time").GetDouble();

                // Extract turn-by-turn steps if available
                var steps = new List<RouteStep>();
                if (properties.TryGetProperty("legs", out var legs) && legs.GetArrayLength() > 0)
                {
                    var firstLeg = legs[0];
                    if (firstLeg.TryGetProperty("steps", out var stepsArray))
                    {
                        foreach (var step in stepsArray.EnumerateArray())
                        {
                            steps.Add(new RouteStep
                            {
                                Instruction = step.TryGetProperty("instruction", out var inst)
                                    ? inst.TryGetProperty("text", out var text)
                                        ? text.GetString() ?? "Continue"
                                        : "Continue"
                                    : "Continue",
                                DistanceMeters = step.GetProperty("distance").GetDouble(),
                                TimeSeconds = (int)step.GetProperty("time").GetDouble()
                            });
                        }
                    }
                }

                return new GeoapifyRouteResponse
                {
                    Success = true,
                    DistanceKm = Math.Round(distanceMeters / 1000, 2),
                    TimeMinutes = (int)Math.Ceiling(timeSeconds / 60),
                    Steps = steps,
                    StaticMapUrl = GetStaticMapUrl(request.OriginLat, request.OriginLng, request.DestLat, request.DestLng)
                };
            }

            return new GeoapifyRouteResponse
            {
                Success = false,
                ErrorMessage = "No route found"
            };
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Geoapify route request timed out");
            return new GeoapifyRouteResponse
            {
                Success = false,
                ErrorMessage = "Request timed out. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching route from Geoapify");
            return new GeoapifyRouteResponse
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Message}"
            };
        }
        finally
        {
            _rateLimiter.Release();
        }
    }

    public async Task<GeoapifyIsochroneResponse> GetIsochroneAsync(GeoapifyIsochroneRequest request)
    {
        await _rateLimiter.WaitAsync();
        try
        {
            var url = $"isoline?lat={request.Latitude}&lon={request.Longitude}" +
                     $"&type=time&mode={request.Mode}&range={request.RangeSeconds}&apiKey={_settings.ApiKey}";

            _logger.LogInformation("Geoapify: Fetching isochrone for {Lat},{Lng} with range {Range}s",
                request.Latitude, request.Longitude, request.RangeSeconds);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Geoapify isochrone error: {StatusCode} - {Content}", response.StatusCode, errorContent);

                return new GeoapifyIsochroneResponse
                {
                    Success = false,
                    ErrorMessage = $"API request failed: {response.StatusCode}"
                };
            }

            var content = await response.Content.ReadAsStringAsync();

            return new GeoapifyIsochroneResponse
            {
                Success = true,
                GeoJsonPolygon = content
            };
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Geoapify isochrone request timed out");
            return new GeoapifyIsochroneResponse
            {
                Success = false,
                ErrorMessage = "Request timed out. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching isochrone from Geoapify");
            return new GeoapifyIsochroneResponse
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Message}"
            };
        }
        finally
        {
            _rateLimiter.Release();
        }
    }

    public async Task<GeoapifyMatrixResponse> GetDistanceMatrixAsync(GeoapifyMatrixRequest request)
    {
        await _rateLimiter.WaitAsync();
        try
        {
            // Build sources (origin) and targets (destinations)
            var sources = $"{request.OriginLat},{request.OriginLng}";
            var targets = string.Join("|", request.Destinations.Select(d => $"{d.Latitude},{d.Longitude}"));

            var url = $"routematrix?sources={sources}&targets={targets}&mode=drive&apiKey={_settings.ApiKey}";

            _logger.LogInformation("Geoapify: Fetching distance matrix from {Origin} to {Count} destinations",
                sources, request.Destinations.Count);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Geoapify matrix error: {StatusCode} - {Content}", response.StatusCode, errorContent);

                return new GeoapifyMatrixResponse
                {
                    Success = false,
                    ErrorMessage = $"API request failed: {response.StatusCode}"
                };
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);

            var results = new List<DistanceResult>();

            if (jsonDoc.RootElement.TryGetProperty("sources_to_targets", out var matrix))
            {
                var firstRow = matrix[0]; // We only have one source
                for (int i = 0; i < firstRow.GetArrayLength(); i++)
                {
                    var cell = firstRow[i];
                    var distance = cell.GetProperty("distance").GetDouble();
                    var time = cell.GetProperty("time").GetDouble();

                    results.Add(new DistanceResult
                    {
                        DestinationIndex = i,
                        DistanceKm = Math.Round(distance / 1000, 2),
                        TimeMinutes = (int)Math.Ceiling(time / 60)
                    });
                }
            }

            return new GeoapifyMatrixResponse
            {
                Success = true,
                Results = results
            };
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Geoapify matrix request timed out");
            return new GeoapifyMatrixResponse
            {
                Success = false,
                ErrorMessage = "Request timed out. Please try again."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching distance matrix from Geoapify");
            return new GeoapifyMatrixResponse
            {
                Success = false,
                ErrorMessage = $"Error: {ex.Message}"
            };
        }
        finally
        {
            _rateLimiter.Release();
        }
    }

    public string GetStaticMapUrl(double originLat, double originLng, double destLat, double destLng, int width = 800, int height = 400)
    {
        // Calculate center point and zoom level
        var centerLat = (originLat + destLat) / 2;
        var centerLng = (originLng + destLng) / 2;

        // Create markers for origin (green) and destination (red)
        var originMarker = $"lonlat:{originLng},{originLat};color:%2300ff00;size:medium";
        var destMarker = $"lonlat:{destLng},{destLat};color:%23ff0000;size:medium";

        // Build static map URL
        var mapUrl = $"https://maps.geoapify.com/v1/staticmap?" +
                    $"style=osm-bright-smooth&" +
                    $"width={width}&height={height}&" +
                    $"center=lonlat:{centerLng},{centerLat}&" +
                    $"zoom=12&" +
                    $"marker={originMarker}&" +
                    $"marker={destMarker}&" +
                    $"apiKey={_settings.ApiKey}";

        return mapUrl;
    }
}

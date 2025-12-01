using System.Text.Json.Serialization;

namespace MediChatAI_GraphQl.Features.Emergency.DTOs;

public class Hospital
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("full_address")]
    public string? FullAddress { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("photo")]
    public string? Photo { get; set; }

    [JsonPropertyName("photos")]
    public List<string>? Photos { get; set; }

    [JsonPropertyName("open_hours")]
    public string? OpenHours { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("reviews")]
    public int? Reviews { get; set; }

    // Calculated property
    public double? DistanceInKm { get; set; }

    // Derived property
    public bool IsOpen { get; set; }
}

public class HospitalSearchResult
{
    public List<Hospital> Hospitals { get; set; } = new();
    public int TotalCount { get; set; }
    public double? UserLatitude { get; set; }
    public double? UserLongitude { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Success { get; set; }
}

public class HospitalSearchInput
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Limit { get; set; } = 20;
    public int Radius { get; set; } = 5000; // meters
    public string Language { get; set; } = "en";
    public string Country { get; set; } = "in";
}

public class RapidApiHospitalResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("data")]
    public List<RapidApiHospitalData>? Data { get; set; }
}

public class RapidApiHospitalData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("full_address")]
    public string? FullAddress { get; set; }

    [JsonPropertyName("latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("photo")]
    public string? Photo { get; set; }

    [JsonPropertyName("photos")]
    public List<PhotoObject>? Photos { get; set; }

    [JsonPropertyName("open_hours")]
    public string? OpenHours { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("reviews")]
    public int? Reviews { get; set; }
}

public class PhotoObject
{
    [JsonPropertyName("photo")]
    public string? Photo { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }
}

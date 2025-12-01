namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Models;

public class HospitalDto
{
    public string Name { get; set; } = string.Empty;
    public double? Rating { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FullAddress { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Website { get; set; }
    public string? Photo { get; set; }
    public List<string>? Photos { get; set; }
    public string? OpenHours { get; set; }
    public string? Type { get; set; }
    public int? Reviews { get; set; }
    public double? DistanceInKm { get; set; }
    public bool IsOpen { get; set; }
}

public class HospitalSearchResultDto
{
    public List<HospitalDto> Hospitals { get; set; } = new();
    public int TotalCount { get; set; }
    public double? UserLatitude { get; set; }
    public double? UserLongitude { get; set; }
    public string? ErrorMessage { get; set; }
    public bool Success { get; set; }
}

public class HospitalSearchInputDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Limit { get; set; } = 20;
    public int Radius { get; set; } = 5000;
    public string Language { get; set; } = "en";
    public string Country { get; set; } = "in";
}

public class UserLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Accuracy { get; set; }
}

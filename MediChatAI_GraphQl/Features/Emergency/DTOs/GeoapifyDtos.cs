namespace MediChatAI_GraphQl.Features.Emergency.DTOs;

public class GeoapifyRouteRequest
{
    public double OriginLat { get; set; }
    public double OriginLng { get; set; }
    public double DestLat { get; set; }
    public double DestLng { get; set; }
    public string Mode { get; set; } = "drive"; // drive, truck, bicycle, walk
}

public class GeoapifyRouteResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public double? DistanceKm { get; set; }
    public int? TimeMinutes { get; set; }
    public string? StaticMapUrl { get; set; }
    public List<RouteStep>? Steps { get; set; }
}

public class RouteStep
{
    public string Instruction { get; set; } = string.Empty;
    public double DistanceMeters { get; set; }
    public int TimeSeconds { get; set; }
}

public class GeoapifyIsochroneRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int RangeSeconds { get; set; } // e.g., 300 for 5 minutes, 600 for 10 minutes
    public string Mode { get; set; } = "drive";
}

public class GeoapifyIsochroneResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? GeoJsonPolygon { get; set; } // JSON string of polygon coordinates
}

public class GeoapifyMatrixRequest
{
    public double OriginLat { get; set; }
    public double OriginLng { get; set; }
    public List<Location> Destinations { get; set; } = new();
}

public class Location
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Name { get; set; }
}

public class GeoapifyMatrixResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<DistanceResult>? Results { get; set; }
}

public class DistanceResult
{
    public int DestinationIndex { get; set; }
    public double? DistanceKm { get; set; }
    public int? TimeMinutes { get; set; }
}

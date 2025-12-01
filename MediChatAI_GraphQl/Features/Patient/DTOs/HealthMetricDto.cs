namespace MediChatAI_GraphQl.Features.Patient.DTOs;

public class HealthMetricDto
{
    public Guid Id { get; set; }
    public string PatientId { get; set; } = "";
    public string MetricType { get; set; } = ""; // HeartRate, BloodPressure, Temperature, Weight, BloodGlucose, OxygenSaturation
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public int? SystolicValue { get; set; } // For Blood Pressure
    public int? DiastolicValue { get; set; } // For Blood Pressure
    public DateTime RecordedDate { get; set; }
    public string? Notes { get; set; }
    public string? RecordedBy { get; set; } // PatientId or DoctorId
    public string Status { get; set; } = "Normal"; // Low, Normal, High, Critical
}

public class RecordHealthMetricInput
{
    public string MetricType { get; set; } = "";
    public double Value { get; set; }
    public string Unit { get; set; } = "";
    public int? SystolicValue { get; set; }
    public int? DiastolicValue { get; set; }
    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

public class GetHealthMetricsInput
{
    public string? MetricType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Limit { get; set; } = 100;
}

public class HealthMetricsResponse
{
    public List<HealthMetricDto> Metrics { get; set; } = new();
    public int TotalCount { get; set; }
    public HealthMetricsSummary Summary { get; set; } = new();
}

public class HealthMetricsSummary
{
    public Dictionary<string, MetricStatistics> StatsByType { get; set; } = new();
}

public class MetricStatistics
{
    public double Average { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public double Latest { get; set; }
    public DateTime LatestRecordedDate { get; set; }
    public int TotalRecords { get; set; }
    public string Trend { get; set; } = "Stable"; // Increasing, Decreasing, Stable
}

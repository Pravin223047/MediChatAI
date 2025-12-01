using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public class HealthMetricsService : IHealthMetricsService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<HealthMetricsService> _logger;

    public HealthMetricsService(IGraphQLService graphQLService, ILogger<HealthMetricsService> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
    }

    public async Task<List<HealthMetricDto>> GetHealthMetricsAsync(GetHealthMetricsInput input)
    {
        try
        {
            const string query = @"
                query GetPatientHealthMetrics($input: GetHealthMetricsInput!) {
                  patientHealthMetrics(input: $input) {
                    metrics {
                      id
                      patientId
                      metricType
                      value
                      unit
                      systolicValue
                      diastolicValue
                      recordedDate
                      notes
                      recordedBy
                      status
                    }
                    totalCount
                  }
                }
                ";

            var variables = new { input };
            var response = await _graphQLService.SendQueryAsync<GetPatientHealthMetricsResponse>(query, variables);

            if (response?.PatientHealthMetrics?.Metrics != null)
            {
                _logger.LogInformation("Successfully loaded {Count} health metrics", response.PatientHealthMetrics.Metrics.Count);
                return response.PatientHealthMetrics.Metrics;
            }

            _logger.LogWarning("No health metrics received from GraphQL");
            return new List<HealthMetricDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading health metrics from GraphQL");
            throw;
        }
    }

    public async Task<HealthMetricDto?> RecordHealthMetricAsync(RecordHealthMetricInput input)
    {
        try
        {
            const string mutation = @"
                mutation RecordHealthMetric($input: RecordHealthMetricInput!) {
                  recordHealthMetric(input: $input) {
                    id
                    patientId
                    metricType
                    value
                    unit
                    systolicValue
                    diastolicValue
                    recordedDate
                    notes
                    recordedBy
                    status
                  }
                }
                ";

            var variables = new { input };
            var response = await _graphQLService.SendQueryAsync<RecordHealthMetricResponse>(mutation, variables);

            if (response?.RecordHealthMetric != null)
            {
                _logger.LogInformation("Successfully recorded health metric: {MetricType}", input.MetricType);
                return response.RecordHealthMetric;
            }

            _logger.LogWarning("Failed to record health metric");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording health metric");
            throw;
        }
    }
}

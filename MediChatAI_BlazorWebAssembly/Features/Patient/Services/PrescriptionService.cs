using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(IGraphQLService graphQLService, ILogger<PrescriptionService> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
    }

    public async Task<List<PrescriptionDto>> GetMyPrescriptionsAsync()
    {
        try
        {
            const string query = """
                query GetMyPrescriptions {
                  myPrescriptions {
                    id
                    patientId
                    patientName
                    doctorId
                    doctorName
                    prescribedDate
                    startDate
                    endDate
                    status
                    refillsAllowed
                    refillsUsed
                    diagnosis
                    doctorNotes
                    items {
                      id
                      medicationName
                      genericName
                      dosage
                      frequency
                      route
                      durationDays
                      quantity
                      form
                      instructions
                      warnings
                      sideEffects
                    }
                  }
                }
                """;

            var response = await _graphQLService.SendQueryAsync<PrescriptionsResponse>(query);

            if (response?.MyPrescriptions != null)
            {
                _logger.LogInformation("Loaded {Count} prescriptions from GraphQL", response.MyPrescriptions.Count);
                return response.MyPrescriptions;
            }

            _logger.LogWarning("No prescriptions data received from GraphQL");
            return new List<PrescriptionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading prescriptions from GraphQL");
            throw;
        }
    }

    public async Task<List<PrescriptionDto>> GetMyActivePrescriptionsAsync()
    {
        try
        {
            const string query = """
                query GetMyActivePrescriptions {
                  myActivePrescriptions {
                    id
                    patientId
                    patientName
                    doctorId
                    doctorName
                    prescribedDate
                    startDate
                    endDate
                    status
                    refillsAllowed
                    refillsUsed
                    diagnosis
                    doctorNotes
                    items {
                      id
                      medicationName
                      genericName
                      dosage
                      frequency
                      route
                      durationDays
                      quantity
                      form
                      instructions
                      warnings
                      sideEffects
                    }
                  }
                }
                """;

            var response = await _graphQLService.SendQueryAsync<ActivePrescriptionsResponse>(query);

            if (response?.MyActivePrescriptions != null)
            {
                _logger.LogInformation("Loaded {Count} active prescriptions from GraphQL", response.MyActivePrescriptions.Count);
                return response.MyActivePrescriptions;
            }

            _logger.LogWarning("No active prescriptions data received from GraphQL");
            return new List<PrescriptionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading active prescriptions from GraphQL");
            throw;
        }
    }

    public async Task<bool> RequestRefillAsync(int prescriptionId)
    {
        try
        {
            const string mutation = """
                mutation RequestPrescriptionRefill($prescriptionId: Int!) {
                  requestPrescriptionRefill(prescriptionId: $prescriptionId)
                }
                """;

            var variables = new
            {
                prescriptionId
            };

            var response = await _graphQLService.SendQueryAsync<RefillResponse>(mutation, variables);

            if (response?.RequestPrescriptionRefill == true)
            {
                _logger.LogInformation("Successfully requested refill for prescription {PrescriptionId}", prescriptionId);
                return true;
            }

            _logger.LogWarning("Failed to request refill for prescription {PrescriptionId}", prescriptionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting refill for prescription {PrescriptionId}", prescriptionId);
            throw;
        }
    }

    private class PrescriptionsResponse
    {
        public List<PrescriptionDto> MyPrescriptions { get; set; } = new();
    }

    private class ActivePrescriptionsResponse
    {
        public List<PrescriptionDto> MyActivePrescriptions { get; set; } = new();
    }

    private class RefillResponse
    {
        public bool RequestPrescriptionRefill { get; set; }
    }
}

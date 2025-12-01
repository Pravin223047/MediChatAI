using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public class ConsultationService : IConsultationService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<ConsultationService> _logger;

    public ConsultationService(IGraphQLService graphQLService, ILogger<ConsultationService> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
    }

    public async Task<List<ConsultationHistoryDto>> GetPatientConsultationsAsync(GetConsultationsInput? input = null)
    {
        try
        {
            input ??= new GetConsultationsInput();

            const string query = """
                query GetPatientConsultations($input: GetConsultationsInput!) {
                  getPatientConsultations(input: $input) {
                    id
                    doctorId
                    doctorName
                    specialization
                    doctorProfileImage
                    consultationDate
                    chiefComplaint
                    diagnosis
                    observations
                    treatmentPlan
                    prescriptions {
                      id
                      medicationName
                      dosage
                      frequency
                      duration
                    }
                    followUpInstructions
                    nextFollowUpDate
                    isRated
                    rating
                    patientFeedback
                    consultationType
                  }
                }
                """;

            var variables = new { input };
            var response = await _graphQLService.SendQueryAsync<GetPatientConsultationsResponse>(query, variables);

            if (response?.GetPatientConsultations != null)
            {
                _logger.LogInformation("Loaded {Count} consultations from GraphQL", response.GetPatientConsultations.Count);
                return response.GetPatientConsultations;
            }

            _logger.LogWarning("No consultation data received from GraphQL");
            return new List<ConsultationHistoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading consultations from GraphQL");
            throw;
        }
    }

    public async Task<bool> RateConsultationAsync(int consultationId, int rating, string? feedback = null)
    {
        try
        {
            const string mutation = """
                mutation RateConsultation($input: RateConsultationInputInput!) {
                  rateConsultation(input: $input)
                }
                """;

            var input = new RateConsultationInput
            {
                ConsultationId = consultationId,
                Rating = rating,
                Feedback = feedback
            };

            var variables = new { input };
            var response = await _graphQLService.SendQueryAsync<RateConsultationResponse>(mutation, variables);

            if (response?.RateConsultation == true)
            {
                _logger.LogInformation("Successfully rated consultation {Id} with {Rating} stars", consultationId, rating);
                return true;
            }

            _logger.LogWarning("Failed to rate consultation {Id}", consultationId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating consultation {Id}", consultationId);
            throw;
        }
    }
}

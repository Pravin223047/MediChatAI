using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(IGraphQLService graphQLService, ILogger<AppointmentService> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
    }

    public async Task<List<AppointmentDto>> GetMyAppointmentsAsync()
    {
        try
        {
            const string query = """
                query GetMyAppointments {
                  myAppointments {
                    id
                    patientId
                    patientName
                    patientEmail
                    patientPhone
                    doctorId
                    doctorName
                    doctorSpecialization
                    appointmentDateTime
                    durationMinutes
                    status
                    type
                    roomNumber
                    location
                    isVirtual
                    meetingLink
                    reasonForVisit
                    doctorNotes
                    patientNotes
                    consultationFee
                    isPaid
                    createdAt
                  }
                }
                """;

            var response = await _graphQLService.SendQueryAsync<AppointmentsResponse>(query);

            if (response?.MyAppointments != null)
            {
                _logger.LogInformation("Loaded {Count} appointments from GraphQL", response.MyAppointments.Count);
                return response.MyAppointments;
            }

            _logger.LogWarning("No appointments data received from GraphQL");
            return new List<AppointmentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointments from GraphQL");
            throw;
        }
    }

    public async Task<bool> CancelAppointmentAsync(int appointmentId, string reason)
    {
        try
        {
            const string mutation = """
                mutation CancelAppointment($appointmentId: Int!, $reason: String!) {
                  cancelAppointment(appointmentId: $appointmentId, reason: $reason)
                }
                """;

            var variables = new
            {
                appointmentId,
                reason
            };

            var response = await _graphQLService.SendQueryAsync<CancelAppointmentResponse>(mutation, variables);

            if (response?.CancelAppointment == true)
            {
                _logger.LogInformation("Successfully cancelled appointment {AppointmentId}", appointmentId);
                return true;
            }

            _logger.LogWarning("Failed to cancel appointment {AppointmentId}", appointmentId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment {AppointmentId}", appointmentId);
            throw;
        }
    }
}

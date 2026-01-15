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
                
                // Debug: Log each appointment's status
                foreach (var apt in response.MyAppointments)
                {
                    _logger.LogInformation("Appointment ID: {Id}, Status: {Status}, DateTime: {DateTime}", 
                        apt.Id, apt.Status, apt.AppointmentDateTime);
                }
                
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

    public async Task<List<AppointmentRequestDto>> GetMyAppointmentRequestsAsync()
    {
        try
        {
            const string query = """
                query GetMyAppointmentRequests {
                  myAppointmentRequests {
                    id
                    patientId
                    preferredDoctorId
                    preferredDoctorName
                    fullName
                    age
                    gender
                    email
                    phoneNumber
                    bloodType
                    allergies
                    symptomDescription
                    symptomSeverity
                    reasonForVisit
                    insuranceProvider
                    preferredDate
                    preferredTimeSlot
                    isUrgent
                    status
                    requestedAt
                    appointmentId
                  }
                }
                """;

            var response = await _graphQLService.SendQueryAsync<AppointmentRequestsResponse>(query);

            if (response?.MyAppointmentRequests != null)
            {
                _logger.LogInformation("Loaded {Count} appointment requests from GraphQL", response.MyAppointmentRequests.Count);
                
                foreach (var req in response.MyAppointmentRequests)
                {
                    _logger.LogInformation("Request ID: {Id}, Status: {Status}, RequestedAt: {DateTime}", 
                        req.Id, req.Status, req.RequestedAt);
                }
                
                return response.MyAppointmentRequests;
            }

            _logger.LogWarning("No appointment requests received from GraphQL");
            return new List<AppointmentRequestDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading appointment requests from GraphQL");
            throw;
        }
    }

    public async Task<bool> CancelAppointmentRequestAsync(int requestId, string reason)
    {
        try
        {
            const string mutation = """
                mutation CancelAppointmentRequest($requestId: Int!, $reason: String!) {
                  cancelAppointmentRequest(requestId: $requestId, reason: $reason)
                }
                """;

            var variables = new
            {
                requestId,
                reason
            };

            var response = await _graphQLService.SendQueryAsync<CancelAppointmentRequestResponse>(mutation, variables);

            if (response?.CancelAppointmentRequest == true)
            {
                _logger.LogInformation("Successfully cancelled appointment request {RequestId}", requestId);
                return true;
            }

            _logger.LogWarning("Failed to cancel appointment request {RequestId}", requestId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling appointment request {RequestId}", requestId);
            throw;
        }
    }
}

public class CancelAppointmentRequestResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("cancelAppointmentRequest")]
    public bool CancelAppointmentRequest { get; set; }
}

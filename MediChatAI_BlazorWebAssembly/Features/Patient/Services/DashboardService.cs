using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public class DashboardService : IDashboardService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IGraphQLService graphQLService, ILogger<DashboardService> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
    }

    public async Task<PatientDashboardData?> GetDashboardDataAsync()
    {
        try
        {
            // Get comprehensive dashboard data from backend
            const string dashboardQuery = @"
                query {
                    patientDashboardData {
                        overviewStats {
                            totalAppointments
                            upcomingAppointments
                            completedAppointments
                            activePrescriptions
                            totalDocuments
                            unreadMessages
                            pendingRefills
                        }
                        upcomingAppointments {
                            id
                            doctorName
                            specialization
                            doctorProfileImage
                            appointmentDate
                            appointmentTime
                            appointmentType
                            status
                            isVirtual
                            meetingLink
                            reasonForVisit
                        }
                        activePrescriptions {
                            id
                            medicationName
                            dosage
                            frequency
                            startDate
                            endDate
                            refillsRemaining
                            doctorName
                            isActive
                            instructions
                        }
                        recentDocuments {
                            id
                            documentName
                            documentType
                            documentCategory
                            uploadedDate
                            fileUrl
                            fileSizeBytes
                        }
                        recentNotifications {
                            id
                            title
                            message
                            category
                            createdAt
                            isRead
                        }
                        latestVitals {
                            bloodPressure
                            heartRate
                            temperature
                            weight
                            oxygenSaturation
                            lastRecorded
                        }
                    }
                }";

            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(dashboardQuery);

            if (response != null && response.ContainsKey("patientDashboardData"))
            {
                var json = JsonSerializer.Serialize(response["patientDashboardData"]);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };

                var dashboardData = JsonSerializer.Deserialize<PatientDashboardData>(json, options);

                if (dashboardData != null)
                {
                    _logger.LogInformation("Successfully loaded dashboard data: {UpcomingAppointments} upcoming appointments, {ActivePrescriptions} active prescriptions, {TotalDocuments} documents",
                        dashboardData.OverviewStats.UpcomingAppointments,
                        dashboardData.OverviewStats.ActivePrescriptions,
                        dashboardData.OverviewStats.TotalDocuments);

                    return dashboardData;
                }
            }

            _logger.LogWarning("No dashboard data received from GraphQL");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data from GraphQL");
            throw;
        }
    }
}

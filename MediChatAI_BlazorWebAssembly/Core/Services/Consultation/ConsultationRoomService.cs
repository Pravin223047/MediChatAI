using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Core.Services.Consultation;

/// <summary>
/// Service for managing real-time consultation room sessions
/// </summary>
public class ConsultationRoomService : IConsultationRoomService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<ConsultationRoomService> _logger;

    public ConsultationRoomService(
        IGraphQLService graphQLService,
        ILogger<ConsultationRoomService> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
    }

    #region Session Management

    public async Task<ConsultationSessionDto?> CreateConsultationSessionAsync(int appointmentId, string doctorId, string patientId)
    {
        try
        {
            const string mutation = """
                mutation CreateConsultationSession($appointmentId: Int!, $doctorId: String!, $patientId: String!) {
                  createConsultationSession(appointmentId: $appointmentId, doctorId: $doctorId, patientId: $patientId) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var variables = new { appointmentId, doctorId, patientId };
            var response = await _graphQLService.SendQueryAsync<CreateConsultationSessionResponse>(mutation, variables);

            if (response?.ConsultationSession != null)
            {
                _logger.LogInformation("Created consultation session {SessionId} for appointment {AppointmentId}",
                    response.ConsultationSession.Id, appointmentId);
                return response.ConsultationSession;
            }

            _logger.LogWarning("Failed to create consultation session for appointment {AppointmentId}", appointmentId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consultation session for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task<ConsultationSessionDto?> StartConsultationAsync(int sessionId)
    {
        try
        {
            const string mutation = """
                mutation StartConsultation($sessionId: Int!) {
                  startConsultation(sessionId: $sessionId) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var variables = new { sessionId };
            var response = await _graphQLService.SendQueryAsync<StartConsultationResponse>(mutation, variables);

            if (response?.ConsultationSession != null)
            {
                _logger.LogInformation("Started consultation session {SessionId}", sessionId);
                return response.ConsultationSession;
            }

            _logger.LogWarning("Failed to start consultation session {SessionId}", sessionId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<ConsultationSessionDto?> EndConsultationAsync(int sessionId)
    {
        try
        {
            const string mutation = """
                mutation EndConsultation($sessionId: Int!) {
                  endConsultation(sessionId: $sessionId) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var variables = new { sessionId };
            var response = await _graphQLService.SendQueryAsync<EndConsultationResponse>(mutation, variables);

            if (response?.ConsultationSession != null)
            {
                _logger.LogInformation("Ended consultation session {SessionId}", sessionId);
                return response.ConsultationSession;
            }

            _logger.LogWarning("Failed to end consultation session {SessionId}", sessionId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<ConsultationSessionDto?> GetConsultationSessionByIdAsync(int sessionId)
    {
        try
        {
            const string query = """
                query GetConsultationSession($sessionId: Int!) {
                  consultationSession(sessionId: $sessionId) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                    participants {
                      id
                      consultationSessionId
                      userId
                      name
                      email
                      phoneNumber
                      relation
                      role
                      permissions
                      joinedAt
                      leftAt
                      durationSeconds
                      isOnline
                      invitationToken
                      tokenExpiresAt
                      invitedByUserId
                      invitedByUserName
                      invitedAt
                      isRemoved
                      removedAt
                      removalReason
                    }
                  }
                }
                """;

            var variables = new { sessionId };
            var response = await _graphQLService.SendQueryAsync<GetConsultationSessionResponse>(query, variables);

            if (response?.ConsultationSession != null)
            {
                _logger.LogInformation("Retrieved consultation session {SessionId}", sessionId);
                return response.ConsultationSession;
            }

            _logger.LogWarning("Consultation session {SessionId} not found", sessionId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<ConsultationSessionDto?> GetActiveConsultationByAppointmentAsync(int appointmentId)
    {
        try
        {
            const string query = """
                query GetActiveConsultationByAppointment($appointmentId: Int!) {
                  activeConsultationByAppointment(appointmentId: $appointmentId) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var variables = new { appointmentId };
            var response = await _graphQLService.SendQueryAsync<GetActiveConsultationResponse>(query, variables);

            if (response?.ConsultationSession != null)
            {
                _logger.LogInformation("Retrieved active consultation for appointment {AppointmentId}", appointmentId);
                return response.ConsultationSession;
            }

            _logger.LogDebug("No active consultation found for appointment {AppointmentId}", appointmentId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active consultation for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task<ConsultationSessionDto?> GetConsultationByRoomIdAsync(string roomId)
    {
        try
        {
            const string query = """
                query GetConsultationByRoomId($roomId: String!) {
                  consultationByRoomId(roomId: $roomId) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var variables = new { roomId };
            var response = await _graphQLService.SendQueryAsync<GetConsultationByRoomIdResponse>(query, variables);

            if (response?.ConsultationSession != null)
            {
                _logger.LogInformation("Retrieved consultation by room ID {RoomId}", roomId);
                return response.ConsultationSession;
            }

            _logger.LogWarning("Consultation with room ID {RoomId} not found", roomId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consultation by room ID {RoomId}", roomId);
            throw;
        }
    }

    public async Task<ConsultationSessionDto?> CancelConsultationAsync(int sessionId, string reason)
    {
        try
        {
            const string mutation = """
                mutation CancelConsultation($sessionId: Int!, $reason: String!) {
                  cancelConsultation(sessionId: $sessionId, reason: $reason) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var variables = new { sessionId, reason };
            var response = await _graphQLService.SendQueryAsync<CancelConsultationResponse>(mutation, variables);

            if (response?.ConsultationSession != null)
            {
                _logger.LogInformation("Cancelled consultation session {SessionId} with reason: {Reason}", sessionId, reason);
                return response.ConsultationSession;
            }

            _logger.LogWarning("Failed to cancel consultation session {SessionId}", sessionId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> RateConsultationAsync(int sessionId, int rating, string? feedback = null)
    {
        try
        {
            const string mutation = """
                mutation RateConsultation($sessionId: Int!, $rating: Int!, $feedback: String) {
                  rateConsultationSession(sessionId: $sessionId, rating: $rating, feedback: $feedback)
                }
                """;

            var variables = new { sessionId, rating, feedback };
            var response = await _graphQLService.SendQueryAsync<RateConsultationSessionResponse>(mutation, variables);

            if (response?.Success == true)
            {
                _logger.LogInformation("Rated consultation session {SessionId} with {Rating} stars", sessionId, rating);
                return true;
            }

            _logger.LogWarning("Failed to rate consultation session {SessionId}", sessionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rating consultation session {SessionId}", sessionId);
            throw;
        }
    }

    #endregion

    #region History & Lists

    public async Task<List<ConsultationSessionDto>> GetConsultationsByDoctorAsync(string doctorId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            const string query = """
                query GetConsultationsByDoctor($doctorId: String!, $fromDate: DateTime, $toDate: DateTime) {
                  consultationsByDoctor(doctorId: $doctorId, fromDate: $fromDate, toDate: $toDate) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var variables = new { doctorId, fromDate, toDate };
            var response = await _graphQLService.SendQueryAsync<GetConsultationsByDoctorResponse>(query, variables);

            if (response?.ConsultationSessions != null)
            {
                _logger.LogInformation("Retrieved {Count} consultations for doctor {DoctorId}",
                    response.ConsultationSessions.Count, doctorId);
                return response.ConsultationSessions;
            }

            _logger.LogWarning("No consultations found for doctor {DoctorId}", doctorId);
            return new List<ConsultationSessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consultations for doctor {DoctorId}", doctorId);
            throw;
        }
    }

    public async Task<List<ConsultationSessionDto>> GetConsultationsByPatientAsync(string patientId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            const string query = """
                query GetConsultationsByPatient($patientId: String!, $fromDate: DateTime, $toDate: DateTime) {
                  consultationsByPatient(patientId: $patientId, fromDate: $fromDate, toDate: $toDate) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                    recordings {
                      id
                      consultationSessionId
                      recordingUrl
                      thumbnailUrl
                      type
                      status
                      fileSizeBytes
                      durationSeconds
                      format
                      resolution
                      transcriptUrl
                      transcriptText
                      isTranscribed
                      transcribedAt
                      aiSummary
                      isSummaryGenerated
                      recordedByUserId
                      recordedByUserName
                      isPatientAccessible
                      isDoctorAccessible
                      startedAt
                      completedAt
                      createdAt
                    }
                  }
                }
                """;

            var variables = new { patientId, fromDate, toDate };
            var response = await _graphQLService.SendQueryAsync<GetConsultationsByPatientResponse>(query, variables);

            if (response?.ConsultationSessions != null)
            {
                _logger.LogInformation("Retrieved {Count} consultations for patient {PatientId}",
                    response.ConsultationSessions.Count, patientId);
                return response.ConsultationSessions;
            }

            _logger.LogWarning("No consultations found for patient {PatientId}", patientId);
            return new List<ConsultationSessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consultations for patient {PatientId}", patientId);
            throw;
        }
    }

    public async Task<List<ConsultationSessionDto>> GetActiveConsultationsAsync()
    {
        try
        {
            const string query = """
                query GetActiveConsultations {
                  activeConsultations {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var response = await _graphQLService.SendQueryAsync<GetActiveConsultationsResponse>(query);

            if (response?.ConsultationSessions != null)
            {
                _logger.LogInformation("Retrieved {Count} active consultations", response.ConsultationSessions.Count);
                return response.ConsultationSessions;
            }

            _logger.LogDebug("No active consultations found");
            return new List<ConsultationSessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active consultations");
            throw;
        }
    }

    public async Task<List<ConsultationSessionDto>> GetDoctorActiveConsultationsAsync(string doctorId)
    {
        try
        {
            const string query = """
                query GetDoctorActiveConsultations($doctorId: String!) {
                  doctorActiveConsultations(doctorId: $doctorId) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                    participants {
                      id
                      consultationSessionId
                      userId
                      name
                      email
                      phoneNumber
                      relation
                      role
                      permissions
                      joinedAt
                      leftAt
                      durationSeconds
                      isOnline
                      invitationToken
                      tokenExpiresAt
                      invitedByUserId
                      invitedByUserName
                      invitedAt
                      isRemoved
                      removedAt
                      removalReason
                    }
                  }
                }
                """;

            var variables = new { doctorId };
            var response = await _graphQLService.SendQueryAsync<GetDoctorActiveConsultationsResponse>(query, variables);

            if (response?.ConsultationSessions != null)
            {
                _logger.LogInformation("Retrieved {Count} active consultations for doctor {DoctorId}",
                    response.ConsultationSessions.Count, doctorId);
                return response.ConsultationSessions;
            }

            _logger.LogDebug("No active consultations found for doctor {DoctorId}", doctorId);
            return new List<ConsultationSessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active consultations for doctor {DoctorId}", doctorId);
            throw;
        }
    }

    public async Task<List<ConsultationSessionDto>> GetDoctorRecentConsultationsAsync(string doctorId, int count = 10)
    {
        try
        {
            const string query = """
                query GetDoctorRecentConsultations($doctorId: String!, $count: Int!) {
                  doctorRecentConsultations(doctorId: $doctorId, count: $count) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var variables = new { doctorId, count };
            var response = await _graphQLService.SendQueryAsync<GetDoctorRecentConsultationsResponse>(query, variables);

            if (response?.ConsultationSessions != null)
            {
                _logger.LogInformation("Retrieved {Count} recent consultations for doctor {DoctorId}",
                    response.ConsultationSessions.Count, doctorId);
                return response.ConsultationSessions;
            }

            _logger.LogDebug("No recent consultations found for doctor {DoctorId}", doctorId);
            return new List<ConsultationSessionDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent consultations for doctor {DoctorId}", doctorId);
            throw;
        }
    }

    #endregion

    #region Clinical Notes

    public async Task<ConsultationSessionDto?> UpdateConsultationNotesAsync(
        int sessionId,
        string? chiefComplaint = null,
        string? observations = null,
        string? diagnosis = null,
        string? treatmentPlan = null,
        string? followUpInstructions = null,
        DateTime? nextFollowUpDate = null)
    {
        try
        {
            const string mutation = """
                mutation UpdateConsultationNotes($input: UpdateConsultationNotesInput!) {
                  updateConsultationNotes(input: $input) {
                    id
                    appointmentId
                    doctorId
                    doctorName
                    doctorSpecialization
                    doctorProfileImage
                    patientId
                    patientName
                    patientProfileImage
                    roomId
                    meetingLink
                    status
                    scheduledStartTime
                    actualStartTime
                    actualEndTime
                    plannedDurationMinutes
                    actualDurationMinutes
                    isRecording
                    recordingStartTime
                    chiefComplaint
                    doctorObservations
                    diagnosis
                    treatmentPlan
                    followUpInstructions
                    nextFollowUpDate
                    aiSummary
                    transcriptUrl
                    videoQuality
                    audioQuality
                    connectionIssues
                    patientRating
                    patientFeedback
                    createdAt
                    updatedAt
                    cancellationReason
                    cancelledAt
                    recordingCount
                    participantCount
                    prescriptionCount
                  }
                }
                """;

            var input = new UpdateConsultationNotesInput
            {
                SessionId = sessionId,
                ChiefComplaint = chiefComplaint,
                DoctorObservations = observations,
                Diagnosis = diagnosis,
                TreatmentPlan = treatmentPlan,
                FollowUpInstructions = followUpInstructions,
                NextFollowUpDate = nextFollowUpDate
            };

            var variables = new { input };
            var response = await _graphQLService.SendQueryAsync<UpdateConsultationNotesResponse>(mutation, variables);

            if (response?.ConsultationSession != null)
            {
                _logger.LogInformation("Updated clinical notes for consultation session {SessionId}", sessionId);
                return response.ConsultationSession;
            }

            _logger.LogWarning("Failed to update clinical notes for consultation session {SessionId}", sessionId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating clinical notes for consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<ConsultationNoteDto?> AddConsultationNoteAsync(int sessionId, string content, string type = "General", bool isMarkdown = false, bool isPrivate = false)
    {
        try
        {
            const string mutation = """
                mutation AddConsultationNote($input: AddConsultationNoteInput!) {
                  addConsultationNote(input: $input) {
                    id
                    consultationSessionId
                    authorId
                    authorName
                    content
                    type
                    title
                    isMarkdown
                    isPrivate
                    isSharedWithPatient
                    createdAt
                    updatedAt
                  }
                }
                """;

            var input = new AddConsultationNoteInput
            {
                SessionId = sessionId,
                Content = content,
                Type = type,
                IsMarkdown = isMarkdown,
                IsPrivate = isPrivate
            };

            var variables = new { input };
            var response = await _graphQLService.SendQueryAsync<AddConsultationNoteResponse>(mutation, variables);

            if (response?.Note != null)
            {
                _logger.LogInformation("Added note to consultation session {SessionId}", sessionId);
                return response.Note;
            }

            _logger.LogWarning("Failed to add note to consultation session {SessionId}", sessionId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding note to consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<ConsultationNoteDto?> UpdateConsultationNoteAsync(int noteId, string content, bool isPrivate)
    {
        try
        {
            const string mutation = """
                mutation UpdateConsultationNote($noteId: Int!, $content: String!, $isPrivate: Boolean!) {
                  updateConsultationNote(noteId: $noteId, content: $content, isPrivate: $isPrivate) {
                    id
                    consultationSessionId
                    authorId
                    authorName
                    content
                    type
                    title
                    isMarkdown
                    isPrivate
                    isSharedWithPatient
                    createdAt
                    updatedAt
                  }
                }
                """;

            var variables = new { noteId, content, isPrivate };
            var response = await _graphQLService.SendQueryAsync<UpdateConsultationNoteResponse>(mutation, variables);

            if (response?.Note != null)
            {
                _logger.LogInformation("Updated consultation note {NoteId}", noteId);
                return response.Note;
            }

            _logger.LogWarning("Failed to update consultation note {NoteId}", noteId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consultation note {NoteId}", noteId);
            throw;
        }
    }

    public async Task<bool> DeleteConsultationNoteAsync(int noteId)
    {
        try
        {
            const string mutation = """
                mutation DeleteConsultationNote($noteId: Int!) {
                  deleteConsultationNote(noteId: $noteId)
                }
                """;

            var variables = new { noteId };
            var response = await _graphQLService.SendQueryAsync<DeleteConsultationNoteResponse>(mutation, variables);

            if (response?.Success == true)
            {
                _logger.LogInformation("Deleted consultation note {NoteId}", noteId);
                return true;
            }

            _logger.LogWarning("Failed to delete consultation note {NoteId}", noteId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting consultation note {NoteId}", noteId);
            throw;
        }
    }

    public async Task<List<ConsultationNoteDto>> GetSessionNotesAsync(int sessionId)
    {
        try
        {
            const string query = """
                query GetSessionNotes($sessionId: Int!) {
                  sessionNotes(sessionId: $sessionId) {
                    id
                    consultationSessionId
                    authorId
                    authorName
                    content
                    type
                    title
                    isMarkdown
                    isPrivate
                    isSharedWithPatient
                    createdAt
                    updatedAt
                  }
                }
                """;

            var variables = new { sessionId };
            var response = await _graphQLService.SendQueryAsync<GetSessionNotesResponse>(query, variables);

            if (response?.Notes != null)
            {
                _logger.LogInformation("Retrieved {Count} notes for consultation session {SessionId}",
                    response.Notes.Count, sessionId);
                return response.Notes;
            }

            _logger.LogDebug("No notes found for consultation session {SessionId}", sessionId);
            return new List<ConsultationNoteDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notes for consultation session {SessionId}", sessionId);
            throw;
        }
    }

    #endregion

    #region Participants

    public async Task<ConsultationParticipantDto?> AddParticipantAsync(int sessionId, CreateParticipantInput participant)
    {
        try
        {
            const string mutation = """
                mutation AddParticipant($sessionId: Int!, $participant: CreateParticipantInput!) {
                  addParticipant(sessionId: $sessionId, participant: $participant) {
                    id
                    consultationSessionId
                    userId
                    name
                    email
                    phoneNumber
                    relation
                    role
                    permissions
                    joinedAt
                    leftAt
                    durationSeconds
                    isOnline
                    invitationToken
                    tokenExpiresAt
                    invitedByUserId
                    invitedByUserName
                    invitedAt
                    isRemoved
                    removedAt
                    removalReason
                  }
                }
                """;

            var variables = new { sessionId, participant };
            var response = await _graphQLService.SendQueryAsync<AddParticipantResponse>(mutation, variables);

            if (response?.Participant != null)
            {
                _logger.LogInformation("Added participant {Name} to consultation session {SessionId}",
                    participant.Name, sessionId);
                return response.Participant;
            }

            _logger.LogWarning("Failed to add participant to consultation session {SessionId}", sessionId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding participant to consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> RemoveParticipantAsync(int sessionId, int participantId, string? reason = null)
    {
        try
        {
            const string mutation = """
                mutation RemoveParticipant($sessionId: Int!, $participantId: Int!, $reason: String) {
                  removeParticipant(sessionId: $sessionId, participantId: $participantId, reason: $reason)
                }
                """;

            var variables = new { sessionId, participantId, reason };
            var response = await _graphQLService.SendQueryAsync<RemoveParticipantResponse>(mutation, variables);

            if (response?.Success == true)
            {
                _logger.LogInformation("Removed participant {ParticipantId} from consultation session {SessionId}",
                    participantId, sessionId);
                return true;
            }

            _logger.LogWarning("Failed to remove participant {ParticipantId} from consultation session {SessionId}",
                participantId, sessionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing participant {ParticipantId} from consultation session {SessionId}",
                participantId, sessionId);
            throw;
        }
    }

    public async Task<List<ConsultationParticipantDto>> GetParticipantsAsync(int sessionId)
    {
        try
        {
            const string query = """
                query GetParticipants($sessionId: Int!) {
                  participants(sessionId: $sessionId) {
                    id
                    consultationSessionId
                    userId
                    name
                    email
                    phoneNumber
                    relation
                    role
                    permissions
                    joinedAt
                    leftAt
                    durationSeconds
                    isOnline
                    invitationToken
                    tokenExpiresAt
                    invitedByUserId
                    invitedByUserName
                    invitedAt
                    isRemoved
                    removedAt
                    removalReason
                  }
                }
                """;

            var variables = new { sessionId };
            var response = await _graphQLService.SendQueryAsync<GetParticipantsResponse>(query, variables);

            if (response?.Participants != null)
            {
                _logger.LogInformation("Retrieved {Count} participants for consultation session {SessionId}",
                    response.Participants.Count, sessionId);
                return response.Participants;
            }

            _logger.LogDebug("No participants found for consultation session {SessionId}", sessionId);
            return new List<ConsultationParticipantDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving participants for consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<ConsultationParticipantDto?> ValidateParticipantTokenAsync(string token)
    {
        try
        {
            const string query = """
                query ValidateParticipantToken($token: String!) {
                  validateParticipantToken(token: $token) {
                    id
                    consultationSessionId
                    userId
                    name
                    email
                    phoneNumber
                    relation
                    role
                    permissions
                    joinedAt
                    leftAt
                    durationSeconds
                    isOnline
                    invitationToken
                    tokenExpiresAt
                    invitedByUserId
                    invitedByUserName
                    invitedAt
                    isRemoved
                    removedAt
                    removalReason
                  }
                }
                """;

            var variables = new { token };
            var response = await _graphQLService.SendQueryAsync<ValidateParticipantTokenResponse>(query, variables);

            if (response?.Participant != null)
            {
                _logger.LogInformation("Validated participant token");
                return response.Participant;
            }

            _logger.LogWarning("Invalid or expired participant token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating participant token");
            throw;
        }
    }

    public async Task<ConsultationParticipantDto?> JoinWithTokenAsync(string token)
    {
        try
        {
            const string mutation = """
                mutation JoinWithToken($token: String!) {
                  joinWithToken(token: $token) {
                    id
                    consultationSessionId
                    userId
                    name
                    email
                    phoneNumber
                    relation
                    role
                    permissions
                    joinedAt
                    leftAt
                    durationSeconds
                    isOnline
                    invitationToken
                    tokenExpiresAt
                    invitedByUserId
                    invitedByUserName
                    invitedAt
                    isRemoved
                    removedAt
                    removalReason
                  }
                }
                """;

            var variables = new { token };
            var response = await _graphQLService.SendQueryAsync<JoinWithTokenResponse>(mutation, variables);

            if (response?.Participant != null)
            {
                _logger.LogInformation("Participant joined consultation using token");
                return response.Participant;
            }

            _logger.LogWarning("Failed to join consultation with token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining consultation with token");
            throw;
        }
    }

    public async Task<bool> UpdateParticipantOnlineStatusAsync(int participantId, bool isOnline)
    {
        try
        {
            const string mutation = """
                mutation UpdateParticipantOnlineStatus($participantId: Int!, $isOnline: Boolean!) {
                  updateParticipantOnlineStatus(participantId: $participantId, isOnline: $isOnline)
                }
                """;

            var variables = new { participantId, isOnline };
            var response = await _graphQLService.SendQueryAsync<UpdateParticipantOnlineStatusResponse>(mutation, variables);

            if (response?.Success == true)
            {
                _logger.LogDebug("Updated participant {ParticipantId} online status to {IsOnline}",
                    participantId, isOnline);
                return true;
            }

            _logger.LogWarning("Failed to update participant {ParticipantId} online status", participantId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating participant {ParticipantId} online status", participantId);
            throw;
        }
    }

    #endregion

    #region Recording Management

    public async Task<bool> UpdateRecordingStatusAsync(int sessionId, bool isRecording)
    {
        try
        {
            const string mutation = """
                mutation UpdateRecordingStatus($sessionId: Int!, $isRecording: Boolean!) {
                  updateRecordingStatus(sessionId: $sessionId, isRecording: $isRecording)
                }
                """;

            var variables = new { sessionId, isRecording };
            var response = await _graphQLService.SendQueryAsync<UpdateRecordingStatusResponse>(mutation, variables);

            if (response?.Success == true)
            {
                _logger.LogInformation("Updated recording status for consultation session {SessionId} to {IsRecording}",
                    sessionId, isRecording);
                return true;
            }

            _logger.LogWarning("Failed to update recording status for consultation session {SessionId}", sessionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating recording status for consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<List<ConsultationRecordingDto>> GetRecordingsBySessionAsync(int sessionId)
    {
        try
        {
            const string query = """
                query GetRecordingsBySession($sessionId: Int!) {
                  recordingsBySession(sessionId: $sessionId) {
                    id
                    consultationSessionId
                    recordingUrl
                    thumbnailUrl
                    type
                    status
                    fileSizeBytes
                    durationSeconds
                    format
                    resolution
                    transcriptUrl
                    transcriptText
                    isTranscribed
                    transcribedAt
                    aiSummary
                    isSummaryGenerated
                    recordedByUserId
                    recordedByUserName
                    isPatientAccessible
                    isDoctorAccessible
                    startedAt
                    completedAt
                    createdAt
                  }
                }
                """;

            var variables = new { sessionId };
            var response = await _graphQLService.SendQueryAsync<GetRecordingsBySessionResponse>(query, variables);

            if (response?.Recordings != null)
            {
                _logger.LogInformation("Retrieved {Count} recordings for consultation session {SessionId}",
                    response.Recordings.Count, sessionId);
                return response.Recordings;
            }

            _logger.LogDebug("No recordings found for consultation session {SessionId}", sessionId);
            return new List<ConsultationRecordingDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recordings for consultation session {SessionId}", sessionId);
            throw;
        }
    }

    #endregion

    #region AI & Summary

    public async Task<bool> GenerateAISummaryAsync(int sessionId)
    {
        try
        {
            const string mutation = """
                mutation GenerateAISummary($sessionId: Int!) {
                  generateAISummary(sessionId: $sessionId)
                }
                """;

            var variables = new { sessionId };
            var response = await _graphQLService.SendQueryAsync<GenerateAISummaryResponse>(mutation, variables);

            if (response?.Success == true)
            {
                _logger.LogInformation("Generated AI summary for consultation session {SessionId}", sessionId);
                return true;
            }

            _logger.LogWarning("Failed to generate AI summary for consultation session {SessionId}", sessionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI summary for consultation session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<bool> UpdateAISummaryAsync(int sessionId, string summary)
    {
        try
        {
            const string mutation = """
                mutation UpdateAISummary($sessionId: Int!, $summary: String!) {
                  updateAISummary(sessionId: $sessionId, summary: $summary)
                }
                """;

            var variables = new { sessionId, summary };
            var response = await _graphQLService.SendQueryAsync<UpdateAISummaryResponse>(mutation, variables);

            if (response?.Success == true)
            {
                _logger.LogInformation("Updated AI summary for consultation session {SessionId}", sessionId);
                return true;
            }

            _logger.LogWarning("Failed to update AI summary for consultation session {SessionId}", sessionId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI summary for consultation session {SessionId}", sessionId);
            throw;
        }
    }

    #endregion

    #region Meeting Link

    public async Task<string?> GenerateMeetingLinkAsync(int sessionId)
    {
        try
        {
            const string mutation = """
                mutation GenerateMeetingLink($sessionId: Int!) {
                  generateMeetingLink(sessionId: $sessionId)
                }
                """;

            var variables = new { sessionId };
            var response = await _graphQLService.SendQueryAsync<GenerateMeetingLinkResponse>(mutation, variables);

            if (response?.MeetingLink != null)
            {
                _logger.LogInformation("Generated meeting link for consultation session {SessionId}", sessionId);
                return response.MeetingLink;
            }

            _logger.LogWarning("Failed to generate meeting link for consultation session {SessionId}", sessionId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating meeting link for consultation session {SessionId}", sessionId);
            throw;
        }
    }

    #endregion

    #region Response Models

    private class CreateConsultationSessionResponse { public ConsultationSessionDto? ConsultationSession { get; set; } }
    private class StartConsultationResponse { public ConsultationSessionDto? ConsultationSession { get; set; } }
    private class EndConsultationResponse { public ConsultationSessionDto? ConsultationSession { get; set; } }
    private class GetConsultationSessionResponse { public ConsultationSessionDto? ConsultationSession { get; set; } }
    private class GetActiveConsultationResponse { public ConsultationSessionDto? ConsultationSession { get; set; } }
    private class GetConsultationByRoomIdResponse { public ConsultationSessionDto? ConsultationSession { get; set; } }
    private class CancelConsultationResponse { public ConsultationSessionDto? ConsultationSession { get; set; } }
    private class RateConsultationSessionResponse { public bool Success { get; set; } }
    private class GetConsultationsByDoctorResponse { public List<ConsultationSessionDto> ConsultationSessions { get; set; } = new(); }
    private class GetConsultationsByPatientResponse { public List<ConsultationSessionDto> ConsultationSessions { get; set; } = new(); }
    private class GetActiveConsultationsResponse { public List<ConsultationSessionDto> ConsultationSessions { get; set; } = new(); }
    private class GetDoctorActiveConsultationsResponse { public List<ConsultationSessionDto> ConsultationSessions { get; set; } = new(); }
    private class GetDoctorRecentConsultationsResponse { public List<ConsultationSessionDto> ConsultationSessions { get; set; } = new(); }
    private class UpdateConsultationNotesResponse { public ConsultationSessionDto? ConsultationSession { get; set; } }
    private class AddConsultationNoteResponse { public ConsultationNoteDto? Note { get; set; } }
    private class UpdateConsultationNoteResponse { public ConsultationNoteDto? Note { get; set; } }
    private class DeleteConsultationNoteResponse { public bool Success { get; set; } }
    private class GetSessionNotesResponse { public List<ConsultationNoteDto> Notes { get; set; } = new(); }
    private class AddParticipantResponse { public ConsultationParticipantDto? Participant { get; set; } }
    private class RemoveParticipantResponse { public bool Success { get; set; } }
    private class GetParticipantsResponse { public List<ConsultationParticipantDto> Participants { get; set; } = new(); }
    private class ValidateParticipantTokenResponse { public ConsultationParticipantDto? Participant { get; set; } }
    private class JoinWithTokenResponse { public ConsultationParticipantDto? Participant { get; set; } }
    private class UpdateParticipantOnlineStatusResponse { public bool Success { get; set; } }
    private class UpdateRecordingStatusResponse { public bool Success { get; set; } }
    private class GetRecordingsBySessionResponse { public List<ConsultationRecordingDto> Recordings { get; set; } = new(); }
    private class GenerateAISummaryResponse { public bool Success { get; set; } }
    private class UpdateAISummaryResponse { public bool Success { get; set; } }
    private class GenerateMeetingLinkResponse { public string? MeetingLink { get; set; } }

    #endregion
}

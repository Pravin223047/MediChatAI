using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Consultation;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Notifications.Hubs;

namespace MediChatAI_GraphQl.Shared.Services;

public class ConsultationSessionService : IConsultationSessionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConsultationSessionService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly INotificationService _notificationService;

    public ConsultationSessionService(
        ApplicationDbContext context,
        ILogger<ConsultationSessionService> logger,
        IHubContext<NotificationHub> hubContext,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
        _notificationService = notificationService;
    }

    #region Session Management

    public async Task<ConsultationSessionDto> CreateConsultationSessionAsync(
        int appointmentId,
        string doctorId,
        string patientId)
    {
        _logger.LogInformation("Creating consultation session for appointment {AppointmentId}", appointmentId);

        // Check if session already exists
        var existingSession = await _context.ConsultationSessions
            .FirstOrDefaultAsync(cs => cs.AppointmentId == appointmentId);

        if (existingSession != null)
        {
            return await MapToDtoAsync(existingSession);
        }

        // Get appointment details
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
        {
            throw new ArgumentException($"Appointment with ID {appointmentId} not found");
        }

        // Generate unique room ID
        var roomId = $"room_{Guid.NewGuid():N}";

        // Create consultation session
        var session = new ConsultationSession
        {
            AppointmentId = appointmentId,
            DoctorId = doctorId,
            PatientId = patientId,
            RoomId = roomId,
            ScheduledStartTime = appointment.AppointmentDateTime,
            PlannedDurationMinutes = appointment.DurationMinutes,
            Status = ConsultationStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _context.ConsultationSessions.Add(session);

        // Add doctor and patient as default participants
        var doctorParticipant = new ConsultationParticipant
        {
            ConsultationSession = session,
            UserId = doctorId,
            Name = "", // Will be populated from user
            Role = ConsultationParticipantRole.Doctor,
            Permissions = ParticipantPermission.All,
            JoinedAt = DateTime.UtcNow,
            IsOnline = false
        };

        var patientParticipant = new ConsultationParticipant
        {
            ConsultationSession = session,
            UserId = patientId,
            Name = "", // Will be populated from user
            Role = ConsultationParticipantRole.Patient,
            Permissions = ParticipantPermission.All,
            JoinedAt = DateTime.UtcNow,
            IsOnline = false
        };

        _context.ConsultationParticipants.Add(doctorParticipant);
        _context.ConsultationParticipants.Add(patientParticipant);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created consultation session {SessionId} for appointment {AppointmentId}",
            session.Id, appointmentId);

        return await MapToDtoAsync(session);
    }

    public async Task<ConsultationSessionDto> StartConsultationAsync(int sessionId)
    {
        _logger.LogInformation("Starting consultation session {SessionId}", sessionId);

        var session = await _context.ConsultationSessions
            .FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Consultation session with ID {sessionId} not found");
        }

        if (session.Status == ConsultationStatus.Active)
        {
            _logger.LogWarning("Consultation session {SessionId} is already active", sessionId);
            return await MapToDtoAsync(session);
        }

        // Update session status
        session.Status = ConsultationStatus.Active;
        session.ActualStartTime = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        // Update appointment status
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == session.AppointmentId);
        if (appointment != null)
        {
            appointment.Status = AppointmentStatus.InProgress;
            appointment.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Send notifications
        await _notificationService.CreateNotificationAsync(
            session.PatientId,
            "Consultation Started",
            $"Your consultation has started. Please join now.",
            NotificationType.Info,
            NotificationCategory.Appointment,
            NotificationPriority.Normal,
            $"/patient/consultation/{session.AppointmentId}"
        );

        // Broadcast via SignalR
        await _hubContext.Clients.User(session.PatientId)
            .SendAsync("ConsultationStarted", new { sessionId = session.Id, roomId = session.RoomId });

        _logger.LogInformation("Started consultation session {SessionId}", sessionId);

        return await MapToDtoAsync(session);
    }

    public async Task<ConsultationSessionDto> EndConsultationAsync(int sessionId)
    {
        _logger.LogInformation("Ending consultation session {SessionId}", sessionId);

        var session = await _context.ConsultationSessions
            .Include(cs => cs.Participants)
            .FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Consultation session with ID {sessionId} not found");
        }

        if (session.Status == ConsultationStatus.Completed)
        {
            _logger.LogWarning("Consultation session {SessionId} is already completed", sessionId);
            return await MapToDtoAsync(session);
        }

        // Update session status
        session.Status = ConsultationStatus.Completed;
        session.ActualEndTime = DateTime.UtcNow;

        if (session.ActualStartTime.HasValue)
        {
            session.ActualDurationMinutes = (int)(session.ActualEndTime.Value - session.ActualStartTime.Value).TotalMinutes;
        }

        session.UpdatedAt = DateTime.UtcNow;

        // Update participant left times
        foreach (var participant in session.Participants.Where(p => !p.LeftAt.HasValue))
        {
            participant.LeftAt = DateTime.UtcNow;
            participant.IsOnline = false;
            participant.DurationSeconds = (int)(DateTime.UtcNow - participant.JoinedAt).TotalSeconds;
        }

        // Update appointment status
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == session.AppointmentId);
        if (appointment != null)
        {
            appointment.Status = AppointmentStatus.Completed;
            appointment.CompletedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Send notifications
        await _notificationService.CreateNotificationAsync(
            session.PatientId,
            "Consultation Completed",
            "Your consultation has ended. Summary will be available shortly.",
            NotificationType.Success,
            NotificationCategory.Appointment,
            NotificationPriority.Normal,
            $"/patient/consultations"
        );

        await _notificationService.CreateNotificationAsync(
            session.DoctorId,
            "Consultation Completed",
            "Consultation has ended. Please review and sign any pending prescriptions.",
            NotificationType.Info,
            NotificationCategory.Appointment,
            NotificationPriority.Normal,
            $"/doctor/prescriptions/review/{session.Id}"
        );

        // Broadcast via SignalR
        await _hubContext.Clients.Users(new[] { session.PatientId, session.DoctorId })
            .SendAsync("ConsultationEnded", new { sessionId = session.Id });

        _logger.LogInformation("Ended consultation session {SessionId}", sessionId);

        // Note: AI summary generation will be triggered separately
        return await MapToDtoAsync(session);
    }

    public async Task<ConsultationSessionDto?> GetConsultationSessionByIdAsync(int sessionId)
    {
        var session = await _context.ConsultationSessions
            .Include(cs => cs.Doctor)
            .Include(cs => cs.Patient)
            .Include(cs => cs.Participants)
            .Include(cs => cs.Recordings)
            .FirstOrDefaultAsync(cs => cs.Id == sessionId);

        return session == null ? null : await MapToDtoAsync(session);
    }

    public async Task<ConsultationSessionDto?> GetActiveConsultationByAppointmentAsync(int appointmentId)
    {
        var session = await _context.ConsultationSessions
            .Include(cs => cs.Doctor)
            .Include(cs => cs.Patient)
            .Include(cs => cs.Participants)
            .Where(cs => cs.AppointmentId == appointmentId &&
                        (cs.Status == ConsultationStatus.Active || cs.Status == ConsultationStatus.WaitingForDoctor))
            .FirstOrDefaultAsync();

        return session == null ? null : await MapToDtoAsync(session);
    }

    public async Task<ConsultationSessionDto?> GetConsultationByRoomIdAsync(string roomId)
    {
        var session = await _context.ConsultationSessions
            .Include(cs => cs.Doctor)
            .Include(cs => cs.Patient)
            .Include(cs => cs.Participants)
            .FirstOrDefaultAsync(cs => cs.RoomId == roomId);

        return session == null ? null : await MapToDtoAsync(session);
    }

    #endregion

    #region History & Lists

    public async Task<List<ConsultationSessionDto>> GetConsultationsByDoctorAsync(
        string doctorId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.ConsultationSessions
            .Include(cs => cs.Doctor)
            .Include(cs => cs.Patient)
            .Where(cs => cs.DoctorId == doctorId);

        if (fromDate.HasValue)
        {
            query = query.Where(cs => cs.ScheduledStartTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(cs => cs.ScheduledStartTime <= toDate.Value);
        }

        var sessions = await query
            .OrderByDescending(cs => cs.ScheduledStartTime)
            .ToListAsync();

        var dtos = new List<ConsultationSessionDto>();
        foreach (var session in sessions)
        {
            dtos.Add(await MapToDtoAsync(session));
        }

        return dtos;
    }

    public async Task<List<ConsultationSessionDto>> GetConsultationsByPatientAsync(
        string patientId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.ConsultationSessions
            .Include(cs => cs.Doctor)
            .Include(cs => cs.Patient)
            .Where(cs => cs.PatientId == patientId);

        if (fromDate.HasValue)
        {
            query = query.Where(cs => cs.ScheduledStartTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(cs => cs.ScheduledStartTime <= toDate.Value);
        }

        var sessions = await query
            .OrderByDescending(cs => cs.ScheduledStartTime)
            .ToListAsync();

        var dtos = new List<ConsultationSessionDto>();
        foreach (var session in sessions)
        {
            dtos.Add(await MapToDtoAsync(session));
        }

        return dtos;
    }

    public async Task<List<ConsultationSessionDto>> GetActiveConsultationsAsync()
    {
        var sessions = await _context.ConsultationSessions
            .Include(cs => cs.Doctor)
            .Include(cs => cs.Patient)
            .Where(cs => cs.Status == ConsultationStatus.Active || cs.Status == ConsultationStatus.WaitingForDoctor)
            .OrderBy(cs => cs.ScheduledStartTime)
            .ToListAsync();

        var dtos = new List<ConsultationSessionDto>();
        foreach (var session in sessions)
        {
            dtos.Add(await MapToDtoAsync(session));
        }

        return dtos;
    }

    #endregion

    #region Clinical Notes

    public async Task<ConsultationSessionDto> UpdateConsultationNotesAsync(
        int sessionId,
        string? chiefComplaint = null,
        string? observations = null,
        string? diagnosis = null,
        string? treatmentPlan = null,
        string? followUpInstructions = null,
        DateTime? nextFollowUpDate = null)
    {
        var session = await _context.ConsultationSessions.FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Consultation session with ID {sessionId} not found");
        }

        if (chiefComplaint != null) session.ChiefComplaint = chiefComplaint;
        if (observations != null) session.DoctorObservations = observations;
        if (diagnosis != null) session.Diagnosis = diagnosis;
        if (treatmentPlan != null) session.TreatmentPlan = treatmentPlan;
        if (followUpInstructions != null) session.FollowUpInstructions = followUpInstructions;
        if (nextFollowUpDate.HasValue) session.NextFollowUpDate = nextFollowUpDate;

        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated notes for consultation session {SessionId}", sessionId);

        return await MapToDtoAsync(session);
    }

    #endregion

    #region Participants

    public async Task<ConsultationParticipantDto> AddParticipantAsync(int sessionId, CreateParticipantDto dto)
    {
        _logger.LogInformation("Adding participant to consultation session {SessionId}", sessionId);

        var session = await _context.ConsultationSessions.FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Consultation session with ID {sessionId} not found");
        }

        // Generate invitation token
        var token = Guid.NewGuid().ToString("N");
        var tokenExpiry = DateTime.UtcNow.AddHours(24);

        var participant = new ConsultationParticipant
        {
            ConsultationSessionId = sessionId,
            Name = dto.Name,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Relation = dto.Relation,
            Role = dto.Role,
            Permissions = dto.Permissions,
            InvitationToken = token,
            TokenExpiresAt = tokenExpiry,
            InvitedByUserId = dto.InvitedByUserId,
            InvitationMessage = dto.InvitationMessage,
            InvitedAt = DateTime.UtcNow,
            JoinedAt = DateTime.UtcNow,
            IsOnline = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.ConsultationParticipants.Add(participant);
        await _context.SaveChangesAsync();

        // Broadcast participant added
        await _hubContext.Clients.Group($"consultation_{sessionId}")
            .SendAsync("ParticipantAdded", MapParticipantToDto(participant));

        _logger.LogInformation("Added participant {ParticipantId} to session {SessionId}", participant.Id, sessionId);

        return MapParticipantToDto(participant);
    }

    public async Task<bool> RemoveParticipantAsync(int sessionId, int participantId, string? reason = null)
    {
        var participant = await _context.ConsultationParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConsultationSessionId == sessionId);

        if (participant == null)
        {
            return false;
        }

        // Don't actually delete, just mark as removed
        participant.IsRemoved = true;
        participant.RemovedAt = DateTime.UtcNow;
        participant.RemovalReason = reason;
        participant.LeftAt = DateTime.UtcNow;
        participant.IsOnline = false;

        participant.DurationSeconds = (int)(DateTime.UtcNow - participant.JoinedAt).TotalSeconds;

        await _context.SaveChangesAsync();

        // Broadcast participant removed
        await _hubContext.Clients.Group($"consultation_{sessionId}")
            .SendAsync("ParticipantRemoved", new { participantId = participant.Id });

        _logger.LogInformation("Removed participant {ParticipantId} from session {SessionId}", participantId, sessionId);

        return true;
    }

    public async Task<List<ConsultationParticipantDto>> GetParticipantsAsync(int sessionId)
    {
        var participants = await _context.ConsultationParticipants
            .Where(p => p.ConsultationSessionId == sessionId && !p.IsRemoved)
            .ToListAsync();

        return participants.Select(MapParticipantToDto).ToList();
    }

    public async Task<ConsultationParticipantDto?> ValidateParticipantTokenAsync(string token)
    {
        var participant = await _context.ConsultationParticipants
            .Include(p => p.ConsultationSession)
            .FirstOrDefaultAsync(p => p.InvitationToken == token &&
                                     p.TokenExpiresAt > DateTime.UtcNow &&
                                     !p.TokenUsed &&
                                     !p.IsRemoved);

        if (participant == null)
        {
            return null;
        }

        // Mark token as used
        participant.TokenUsed = true;
        await _context.SaveChangesAsync();

        return MapParticipantToDto(participant);
    }

    #endregion

    #region Recording Management

    public async Task<bool> UpdateRecordingStatusAsync(int sessionId, bool isRecording)
    {
        var session = await _context.ConsultationSessions.FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            return false;
        }

        session.IsRecording = isRecording;
        session.RecordingStartTime = isRecording ? DateTime.UtcNow : session.RecordingStartTime;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Broadcast recording status change
        await _hubContext.Clients.Group($"consultation_{sessionId}")
            .SendAsync("RecordingStatusChanged", new { sessionId, isRecording });

        _logger.LogInformation("Updated recording status for session {SessionId}: {IsRecording}",
            sessionId, isRecording);

        return true;
    }

    #endregion

    #region Session Status

    public async Task<ConsultationSessionDto> CancelConsultationAsync(int sessionId, string reason)
    {
        var session = await _context.ConsultationSessions.FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Consultation session with ID {sessionId} not found");
        }

        session.Status = ConsultationStatus.Cancelled;
        session.CancellationReason = reason;
        session.CancelledAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;

        // Update appointment
        var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == session.AppointmentId);
        if (appointment != null)
        {
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationReason = reason;
            appointment.CancelledAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Send notifications
        await _notificationService.CreateNotificationAsync(
            session.PatientId,
            "Consultation Cancelled",
            $"Your consultation has been cancelled. Reason: {reason}",
            NotificationType.Warning,
            NotificationCategory.Appointment,
            NotificationPriority.Normal,
            $"/patient/appointments"
        );

        _logger.LogInformation("Cancelled consultation session {SessionId}: {Reason}", sessionId, reason);

        return await MapToDtoAsync(session);
    }

    public async Task<bool> RateConsultationAsync(int sessionId, int rating, string? feedback = null)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5");
        }

        var session = await _context.ConsultationSessions.FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            _logger.LogWarning("Consultation session with ID {SessionId} not found", sessionId);
            return false;
        }

        session.PatientRating = rating;
        session.PatientFeedback = feedback;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Patient rated consultation session {SessionId}: {Rating} stars", sessionId, rating);

        return true;
    }

    #endregion

    #region AI & Summary

    public async Task<bool> GenerateAISummaryAsync(int sessionId)
    {
        // This will be implemented by ConsultationTranscriptionService
        // This method just marks that summary generation was requested
        _logger.LogInformation("AI summary generation requested for session {SessionId}", sessionId);
        return true;
    }

    public async Task<bool> UpdateAISummaryAsync(int sessionId, string summary)
    {
        var session = await _context.ConsultationSessions.FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            return false;
        }

        session.AISummary = summary;
        session.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Notify patient that summary is ready
        await _notificationService.CreateNotificationAsync(
            session.PatientId,
            "Consultation Summary Ready",
            "Your consultation summary is now available to view.",
            NotificationType.Success,
            NotificationCategory.Appointment,
            NotificationPriority.Normal,
            $"/patient/consultations"
        );

        _logger.LogInformation("Updated AI summary for session {SessionId}", sessionId);

        return true;
    }

    #endregion

    #region Meeting Link

    public async Task<string> GenerateMeetingLinkAsync(int sessionId)
    {
        var session = await _context.ConsultationSessions.FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Consultation session with ID {sessionId} not found");
        }

        if (string.IsNullOrEmpty(session.MeetingLink))
        {
            // Generate meeting link based on environment
            var baseUrl = "https://yourdomain.com"; // TODO: Get from configuration
            session.MeetingLink = $"{baseUrl}/consultation/join/{session.RoomId}";
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated meeting link for session {SessionId}: {MeetingLink}",
                sessionId, session.MeetingLink);
        }

        return session.MeetingLink;
    }

    #endregion

    #region Mapping

    private async Task<ConsultationSessionDto> MapToDtoAsync(ConsultationSession session)
    {
        // Load navigation properties if not already loaded
        if (session.Doctor == null || session.Patient == null)
        {
            await _context.Entry(session).Reference(s => s.Doctor).LoadAsync();
            await _context.Entry(session).Reference(s => s.Patient).LoadAsync();
        }

        // Get counts
        var recordingCount = await _context.ConsultationRecordings
            .Where(r => r.ConsultationSessionId == session.Id)
            .CountAsync();

        var participantCount = await _context.ConsultationParticipants
            .Where(p => p.ConsultationSessionId == session.Id && !p.IsRemoved)
            .CountAsync();

        var prescriptionCount = await _context.Prescriptions
            .Where(p => p.ConsultationSessionId == session.Id)
            .CountAsync();

        return new ConsultationSessionDto
        {
            Id = session.Id,
            AppointmentId = session.AppointmentId,
            DoctorId = session.DoctorId,
            DoctorName = session.Doctor != null ? $"{session.Doctor.FirstName} {session.Doctor.LastName}" : "Unknown",
            DoctorSpecialization = session.Doctor?.Specialization,
            DoctorProfileImage = session.Doctor?.ProfileImage,
            PatientId = session.PatientId,
            PatientName = session.Patient != null ? $"{session.Patient.FirstName} {session.Patient.LastName}" : "Unknown",
            PatientProfileImage = session.Patient?.ProfileImage,
            RoomId = session.RoomId,
            MeetingLink = session.MeetingLink,
            Status = session.Status,
            ScheduledStartTime = session.ScheduledStartTime,
            ActualStartTime = session.ActualStartTime,
            ActualEndTime = session.ActualEndTime,
            PlannedDurationMinutes = session.PlannedDurationMinutes,
            ActualDurationMinutes = session.ActualDurationMinutes,
            IsRecording = session.IsRecording,
            RecordingStartTime = session.RecordingStartTime,
            ChiefComplaint = session.ChiefComplaint,
            DoctorObservations = session.DoctorObservations,
            Diagnosis = session.Diagnosis,
            TreatmentPlan = session.TreatmentPlan,
            FollowUpInstructions = session.FollowUpInstructions,
            NextFollowUpDate = session.NextFollowUpDate,
            AISummary = session.AISummary,
            TranscriptUrl = session.TranscriptUrl,
            VideoQuality = session.VideoQuality,
            AudioQuality = session.AudioQuality,
            ConnectionIssues = session.ConnectionIssues,
            PatientRating = session.PatientRating,
            PatientFeedback = session.PatientFeedback,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            CancellationReason = session.CancellationReason,
            CancelledAt = session.CancelledAt,
            RecordingCount = recordingCount,
            ParticipantCount = participantCount,
            PrescriptionCount = prescriptionCount
        };
    }

    private ConsultationParticipantDto MapParticipantToDto(ConsultationParticipant participant)
    {
        return new ConsultationParticipantDto
        {
            Id = participant.Id,
            ConsultationSessionId = participant.ConsultationSessionId,
            UserId = participant.UserId,
            Name = participant.Name,
            Email = participant.Email,
            PhoneNumber = participant.PhoneNumber,
            Relation = participant.Relation,
            Role = participant.Role,
            Permissions = participant.Permissions,
            JoinedAt = participant.JoinedAt,
            LeftAt = participant.LeftAt,
            DurationSeconds = participant.DurationSeconds,
            IsOnline = participant.IsOnline,
            InvitationToken = participant.InvitationToken,
            TokenExpiresAt = participant.TokenExpiresAt,
            InvitedByUserId = participant.InvitedByUserId,
            InvitedAt = participant.InvitedAt,
            IsRemoved = participant.IsRemoved,
            RemovedAt = participant.RemovedAt,
            RemovalReason = participant.RemovalReason
        };
    }

    #endregion

    #region Wrapper Methods for Interface Compatibility

    /// <summary>
    /// Alias method for GetConsultationsByDoctorAsync
    /// </summary>
    public async Task<List<ConsultationSessionDto>> GetSessionsByDoctorIdAsync(
        string doctorId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        return await GetConsultationsByDoctorAsync(doctorId, fromDate, toDate);
    }

    /// <summary>
    /// Alias method for GetConsultationsByPatientAsync
    /// </summary>
    public async Task<List<ConsultationSessionDto>> GetSessionsByPatientIdAsync(
        string patientId,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        return await GetConsultationsByPatientAsync(patientId, fromDate, toDate);
    }

    /// <summary>
    /// 4-parameter version of UpdateConsultationNotesAsync
    /// Calls the 6-parameter version with nulls for the last 2 parameters
    /// </summary>
    public async Task<bool> UpdateClinicalNotesAsync(
        int sessionId,
        string? chiefComplaint,
        string? observations,
        string? diagnosis,
        string? treatmentPlan)
    {
        try
        {
            await UpdateConsultationNotesAsync(sessionId, chiefComplaint, observations, diagnosis, treatmentPlan, null, null);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating clinical notes for session {SessionId}", sessionId);
            return false;
        }
    }

    /// <summary>
    /// Alias method for GetParticipantsAsync
    /// </summary>
    public async Task<List<ConsultationParticipantDto>> GetSessionParticipantsAsync(int sessionId)
    {
        return await GetParticipantsAsync(sessionId);
    }

    /// <summary>
    /// Alias method for ValidateParticipantTokenAsync
    /// </summary>
    public async Task<ConsultationParticipantDto?> VerifyParticipantTokenAsync(string token)
    {
        return await ValidateParticipantTokenAsync(token);
    }

    /// <summary>
    /// Join a participant to the consultation session after token validation
    /// </summary>
    public async Task<ConsultationParticipantDto?> JoinWithTokenAsync(string token)
    {
        _logger.LogInformation("Participant joining with token");

        var participant = await _context.ConsultationParticipants
            .Include(p => p.ConsultationSession)
            .FirstOrDefaultAsync(p => p.InvitationToken == token &&
                                     p.TokenExpiresAt > DateTime.UtcNow &&
                                     !p.TokenUsed &&
                                     !p.IsRemoved);

        if (participant == null)
        {
            _logger.LogWarning("Invalid or expired token");
            return null;
        }

        // Mark token as used
        participant.TokenUsed = true;

        // Join the participant
        participant.JoinedAt = DateTime.UtcNow;
        participant.IsOnline = true;

        await _context.SaveChangesAsync();

        // Broadcast participant joined
        if (participant.ConsultationSession != null)
        {
            await _hubContext.Clients.Group($"consultation_{participant.ConsultationSessionId}")
                .SendAsync("ParticipantJoined", MapParticipantToDto(participant));
        }

        _logger.LogInformation("Participant {ParticipantId} joined session {SessionId}",
            participant.Id, participant.ConsultationSessionId);

        return MapParticipantToDto(participant);
    }

    /// <summary>
    /// Update the online status of a participant
    /// </summary>
    public async Task<bool> UpdateParticipantOnlineStatusAsync(int participantId, bool isOnline)
    {
        _logger.LogInformation("Updating online status for participant {ParticipantId}: {IsOnline}",
            participantId, isOnline);

        var participant = await _context.ConsultationParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId);

        if (participant == null)
        {
            _logger.LogWarning("Participant {ParticipantId} not found", participantId);
            return false;
        }

        participant.IsOnline = isOnline;

        if (!isOnline && !participant.LeftAt.HasValue)
        {
            participant.LeftAt = DateTime.UtcNow;
            participant.DurationSeconds = (int)(DateTime.UtcNow - participant.JoinedAt).TotalSeconds;
        }

        await _context.SaveChangesAsync();

        // Broadcast status change
        await _hubContext.Clients.Group($"consultation_{participant.ConsultationSessionId}")
            .SendAsync("ParticipantStatusChanged", new
            {
                participantId = participant.Id,
                isOnline = isOnline
            });

        _logger.LogInformation("Updated online status for participant {ParticipantId}", participantId);

        return true;
    }

    /// <summary>
    /// Add a new note to a consultation session
    /// </summary>
    public async Task<ConsultationNoteDto> AddConsultationNoteAsync(
        int sessionId,
        string authorId,
        string content,
        NoteType noteType,
        bool isMarkdown,
        bool isPrivate)
    {
        _logger.LogInformation("Adding note to consultation session {SessionId} by user {AuthorId}",
            sessionId, authorId);

        var session = await _context.ConsultationSessions
            .FirstOrDefaultAsync(cs => cs.Id == sessionId);

        if (session == null)
        {
            throw new ArgumentException($"Consultation session with ID {sessionId} not found");
        }

        var author = await _context.Users.FirstOrDefaultAsync(u => u.Id == authorId);

        var note = new ConsultationNote
        {
            ConsultationSessionId = sessionId,
            AuthorId = authorId,
            Content = content,
            Type = noteType,
            IsMarkdown = isMarkdown,
            IsPrivate = isPrivate,
            CreatedAt = DateTime.UtcNow,
            IsSharedWithPatient = !isPrivate
        };

        _context.ConsultationNotes.Add(note);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added note {NoteId} to session {SessionId}", note.Id, sessionId);

        return new ConsultationNoteDto
        {
            Id = note.Id,
            ConsultationSessionId = note.ConsultationSessionId,
            AuthorId = note.AuthorId,
            AuthorName = author != null ? $"{author.FirstName} {author.LastName}" : "Unknown",
            Content = note.Content,
            Type = note.Type,
            Title = note.Title,
            IsMarkdown = note.IsMarkdown,
            IsPrivate = note.IsPrivate,
            IsSharedWithPatient = note.IsSharedWithPatient,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }

    /// <summary>
    /// Update an existing consultation note
    /// </summary>
    public async Task<ConsultationNoteDto> UpdateConsultationNoteAsync(
        int noteId,
        string authorId,
        string content,
        bool isPrivate)
    {
        _logger.LogInformation("Updating note {NoteId} by user {AuthorId}", noteId, authorId);

        var note = await _context.ConsultationNotes
            .FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null)
        {
            throw new ArgumentException($"Consultation note with ID {noteId} not found");
        }

        // Only author or admin can update
        if (note.AuthorId != authorId)
        {
            throw new UnauthorizedAccessException("You do not have permission to update this note");
        }

        note.Content = content;
        note.IsPrivate = isPrivate;
        note.IsSharedWithPatient = !isPrivate;
        note.UpdatedAt = DateTime.UtcNow;
        note.Version++;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated note {NoteId}", noteId);

        var author = await _context.Users.FirstOrDefaultAsync(u => u.Id == note.AuthorId);

        return new ConsultationNoteDto
        {
            Id = note.Id,
            ConsultationSessionId = note.ConsultationSessionId,
            AuthorId = note.AuthorId,
            AuthorName = author != null ? $"{author.FirstName} {author.LastName}" : "Unknown",
            Content = note.Content,
            Type = note.Type,
            Title = note.Title,
            IsMarkdown = note.IsMarkdown,
            IsPrivate = note.IsPrivate,
            IsSharedWithPatient = note.IsSharedWithPatient,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }

    /// <summary>
    /// Delete a consultation note (soft delete)
    /// </summary>
    public async Task<bool> DeleteConsultationNoteAsync(int noteId, string authorId)
    {
        _logger.LogInformation("Deleting note {NoteId} by user {AuthorId}", noteId, authorId);

        var note = await _context.ConsultationNotes
            .FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null)
        {
            _logger.LogWarning("Note {NoteId} not found", noteId);
            return false;
        }

        // Only author or admin can delete
        if (note.AuthorId != authorId)
        {
            _logger.LogWarning("User {AuthorId} not authorized to delete note {NoteId}", authorId, noteId);
            return false;
        }

        // Soft delete
        note.IsDeleted = true;
        note.DeletedAt = DateTime.UtcNow;
        note.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted note {NoteId}", noteId);

        return true;
    }

    /// <summary>
    /// Get all notes for a consultation session
    /// </summary>
    public async Task<List<ConsultationNoteDto>> GetSessionNotesAsync(int sessionId)
    {
        _logger.LogInformation("Fetching notes for session {SessionId}", sessionId);

        var notes = await _context.ConsultationNotes
            .Where(n => n.ConsultationSessionId == sessionId && !n.IsDeleted)
            .Include(n => n.Author)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();

        var dtos = new List<ConsultationNoteDto>();
        foreach (var note in notes)
        {
            dtos.Add(new ConsultationNoteDto
            {
                Id = note.Id,
                ConsultationSessionId = note.ConsultationSessionId,
                AuthorId = note.AuthorId,
                AuthorName = note.Author != null ? $"{note.Author.FirstName} {note.Author.LastName}" : "Unknown",
                Content = note.Content,
                Type = note.Type,
                Title = note.Title,
                IsMarkdown = note.IsMarkdown,
                IsPrivate = note.IsPrivate,
                IsSharedWithPatient = note.IsSharedWithPatient,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            });
        }

        return dtos;
    }

    #endregion
}

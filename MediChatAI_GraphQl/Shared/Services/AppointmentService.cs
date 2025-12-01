using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Interfaces.Services.Appointment;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Features.Notifications.Hubs;
using MediChatAI_GraphQl.Features.Notifications.Services;
using System.Collections.Generic;

namespace MediChatAI_GraphQl.Shared.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AppointmentService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public AppointmentService(
        ApplicationDbContext context,
        ILogger<AppointmentService> logger,
        IHubContext<NotificationHub> hubContext,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.Id == id)
            .Select(a => MapToDto(a))
            .FirstOrDefaultAsync();
    }

    public async Task<List<AppointmentDto>> GetAppointmentsByPatientIdAsync(string patientId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDateTime)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<List<AppointmentDto>> GetAppointmentsByDoctorIdAsync(string doctorId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.AppointmentDateTime)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<List<AppointmentDto>> GetUpcomingAppointmentsByPatientIdAsync(string patientId)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.PatientId == patientId &&
                       a.AppointmentDateTime > now &&
                       a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDateTime)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<List<AppointmentDto>> GetUpcomingAppointmentsByDoctorIdAsync(string doctorId)
    {
        var now = DateTime.UtcNow;
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDateTime > now &&
                       a.Status != AppointmentStatus.Cancelled)
            .OrderBy(a => a.AppointmentDateTime)
            .Select(a => MapToDto(a))
            .ToListAsync();
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto)
    {
        var appointment = new Core.Entities.Appointment
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            AppointmentDateTime = dto.AppointmentDateTime,
            DurationMinutes = dto.DurationMinutes,
            Type = dto.Type,
            RoomNumber = dto.RoomNumber,
            IsVirtual = dto.IsVirtual,
            ReasonForVisit = dto.ReasonForVisit,
            ConsultationFee = dto.ConsultationFee,
            Status = AppointmentStatus.Confirmed,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return (await GetAppointmentByIdAsync(appointment.Id))!;
    }

    public async Task<AppointmentDto> UpdateAppointmentAsync(UpdateAppointmentDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(dto.Id);
        if (appointment == null)
            throw new System.Collections.Generic.KeyNotFoundException("Appointment not found");

        if (dto.AppointmentDateTime.HasValue)
        {
#if DEBUG
            Console.WriteLine($"🔧 [BACKEND] Updating Appointment {dto.Id}:");
            Console.WriteLine($"   Old DateTime: {appointment.AppointmentDateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"   New DateTime: {dto.AppointmentDateTime.Value:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"   New DateTime Kind: {dto.AppointmentDateTime.Value.Kind}");
#endif
            appointment.AppointmentDateTime = dto.AppointmentDateTime.Value;
        }

        if (dto.Status.HasValue)
            appointment.Status = dto.Status.Value;

        if (!string.IsNullOrEmpty(dto.RoomNumber))
            appointment.RoomNumber = dto.RoomNumber;

        if (!string.IsNullOrEmpty(dto.DoctorNotes))
            appointment.DoctorNotes = dto.DoctorNotes;

        if (!string.IsNullOrEmpty(dto.PatientNotes))
            appointment.PatientNotes = dto.PatientNotes;

        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

#if DEBUG
        Console.WriteLine($"✅ [BACKEND] Appointment {dto.Id} updated successfully");
        Console.WriteLine($"   Saved DateTime: {appointment.AppointmentDateTime:yyyy-MM-dd HH:mm:ss}");
#endif

        var result = (await GetAppointmentByIdAsync(appointment.Id))!;

#if DEBUG
        Console.WriteLine($"📤 [BACKEND] Returning AppointmentDto:");
        Console.WriteLine($"   ID: {result.Id}");
        Console.WriteLine($"   DateTime: {result.AppointmentDateTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"   TimeOfDay: {result.AppointmentDateTime.TimeOfDay}");
#endif

        return result;
    }

    public async Task<bool> CancelAppointmentAsync(int id, string cancellationReason)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return false;

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.CancellationReason = cancellationReason;
        appointment.CancelledAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CompleteAppointmentAsync(int id, string? doctorNotes)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return false;

        appointment.Status = AppointmentStatus.Completed;
        if (!string.IsNullOrEmpty(doctorNotes))
            appointment.DoctorNotes = doctorNotes;
        appointment.CompletedAt = DateTime.UtcNow;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AppointmentRequestDto?> GetAppointmentRequestByIdAsync(int id)
    {
        return await _context.AppointmentRequests
            .Where(r => r.Id == id)
            .Select(r => MapRequestToDto(r))
            .FirstOrDefaultAsync();
    }

    public async Task<List<AppointmentRequestDto>> GetPendingRequestsByDoctorIdAsync(string doctorId)
    {
        _logger.LogInformation("Getting pending appointment requests for doctor {DoctorId}", doctorId);
        
        // First, let's check all appointment requests to debug
        var allRequests = await _context.AppointmentRequests.ToListAsync();
        _logger.LogInformation("Total appointment requests in database: {Count}", allRequests.Count);
        
        var pendingRequests = allRequests.Where(r => r.Status == RequestStatus.Pending).ToList();
        _logger.LogInformation("Total pending requests: {Count}", pendingRequests.Count);
        
        var doctorRequests = pendingRequests.Where(r => r.PreferredDoctorId == doctorId || r.PreferredDoctorId == null).ToList();
        _logger.LogInformation("Pending requests for doctor {DoctorId}: {Count}", doctorId, doctorRequests.Count);
        
        // Log details of each request for debugging
        foreach (var req in doctorRequests)
        {
            _logger.LogInformation("Request {Id}: Patient={PatientId}, PreferredDoctor={PreferredDoctorId}, Status={Status}, Urgent={IsUrgent}", 
                req.Id, req.PatientId, req.PreferredDoctorId ?? "null", req.Status, req.IsUrgent);
        }
        
        return await _context.AppointmentRequests
            .Include(r => r.PreferredDoctor) // Include doctor info for mapping
            .Where(r => (r.PreferredDoctorId == doctorId || r.PreferredDoctorId == null) &&
                       r.Status == RequestStatus.Pending)
            .OrderByDescending(r => r.IsUrgent)
            .ThenByDescending(r => r.RequestedAt)
            .Select(r => MapRequestToDto(r))
            .ToListAsync();
    }

    public async Task<List<AppointmentRequestDto>> GetRequestsByPatientIdAsync(string patientId)
    {
        return await _context.AppointmentRequests
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => MapRequestToDto(r))
            .ToListAsync();
    }

    public async Task<AppointmentRequestDto> CreateAppointmentRequestAsync(CreateAppointmentRequestDto dto)
    {
        try
        {
            _logger.LogInformation("Creating appointment request for patient {PatientId}", dto.PatientId);

            // Convert empty strings to null for optional fields
            var request = new AppointmentRequest
            {
                PatientId = dto.PatientId,
                PreferredDoctorId = string.IsNullOrWhiteSpace(dto.PreferredDoctorId) ? null : dto.PreferredDoctorId,
                FullName = dto.FullName,
                Age = dto.Age,
                Gender = dto.Gender,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                AlternatePhoneNumber = string.IsNullOrWhiteSpace(dto.AlternatePhoneNumber) ? null : dto.AlternatePhoneNumber,
                Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address,
                City = string.IsNullOrWhiteSpace(dto.City) ? null : dto.City,
                State = string.IsNullOrWhiteSpace(dto.State) ? null : dto.State,
                ZipCode = string.IsNullOrWhiteSpace(dto.ZipCode) ? null : dto.ZipCode,
                EmergencyContactName = string.IsNullOrWhiteSpace(dto.EmergencyContactName) ? null : dto.EmergencyContactName,
                EmergencyContactPhone = string.IsNullOrWhiteSpace(dto.EmergencyContactPhone) ? null : dto.EmergencyContactPhone,
                EmergencyContactRelation = string.IsNullOrWhiteSpace(dto.EmergencyContactRelation) ? null : dto.EmergencyContactRelation,
                BloodType = string.IsNullOrWhiteSpace(dto.BloodType) ? null : dto.BloodType,
                Allergies = string.IsNullOrWhiteSpace(dto.Allergies) ? null : dto.Allergies,
                CurrentMedications = string.IsNullOrWhiteSpace(dto.CurrentMedications) ? null : dto.CurrentMedications,
                PastMedicalConditions = string.IsNullOrWhiteSpace(dto.PastMedicalConditions) ? null : dto.PastMedicalConditions,
                ChronicDiseases = string.IsNullOrWhiteSpace(dto.ChronicDiseases) ? null : dto.ChronicDiseases,
                PreviousSurgeries = string.IsNullOrWhiteSpace(dto.PreviousSurgeries) ? null : dto.PreviousSurgeries,
                FamilyMedicalHistory = string.IsNullOrWhiteSpace(dto.FamilyMedicalHistory) ? null : dto.FamilyMedicalHistory,
                SymptomDescription = dto.SymptomDescription,
                SymptomDuration = string.IsNullOrWhiteSpace(dto.SymptomDuration) ? null : dto.SymptomDuration,
                SymptomSeverity = string.IsNullOrWhiteSpace(dto.SymptomSeverity) ? null : dto.SymptomSeverity,
                ReasonForVisit = string.IsNullOrWhiteSpace(dto.ReasonForVisit) ? null : dto.ReasonForVisit,
                PreferredAppointmentType = dto.PreferredAppointmentType,
                VoiceRecordingUrl = string.IsNullOrWhiteSpace(dto.VoiceRecordingUrl) ? null : dto.VoiceRecordingUrl,
                VoiceRecordingTranscript = string.IsNullOrWhiteSpace(dto.VoiceRecordingTranscript) ? null : dto.VoiceRecordingTranscript,

                // Multi-modal arrays - serialize to JSON
                PhotoUrls = dto.PhotoUrls != null && dto.PhotoUrls.Any()
                    ? System.Text.Json.JsonSerializer.Serialize(dto.PhotoUrls)
                    : null,
                VideoUrls = dto.VideoUrls != null && dto.VideoUrls.Any()
                    ? System.Text.Json.JsonSerializer.Serialize(dto.VideoUrls)
                    : null,
                MedicalDocumentUrls = dto.MedicalDocumentUrls != null && dto.MedicalDocumentUrls.Any()
                    ? System.Text.Json.JsonSerializer.Serialize(dto.MedicalDocumentUrls)
                    : null,
                InsuranceCardUrls = dto.InsuranceCardUrls != null && dto.InsuranceCardUrls.Any()
                    ? System.Text.Json.JsonSerializer.Serialize(dto.InsuranceCardUrls)
                    : null,

                InsuranceProvider = string.IsNullOrWhiteSpace(dto.InsuranceProvider) ? null : dto.InsuranceProvider,
                InsurancePolicyNumber = string.IsNullOrWhiteSpace(dto.InsurancePolicyNumber) ? null : dto.InsurancePolicyNumber,
                InsuranceGroupNumber = string.IsNullOrWhiteSpace(dto.InsuranceGroupNumber) ? null : dto.InsuranceGroupNumber,
                InsuranceCardFrontUrl = string.IsNullOrWhiteSpace(dto.InsuranceCardFrontUrl) ? null : dto.InsuranceCardFrontUrl,
                InsuranceCardBackUrl = string.IsNullOrWhiteSpace(dto.InsuranceCardBackUrl) ? null : dto.InsuranceCardBackUrl,
                InsuranceExpiryDate = dto.InsuranceExpiryDate,
                PaymentMethod = string.IsNullOrWhiteSpace(dto.PaymentMethod) ? null : dto.PaymentMethod,
                PreferredDate = dto.PreferredDate,
                PreferredTimeSlot = string.IsNullOrWhiteSpace(dto.PreferredTimeSlot) ? null : dto.PreferredTimeSlot,
                IsUrgent = dto.IsUrgent,
                PatientAdditionalNotes = string.IsNullOrWhiteSpace(dto.PatientAdditionalNotes) ? null : dto.PatientAdditionalNotes,
                Status = RequestStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.AppointmentRequests.Add(request);
            _logger.LogInformation("Saving appointment request to database");
            await _context.SaveChangesAsync();
            _logger.LogInformation("Appointment request saved with ID {RequestId}", request.Id);

            var requestDto = await GetAppointmentRequestByIdAsync(request.Id);
            if (requestDto == null)
            {
                throw new Exception($"Failed to retrieve created appointment request with ID {request.Id}");
            }

        // Send notifications and emails
        try
        {
            // Get patient and doctor information
            var patient = await _context.Users.FindAsync(dto.PatientId);
            var doctor = dto.PreferredDoctorId != null ? await _context.Users.FindAsync(dto.PreferredDoctorId) : null;

            // 1. Send confirmation email to patient
            if (patient != null)
            {
                await _emailService.SendAppointmentRequestConfirmationAsync(
                    patient.Email!,
                    patient.FirstName,
                    requestDto.Id,
                    requestDto.PreferredDate?.ToString("MMM dd, yyyy") ?? "To be determined",
                    requestDto.SymptomDescription
                );
                _logger.LogInformation("Sent appointment request confirmation email to patient {PatientId}", dto.PatientId);
            }

            // 2. Send notification email to doctor (if preferred doctor is specified)
            if (doctor != null && dto.PreferredDoctorId != null)
            {
                await _emailService.SendNewAppointmentRequestNotificationAsync(
                    doctor.Email!,
                    doctor.FirstName,
                    requestDto.FullName,
                    requestDto.PreferredDate?.ToString("MMM dd, yyyy") ?? "To be determined",
                    requestDto.SymptomDescription,
                    requestDto.IsUrgent
                );
                _logger.LogInformation("Sent appointment request notification email to doctor {DoctorId}", dto.PreferredDoctorId);

                // 3. Send SignalR notification to doctor
                await _hubContext.Clients.Group($"user_{dto.PreferredDoctorId}")
                    .SendAsync("NewAppointmentRequest", new
                    {
                        RequestId = requestDto.Id,
                        PatientName = requestDto.FullName,
                        PatientId = requestDto.PatientId,
                        Symptoms = requestDto.SymptomDescription,
                        PreferredDate = requestDto.PreferredDate,
                        IsUrgent = requestDto.IsUrgent,
                        RequestedAt = requestDto.RequestedAt
                    });
                _logger.LogInformation("Sent SignalR notification to doctor {DoctorId}", dto.PreferredDoctorId);

                // 4. Create in-app notification for doctor
                await _notificationService.CreateNotificationAsync(
                    dto.PreferredDoctorId,
                    "New Appointment Request",
                    $"{requestDto.FullName} has requested an appointment. {(requestDto.IsUrgent ? "⚠️ URGENT" : "")}",
                    NotificationType.Info,
                    NotificationCategory.Appointment,
                    NotificationPriority.Normal,
                    $"/doctor/appointments/requests/{requestDto.Id}"
                );
            }
            else
            {
                // If no preferred doctor, notify all doctors or admin
                _logger.LogInformation("No preferred doctor specified for appointment request {RequestId}", requestDto.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notifications for appointment request {RequestId}", requestDto.Id);
            // Don't throw - appointment is still created, just notification failed
        }

            return requestDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating appointment request: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<AppointmentDto> ApproveAppointmentRequestAsync(ReviewAppointmentRequestDto dto)
    {
        var request = await _context.AppointmentRequests.FindAsync(dto.RequestId);
        if (request == null)
            throw new System.Collections.Generic.KeyNotFoundException("Appointment request not found");

        // Create the appointment with comprehensive patient details from the request
        var appointment = new Core.Entities.Appointment
        {
            PatientId = request.PatientId,
            DoctorId = dto.ReviewedByDoctorId,
            // FIXED: Combine date and time slot properly
            AppointmentDateTime = dto.AppointmentDateTime
                ?? CombineDateAndTimeSlot(request.PreferredDate, request.PreferredTimeSlot)
                ?? DateTime.UtcNow.AddDays(1),
            DurationMinutes = dto.DurationMinutes ?? 30,
            Type = request.PreferredAppointmentType,
            RoomNumber = dto.RoomNumber,

            // Copy comprehensive patient details and medical history
            ReasonForVisit = !string.IsNullOrWhiteSpace(request.ReasonForVisit)
                ? request.ReasonForVisit
                : request.SymptomDescription,

            // Compile patient notes with all relevant medical information
            PatientNotes = BuildPatientNotes(request),

            // Determine if virtual based on appointment type or preference
            IsVirtual = request.PreferredAppointmentType == AppointmentType.Consultation,

            Status = AppointmentStatus.Confirmed,
            CreatedAt = DateTime.UtcNow,
            ConfirmedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Update the request with link to created appointment
        request.Status = RequestStatus.Approved;
        request.ReviewedByDoctorId = dto.ReviewedByDoctorId;
        request.ReviewNotes = dto.ReviewNotes;
        request.ReviewedAt = DateTime.UtcNow;
        request.AppointmentId = appointment.Id;

        await _context.SaveChangesAsync();

        // Send notification to patient and create initial conversation
        try
        {
            var doctor = await _context.Users.FindAsync(dto.ReviewedByDoctorId);
            var patient = await _context.Users.FindAsync(request.PatientId);

            if (doctor != null && patient != null)
            {
                // Create in-app notification for patient
                await _notificationService.CreateNotificationAsync(
                    request.PatientId,
                    "Appointment Confirmed",
                    $"Your appointment with Dr. {doctor.FirstName} {doctor.LastName} has been confirmed for {appointment.AppointmentDateTime:MMM dd, yyyy 'at' hh:mm tt}",
                    NotificationType.Success,
                    NotificationCategory.Appointment,
                    NotificationPriority.High,
                    $"/patient/appointments/{appointment.Id}"
                );

                // Create initial conversation message from doctor to patient
                var initialMessage = new DoctorMessage
                {
                    SenderId = dto.ReviewedByDoctorId,
                    ReceiverId = request.PatientId,
                    Content = $"Hello {patient.FirstName}, your appointment has been confirmed for {appointment.AppointmentDateTime:MMM dd, yyyy 'at' hh:mm tt}. Feel free to message me if you have any questions before our appointment.",
                    MessageType = MessageType.System,
                    Status = MessageStatus.Sent,
                    SentAt = DateTime.UtcNow
                };

                _context.DoctorMessages.Add(initialMessage);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created initial conversation message between patient {PatientId} and doctor {DoctorId}", request.PatientId, dto.ReviewedByDoctorId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending appointment confirmation notification or creating conversation");
        }

        return (await GetAppointmentByIdAsync(appointment.Id))!;
    }

    /// <summary>
    /// Build comprehensive patient notes from appointment request data
    /// </summary>
    private static string BuildPatientNotes(AppointmentRequest request)
    {
        var notes = new List<string>();

        // Patient Information
        notes.Add($"=== PATIENT INFORMATION ===");
        notes.Add($"Name: {request.FullName}");
        notes.Add($"Age: {request.Age}, Gender: {request.Gender}");
        notes.Add($"Contact: {request.PhoneNumber}");
        if (!string.IsNullOrWhiteSpace(request.Email))
            notes.Add($"Email: {request.Email}");

        // Emergency Contact
        if (!string.IsNullOrWhiteSpace(request.EmergencyContactName))
        {
            notes.Add($"\nEmergency Contact: {request.EmergencyContactName} ({request.EmergencyContactRelation})");
            notes.Add($"Emergency Phone: {request.EmergencyContactPhone}");
        }

        // Medical History
        notes.Add($"\n=== MEDICAL HISTORY ===");
        if (!string.IsNullOrWhiteSpace(request.BloodType))
            notes.Add($"Blood Type: {request.BloodType}");
        if (!string.IsNullOrWhiteSpace(request.Allergies))
            notes.Add($"Allergies: {request.Allergies}");
        if (!string.IsNullOrWhiteSpace(request.CurrentMedications))
            notes.Add($"Current Medications: {request.CurrentMedications}");
        if (!string.IsNullOrWhiteSpace(request.ChronicDiseases))
            notes.Add($"Chronic Diseases: {request.ChronicDiseases}");
        if (!string.IsNullOrWhiteSpace(request.PastMedicalConditions))
            notes.Add($"Past Medical Conditions: {request.PastMedicalConditions}");
        if (!string.IsNullOrWhiteSpace(request.PreviousSurgeries))
            notes.Add($"Previous Surgeries: {request.PreviousSurgeries}");
        if (!string.IsNullOrWhiteSpace(request.FamilyMedicalHistory))
            notes.Add($"Family Medical History: {request.FamilyMedicalHistory}");

        // Symptoms
        notes.Add($"\n=== CURRENT SYMPTOMS ===");
        notes.Add($"Description: {request.SymptomDescription}");
        if (!string.IsNullOrWhiteSpace(request.SymptomDuration))
            notes.Add($"Duration: {request.SymptomDuration}");
        if (!string.IsNullOrWhiteSpace(request.SymptomSeverity))
            notes.Add($"Severity: {request.SymptomSeverity}");

        // Insurance Information
        if (!string.IsNullOrWhiteSpace(request.InsuranceProvider))
        {
            notes.Add($"\n=== INSURANCE ===");
            notes.Add($"Provider: {request.InsuranceProvider}");
            if (!string.IsNullOrWhiteSpace(request.InsurancePolicyNumber))
                notes.Add($"Policy #: {request.InsurancePolicyNumber}");
            if (!string.IsNullOrWhiteSpace(request.InsuranceGroupNumber))
                notes.Add($"Group #: {request.InsuranceGroupNumber}");
        }

        // Additional Notes
        if (!string.IsNullOrWhiteSpace(request.PatientAdditionalNotes))
        {
            notes.Add($"\n=== ADDITIONAL PATIENT NOTES ===");
            notes.Add(request.PatientAdditionalNotes);
        }

        // Multi-modal Data References
        var hasMultiModal = false;
        if (!string.IsNullOrWhiteSpace(request.VoiceRecordingUrl))
        {
            if (!hasMultiModal)
            {
                notes.Add($"\n=== ATTACHED MEDIA ===");
                hasMultiModal = true;
            }
            notes.Add($"Voice Recording: Available");
            if (!string.IsNullOrWhiteSpace(request.VoiceRecordingTranscript))
                notes.Add($"Transcript: {request.VoiceRecordingTranscript}");
        }
        if (!string.IsNullOrWhiteSpace(request.PhotoUrls))
        {
            if (!hasMultiModal)
            {
                notes.Add($"\n=== ATTACHED MEDIA ===");
                hasMultiModal = true;
            }
            notes.Add($"Photos: Attached");
        }
        if (!string.IsNullOrWhiteSpace(request.MedicalDocumentUrls))
        {
            if (!hasMultiModal)
            {
                notes.Add($"\n=== ATTACHED MEDIA ===");
                hasMultiModal = true;
            }
            notes.Add($"Medical Documents: Attached");
        }

        notes.Add($"\n[Note: Full request details available in AppointmentRequest #{request.Id}]");

        return string.Join("\n", notes);
    }

    public async Task<bool> RejectAppointmentRequestAsync(int requestId, string doctorId, string reason)
    {
        var request = await _context.AppointmentRequests.FindAsync(requestId);
        if (request == null)
            return false;

        request.Status = RequestStatus.Rejected;
        request.ReviewedByDoctorId = doctorId;
        request.RejectionReason = reason;
        request.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DoctorAvailabilityDto>> GetDoctorAvailabilityAsync(string doctorId, DateTime startDate, DateTime endDate)
    {
        // Get all appointments in the date range
        var appointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDateTime >= startDate &&
                       a.AppointmentDateTime <= endDate &&
                       a.Status != AppointmentStatus.Cancelled)
            .ToListAsync();

        var doctor = await _context.Users.FindAsync(doctorId);
        var availabilityList = new List<DoctorAvailabilityDto>();

        // Generate availability for each day
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var slots = GenerateTimeSlots(date, appointments);
            availabilityList.Add(new DoctorAvailabilityDto
            {
                DoctorId = doctorId,
                DoctorName = $"{doctor?.FirstName} {doctor?.LastName}",
                Specialization = doctor?.Specialization,
                Date = date,
                AvailableSlots = slots
            });
        }

        return availabilityList;
    }

    public async Task<List<TimeSlotDto>> GetAvailableTimeSlotsAsync(string doctorId, DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = date.Date.AddDays(1);

        var appointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDateTime >= startOfDay &&
                       a.AppointmentDateTime < endOfDay &&
                       a.Status != AppointmentStatus.Cancelled)
            .ToListAsync();

        return GenerateTimeSlots(date, appointments);
    }

    private List<TimeSlotDto> GenerateTimeSlots(DateTime date, List<Core.Entities.Appointment> appointments)
    {
        var slots = new List<TimeSlotDto>();

        // Generate slots from 9 AM to 5 PM in 30-minute intervals
        var currentTime = date.Date.AddHours(9);
        var endTime = date.Date.AddHours(17);

        while (currentTime < endTime)
        {
            var slotEnd = currentTime.AddMinutes(30);
            var isBooked = appointments.Any(a =>
                a.AppointmentDateTime < slotEnd &&
                a.AppointmentDateTime.AddMinutes(a.DurationMinutes) > currentTime);

            slots.Add(new TimeSlotDto
            {
                StartTime = currentTime,
                EndTime = slotEnd,
                IsAvailable = !isBooked && currentTime > DateTime.UtcNow
            });

            currentTime = slotEnd;
        }

        return slots;
    }

    private static AppointmentDto MapToDto(Core.Entities.Appointment a)
    {
        return new AppointmentDto
        {
            Id = a.Id,
            PatientId = a.PatientId,
            PatientName = a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : "",
            PatientEmail = a.Patient?.Email,
            PatientPhone = a.Patient?.PhoneNumber,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor != null ? $"{a.Doctor.FirstName} {a.Doctor.LastName}" : "",
            DoctorSpecialization = a.Doctor?.Specialization,
            AppointmentDateTime = a.AppointmentDateTime,
            DurationMinutes = a.DurationMinutes,
            Status = a.Status,
            Type = a.Type,
            RoomNumber = a.RoomNumber,
            Location = a.Location,
            IsVirtual = a.IsVirtual,
            MeetingLink = a.MeetingLink,
            ReasonForVisit = a.ReasonForVisit,
            DoctorNotes = a.DoctorNotes,
            PatientNotes = a.PatientNotes,
            ConsultationFee = a.ConsultationFee,
            IsPaid = a.IsPaid,
            CreatedAt = a.CreatedAt
        };
    }

    private static AppointmentRequestDto MapRequestToDto(AppointmentRequest r)
    {
        return new AppointmentRequestDto
        {
            Id = r.Id,
            PatientId = r.PatientId,
            PreferredDoctorId = r.PreferredDoctorId,
            PreferredDoctorName = r.PreferredDoctor != null ? $"{r.PreferredDoctor.FirstName} {r.PreferredDoctor.LastName}" : null,
            FullName = r.FullName,
            Age = r.Age,
            Gender = r.Gender,
            Email = r.Email,
            PhoneNumber = r.PhoneNumber,
            BloodType = r.BloodType,
            Allergies = r.Allergies,
            SymptomDescription = r.SymptomDescription,
            SymptomSeverity = r.SymptomSeverity,
            InsuranceProvider = r.InsuranceProvider,
            PreferredDate = r.PreferredDate,
            PreferredTimeSlot = r.PreferredTimeSlot,
            IsUrgent = r.IsUrgent,
            Status = r.Status,
            RequestedAt = r.RequestedAt,
            AppointmentId = r.AppointmentId,
            // Add ReasonForVisit field that the frontend expects
            ReasonForVisit = r.ReasonForVisit ?? r.SymptomDescription
        };
    }

    /// <summary>
    /// Generate deterministic conversation ID from two user IDs
    /// </summary>
    private static Guid GenerateConversationId(string userId1, string userId2)
    {
        var sortedIds = new[] { userId1, userId2 }.OrderBy(id => id).ToArray();
        var combined = $"{sortedIds[0]}_{sortedIds[1]}";
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
            return new Guid(hash);
        }
    }

    /// <summary>
    /// Combine date and time slot string into a single DateTime
    /// </summary>
    private static DateTime? CombineDateAndTimeSlot(DateTime? date, string? timeSlot)
    {
        if (!date.HasValue || string.IsNullOrWhiteSpace(timeSlot))
            return date;

        // Try to parse the time slot (format: "09:30 AM" or "2:00 PM")
        if (DateTime.TryParse(timeSlot, out var time))
        {
            return date.Value.Date.Add(time.TimeOfDay);
        }

        // If parsing fails, return the date as-is
        return date;
    }
}

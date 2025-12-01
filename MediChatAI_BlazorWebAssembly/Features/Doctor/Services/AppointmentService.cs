using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ITimeBlockService _timeBlockService;

    public AppointmentService(IGraphQLService graphQLService, ITimeBlockService timeBlockService)
    {
        _graphQLService = graphQLService;
        _timeBlockService = timeBlockService;
    }

    // Get all appointments for the logged-in doctor
    public async Task<List<AppointmentDto>> GetDoctorAppointmentsAsync()
    {
        const string query = @"
            query {
                doctorAppointments {
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
            }";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("doctorAppointments"))
            {
                var json = JsonSerializer.Serialize(response["doctorAppointments"]);
                Console.WriteLine($"GraphQL Response JSON: {json}");
                var appointments = JsonSerializer.Deserialize<List<AppointmentDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AppointmentDto>();

                // Log each appointment's DateTime
                foreach (var apt in appointments)
                {
                    Console.WriteLine($"Appointment {apt.Id}: AppointmentDateTime={apt.AppointmentDateTime}, TimeOfDay={apt.AppointmentDateTime.TimeOfDay}");
                }

                return appointments;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching doctor appointments: {ex.Message}");
        }

        return new List<AppointmentDto>();
    }

    // Get pending appointment requests for the doctor
    public async Task<List<AppointmentRequestDto>> GetPendingAppointmentRequestsAsync()
    {
        const string query = @"
            query {
                pendingAppointmentRequests {
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
            }";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            
            if (response != null && response.ContainsKey("pendingAppointmentRequests"))
            {
                var json = JsonSerializer.Serialize(response["pendingAppointmentRequests"]);
                return JsonSerializer.Deserialize<List<AppointmentRequestDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<AppointmentRequestDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching pending appointment requests: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        return new List<AppointmentRequestDto>();
    }

    // Get single appointment by ID
    public async Task<AppointmentDto?> GetAppointmentDetailsByIdAsync(string appointmentId)
    {
        if (!int.TryParse(appointmentId, out var id))
        {
            Console.WriteLine($"❌ GetAppointmentDetailsByIdAsync: Invalid appointment ID format: {appointmentId}");
            return null;
        }

        var query = $@"
            query {{
                appointment(id: {id}) {{
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
                }}
            }}";

        try
        {
            Console.WriteLine($"🔍 Fetching appointment details for ID: {id}");
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);

            if (response != null)
            {
                Console.WriteLine($"📦 GraphQL response keys: {string.Join(", ", response.Keys)}");

                if (response.ContainsKey("appointment") && response["appointment"] != null)
                {
                    var json = JsonSerializer.Serialize(response["appointment"]);
                    Console.WriteLine($"📄 Appointment JSON: {json}");

                    var appointment = JsonSerializer.Deserialize<AppointmentDto>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (appointment != null)
                    {
                        Console.WriteLine($"✅ Successfully loaded appointment: {appointment.PatientName} on {appointment.AppointmentDateTime}");
                        return appointment;
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to deserialize appointment from JSON");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Response does not contain 'appointment' key or it's null");
                }
            }
            else
            {
                Console.WriteLine($"❌ GraphQL response is null");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GetAppointmentDetailsByIdAsync: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        return null;
    }

    // Approve appointment request
    public async Task<AppointmentDto?> ApproveAppointmentRequestAsync(ReviewAppointmentRequestInput input)
    {
        var mutation = $@"
            mutation {{
                approveAppointmentRequest(input: {{
                    requestId: {input.RequestId}
                    status: APPROVED
                    reviewedByDoctorId: ""725dc571-4d22-4a7f-b09a-585ec57dc75a""
                    reviewNotes: {(string.IsNullOrEmpty(input.ReviewNotes) ? "null" : $"\"{input.ReviewNotes}\"")}
                    appointmentDateTime: {(input.AppointmentDateTime.HasValue ? $"\"{input.AppointmentDateTime.Value:yyyy-MM-ddTHH:mm:ssZ}\"" : "null")}
                    roomNumber: {(string.IsNullOrEmpty(input.RoomNumber) ? "null" : $"\"{input.RoomNumber}\"")}
                    durationMinutes: {(input.DurationMinutes.HasValue ? input.DurationMinutes.Value.ToString() : "null")}
                }}) {{
                    id
                    patientName
                    appointmentDateTime
                    status
                }}
            }}";

        var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(mutation);
        if (response != null && response.ContainsKey("approveAppointmentRequest"))
        {
            var json = JsonSerializer.Serialize(response["approveAppointmentRequest"]);
            return JsonSerializer.Deserialize<AppointmentDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        return null;
    }

    // Reject appointment request
    public async Task<bool> RejectAppointmentRequestAsync(int requestId, string reason)
    {
        var mutation = $@"
            mutation {{
                rejectAppointmentRequest(requestId: {requestId}, reason: ""{reason}"")
            }}";

        var response = await _graphQLService.SendQueryAsync<GraphQLResponse<bool>>(mutation);
        return response?.Data?.RejectAppointmentRequest ?? false;
    }

    // Mark appointment as completed
    public async Task<bool> MarkAsCompletedAsync(string appointmentId)
    {
        if (!int.TryParse(appointmentId, out var id))
            return false;

        var mutation = $@"
            mutation {{
                completeAppointment(appointmentId: {id}, doctorNotes: null)
            }}";

        var response = await _graphQLService.SendQueryAsync<GraphQLResponse<bool>>(mutation);
        return response?.Data?.CompleteAppointment ?? false;
    }

    // Cancel appointment
    public async Task<bool> CancelAppointmentWithReasonAsync(string appointmentId, string reason)
    {
        if (!int.TryParse(appointmentId, out var id))
            return false;

        var mutation = $@"
            mutation {{
                cancelAppointment(appointmentId: {id}, reason: ""{reason}"")
            }}";

        var response = await _graphQLService.SendQueryAsync<GraphQLResponse<bool>>(mutation);
        return response?.Data?.CancelAppointment ?? false;
    }

    // Get appointments filtered by date range
    private async Task<List<AppointmentModel>> GetAppointmentsInternalAsync(AppointmentFilterModel filter)
    {
        var dtoList = await GetAppointmentsFilteredAsync(filter);
        return dtoList.Select(a => ValidateAndMapAppointment(a, "Internal")).ToList();
    }

    public async Task<List<AppointmentDto>> GetAppointmentsFilteredAsync(AppointmentFilterModel filter)
    {
        var allAppointments = await GetDoctorAppointmentsAsync();

        // Apply filters in memory
        var query = allAppointments.AsQueryable();

        if (filter.StartDate.HasValue)
            query = query.Where(a => a.AppointmentDateTime.Date >= filter.StartDate.Value.Date);

        if (filter.EndDate.HasValue)
            query = query.Where(a => a.AppointmentDateTime.Date <= filter.EndDate.Value.Date);

        if (filter.Statuses.Any())
            query = query.Where(a => filter.Statuses.Contains(a.Status));

        if (filter.Types.Any())
            query = query.Where(a => filter.Types.Contains(a.Type));

        if (!string.IsNullOrEmpty(filter.PatientId))
            query = query.Where(a => a.PatientId == filter.PatientId);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            var searchLower = filter.SearchTerm.ToLower();
            query = query.Where(a =>
                a.PatientName.ToLower().Contains(searchLower) ||
                (a.PatientPhone != null && a.PatientPhone.Contains(searchLower)) ||
                (a.PatientEmail != null && a.PatientEmail.ToLower().Contains(searchLower)));
        }

        if (filter.IncludeVirtualOnly)
            query = query.Where(a => a.IsVirtual);

        if (!filter.IncludeCancelled)
            query = query.Where(a => a.Status != "CANCELLED" && a.Status != "Cancelled");

        return query.OrderBy(a => a.AppointmentDateTime).ToList();
    }

    // Day schedule
    public async Task<DayScheduleModel> GetDayScheduleAsync(DateTime date)
    {
        var appointments = await GetAppointmentsFilteredAsync(new AppointmentFilterModel
        {
            StartDate = date.Date,
            EndDate = date.Date.AddDays(1).AddTicks(-1)
        });

        var dayAppointments = appointments.Select(a => ValidateAndMapAppointment(a, "DaySchedule"))
            .OrderBy(a => a.StartTime)
            .ToList();

        // Fetch time blocks for this day
        var dayStart = date.Date;
        var dayEnd = date.Date.AddDays(1);
        var timeBlocks = await _timeBlockService.GetMyTimeBlocksByDateRangeAsync(dayStart, dayEnd);
        var dayTimeBlocks = timeBlocks.Where(tb => tb.Date.Date == date.Date && tb.IsActive).ToList();

        var schedule = new DayScheduleModel
        {
            Date = date,
            Appointments = dayAppointments,
            TimeBlocks = dayTimeBlocks,
            AvailableSlots = await GetAvailableSlotsAsync(date)
        };

        return schedule;
    }

    // Week schedule
    public async Task<WeekScheduleModel> GetWeekScheduleAsync(DateTime weekStartDate)
    {
        var weekStart = weekStartDate.Date;

        // Use GraphQL query to get week schedule with appointments AND time blocks
        var dto = await GetMyWeekScheduleAsync(weekStart);

        if (dto == null)
        {
            // Fallback to empty schedule if GraphQL fails
            return new WeekScheduleModel
            {
                WeekStartDate = weekStart,
                WeekEndDate = weekStart.AddDays(7),
                Days = Enumerable.Range(0, 7).Select(i => new DayScheduleModel
                {
                    Date = weekStart.AddDays(i),
                    Appointments = new List<AppointmentModel>(),
                    TimeBlocks = new List<TimeBlockDto>(),
                    AvailableSlots = new List<TimeSlotModel>()
                }).ToList()
            };
        }

        // Create week schedule model
        var weekSchedule = new WeekScheduleModel
        {
            WeekStartDate = dto.WeekStartDate,
            WeekEndDate = dto.WeekEndDate,
            Days = new List<DayScheduleModel>()
        };

        // Organize appointments and time blocks by day
        for (int i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i);

            // Filter appointments for this day
            var dayAppointments = dto.Appointments
                .Where(a => a.AppointmentDateTime.Date == date)
                .Select(a =>
                {
                    var startTime = a.AppointmentDateTime.TimeOfDay;
                    var endTime = startTime.Add(TimeSpan.FromMinutes(a.DurationMinutes));

#if DEBUG
                    Console.WriteLine($"📋 [MAPPING] Appointment {a.Id} for {date:yyyy-MM-dd}:");
                    Console.WriteLine($"   AppointmentDateTime: {a.AppointmentDateTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"   TimeOfDay: {a.AppointmentDateTime.TimeOfDay}");
                    Console.WriteLine($"   StartTime: {startTime}");
                    Console.WriteLine($"   EndTime: {endTime}");

                    if (startTime == TimeSpan.Zero)
                    {
                        Console.WriteLine($"   ⚠️ WARNING: Midnight time detected for appointment {a.Id}!");
                        Console.WriteLine($"   This appointment will NOT render in any 7 AM - 9 PM time slot!");
                    }
#endif

                    return new AppointmentModel
                    {
                        AppointmentId = a.Id.ToString(),
                        PatientId = a.PatientId,
                        PatientName = a.PatientName,
                        PatientPhone = a.PatientPhone ?? string.Empty,
                        PatientEmail = a.PatientEmail ?? string.Empty,
                        DoctorId = a.DoctorId,
                        DoctorName = a.DoctorName ?? string.Empty,
                        AppointmentDate = a.AppointmentDateTime.Date,
                        StartTime = startTime,
                        EndTime = endTime,
                        DurationMinutes = a.DurationMinutes,
                        Type = a.Type,
                        Status = a.Status,
                        Reason = a.ReasonForVisit,
                        Notes = a.DoctorNotes
                    };
                })
                .OrderBy(a => a.StartTime)
                .ToList();

            // Filter time blocks for this day
            var dayTimeBlocks = dto.TimeBlocks
                .Where(tb => tb.Date.Date == date && tb.IsActive)
                .OrderBy(tb => tb.StartTime)
                .ToList();

            // Create day schedule
            var daySchedule = new DayScheduleModel
            {
                Date = date,
                Appointments = dayAppointments,
                TimeBlocks = dayTimeBlocks,
                AvailableSlots = await GetAvailableSlotsAsync(date)
            };

            weekSchedule.Days.Add(daySchedule);
        }

        return weekSchedule;
    }

    // Month calendar
    public async Task<CalendarViewModel> GetMonthCalendarAsync(DateTime month)
    {
        var firstDayOfMonth = new DateTime(month.Year, month.Month, 1);
        var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        // Get first day of calendar (might be from previous month)
        var firstCalendarDay = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);

        // Get appointments for the entire calendar view
        var calendarEnd = firstCalendarDay.AddDays(42); // 6 weeks
        var appointments = await GetAppointmentsFilteredAsync(new AppointmentFilterModel
        {
            StartDate = firstCalendarDay,
            EndDate = calendarEnd
        });

        var appointmentModels = appointments.Select(a => ValidateAndMapAppointment(a, "MonthCalendar")).ToList();

        var appointmentsByDate = appointmentModels
            .GroupBy(a => a.AppointmentDate.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        var days = new List<CalendarDayModel>();
        var currentDate = firstCalendarDay;

        for (int i = 0; i < 42; i++)
        {
            var dayAppointments = appointmentsByDate.ContainsKey(currentDate) ? appointmentsByDate[currentDate] : new List<AppointmentModel>();

            days.Add(new CalendarDayModel
            {
                Date = currentDate,
                IsCurrentMonth = currentDate.Month == month.Month,
                IsToday = currentDate.Date == DateTime.Now.Date,
                IsPast = currentDate.Date < DateTime.Now.Date,
                AppointmentCount = dayAppointments.Count,
                HasCriticalAppointment = dayAppointments.Any(a => a.Type.Contains("Emergency", StringComparison.OrdinalIgnoreCase))
            });

            currentDate = currentDate.AddDays(1);
        }

        var calendar = new CalendarViewModel
        {
            CurrentMonth = month,
            Days = days,
            AppointmentsByDate = appointmentsByDate
        };

        return calendar;
    }

    // Statistics
    public async Task<AppointmentStatisticsModel> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var appointments = await GetAppointmentsFilteredAsync(new AppointmentFilterModel
        {
            StartDate = startDate,
            EndDate = endDate
        });

        var appointmentModels = appointments.Select(a => new AppointmentModel
        {
            AppointmentId = a.Id.ToString(),
            AppointmentDate = a.AppointmentDateTime.Date,
            Type = a.Type,
            Status = a.Status
        }).ToList();

        var today = DateTime.Now.Date;

        var stats = new AppointmentStatisticsModel
        {
            TotalAppointments = appointmentModels.Count,
            TodayAppointments = appointmentModels.Count(a => a.AppointmentDate == today),
            WeekAppointments = appointmentModels.Count(a => a.AppointmentDate >= today && a.AppointmentDate < today.AddDays(7)),
            MonthAppointments = appointmentModels.Count(a => a.AppointmentDate.Year == today.Year && a.AppointmentDate.Month == today.Month),
            CompletedAppointments = appointmentModels.Count(a => a.Status.Contains("Completed", StringComparison.OrdinalIgnoreCase)),
            CancelledAppointments = appointmentModels.Count(a => a.Status.Contains("Cancelled", StringComparison.OrdinalIgnoreCase)),
            NoShowAppointments = appointmentModels.Count(a => a.Status.Contains("NoShow", StringComparison.OrdinalIgnoreCase) || a.Status.Contains("No-Show", StringComparison.OrdinalIgnoreCase)),
            AppointmentsByType = appointmentModels.GroupBy(a => a.Type).ToDictionary(g => g.Key, g => g.Count()),
            AppointmentsByStatus = appointmentModels.GroupBy(a => a.Status).ToDictionary(g => g.Key, g => g.Count())
        };

        if (startDate.HasValue && endDate.HasValue)
        {
            var days = (endDate.Value - startDate.Value).TotalDays;
            stats.AverageAppointmentsPerDay = days > 0 ? appointmentModels.Count / days : 0;
        }

        return stats;
    }

    // Helper methods for compatibility
    public Task<AppointmentModel?> CreateAppointmentAsync(CreateAppointmentModel model) => Task.FromResult<AppointmentModel?>(null);
    public Task<AppointmentModel?> GetAppointmentByIdAsync(string appointmentId) => Task.FromResult<AppointmentModel?>(null);
    public Task<List<AppointmentModel>> GetAppointmentsAsync(AppointmentFilterModel filter) => GetAppointmentsInternalAsync(filter);
    public Task<bool> UpdateAppointmentAsync(UpdateAppointmentModel model) => Task.FromResult(false);
    public Task<bool> CancelAppointmentAsync(CancelAppointmentModel model) => CancelAppointmentWithReasonAsync(model.AppointmentId, model.CancellationReason);
    public Task<bool> DeleteAppointmentAsync(string appointmentId) => Task.FromResult(false);
    public Task<bool> UpdateAppointmentStatusAsync(string appointmentId, string newStatus) => Task.FromResult(false);
    public Task<bool> ConfirmAppointmentAsync(string appointmentId) => Task.FromResult(false);
    public Task<bool> MarkAsNoShowAsync(string appointmentId) => Task.FromResult(false);
    public Task<List<TimeSlotModel>> GetAvailableSlotsAsync(DateTime date, int durationMinutes = 30) => Task.FromResult(new List<TimeSlotModel>());
    public Task<ConflictCheckResult> CheckConflictsAsync(DateTime date, TimeSpan startTime, int durationMinutes, string? excludeAppointmentId = null) => Task.FromResult(new ConflictCheckResult());
    public Task<bool> IsTimeSlotAvailableAsync(DateTime date, TimeSpan startTime, int durationMinutes) => Task.FromResult(false);
    public Task<List<AppointmentReminderModel>> GetUpcomingRemindersAsync(int hoursAhead = 24) => Task.FromResult(new List<AppointmentReminderModel>());
    public Task<bool> SendReminderAsync(string appointmentId) => Task.FromResult(false);
    public Task<List<AppointmentModel>> GetPatientAppointmentsAsync(string patientId) => Task.FromResult(new List<AppointmentModel>());
    public Task<AppointmentModel?> GetNextAppointmentForPatientAsync(string patientId) => Task.FromResult<AppointmentModel?>(null);

    // Data validation helper to check for invalid appointment times
    private AppointmentModel ValidateAndMapAppointment(AppointmentDto dto, string source = "")
    {
        var startTime = dto.AppointmentDateTime.TimeOfDay;
        var endTime = dto.AppointmentDateTime.AddMinutes(dto.DurationMinutes).TimeOfDay;

        // Check if time is midnight (likely indicates incorrect data from backend)
        if (startTime == TimeSpan.Zero)
        {
            Console.WriteLine($"⚠️ WARNING [{source}]: Appointment {dto.Id} has midnight time (00:00:00)");
            Console.WriteLine($"   Patient: {dto.PatientName}");
            Console.WriteLine($"   DateTime from backend: {dto.AppointmentDateTime}");
            Console.WriteLine($"   This may indicate incorrect data from GraphQL backend!");
        }

        // Check if appointment time is in the future but date is in the past (time zone issue)
        if (dto.AppointmentDateTime.Date < DateTime.Now.Date && startTime > TimeSpan.Zero)
        {
            Console.WriteLine($"⚠️ WARNING [{source}]: Appointment {dto.Id} date is in past but has non-zero time");
            Console.WriteLine($"   This may indicate a timezone conversion issue");
        }

        return new AppointmentModel
        {
            AppointmentId = dto.Id.ToString(),
            PatientId = dto.PatientId,
            PatientName = dto.PatientName,
            PatientPhone = dto.PatientPhone ?? "",
            PatientEmail = dto.PatientEmail ?? "",
            AppointmentDate = dto.AppointmentDateTime.Date,
            StartTime = startTime,
            EndTime = endTime,
            DurationMinutes = dto.DurationMinutes,
            Type = dto.Type.ToString(),
            Status = dto.Status.ToString(),
            Reason = dto.ReasonForVisit ?? "",
            IsVirtual = dto.IsVirtual,
            RoomNumber = dto.RoomNumber
        };
    }

    public async Task<AppointmentDto?> RescheduleAppointmentAsync(int appointmentId, DateTime newAppointmentDateTime)
    {
#if DEBUG
        Console.WriteLine($"📡 [FRONTEND SERVICE] RescheduleAppointmentAsync called:");
        Console.WriteLine($"   Appointment ID: {appointmentId}");
        Console.WriteLine($"   New DateTime (input): {newAppointmentDateTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"   DateTime Kind: {newAppointmentDateTime.Kind}");
#endif

        // Use GraphQL variables for type-safe DateTime serialization
        var mutation = @"
            mutation RescheduleAppointment($appointmentId: Int!, $newDateTime: DateTime!) {
                rescheduleAppointment(appointmentId: $appointmentId, newAppointmentDateTime: $newDateTime) {
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
            }";

        var variables = new
        {
            appointmentId = appointmentId,
            newDateTime = newAppointmentDateTime
        };

#if DEBUG
        Console.WriteLine($"   Using GraphQL variables with JsonDateTimeConverter");
#endif

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(mutation, variables);
            if (response != null && response.ContainsKey("rescheduleAppointment"))
            {
                var json = JsonSerializer.Serialize(response["rescheduleAppointment"]);
                var result = JsonSerializer.Deserialize<AppointmentDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

#if DEBUG
                if (result != null)
                {
                    Console.WriteLine($"📥 [FRONTEND SERVICE] Received rescheduled appointment:");
                    Console.WriteLine($"   ID: {result.Id}");
                    Console.WriteLine($"   DateTime: {result.AppointmentDateTime:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"   TimeOfDay: {result.AppointmentDateTime.TimeOfDay}");
                }
                else
                {
                    Console.WriteLine($"⚠️ [FRONTEND SERVICE] Deserialization returned null!");
                }
#endif

                return result;
            }

#if DEBUG
            Console.WriteLine($"⚠️ [FRONTEND SERVICE] No rescheduleAppointment in response");
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [FRONTEND SERVICE] Error rescheduling appointment: {ex.Message}");
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
        }

        return null;
    }

    public async Task<DoctorWeekScheduleDto?> GetMyWeekScheduleAsync(DateTime weekStartDate)
    {
        var query = $@"
            query {{
                myWeekSchedule(weekStartDate: ""{weekStartDate:yyyy-MM-ddTHH:mm:ssZ}"") {{
                    weekStartDate
                    weekEndDate
                    appointments {{
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
                    }}
                    timeBlocks {{
                        id
                        doctorId
                        doctorName
                        blockType
                        date
                        startTime
                        endTime
                        isRecurring
                        recurrencePattern
                        repeatUntilDate
                        notes
                        createdAt
                        updatedAt
                        isActive
                    }}
                }}
            }}";

        try
        {
            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);
            if (response != null && response.ContainsKey("myWeekSchedule"))
            {
                var json = JsonSerializer.Serialize(response["myWeekSchedule"]);
                return JsonSerializer.Deserialize<DoctorWeekScheduleDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching week schedule: {ex.Message}");
        }

        return null;
    }

    private class GraphQLResponse<T>
    {
        public GraphQLData<T>? Data { get; set; }
    }

    private class GraphQLData<T>
    {
        public T? DoctorAppointments { get; set; }
        public T? PendingAppointmentRequests { get; set; }
        public T? Appointment { get; set; }
        public T? ApproveAppointmentRequest { get; set; }
        public T? RejectAppointmentRequest { get; set; }
        public T? CompleteAppointment { get; set; }
        public T? CancelAppointment { get; set; }
    }
}

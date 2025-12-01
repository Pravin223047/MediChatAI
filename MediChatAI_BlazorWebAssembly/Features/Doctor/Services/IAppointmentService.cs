using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public interface IAppointmentService
{
    // Appointment CRUD
    Task<AppointmentModel?> CreateAppointmentAsync(CreateAppointmentModel model);
    Task<AppointmentModel?> GetAppointmentByIdAsync(string appointmentId);
    Task<List<AppointmentModel>> GetAppointmentsAsync(AppointmentFilterModel filter);
    Task<bool> UpdateAppointmentAsync(UpdateAppointmentModel model);
    Task<bool> CancelAppointmentAsync(CancelAppointmentModel model);
    Task<bool> DeleteAppointmentAsync(string appointmentId);

    // Status Management
    Task<bool> UpdateAppointmentStatusAsync(string appointmentId, string newStatus);
    Task<bool> ConfirmAppointmentAsync(string appointmentId);
    Task<bool> MarkAsNoShowAsync(string appointmentId);
    Task<bool> MarkAsCompletedAsync(string appointmentId);

    // Schedule Views
    Task<DayScheduleModel> GetDayScheduleAsync(DateTime date);
    Task<WeekScheduleModel> GetWeekScheduleAsync(DateTime weekStartDate);
    Task<CalendarViewModel> GetMonthCalendarAsync(DateTime month);

    // Availability & Conflicts
    Task<List<TimeSlotModel>> GetAvailableSlotsAsync(DateTime date, int durationMinutes = 30);
    Task<ConflictCheckResult> CheckConflictsAsync(DateTime date, TimeSpan startTime, int durationMinutes, string? excludeAppointmentId = null);
    Task<bool> IsTimeSlotAvailableAsync(DateTime date, TimeSpan startTime, int durationMinutes);

    // Statistics
    Task<AppointmentStatisticsModel> GetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Reminders
    Task<List<AppointmentReminderModel>> GetUpcomingRemindersAsync(int hoursAhead = 24);
    Task<bool> SendReminderAsync(string appointmentId);

    // Patient Appointments
    Task<List<AppointmentModel>> GetPatientAppointmentsAsync(string patientId);
    Task<AppointmentModel?> GetNextAppointmentForPatientAsync(string patientId);

    // New methods for real data and appointment requests
    Task<List<AppointmentDto>> GetDoctorAppointmentsAsync();
    Task<List<AppointmentRequestDto>> GetPendingAppointmentRequestsAsync();
    Task<AppointmentDto?> GetAppointmentDetailsByIdAsync(string appointmentId);
    Task<AppointmentDto?> ApproveAppointmentRequestAsync(ReviewAppointmentRequestInput input);
    Task<bool> RejectAppointmentRequestAsync(int requestId, string reason);
    Task<bool> CancelAppointmentWithReasonAsync(string appointmentId, string reason);
    Task<AppointmentDto?> RescheduleAppointmentAsync(int appointmentId, DateTime newAppointmentDateTime);
    Task<DoctorWeekScheduleDto?> GetMyWeekScheduleAsync(DateTime weekStartDate);
}

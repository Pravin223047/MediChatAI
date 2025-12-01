using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Infrastructure.Data;

namespace MediChatAI_GraphQl.Shared.Services;

public class ConversationInitializationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConversationInitializationService> _logger;

    public ConversationInitializationService(ApplicationDbContext context, ILogger<ConversationInitializationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Initialize conversations for existing appointments that don't have any messages yet
    /// </summary>
    public async Task<int> InitializeConversationsForExistingAppointmentsAsync()
    {
        var conversationsCreated = 0;

        try
        {
            // Get all confirmed appointments that don't have any messages between patient and doctor
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.Status == AppointmentStatus.Confirmed && 
                           a.PatientId != null && 
                           a.DoctorId != null)
                .ToListAsync();

            _logger.LogInformation("Found {Count} confirmed appointments to check for conversations", appointments.Count);

            foreach (var appointment in appointments)
            {
                if (appointment.PatientId == null || appointment.DoctorId == null) continue;

                // Check if there are already messages between this patient and doctor
                var existingMessages = await _context.DoctorMessages
                    .AnyAsync(m => (m.SenderId == appointment.PatientId && m.ReceiverId == appointment.DoctorId) ||
                                  (m.SenderId == appointment.DoctorId && m.ReceiverId == appointment.PatientId));

                if (!existingMessages)
                {
                    // Create initial conversation message
                        var initialMessage = new DoctorMessage
                    {
                        SenderId = appointment.DoctorId,
                        ReceiverId = appointment.PatientId,
                        Content = $"Hello {appointment.Patient?.FirstName}, your appointment has been confirmed for {appointment.AppointmentDateTime:MMM dd, yyyy 'at' hh:mm tt}. Feel free to message me if you have any questions before our appointment.",
                        MessageType = MessageType.System,
                        Status = MessageStatus.Sent,
                        SentAt = appointment.ConfirmedAt ?? appointment.CreatedAt
                    };

                    _context.DoctorMessages.Add(initialMessage);
                    conversationsCreated++;

                    _logger.LogInformation("Created conversation for appointment {AppointmentId} between patient {PatientId} and doctor {DoctorId}", 
                        appointment.Id, appointment.PatientId, appointment.DoctorId);
                }
            }

            if (conversationsCreated > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully created {Count} conversations for existing appointments", conversationsCreated);
            }
            else
            {
                _logger.LogInformation("No new conversations needed - all appointments already have messages");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing conversations for existing appointments");
            throw;
        }

        return conversationsCreated;
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
}
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.Core.Domain.Entities;

namespace MediChatAI_GraphQl.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<UserActivity> UserActivities { get; set; }
    public DbSet<SystemSettings> SystemSettings { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }
    public DbSet<PatientVital> PatientVitals { get; set; }
    public DbSet<DoctorMessage> DoctorMessages { get; set; }
    public DbSet<EmergencyAlert> EmergencyAlerts { get; set; }
    public DbSet<DoctorPreference> DoctorPreferences { get; set; }
    public DbSet<DoctorAnalytics> DoctorAnalytics { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppointmentRequest> AppointmentRequests { get; set; }
    public DbSet<TimeBlock> TimeBlocks { get; set; }
    public DbSet<PatientDocument> PatientDocuments { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
    public DbSet<LabResult> LabResults { get; set; }
    public DbSet<MedicationReminder> MedicationReminders { get; set; }
    public DbSet<MedicationAdherence> MedicationAdherences { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }
    public DbSet<GroupConversation> GroupConversations { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<GroupMessage> GroupMessages { get; set; }
    public DbSet<GroupMessageReaction> GroupMessageReactions { get; set; }
    public DbSet<GroupMessageReadStatus> GroupMessageReadStatuses { get; set; }
    public DbSet<ConsultationSession> ConsultationSessions { get; set; }
    public DbSet<ConsultationRecording> ConsultationRecordings { get; set; }
    public DbSet<ConsultationParticipant> ConsultationParticipants { get; set; }
    public DbSet<ConsultationNote> ConsultationNotes { get; set; }
    public DbSet<AIChatSession> AIChatSessions { get; set; }
    public DbSet<AIChatMessage> AIChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();

            // Doctor onboarding fields
            entity.Property(e => e.MedicalRegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.EducationHistory).HasMaxLength(1000);
            entity.Property(e => e.AffiliatedHospitals).HasMaxLength(500);
            entity.Property(e => e.ConsultationHours).HasMaxLength(200);
            entity.Property(e => e.AadhaarNumber).HasMaxLength(12);
            entity.Property(e => e.AadhaarCardImagePath).HasMaxLength(500);
            entity.Property(e => e.MedicalCertificateUrl).HasMaxLength(500);
            entity.Property(e => e.MedicalCertificateFilePath).HasMaxLength(500);
            entity.Property(e => e.AiVerificationNotes).HasMaxLength(2000);
            entity.Property(e => e.AadhaarVerificationFailureReason).HasMaxLength(1000);
            entity.Property(e => e.MedicalCertificateVerificationFailureReason).HasMaxLength(1000);
            entity.Property(e => e.ApprovedByAdminId).HasMaxLength(450);
            entity.Property(e => e.AdminRejectionReason).HasMaxLength(500);

            // Indexes for better performance
            entity.HasIndex(e => e.Email).HasDatabaseName("IX_Users_Email"); // Critical for login performance
            entity.HasIndex(e => e.IsApprovedByAdmin);
            entity.HasIndex(e => e.IsProfileCompleted);
            entity.HasIndex(e => e.MedicalRegistrationNumber);
        });

        // Configure UserActivity
        builder.Entity<UserActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.UserEmail).HasMaxLength(256).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(256).IsRequired();
            entity.Property(e => e.UserRole).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ActivityType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(1000);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ActivityType);
            entity.HasIndex(e => e.Timestamp);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Notification
        builder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ActionUrl).HasMaxLength(500);
            entity.Property(e => e.ActionText).HasMaxLength(100);
            entity.Property(e => e.Metadata).HasMaxLength(2000);
            entity.Property(e => e.Icon).HasMaxLength(100);

            // Indexes for performance - Optimized composite index for unread count queries
            entity.HasIndex(e => new { e.UserId, e.IsRead, e.ExpiresAt }).HasDatabaseName("IX_Notifications_UserId_IsRead_ExpiresAt");
            entity.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedAt }).HasDatabaseName("IX_Notifications_UserId_IsRead_CreatedAt");
            entity.HasIndex(e => e.GroupId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Priority);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure NotificationPreference
        builder.Entity<NotificationPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Timezone).HasMaxLength(100);

            // One preference per user
            entity.HasIndex(e => e.UserId).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PatientVital
        builder.Entity<PatientVital>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.RecordedByDoctorId).HasMaxLength(450);
            entity.Property(e => e.Value).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasMaxLength(1000);

            // Indexes for performance
            entity.HasIndex(e => new { e.PatientId, e.RecordedAt }).HasDatabaseName("IX_PatientVitals_PatientId_RecordedAt");
            entity.HasIndex(e => e.RecordedByDoctorId);
            entity.HasIndex(e => e.VitalType);
            entity.HasIndex(e => e.Severity);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RecordedByDoctor)
                .WithMany()
                .HasForeignKey(e => e.RecordedByDoctorId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure DoctorMessage
        builder.Entity<DoctorMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SenderId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.ReceiverId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.AttachmentFileName).HasMaxLength(255);
            entity.Property(e => e.AttachmentMimeType).HasMaxLength(100);
            entity.Property(e => e.Metadata).HasMaxLength(1000);

            // Indexes for performance
            entity.HasIndex(e => new { e.SenderId, e.ReceiverId, e.SentAt }).HasDatabaseName("IX_DoctorMessages_Sender_Receiver_SentAt");
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Receiver)
                .WithMany()
                .HasForeignKey(e => e.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReplyToMessage)
                .WithMany()
                .HasForeignKey(e => e.ReplyToMessageId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure EmergencyAlert
        builder.Entity<EmergencyAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.DoctorId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.AcknowledgedByUserId).HasMaxLength(450);
            entity.Property(e => e.ResolvedByUserId).HasMaxLength(450);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(1000);
            entity.Property(e => e.PatientLocation).HasMaxLength(100);
            entity.Property(e => e.RecommendedAction).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasMaxLength(2000);

            // Indexes for performance
            entity.HasIndex(e => new { e.DoctorId, e.Status, e.Severity }).HasDatabaseName("IX_EmergencyAlerts_Doctor_Status_Severity");
            entity.HasIndex(e => e.PatientId);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AcknowledgedByUser)
                .WithMany()
                .HasForeignKey(e => e.AcknowledgedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.ResolvedByUser)
                .WithMany()
                .HasForeignKey(e => e.ResolvedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.RelatedVital)
                .WithMany()
                .HasForeignKey(e => e.RelatedVitalId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure DoctorPreference
        builder.Entity<DoctorPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DoctorId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.ThemePreset).HasMaxLength(50);
            entity.Property(e => e.ThemeMode).HasMaxLength(20);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.AccentColor).HasMaxLength(20);
            entity.Property(e => e.DefaultDashboardView).HasMaxLength(50);
            entity.Property(e => e.NotificationFrequency).HasMaxLength(20);
            entity.Property(e => e.WorkStartTime).HasMaxLength(10);
            entity.Property(e => e.WorkEndTime).HasMaxLength(10);
            entity.Property(e => e.WorkingDays).HasMaxLength(100);
            entity.Property(e => e.PreferredChartType).HasMaxLength(50);
            entity.Property(e => e.AnalyticsTimeRange).HasMaxLength(20);
            entity.Property(e => e.PreferredLanguage).HasMaxLength(10);
            entity.Property(e => e.TimeZone).HasMaxLength(50);
            entity.Property(e => e.DateFormat).HasMaxLength(10);
            entity.Property(e => e.TimeFormat).HasMaxLength(10);
            entity.Property(e => e.CustomSettings).HasMaxLength(5000);

            // One preference per doctor
            entity.HasIndex(e => e.DoctorId).IsUnique();

            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure DoctorAnalytics
        builder.Entity<DoctorAnalytics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DoctorId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.AdditionalMetrics).HasMaxLength(5000);

            // Configure decimal properties with precision and scale
            entity.Property(e => e.AverageSatisfactionRating).HasPrecision(3, 2); // e.g., 4.75
            entity.Property(e => e.TotalRevenue).HasPrecision(18, 2); // e.g., 999999999999999.99
            entity.Property(e => e.PendingPayments).HasPrecision(18, 2);
            entity.Property(e => e.AverageRevenuePerConsultation).HasPrecision(18, 2);
            entity.Property(e => e.TotalHoursWorked).HasPrecision(10, 2); // e.g., 99999999.99
            entity.Property(e => e.BreakTimeTaken).HasPrecision(10, 2);

            // Indexes for performance
            entity.HasIndex(e => new { e.DoctorId, e.Date }).HasDatabaseName("IX_DoctorAnalytics_Doctor_Date").IsUnique();
            entity.HasIndex(e => new { e.DoctorId, e.Year, e.MonthNumber }).HasDatabaseName("IX_DoctorAnalytics_Doctor_Year_Month");
            entity.HasIndex(e => new { e.DoctorId, e.Year, e.WeekNumber }).HasDatabaseName("IX_DoctorAnalytics_Doctor_Year_Week");

            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Appointment
        builder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.DoctorId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.RoomNumber).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.MeetingLink).HasMaxLength(500);
            entity.Property(e => e.ReasonForVisit).HasMaxLength(1000);
            entity.Property(e => e.DoctorNotes).HasMaxLength(2000);
            entity.Property(e => e.PatientNotes).HasMaxLength(1000);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.PaymentTransactionId).HasMaxLength(100);
            entity.Property(e => e.ConsultationFee).HasPrecision(18, 2);

            // Indexes for performance
            entity.HasIndex(e => new { e.PatientId, e.AppointmentDateTime }).HasDatabaseName("IX_Appointments_Patient_DateTime");
            entity.HasIndex(e => new { e.DoctorId, e.AppointmentDateTime }).HasDatabaseName("IX_Appointments_Doctor_DateTime");
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure AppointmentRequest
        builder.Entity<AppointmentRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.PreferredDoctorId).HasMaxLength(450);
            entity.Property(e => e.ReviewedByDoctorId).HasMaxLength(450);
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.AlternatePhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.ZipCode).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(200);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactRelation).HasMaxLength(50);
            entity.Property(e => e.BloodType).HasMaxLength(10);
            entity.Property(e => e.Allergies).HasMaxLength(1000);
            entity.Property(e => e.CurrentMedications).HasMaxLength(1000);
            entity.Property(e => e.PastMedicalConditions).HasMaxLength(1000);
            entity.Property(e => e.ChronicDiseases).HasMaxLength(500);
            entity.Property(e => e.PreviousSurgeries).HasMaxLength(1000);
            entity.Property(e => e.FamilyMedicalHistory).HasMaxLength(1000);
            entity.Property(e => e.SymptomDescription).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.SymptomDuration).HasMaxLength(100);
            entity.Property(e => e.SymptomSeverity).HasMaxLength(50);
            entity.Property(e => e.ReasonForVisit).HasMaxLength(1000);
            entity.Property(e => e.VoiceRecordingUrl).HasMaxLength(500);
            entity.Property(e => e.VoiceRecordingTranscript).HasMaxLength(5000);
            entity.Property(e => e.InsuranceProvider).HasMaxLength(200);
            entity.Property(e => e.InsurancePolicyNumber).HasMaxLength(100);
            entity.Property(e => e.InsuranceGroupNumber).HasMaxLength(100);
            entity.Property(e => e.InsuranceCardFrontUrl).HasMaxLength(500);
            entity.Property(e => e.InsuranceCardBackUrl).HasMaxLength(500);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PreferredTimeSlot).HasMaxLength(50);
            entity.Property(e => e.ReviewNotes).HasMaxLength(1000);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.PatientAdditionalNotes).HasMaxLength(1000);
            entity.Property(e => e.AdminNotes).HasMaxLength(1000);

            // Indexes for performance
            entity.HasIndex(e => new { e.PatientId, e.Status }).HasDatabaseName("IX_AppointmentRequests_Patient_Status");
            entity.HasIndex(e => new { e.PreferredDoctorId, e.Status }).HasDatabaseName("IX_AppointmentRequests_Doctor_Status");
            entity.HasIndex(e => e.RequestedAt);
            entity.HasIndex(e => e.PreferredDate);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PreferredDoctor)
                .WithMany()
                .HasForeignKey(e => e.PreferredDoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.ReviewedByDoctor)
                .WithMany()
                .HasForeignKey(e => e.ReviewedByDoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure PatientDocument
        builder.Entity<PatientDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.UploadedById).HasMaxLength(450).IsRequired();
            entity.Property(e => e.VerifiedByDoctorId).HasMaxLength(450);
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileUrl).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.PublicId).HasMaxLength(200);
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.DoctorNotes).HasMaxLength(1000);

            // Indexes for performance
            entity.HasIndex(e => new { e.PatientId, e.UploadedAt }).HasDatabaseName("IX_PatientDocuments_Patient_UploadedAt");
            entity.HasIndex(e => e.DocumentType);
            entity.HasIndex(e => e.AppointmentRequestId);
            entity.HasIndex(e => e.AppointmentId);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.UploadedBy)
                .WithMany()
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.VerifiedByDoctor)
                .WithMany()
                .HasForeignKey(e => e.VerifiedByDoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.AppointmentRequest)
                .WithMany()
                .HasForeignKey(e => e.AppointmentRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure Prescription
        builder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.DoctorId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Diagnosis).HasMaxLength(500);
            entity.Property(e => e.DoctorNotes).HasMaxLength(1000);
            entity.Property(e => e.PharmacyNotes).HasMaxLength(500);
            entity.Property(e => e.DoctorSignature).HasMaxLength(500);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.PharmacyName).HasMaxLength(200);
            entity.Property(e => e.PharmacyContact).HasMaxLength(100);

            // Indexes for performance
            entity.HasIndex(e => new { e.PatientId, e.PrescribedDate }).HasDatabaseName("IX_Prescriptions_Patient_PrescribedDate");
            entity.HasIndex(e => new { e.DoctorId, e.PrescribedDate }).HasDatabaseName("IX_Prescriptions_Doctor_PrescribedDate");
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.AppointmentId);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure one-to-many relationship with PrescriptionItems
            entity.HasMany(e => e.Items)
                .WithOne(i => i.Prescription)
                .HasForeignKey(i => i.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PrescriptionItem
        builder.Entity<PrescriptionItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedicationName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.GenericName).HasMaxLength(200);
            entity.Property(e => e.Dosage).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Frequency).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Route).HasMaxLength(50);
            entity.Property(e => e.Form).HasMaxLength(50);
            entity.Property(e => e.Instructions).HasMaxLength(1000);
            entity.Property(e => e.Warnings).HasMaxLength(1000);
            entity.Property(e => e.SideEffects).HasMaxLength(1000);

            // Indexes for performance
            entity.HasIndex(e => e.PrescriptionId).HasDatabaseName("IX_PrescriptionItems_PrescriptionId");
        });

        // Configure LabResult
        builder.Entity<LabResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.OrderedByDoctorId).HasMaxLength(450);
            entity.Property(e => e.ReviewedByDoctorId).HasMaxLength(450);
            entity.Property(e => e.TestName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TestType).HasMaxLength(100);
            entity.Property(e => e.LabName).HasMaxLength(200);
            entity.Property(e => e.LabAddress).HasMaxLength(500);
            entity.Property(e => e.Value).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.ReferenceRange).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Interpretation).HasMaxLength(2000);
            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FileMimeType).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.TestParameters).HasMaxLength(4000);

            // Indexes for performance
            entity.HasIndex(e => new { e.PatientId, e.ResultDate }).HasDatabaseName("IX_LabResults_Patient_ResultDate");
            entity.HasIndex(e => new { e.OrderedByDoctorId, e.OrderedAt }).HasDatabaseName("IX_LabResults_OrderedBy_OrderedAt");
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsCritical);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.OrderedByDoctor)
                .WithMany()
                .HasForeignKey(e => e.OrderedByDoctorId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.ReviewedByDoctor)
                .WithMany()
                .HasForeignKey(e => e.ReviewedByDoctorId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure MedicationReminder
        builder.Entity<MedicationReminder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.MedicationName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Dosage).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ReminderTimes).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Instructions).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.DaysOfWeek).HasMaxLength(100);
            entity.Property(e => e.AdherenceRate).HasPrecision(5, 2); // e.g., 100.00

            // Indexes for performance
            entity.HasIndex(e => new { e.PatientId, e.IsActive }).HasDatabaseName("IX_MedicationReminders_Patient_IsActive");
            entity.HasIndex(e => e.NextReminderAt);
            entity.HasIndex(e => e.PrescriptionId);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Prescription)
                .WithMany()
                .HasForeignKey(e => e.PrescriptionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure MedicationAdherence
        builder.Entity<MedicationAdherence>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Indexes for performance
            entity.HasIndex(e => new { e.ReminderId, e.ScheduledTime }).HasDatabaseName("IX_MedicationAdherence_Reminder_ScheduledTime");
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.Reminder)
                .WithMany(r => r.AdherenceLogs)
                .HasForeignKey(e => e.ReminderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Conversation
        builder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.User1Id).HasMaxLength(450);
            entity.Property(e => e.User2Id).HasMaxLength(450);
            entity.Property(e => e.GroupName).HasMaxLength(200);
            entity.Property(e => e.GroupDescription).HasMaxLength(1000);
            entity.Property(e => e.GroupImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedById).HasMaxLength(450);
            entity.Property(e => e.Metadata).HasMaxLength(2000);

            // Indexes for performance
            entity.HasIndex(e => new { e.User1Id, e.User2Id }).HasDatabaseName("IX_Conversations_Users").IsUnique();
            entity.HasIndex(e => new { e.Type, e.LastActivityAt }).HasDatabaseName("IX_Conversations_Type_LastActivity");
            entity.HasIndex(e => e.IsDeleted);

            // Relationships
            entity.HasOne(e => e.User1)
                .WithMany()
                .HasForeignKey(e => e.User1Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User2)
                .WithMany()
                .HasForeignKey(e => e.User2Id)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.LastMessage)
                .WithMany()
                .HasForeignKey(e => e.LastMessageId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure ConversationParticipant
        builder.Entity<ConversationParticipant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();

            // Indexes for performance
            entity.HasIndex(e => new { e.ConversationId, e.UserId }).HasDatabaseName("IX_ConversationParticipants_Conversation_User").IsUnique();
            entity.HasIndex(e => new { e.UserId, e.IsActive }).HasDatabaseName("IX_ConversationParticipants_User_Active");

            // Relationships
            entity.HasOne(e => e.Conversation)
                .WithMany(c => c.Participants)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MessageReaction
        builder.Entity<MessageReaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Emoji).HasMaxLength(10).IsRequired();

            // Indexes for performance
            entity.HasIndex(e => e.MessageId).HasDatabaseName("IX_MessageReactions_MessageId");
            entity.HasIndex(e => new { e.MessageId, e.UserId }).HasDatabaseName("IX_MessageReactions_Message_User").IsUnique();

            // Relationships
            entity.HasOne(e => e.Message)
                .WithMany()
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure GroupConversation
        builder.Entity<GroupConversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
            entity.Property(e => e.GroupAvatarUrl).HasMaxLength(500);

            // Indexes for performance
            entity.HasIndex(e => new { e.CreatedBy, e.CreatedAt }).HasDatabaseName("IX_GroupConversations_CreatedBy_CreatedAt");
            entity.HasIndex(e => e.IsActive);
        });

        // Configure GroupMember
        builder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired();

            // Indexes for performance
            entity.HasIndex(e => new { e.GroupConversationId, e.UserId }).HasDatabaseName("IX_GroupMembers_Group_User");
            entity.HasIndex(e => new { e.UserId, e.IsActive }).HasDatabaseName("IX_GroupMembers_User_Active");

            // Relationships
            entity.HasOne(e => e.GroupConversation)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.GroupConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure GroupMessage
        builder.Entity<GroupMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SenderId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(5000).IsRequired();
            entity.Property(e => e.MessageType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AttachmentUrl).HasMaxLength(1000);
            entity.Property(e => e.AttachmentName).HasMaxLength(200);

            // Indexes for performance
            entity.HasIndex(e => new { e.GroupConversationId, e.SentAt }).HasDatabaseName("IX_GroupMessages_Group_SentAt");
            entity.HasIndex(e => new { e.SenderId, e.SentAt }).HasDatabaseName("IX_GroupMessages_Sender_SentAt");
            entity.HasIndex(e => e.IsDeleted);

            // Relationships
            entity.HasOne(e => e.GroupConversation)
                .WithMany(g => g.Messages)
                .HasForeignKey(e => e.GroupConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReplyToMessage)
                .WithMany()
                .HasForeignKey(e => e.ReplyToMessageId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure GroupMessageReaction
        builder.Entity<GroupMessageReaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Emoji).HasMaxLength(10).IsRequired();

            // Indexes for performance
            entity.HasIndex(e => e.GroupMessageId).HasDatabaseName("IX_GroupMessageReactions_MessageId");
            entity.HasIndex(e => new { e.GroupMessageId, e.UserId, e.Emoji }).HasDatabaseName("IX_GroupMessageReactions_Message_User_Emoji").IsUnique();

            // Relationships
            entity.HasOne(e => e.GroupMessage)
                .WithMany(m => m.Reactions)
                .HasForeignKey(e => e.GroupMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure GroupMessageReadStatus
        builder.Entity<GroupMessageReadStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();

            // Indexes for performance
            entity.HasIndex(e => new { e.GroupMessageId, e.UserId }).HasDatabaseName("IX_GroupMessageReadStatuses_Message_User").IsUnique();
            entity.HasIndex(e => new { e.UserId, e.ReadAt }).HasDatabaseName("IX_GroupMessageReadStatuses_User_ReadAt");

            // Relationships
            entity.HasOne(e => e.GroupMessage)
                .WithMany(m => m.ReadStatuses)
                .HasForeignKey(e => e.GroupMessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ConsultationSession
        builder.Entity<ConsultationSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DoctorId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.PatientId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.RoomId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.MeetingLink).HasMaxLength(500);
            entity.Property(e => e.ChiefComplaint).HasMaxLength(1000);
            entity.Property(e => e.DoctorObservations).HasMaxLength(2000);
            entity.Property(e => e.Diagnosis).HasMaxLength(1000);
            entity.Property(e => e.TreatmentPlan).HasMaxLength(2000);
            entity.Property(e => e.FollowUpInstructions).HasMaxLength(1000);
            entity.Property(e => e.AISummary).HasMaxLength(5000);
            entity.Property(e => e.TranscriptUrl).HasMaxLength(500);
            entity.Property(e => e.VideoQuality).HasMaxLength(50);
            entity.Property(e => e.AudioQuality).HasMaxLength(50);
            entity.Property(e => e.ConnectionIssues).HasMaxLength(2000);
            entity.Property(e => e.PatientFeedback).HasMaxLength(1000);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);

            // Indexes for performance
            entity.HasIndex(e => new { e.DoctorId, e.ScheduledStartTime }).HasDatabaseName("IX_ConsultationSessions_Doctor_ScheduledStart");
            entity.HasIndex(e => new { e.PatientId, e.ScheduledStartTime }).HasDatabaseName("IX_ConsultationSessions_Patient_ScheduledStart");
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.HasIndex(e => e.RoomId).IsUnique();
            entity.HasIndex(e => e.Status);

            // Relationships
            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ConsultationRecording
        builder.Entity<ConsultationRecording>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RecordingUrl).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(1000);
            entity.Property(e => e.Format).HasMaxLength(50);
            entity.Property(e => e.VideoCodec).HasMaxLength(50);
            entity.Property(e => e.AudioCodec).HasMaxLength(50);
            entity.Property(e => e.Resolution).HasMaxLength(50);
            entity.Property(e => e.TranscriptUrl).HasMaxLength(1000);
            entity.Property(e => e.TranscriptionLanguage).HasMaxLength(20);
            entity.Property(e => e.AISummary).HasMaxLength(5000);
            entity.Property(e => e.RecordedByUserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.CloudinaryPublicId).HasMaxLength(200);
            entity.Property(e => e.CloudinaryAssetId).HasMaxLength(200);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            // Indexes for performance
            entity.HasIndex(e => new { e.ConsultationSessionId, e.StartedAt }).HasDatabaseName("IX_ConsultationRecordings_Session_Started");
            entity.HasIndex(e => e.RecordedByUserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Type);

            // Relationships
            entity.HasOne(e => e.ConsultationSession)
                .WithMany(c => c.Recordings)
                .HasForeignKey(e => e.ConsultationSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RecordedBy)
                .WithMany()
                .HasForeignKey(e => e.RecordedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure ConsultationParticipant
        builder.Entity<ConsultationParticipant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Relation).HasMaxLength(100);
            entity.Property(e => e.InvitationToken).HasMaxLength(200);
            entity.Property(e => e.InvitedByUserId).HasMaxLength(450);
            entity.Property(e => e.InvitationMessage).HasMaxLength(1000);
            entity.Property(e => e.ConnectionId).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.RemovalReason).HasMaxLength(500);

            // Indexes for performance
            entity.HasIndex(e => new { e.ConsultationSessionId, e.JoinedAt }).HasDatabaseName("IX_ConsultationParticipants_Session_Joined");
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.InvitationToken).IsUnique();
            entity.HasIndex(e => e.Role);

            // Relationships
            entity.HasOne(e => e.ConsultationSession)
                .WithMany(c => c.Participants)
                .HasForeignKey(e => e.ConsultationSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.InvitedBy)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure ConsultationNote
        builder.Entity<ConsultationNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AuthorId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(5000).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.FormattedContent).HasMaxLength(10000);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.AttachmentUrls).HasMaxLength(2000);
            entity.Property(e => e.PreviousVersionContent).HasMaxLength(5000);
            entity.Property(e => e.EditReason).HasMaxLength(500);
            entity.Property(e => e.EditedByUserId).HasMaxLength(450);

            // Indexes for performance
            entity.HasIndex(e => new { e.ConsultationSessionId, e.CreatedAt }).HasDatabaseName("IX_ConsultationNotes_Session_Created");
            entity.HasIndex(e => e.AuthorId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsDeleted);

            // Relationships
            entity.HasOne(e => e.ConsultationSession)
                .WithMany(c => c.Notes)
                .HasForeignKey(e => e.ConsultationSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Author)
                .WithMany()
                .HasForeignKey(e => e.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EditedBy)
                .WithMany()
                .HasForeignKey(e => e.EditedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Update Prescription configuration to include ConsultationSession relationship
        builder.Entity<Prescription>().HasIndex(e => e.ConsultationSessionId);
        builder.Entity<Prescription>()
            .HasOne(e => e.ConsultationSession)
            .WithMany(c => c.Prescriptions)
            .HasForeignKey(e => e.ConsultationSessionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
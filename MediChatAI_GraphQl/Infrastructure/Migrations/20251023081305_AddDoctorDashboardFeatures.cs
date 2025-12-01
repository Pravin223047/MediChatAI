using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediChatAI_GraphQl.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorDashboardFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAppointments = table.Column<int>(type: "int", nullable: false),
                    CompletedAppointments = table.Column<int>(type: "int", nullable: false),
                    CancelledAppointments = table.Column<int>(type: "int", nullable: false),
                    NoShowAppointments = table.Column<int>(type: "int", nullable: false),
                    RescheduledAppointments = table.Column<int>(type: "int", nullable: false),
                    AverageAppointmentDuration = table.Column<int>(type: "int", nullable: true),
                    TotalPatients = table.Column<int>(type: "int", nullable: false),
                    NewPatients = table.Column<int>(type: "int", nullable: false),
                    FollowUpPatients = table.Column<int>(type: "int", nullable: false),
                    CriticalPatients = table.Column<int>(type: "int", nullable: false),
                    AverageSatisfactionRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    AverageConsultationTime = table.Column<int>(type: "int", nullable: true),
                    AverageWaitingTime = table.Column<int>(type: "int", nullable: true),
                    PrescriptionsWritten = table.Column<int>(type: "int", nullable: false),
                    LabTestsOrdered = table.Column<int>(type: "int", nullable: false),
                    ReferralsMade = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PendingPayments = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AverageRevenuePerConsultation = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalMessages = table.Column<int>(type: "int", nullable: false),
                    UnreadMessages = table.Column<int>(type: "int", nullable: false),
                    EmailsSent = table.Column<int>(type: "int", nullable: false),
                    SmsSent = table.Column<int>(type: "int", nullable: false),
                    EmergencyAlertsReceived = table.Column<int>(type: "int", nullable: false),
                    EmergencyAlertsResolved = table.Column<int>(type: "int", nullable: false),
                    AverageEmergencyResponseTime = table.Column<int>(type: "int", nullable: true),
                    TotalHoursWorked = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    BreakTimeTaken = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    AdditionalMetrics = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WeekNumber = table.Column<int>(type: "int", nullable: true),
                    MonthNumber = table.Column<int>(type: "int", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorAnalytics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorAnalytics_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReceiverId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MessageType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AttachmentFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    AttachmentFileSize = table.Column<long>(type: "bigint", nullable: true),
                    AttachmentMimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReplyToMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorMessages_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DoctorMessages_DoctorMessages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "DoctorMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoctorPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ThemePreset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThemeMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PrimaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccentColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShowPatientVitals = table.Column<bool>(type: "bit", nullable: false),
                    ShowAppointmentFeed = table.Column<bool>(type: "bit", nullable: false),
                    ShowChatWidget = table.Column<bool>(type: "bit", nullable: false),
                    ShowEmergencyAlerts = table.Column<bool>(type: "bit", nullable: false),
                    ShowAnalytics = table.Column<bool>(type: "bit", nullable: false),
                    DefaultDashboardView = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnableSoundNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableDesktopNotifications = table.Column<bool>(type: "bit", nullable: false),
                    EnableEmailAlerts = table.Column<bool>(type: "bit", nullable: false),
                    EnableSmsAlerts = table.Column<bool>(type: "bit", nullable: false),
                    NotificationFrequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WorkStartTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    WorkEndTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DefaultConsultationDuration = table.Column<int>(type: "int", nullable: true),
                    BreakDuration = table.Column<int>(type: "int", nullable: true),
                    WorkingDays = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreferredChartType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AnalyticsTimeRange = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShowRevenueAnalytics = table.Column<bool>(type: "bit", nullable: false),
                    ShowPerformanceMetrics = table.Column<bool>(type: "bit", nullable: false),
                    EnableAiInsights = table.Column<bool>(type: "bit", nullable: false),
                    EnableVoiceAssistant = table.Column<bool>(type: "bit", nullable: false),
                    EnableSmartScheduling = table.Column<bool>(type: "bit", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DateFormat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TimeFormat = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CustomSettings = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorPreferences_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientVitals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecordedByDoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    VitalType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SystolicValue = table.Column<int>(type: "int", nullable: true),
                    DiastolicValue = table.Column<int>(type: "int", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsAbnormal = table.Column<bool>(type: "bit", nullable: false),
                    IsAlertSent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientVitals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientVitals_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientVitals_AspNetUsers_RecordedByDoctorId",
                        column: x => x.RecordedByDoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmergencyAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RelatedVitalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecommendedAction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsNotificationSent = table.Column<bool>(type: "bit", nullable: false),
                    NotificationSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_AspNetUsers_AcknowledgedByUserId",
                        column: x => x.AcknowledgedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_PatientVitals_RelatedVitalId",
                        column: x => x.RelatedVitalId,
                        principalTable: "PatientVitals",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAnalytics_Doctor_Date",
                table: "DoctorAnalytics",
                columns: new[] { "DoctorId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAnalytics_Doctor_Year_Month",
                table: "DoctorAnalytics",
                columns: new[] { "DoctorId", "Year", "MonthNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAnalytics_Doctor_Year_Week",
                table: "DoctorAnalytics",
                columns: new[] { "DoctorId", "Year", "WeekNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorMessages_ConversationId",
                table: "DoctorMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorMessages_ReceiverId",
                table: "DoctorMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorMessages_ReplyToMessageId",
                table: "DoctorMessages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorMessages_Sender_Receiver_SentAt",
                table: "DoctorMessages",
                columns: new[] { "SenderId", "ReceiverId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorMessages_Status",
                table: "DoctorMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorPreferences_DoctorId",
                table: "DoctorPreferences",
                column: "DoctorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_AcknowledgedByUserId",
                table: "EmergencyAlerts",
                column: "AcknowledgedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_Category",
                table: "EmergencyAlerts",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_CreatedAt",
                table: "EmergencyAlerts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_Doctor_Status_Severity",
                table: "EmergencyAlerts",
                columns: new[] { "DoctorId", "Status", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_PatientId",
                table: "EmergencyAlerts",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_RelatedVitalId",
                table: "EmergencyAlerts",
                column: "RelatedVitalId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_ResolvedByUserId",
                table: "EmergencyAlerts",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVitals_PatientId_RecordedAt",
                table: "PatientVitals",
                columns: new[] { "PatientId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientVitals_RecordedByDoctorId",
                table: "PatientVitals",
                column: "RecordedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVitals_Severity",
                table: "PatientVitals",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_PatientVitals_VitalType",
                table: "PatientVitals",
                column: "VitalType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorAnalytics");

            migrationBuilder.DropTable(
                name: "DoctorMessages");

            migrationBuilder.DropTable(
                name: "DoctorPreferences");

            migrationBuilder.DropTable(
                name: "EmergencyAlerts");

            migrationBuilder.DropTable(
                name: "PatientVitals");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediChatAI_GraphQl.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsultationSessionId",
                table: "Prescriptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConsultationSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RoomId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MeetingLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ScheduledStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ActualDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    IsRecording = table.Column<bool>(type: "bit", nullable: false),
                    RecordingStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DoctorObservations = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TreatmentPlan = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FollowUpInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AISummary = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    TranscriptUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VideoQuality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AudioQuality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConnectionIssues = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PatientRating = table.Column<int>(type: "int", nullable: true),
                    PatientFeedback = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationSessions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationSessions_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationSessions_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultationSessionId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsMarkdown = table.Column<bool>(type: "bit", nullable: false),
                    FormattedContent = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    IsSharedWithPatient = table.Column<bool>(type: "bit", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AttachmentUrls = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    PreviousVersionContent = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    EditReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EditedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LastEditedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationNotes_AspNetUsers_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationNotes_AspNetUsers_EditedByUserId",
                        column: x => x.EditedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsultationNotes_ConsultationSessions_ConsultationSessionId",
                        column: x => x.ConsultationSessionId,
                        principalTable: "ConsultationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultationSessionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Relation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Permissions = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    InvitationToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TokenUsed = table.Column<bool>(type: "bit", nullable: false),
                    InvitedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    InvitedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvitationMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConnectionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovalReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationParticipants_AspNetUsers_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsultationParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConsultationParticipants_ConsultationSessions_ConsultationSessionId",
                        column: x => x.ConsultationSessionId,
                        principalTable: "ConsultationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationRecordings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultationSessionId = table.Column<int>(type: "int", nullable: false),
                    RecordingUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VideoCodec = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AudioCodec = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Bitrate = table.Column<int>(type: "int", nullable: true),
                    TranscriptUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TranscriptText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsTranscribed = table.Column<bool>(type: "bit", nullable: false),
                    TranscribedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TranscriptionLanguage = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AISummary = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    IsSummaryGenerated = table.Column<bool>(type: "bit", nullable: false),
                    SummaryGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsPatientAccessible = table.Column<bool>(type: "bit", nullable: false),
                    IsDoctorAccessible = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CloudinaryPublicId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CloudinaryAssetId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessingAttempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationRecordings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationRecordings_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationRecordings_ConsultationSessions_ConsultationSessionId",
                        column: x => x.ConsultationSessionId,
                        principalTable: "ConsultationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ConsultationSessionId",
                table: "Prescriptions",
                column: "ConsultationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_AuthorId",
                table: "ConsultationNotes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_EditedByUserId",
                table: "ConsultationNotes",
                column: "EditedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_IsDeleted",
                table: "ConsultationNotes",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_Session_Created",
                table: "ConsultationNotes",
                columns: new[] { "ConsultationSessionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationNotes_Type",
                table: "ConsultationNotes",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationParticipants_InvitationToken",
                table: "ConsultationParticipants",
                column: "InvitationToken",
                unique: true,
                filter: "[InvitationToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationParticipants_InvitedByUserId",
                table: "ConsultationParticipants",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationParticipants_Role",
                table: "ConsultationParticipants",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationParticipants_Session_Joined",
                table: "ConsultationParticipants",
                columns: new[] { "ConsultationSessionId", "JoinedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationParticipants_UserId",
                table: "ConsultationParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecordings_RecordedByUserId",
                table: "ConsultationRecordings",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecordings_Session_Started",
                table: "ConsultationRecordings",
                columns: new[] { "ConsultationSessionId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecordings_Status",
                table: "ConsultationRecordings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationRecordings_Type",
                table: "ConsultationRecordings",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationSessions_AppointmentId",
                table: "ConsultationSessions",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationSessions_Doctor_ScheduledStart",
                table: "ConsultationSessions",
                columns: new[] { "DoctorId", "ScheduledStartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationSessions_Patient_ScheduledStart",
                table: "ConsultationSessions",
                columns: new[] { "PatientId", "ScheduledStartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationSessions_RoomId",
                table: "ConsultationSessions",
                column: "RoomId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationSessions_Status",
                table: "ConsultationSessions",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Prescriptions_ConsultationSessions_ConsultationSessionId",
                table: "Prescriptions",
                column: "ConsultationSessionId",
                principalTable: "ConsultationSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prescriptions_ConsultationSessions_ConsultationSessionId",
                table: "Prescriptions");

            migrationBuilder.DropTable(
                name: "ConsultationNotes");

            migrationBuilder.DropTable(
                name: "ConsultationParticipants");

            migrationBuilder.DropTable(
                name: "ConsultationRecordings");

            migrationBuilder.DropTable(
                name: "ConsultationSessions");

            migrationBuilder.DropIndex(
                name: "IX_Prescriptions_ConsultationSessionId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "ConsultationSessionId",
                table: "Prescriptions");
        }
    }
}

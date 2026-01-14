using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediChatAI_GraphQl.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtpExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProfileImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Specialization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsuranceProvider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsurancePolicyNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsuranceGroupNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsuranceExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastProfileUpdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    YearsOfExperience = table.Column<int>(type: "int", nullable: true),
                    MedicalRegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EducationHistory = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AffiliatedHospitals = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConsultationHours = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AadhaarNumber = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    AadhaarCardImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MedicalCertificateUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MedicalCertificateFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsProfileCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsAadhaarVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsMedicalCertificateVerified = table.Column<bool>(type: "bit", nullable: false),
                    AiVerificationNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AadhaarVerificationFailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MedicalCertificateVerificationFailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProfileSubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsApprovedByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    AdminApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByAdminId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AdminRejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    AccountLockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordLastChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPasswordExpired = table.Column<bool>(type: "bit", nullable: false),
                    AuthenticatorKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAuthenticatorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GroupAvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SiteDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timezone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "int", nullable: false),
                    MaxLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    PasswordMinLength = table.Column<int>(type: "int", nullable: false),
                    RequireUppercase = table.Column<bool>(type: "bit", nullable: false),
                    RequireLowercase = table.Column<bool>(type: "bit", nullable: false),
                    RequireNumbers = table.Column<bool>(type: "bit", nullable: false),
                    RequireSpecialChars = table.Column<bool>(type: "bit", nullable: false),
                    EnableTwoFactor = table.Column<bool>(type: "bit", nullable: false),
                    RequireEmailVerification = table.Column<bool>(type: "bit", nullable: false),
                    PasswordExpiryDays = table.Column<int>(type: "int", nullable: false),
                    AccountLockoutMinutes = table.Column<int>(type: "int", nullable: false),
                    SmtpServer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmtpPort = table.Column<int>(type: "int", nullable: false),
                    SmtpUsername = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmtpPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmtpEnableSsl = table.Column<bool>(type: "bit", nullable: false),
                    SmtpFromEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmtpFromName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SmtpIsConfigured = table.Column<bool>(type: "bit", nullable: false),
                    SmtpLastTestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SmtpLastTestSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    SmtpLastTestMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailTemplateTheme = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailPrimaryColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailSecondaryColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailBackgroundColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailTextColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailHeaderImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailIncludeFooter = table.Column<bool>(type: "bit", nullable: false),
                    EmailFooterText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailIncludeSocialLinks = table.Column<bool>(type: "bit", nullable: false),
                    EmailFacebookUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailTwitterUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailLinkedInUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailInstagramUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnableEmailNotifications = table.Column<bool>(type: "bit", nullable: false),
                    NotifyAdminOnNewDoctor = table.Column<bool>(type: "bit", nullable: false),
                    NotifyDoctorOnApproval = table.Column<bool>(type: "bit", nullable: false),
                    NotifyUserOnLogin = table.Column<bool>(type: "bit", nullable: false),
                    NotifyUserOnPasswordChange = table.Column<bool>(type: "bit", nullable: false),
                    NotifyUserOn2FAChange = table.Column<bool>(type: "bit", nullable: false),
                    ThemeMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemePrimaryColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeAccentColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeBackgroundColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeTextColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeSidebarStyle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeFontSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeEnableAnimations = table.Column<bool>(type: "bit", nullable: false),
                    ThemeCompactMode = table.Column<bool>(type: "bit", nullable: false),
                    ThemePreset = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThemeHighContrast = table.Column<bool>(type: "bit", nullable: false),
                    ThemeBorderRadius = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionVersion = table.Column<int>(type: "int", nullable: false),
                    SessionVersionUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AIChatSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSaved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIChatSessions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AppointmentDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsVirtual = table.Column<bool>(type: "bit", nullable: false),
                    MeetingLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReasonForVisit = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PatientNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReminderSent = table.Column<bool>(type: "bit", nullable: false),
                    ReminderSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FollowUpRequired = table.Column<bool>(type: "bit", nullable: false),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsultationFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaymentTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "LabResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OrderedByDoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TestName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TestType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LabName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LabAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OrderedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResultDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferenceRange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Abnormality = table.Column<int>(type: "int", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Interpretation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FileMimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsReviewed = table.Column<bool>(type: "bit", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByDoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TestParameters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabResults_AspNetUsers_OrderedByDoctorId",
                        column: x => x.OrderedByDoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LabResults_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabResults_AspNetUsers_ReviewedByDoctorId",
                        column: x => x.ReviewedByDoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EmailNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    InAppNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PushNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SoundEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AppointmentNotifications = table.Column<bool>(type: "bit", nullable: false),
                    DoctorNotifications = table.Column<bool>(type: "bit", nullable: false),
                    AdminNotifications = table.Column<bool>(type: "bit", nullable: false),
                    SecurityNotifications = table.Column<bool>(type: "bit", nullable: false),
                    SystemNotifications = table.Column<bool>(type: "bit", nullable: false),
                    MedicalNotifications = table.Column<bool>(type: "bit", nullable: false),
                    QuietHoursEnabled = table.Column<bool>(type: "bit", nullable: false),
                    QuietHoursStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    QuietHoursEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    Timezone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActionText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
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
                name: "TimeBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BlockType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RepeatUntilDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeBlocks_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserActivities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserActivities_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MuteNotifications = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMembers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupMembers_GroupConversations_GroupConversationId",
                        column: x => x.GroupConversationId,
                        principalTable: "GroupConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AttachmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AttachmentSize = table.Column<long>(type: "bigint", nullable: true),
                    ReplyToMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupMessages_GroupConversations_GroupConversationId",
                        column: x => x.GroupConversationId,
                        principalTable: "GroupConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMessages_GroupMessages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "GroupMessages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AIChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIChatMessages_AIChatSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "AIChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PreferredDoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AlternatePhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactRelation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CurrentMedications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PastMedicalConditions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChronicDiseases = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreviousSurgeries = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FamilyMedicalHistory = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SymptomDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SymptomDuration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SymptomSeverity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReasonForVisit = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PreferredAppointmentType = table.Column<int>(type: "int", nullable: false),
                    VoiceRecordingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VoiceRecordingTranscript = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    PhotoUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalDocumentUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsuranceCardUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsuranceProvider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InsurancePolicyNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InsuranceGroupNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InsuranceCardFrontUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InsuranceCardBackUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InsuranceExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PreferredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreferredTimeSlot = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByDoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    PatientAdditionalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentRequests_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppointmentRequests_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppointmentRequests_AspNetUsers_PreferredDoctorId",
                        column: x => x.PreferredDoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppointmentRequests_AspNetUsers_ReviewedByDoctorId",
                        column: x => x.ReviewedByDoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateTable(
                name: "GroupMessageReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ReactedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMessageReactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupMessageReactions_GroupMessages_GroupMessageId",
                        column: x => x.GroupMessageId,
                        principalTable: "GroupMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupMessageReadStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMessageReadStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMessageReadStatuses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupMessageReadStatuses_GroupMessages_GroupMessageId",
                        column: x => x.GroupMessageId,
                        principalTable: "GroupMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PublicId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppointmentRequestId = table.Column<int>(type: "int", nullable: true),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    UploadedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedByDoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientDocuments_AppointmentRequests_AppointmentRequestId",
                        column: x => x.AppointmentRequestId,
                        principalTable: "AppointmentRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PatientDocuments_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PatientDocuments_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientDocuments_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PatientDocuments_AspNetUsers_VerifiedByDoctorId",
                        column: x => x.VerifiedByDoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
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

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DoctorId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    ConsultationSessionId = table.Column<int>(type: "int", nullable: true),
                    PrescribedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RefillsAllowed = table.Column<int>(type: "int", nullable: false),
                    RefillsUsed = table.Column<int>(type: "int", nullable: false),
                    LastRefillDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Diagnosis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DoctorNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PharmacyNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DoctorSignature = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PharmacyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PharmacyContact = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsDispensed = table.Column<bool>(type: "bit", nullable: false),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Prescriptions_AspNetUsers_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prescriptions_ConsultationSessions_ConsultationSessionId",
                        column: x => x.ConsultationSessionId,
                        principalTable: "ConsultationSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedicationReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PrescriptionId = table.Column<int>(type: "int", nullable: true),
                    MedicationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    ReminderTimes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IntervalHours = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastReminderSent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextReminderAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DaysOfWeek = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EnablePushNotification = table.Column<bool>(type: "bit", nullable: false),
                    EnableEmail = table.Column<bool>(type: "bit", nullable: false),
                    EnableSms = table.Column<bool>(type: "bit", nullable: false),
                    SnoozeDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveMissedDoses = table.Column<int>(type: "int", nullable: false),
                    TotalDosesTaken = table.Column<int>(type: "int", nullable: false),
                    TotalDosesMissed = table.Column<int>(type: "int", nullable: false),
                    AdherenceRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationReminders_AspNetUsers_PatientId",
                        column: x => x.PatientId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicationReminders_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrescriptionId = table.Column<int>(type: "int", nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GenericName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Dosage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Route = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Form = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Instructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Warnings = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SideEffects = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrescriptionItems_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicationAdherences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SnoozeCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicationAdherences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicationAdherences_MedicationReminders_ReminderId",
                        column: x => x.ReminderId,
                        principalTable: "MedicationReminders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsMuted = table.Column<bool>(type: "bit", nullable: false),
                    MutedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReadMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    User1Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    User2Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    GroupName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GroupDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GroupImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_User1Id",
                        column: x => x.User1Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_User2Id",
                        column: x => x.User2Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                        name: "FK_DoctorMessages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DoctorMessages_DoctorMessages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "DoctorMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageReactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageReactions_DoctorMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "DoctorMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIChatMessages_SessionId",
                table: "AIChatMessages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AIChatSessions_UserId",
                table: "AIChatSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_AppointmentId",
                table: "AppointmentRequests",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_Doctor_Status",
                table: "AppointmentRequests",
                columns: new[] { "PreferredDoctorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_Patient_Status",
                table: "AppointmentRequests",
                columns: new[] { "PatientId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_PreferredDate",
                table: "AppointmentRequests",
                column: "PreferredDate");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_RequestedAt",
                table: "AppointmentRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentRequests_ReviewedByDoctorId",
                table: "AppointmentRequests",
                column: "ReviewedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Doctor_DateTime",
                table: "Appointments",
                columns: new[] { "DoctorId", "AppointmentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Patient_DateTime",
                table: "Appointments",
                columns: new[] { "PatientId", "AppointmentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status",
                table: "Appointments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Type",
                table: "Appointments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsApprovedByAdmin",
                table: "AspNetUsers",
                column: "IsApprovedByAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IsProfileCompleted",
                table: "AspNetUsers",
                column: "IsProfileCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_MedicalRegistrationNumber",
                table: "AspNetUsers",
                column: "MedicalRegistrationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "AspNetUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

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

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_Conversation_User",
                table: "ConversationParticipants",
                columns: new[] { "ConversationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_User_Active",
                table: "ConversationParticipants",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_CreatedById",
                table: "Conversations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_IsDeleted",
                table: "Conversations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_LastMessageId",
                table: "Conversations",
                column: "LastMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Type_LastActivity",
                table: "Conversations",
                columns: new[] { "Type", "LastActivityAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_User2Id",
                table: "Conversations",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_Users",
                table: "Conversations",
                columns: new[] { "User1Id", "User2Id" },
                unique: true,
                filter: "[User1Id] IS NOT NULL AND [User2Id] IS NOT NULL");

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
                name: "IX_GroupConversations_CreatedBy_CreatedAt",
                table: "GroupConversations",
                columns: new[] { "CreatedBy", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupConversations_IsActive",
                table: "GroupConversations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_Group_User",
                table: "GroupMembers",
                columns: new[] { "GroupConversationId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_User_Active",
                table: "GroupMembers",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessageReactions_Message_User_Emoji",
                table: "GroupMessageReactions",
                columns: new[] { "GroupMessageId", "UserId", "Emoji" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessageReactions_MessageId",
                table: "GroupMessageReactions",
                column: "GroupMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessageReactions_UserId",
                table: "GroupMessageReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessageReadStatuses_Message_User",
                table: "GroupMessageReadStatuses",
                columns: new[] { "GroupMessageId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessageReadStatuses_User_ReadAt",
                table: "GroupMessageReadStatuses",
                columns: new[] { "UserId", "ReadAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessages_Group_SentAt",
                table: "GroupMessages",
                columns: new[] { "GroupConversationId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessages_IsDeleted",
                table: "GroupMessages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessages_ReplyToMessageId",
                table: "GroupMessages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMessages_Sender_SentAt",
                table: "GroupMessages",
                columns: new[] { "SenderId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_IsCritical",
                table: "LabResults",
                column: "IsCritical");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_OrderedBy_OrderedAt",
                table: "LabResults",
                columns: new[] { "OrderedByDoctorId", "OrderedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_Patient_ResultDate",
                table: "LabResults",
                columns: new[] { "PatientId", "ResultDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_ReviewedByDoctorId",
                table: "LabResults",
                column: "ReviewedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_Status",
                table: "LabResults",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationAdherence_Reminder_ScheduledTime",
                table: "MedicationAdherences",
                columns: new[] { "ReminderId", "ScheduledTime" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationAdherences_Status",
                table: "MedicationAdherences",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationReminders_NextReminderAt",
                table: "MedicationReminders",
                column: "NextReminderAt");

            migrationBuilder.CreateIndex(
                name: "IX_MedicationReminders_Patient_IsActive",
                table: "MedicationReminders",
                columns: new[] { "PatientId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MedicationReminders_PrescriptionId",
                table: "MedicationReminders",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_Message_User",
                table: "MessageReactions",
                columns: new[] { "MessageId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_MessageId",
                table: "MessageReactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_UserId",
                table: "MessageReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationPreferences_UserId",
                table: "NotificationPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Category",
                table: "Notifications",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_GroupId",
                table: "Notifications",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Priority",
                table: "Notifications",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_ExpiresAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientDocuments_AppointmentId",
                table: "PatientDocuments",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDocuments_AppointmentRequestId",
                table: "PatientDocuments",
                column: "AppointmentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDocuments_DocumentType",
                table: "PatientDocuments",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDocuments_Patient_UploadedAt",
                table: "PatientDocuments",
                columns: new[] { "PatientId", "UploadedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientDocuments_UploadedById",
                table: "PatientDocuments",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDocuments_VerifiedByDoctorId",
                table: "PatientDocuments",
                column: "VerifiedByDoctorId");

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

            migrationBuilder.CreateIndex(
                name: "IX_PrescriptionItems_PrescriptionId",
                table: "PrescriptionItems",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_AppointmentId",
                table: "Prescriptions",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_ConsultationSessionId",
                table: "Prescriptions",
                column: "ConsultationSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_Doctor_PrescribedDate",
                table: "Prescriptions",
                columns: new[] { "DoctorId", "PrescribedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_Patient_PrescribedDate",
                table: "Prescriptions",
                columns: new[] { "PatientId", "PrescribedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_Status",
                table: "Prescriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TimeBlocks_DoctorId",
                table: "TimeBlocks",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_ActivityType",
                table: "UserActivities",
                column: "ActivityType");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_Timestamp",
                table: "UserActivities",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_UserActivities_UserId",
                table: "UserActivities",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationParticipants_Conversations_ConversationId",
                table: "ConversationParticipants",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_DoctorMessages_LastMessageId",
                table: "Conversations",
                column: "LastMessageId",
                principalTable: "DoctorMessages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_CreatedById",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_User1Id",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_User2Id",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorMessages_AspNetUsers_ReceiverId",
                table: "DoctorMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorMessages_AspNetUsers_SenderId",
                table: "DoctorMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorMessages_Conversations_ConversationId",
                table: "DoctorMessages");

            migrationBuilder.DropTable(
                name: "AIChatMessages");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ConsultationNotes");

            migrationBuilder.DropTable(
                name: "ConsultationParticipants");

            migrationBuilder.DropTable(
                name: "ConsultationRecordings");

            migrationBuilder.DropTable(
                name: "ConversationParticipants");

            migrationBuilder.DropTable(
                name: "DoctorAnalytics");

            migrationBuilder.DropTable(
                name: "DoctorPreferences");

            migrationBuilder.DropTable(
                name: "EmergencyAlerts");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "GroupMessageReactions");

            migrationBuilder.DropTable(
                name: "GroupMessageReadStatuses");

            migrationBuilder.DropTable(
                name: "LabResults");

            migrationBuilder.DropTable(
                name: "MedicationAdherences");

            migrationBuilder.DropTable(
                name: "MessageReactions");

            migrationBuilder.DropTable(
                name: "NotificationPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PatientDocuments");

            migrationBuilder.DropTable(
                name: "PrescriptionItems");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "TimeBlocks");

            migrationBuilder.DropTable(
                name: "UserActivities");

            migrationBuilder.DropTable(
                name: "AIChatSessions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "PatientVitals");

            migrationBuilder.DropTable(
                name: "GroupMessages");

            migrationBuilder.DropTable(
                name: "MedicationReminders");

            migrationBuilder.DropTable(
                name: "AppointmentRequests");

            migrationBuilder.DropTable(
                name: "GroupConversations");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "ConsultationSessions");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "DoctorMessages");
        }
    }
}

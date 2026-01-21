using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Entities;
using MediChatAI_GraphQl.GraphQL.Queries;
using MediChatAI_GraphQl.GraphQL.Mutations;
using MediChatAI_GraphQl.GraphQL.Schemas;
using MediChatAI_GraphQl.Features.Notifications.Hubs;
using MediChatAI_GraphQl.Features.Chat.Hubs;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Emergency;
using MediChatAI_GraphQl.Core.Interfaces.Services.Appointment;
using MediChatAI_GraphQl.Core.Interfaces.Services.Document;
using MediChatAI_GraphQl.Core.Interfaces.Services.Prescription;
using MediChatAI_GraphQl.Features.Admin.Services;
using MediChatAI_GraphQl.Features.Authentication.Services;
using MediChatAI_GraphQl.Features.Doctor.Services;
using MediChatAI_GraphQl.Features.Emergency.Services;
using MediChatAI_GraphQl.Features.Notifications.Services;
using MediChatAI_GraphQl.Shared.Services;
using MediChatAI_GraphQl.Features.Patient.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container with optimized connection pooling
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.CommandTimeout(120); // Increased to 120 seconds to handle slow SQL Server operations
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
        })
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
    .EnableDetailedErrors(builder.Environment.IsDevelopment()));

// Identity Configuration with dynamic settings
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Identity options dynamically from database
builder.Services.AddSingleton<IConfigureOptions<IdentityOptions>, DynamicIdentityOptionsConfigurator>();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Configure JWT for SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/notificationHub") || path.StartsWithSegments("/chathub")))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IActivityLoggingService, ActivityLoggingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IScheduledReportService, ScheduledReportService>();
builder.Services.AddHttpClient<IGeminiVerificationService, GeminiVerificationService>();
builder.Services.AddScoped<IDoctorOnboardingService, DoctorOnboardingService>();
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient<IHospitalLocationService, HospitalLocationService>();
builder.Services.AddHttpClient<IGeminiEmergencyChatService, GeminiEmergencyChatService>();

// AI Chatbot Service
builder.Services.AddHttpClient<MediChatAI_GraphQl.Core.Interfaces.Services.AIChat.IAIChatbotService, MediChatAI_GraphQl.Features.AIChat.Services.GeminiAIChatService>();

// Doctor Dashboard Services
builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();
builder.Services.AddScoped<IDoctorAnalyticsService, DoctorAnalyticsService>();
builder.Services.AddScoped<IPatientVitalsService, PatientVitalsService>();
builder.Services.AddScoped<IDoctorChatService, DoctorChatService>();
builder.Services.AddScoped<IEmergencyAlertService, EmergencyAlertService>();
builder.Services.AddScoped<IDoctorPreferencesService, DoctorPreferencesService>();
builder.Services.AddScoped<ITimeBlockService, TimeBlockService>();

// Appointment System Services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPatientDocumentService, PatientDocumentService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IDrugInteractionService, DrugInteractionService>();
builder.Services.AddScoped<IDigitalSignatureService, DigitalSignatureService>();
builder.Services.AddScoped<IPrescriptionPdfService, PrescriptionPdfService>();

// Consultation Services
builder.Services.AddScoped<MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationSessionService, ConsultationSessionService>();
builder.Services.AddScoped<MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationRecordingService, ConsultationRecordingService>();
builder.Services.AddHttpClient<MediChatAI_GraphQl.Core.Interfaces.Services.Consultation.IConsultationTranscriptionService, ConsultationTranscriptionService>();

// Patient Services
builder.Services.AddScoped<MediChatAI_GraphQl.Core.Interfaces.Services.Patient.ILabResultService, MediChatAI_GraphQl.Features.Patient.Services.LabResultService>();
builder.Services.AddScoped<MediChatAI_GraphQl.Core.Interfaces.Services.Patient.IMedicationReminderService, MediChatAI_GraphQl.Features.Patient.Services.MedicationReminderService>();
builder.Services.AddScoped<MediChatAI_GraphQl.Core.Interfaces.Services.Patient.IMessagingService, MediChatAI_GraphQl.Features.Patient.Services.MessagingService>();
builder.Services.AddScoped<MediChatAI_GraphQl.Core.Interfaces.Services.Patient.IPatientManagementService, MediChatAI_GraphQl.Features.Patient.Services.PatientManagementService>();

// Conversation Initialization Service
builder.Services.AddScoped<MediChatAI_GraphQl.Shared.Services.ConversationInitializationService>();

// Cloudinary Service Configuration
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// Geoapify Service Configuration
builder.Services.Configure<MediChatAI_GraphQl.Features.Emergency.Services.GeoapifySettings>(builder.Configuration.GetSection("Geoapify"));
builder.Services.AddHttpClient<MediChatAI_GraphQl.Features.Emergency.Services.IGeoapifyService, MediChatAI_GraphQl.Features.Emergency.Services.GeoapifyService>();

// Background Services
builder.Services.AddHostedService<SeedDataHostedService>();
builder.Services.AddHostedService<ScheduledReportBackgroundService>();

// Memory Cache for performance
builder.Services.AddMemoryCache();

// Validate Gemini API key is configured
if (string.IsNullOrEmpty(builder.Configuration["GeminiApiKey"]))
{
    throw new InvalidOperationException(
        "GeminiApiKey is not configured. Please add it to appsettings.json");
}

// Controllers
builder.Services.AddControllers();

// SignalR
builder.Services.AddSignalR();

// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddType<UserType>()
    .AddTypeExtension<MediChatAI_GraphQl.Features.AIChat.Mutations.AIChatMutations>()
    .AddTypeExtension<MediChatAI_GraphQl.Features.AIChat.Queries.AIChatQueries>()
    .ModifyRequestOptions(opt =>
    {
        opt.ExecutionTimeout = TimeSpan.FromMinutes(5); // Increase timeout for long operations like export
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm", policy =>
    {
        policy.WithOrigins("https://localhost:7023", "http://localhost:5212")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configure static file serving for Storage directory
var storagePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Storage");
if (!Directory.Exists(storagePath))
{
    Directory.CreateDirectory(storagePath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/Storage"
});

// Configure static file serving for uploads directory (for profile images)
var uploadsPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseCors("AllowBlazorWasm");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();
app.MapHub<NotificationHub>("/notificationHub");
app.MapHub<ChatHub>("/chathub");

// Database seeding now handled by SeedDataHostedService in background
// This allows the app to start immediately without waiting for seeding

app.Run();
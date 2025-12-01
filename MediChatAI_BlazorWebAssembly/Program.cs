using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MediChatAI_BlazorWebAssembly;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

// Core Services
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Core.Services.Theme;
using MediChatAI_BlazorWebAssembly.Core.Services.Session;
using MediChatAI_BlazorWebAssembly.Core.Services.UI;
using MediChatAI_BlazorWebAssembly.Core.Services.State;
using MediChatAI_BlazorWebAssembly.Core.Services.Consultation;

// Authentication Services
using MediChatAI_BlazorWebAssembly.Features.Authentication.Services;

// Admin Services
using MediChatAI_BlazorWebAssembly.Features.Admin.Services;

// Doctor Services
using MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

// Emergency Services
using MediChatAI_BlazorWebAssembly.Features.Emergency.Services;

// Profile Services
using MediChatAI_BlazorWebAssembly.Features.Profile.Services;

// Notification Services
using MediChatAI_BlazorWebAssembly.Features.Notifications.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5095") });

// Core Services
builder.Services.AddScoped<IGraphQLService, GraphQLService>();
builder.Services.AddSingleton<AppSettingsState>();
builder.Services.AddSingleton<ThemeState>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddSingleton<IToastService, ToastService>();
builder.Services.AddScoped<IConsultationRoomService, ConsultationRoomService>();
builder.Services.AddScoped<IConsultationRecordingService, ConsultationRecordingService>();
builder.Services.AddScoped<ConsultationState>();

// Session Management Services
builder.Services.AddSingleton<SessionNotificationState>(); // Singleton state tracker (no dependencies)
builder.Services.AddScoped<ITokenValidationService, TokenValidationService>();
builder.Services.AddScoped<ISessionMonitorService, SessionMonitorService>();
builder.Services.AddScoped<ISessionTimerService, SessionTimerService>();

// Authentication Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<CustomAuthStateProvider>());

// Profile Services
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// Admin Services
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IReportService, ReportService>();

// Doctor Services
builder.Services.AddScoped<IDoctorOnboardingService, DoctorOnboardingService>();
builder.Services.AddScoped<IPatientManagementService, PatientManagementService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Doctor.Services.IPrescriptionService, MediChatAI_BlazorWebAssembly.Features.Doctor.Services.PrescriptionService>();
builder.Services.AddScoped<IDoctorDashboardService, DoctorDashboardService>();
builder.Services.AddScoped<ITimeBlockService, TimeBlockService>();
builder.Services.AddScoped<IDoctorReportService, DoctorReportService>();

// Patient Services
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.IPrescriptionService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.PrescriptionService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.IDashboardService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.DashboardService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.IAppointmentService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.AppointmentService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.IHealthRecordService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.HealthRecordService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.IConsultationService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.ConsultationService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.IHealthMetricsService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.HealthMetricsService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.IMessagingService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.MessagingService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.ISmartReplyService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.SmartReplyService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.IMedicalTermService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.MedicalTermService>();

// AI Chatbot Services
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Patient.Services.IAIChatbotService, MediChatAI_BlazorWebAssembly.Features.Patient.Services.AIChatbotService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Doctor.Services.IAIChatbotService, MediChatAI_BlazorWebAssembly.Features.Doctor.Services.AIChatbotService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services.IConversationStorageService, MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services.ConversationStorageService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services.IImageUploadService, MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services.ImageUploadService>();
builder.Services.AddScoped<MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services.IVoiceToTextService, MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Services.VoiceToTextService>();

// Emergency Services
builder.Services.AddScoped<IHospitalService, HospitalService>();

// Emergency Chat with Resilience (Decorator Pattern)
builder.Services.AddScoped<EmergencyChatService>(); // Register original service
builder.Services.AddScoped<IEmergencyChatService, ResilientEmergencyChatService>(); // Wrap with resilient decorator

// Hospital Caching
builder.Services.AddMemoryCache(); // Built-in .NET memory cache
builder.Services.AddScoped<HospitalCacheService>();

// Notification Services
builder.Services.AddScoped<INotificationService, NotificationService>();

// Authentication & Authorization
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();

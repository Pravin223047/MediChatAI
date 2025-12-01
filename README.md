# MediChatAI

A comprehensive full-stack telemedicine and healthcare management platform built with Blazor WebAssembly and ASP.NET Core GraphQL, featuring AI-powered medical assistance, video consultations, and emergency services.

## Overview

MediChatAI is an enterprise-grade healthcare-as-a-service (HaaS) platform that bridges patients, doctors, and AI to provide comprehensive telehealth services. The platform enables remote consultations, intelligent health monitoring, emergency assistance, and complete medical record management.

## Key Features

### For Patients
- **AI-Powered Medical Chatbot** - Get instant health guidance powered by Google Gemini AI
- **Emergency Medical Assistance** - Access emergency chat with automatic hospital locator
- **Appointment Booking** - Schedule consultations with verified doctors
- **Video Consultations** - High-quality video calls with recording and transcription
- **Health Records Management** - Store lab results, prescriptions, and medical documents
- **Medication Tracking** - Automated reminders and adherence monitoring
- **Secure Messaging** - Direct communication with healthcare providers
- **Real-time Notifications** - Stay updated on appointments, messages, and alerts

### For Doctors
- **AI-Powered Verification** - Automated Aadhaar and medical certificate verification
- **Patient Management** - Comprehensive dashboard with patient vitals and history
- **Appointment Management** - Flexible time block scheduling system
- **Digital Prescriptions** - Create prescriptions with drug interaction checking
- **Video Consultations** - Conduct consultations with recording and note-taking
- **Analytics Dashboard** - Practice metrics, patient statistics, and insights
- **Emergency Alerts** - Receive critical patient alerts in real-time
- **Smart Messaging** - Secure communication with AI-powered reply suggestions

### For Administrators
- **User Management** - Approve doctors, manage users, and control access
- **System Settings** - Configure security, email, appearance, and integrations
- **Activity Auditing** - Comprehensive logging and monitoring
- **Reports & Analytics** - Generate system-wide reports and insights
- **Configuration Management** - Manage SMTP, API keys, and system parameters

## Technology Stack

### Frontend
- **Blazor WebAssembly** (.NET 9.0) - Modern SPA framework
- **Tailwind CSS** - Utility-first CSS with dark/light mode support
- **StrawberryShake** - GraphQL client for .NET
- **SignalR Client** - Real-time communication
- **Blazored.LocalStorage** - Client-side data persistence

### Backend
- **ASP.NET Core 9.0** - High-performance web API
- **HotChocolate GraphQL** - GraphQL server implementation
- **Entity Framework Core 9.0** - ORM for database operations
- **SQL Server** - Relational database
- **SignalR** - Real-time bidirectional communication
- **JWT Authentication** - Secure token-based auth

### External Services
- **Google Gemini AI** - Medical chatbot and AI verification
- **Cloudinary** - Media and document storage
- **Geoapify** - Hospital location and mapping services
- **Gmail SMTP** - Email notifications and OTPs
- **RapidAPI Maps** - Additional location data

## Project Structure

```
MediChatAI/
├── MediChatAI.sln
├── MediChatAI_GraphQl/              # Backend API
│   ├── Features/
│   │   ├── Admin/                   # Admin management
│   │   ├── AIChat/                  # Gemini AI integration
│   │   ├── Authentication/          # JWT & Identity
│   │   ├── Chat/                    # SignalR hubs
│   │   ├── Doctor/                  # Doctor services
│   │   ├── Emergency/               # Emergency services
│   │   ├── Notifications/           # Real-time notifications
│   │   └── Patient/                 # Patient services
│   ├── Core/                        # Domain entities & interfaces
│   ├── Infrastructure/              # DbContext & migrations
│   ├── GraphQL/                     # Queries, Mutations, Types
│   └── Program.cs                   # App configuration
└── MediChatAI_BlazorWebAssembly/    # Frontend SPA
    ├── Features/
    │   ├── Admin/                   # Admin dashboard
    │   ├── Authentication/          # Login/Register
    │   ├── Consultation/            # Video consultations
    │   ├── Doctor/                  # Doctor dashboard
    │   ├── Emergency/               # Emergency services UI
    │   ├── Notifications/           # Notification system
    │   ├── Patient/                 # Patient dashboard
    │   ├── Profile/                 # User profiles
    │   └── Shared/                  # Shared components
    ├── Core/                        # Services & state management
    └── Program.cs                   # DI configuration
```

## Prerequisites

### Required Software
- **.NET 9.0 SDK** or later - [Download](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** (v17.8+) or **Visual Studio Code** with C# extension
- **SQL Server 2019+** or **SQL Server Express** - [Download](https://www.microsoft.com/sql-server/sql-server-downloads)
- **Node.js 18+** - [Download](https://nodejs.org/) (for Tailwind CSS)

### Required API Keys
You'll need to obtain API keys from the following services:

1. **Google Gemini AI API** - [Get API Key](https://ai.google.dev/)
2. **Cloudinary Account** - [Sign Up](https://cloudinary.com/)
3. **Geoapify API** - [Get API Key](https://www.geoapify.com/)
4. **RapidAPI Account** - [Sign Up](https://rapidapi.com/)
5. **Gmail App Password** - [Generate](https://support.google.com/accounts/answer/185833)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/YOUR_USERNAME/MediChatAI.git
cd MediChatAI
```

### 2. Database Setup

#### Option A: Using SQL Server Management Studio (SSMS)
1. Open SSMS and connect to your SQL Server instance
2. Create a new database named `MediChatAIDatabase`
3. Update the connection string in your configuration (step 3)

#### Option B: Using Command Line
```bash
sqlcmd -S YOUR_SERVER_NAME\SQLEXPRESS -Q "CREATE DATABASE MediChatAIDatabase"
```

### 3. Backend Configuration

1. Navigate to the backend directory:
```bash
cd MediChatAI_GraphQl
```

2. Copy the template configuration:
```bash
copy appsettings.template.json appsettings.json
```

3. Edit `appsettings.json` and replace all placeholder values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=YOUR_SERVER\\SQLEXPRESS;Initial Catalog=MediChatAIDatabase;..."
  },
  "JwtSettings": {
    "Secret": "YOUR_SECURE_256BIT_SECRET_KEY"
  },
  "SmtpSettings": {
    "Username": "your-email@gmail.com",
    "Password": "your-gmail-app-password"
  },
  "GeminiApiKey": "YOUR_GEMINI_API_KEY",
  "RapidAPI": {
    "MapsData": {
      "ApiKey": "YOUR_RAPIDAPI_KEY"
    }
  },
  "Geoapify": {
    "ApiKey": "YOUR_GEOAPIFY_KEY"
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

4. Run database migrations:
```bash
dotnet ef database update
```

If you don't have EF tools installed:
```bash
dotnet tool install --global dotnet-ef
dotnet ef database update
```

### 4. Frontend Configuration

1. Navigate to the frontend directory:
```bash
cd ..\MediChatAI_BlazorWebAssembly
```

2. Install Node.js dependencies:
```bash
npm install
```

3. Build Tailwind CSS:
```bash
npm run build:css
```

### 5. Running the Application

#### Option A: Using Visual Studio 2022
1. Open `MediChatAI.sln`
2. Right-click the solution → **Configure Startup Projects**
3. Select **Multiple startup projects**
4. Set both `MediChatAI_GraphQl` and `MediChatAI_BlazorWebAssembly` to **Start**
5. Press **F5** to run

#### Option B: Using Command Line

**Terminal 1 - Backend:**
```bash
cd MediChatAI_GraphQl
dotnet run
```
Backend will be available at: `https://localhost:7247`

**Terminal 2 - Frontend:**
```bash
cd MediChatAI_BlazorWebAssembly
dotnet run
```
Frontend will be available at: `https://localhost:7023`

**Terminal 3 - Tailwind CSS Watch (Development):**
```bash
cd MediChatAI_BlazorWebAssembly
npm run watch:css
```

### 6. Initial Setup

1. Open the frontend at `https://localhost:7023`
2. Register a new account
3. For doctor accounts, you'll need admin approval after registration
4. The first user can be manually promoted to admin in the database

## Configuration Guide

### Environment Variables

You can use environment variables instead of `appsettings.json`:

```bash
# Windows
setx ConnectionStrings__DefaultConnection "your-connection-string"
setx GeminiApiKey "your-api-key"

# Linux/Mac
export ConnectionStrings__DefaultConnection="your-connection-string"
export GeminiApiKey="your-api-key"
```

### Database Connection String

Update `YOUR_SERVER` with your SQL Server instance name:
- Local: `localhost\SQLEXPRESS` or `.\SQLEXPRESS`
- Named instance: `COMPUTERNAME\INSTANCENAME`
- Default instance: `localhost` or `.`

### JWT Secret Key

Generate a secure 256-bit key:
```bash
# PowerShell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Max 256 }))
```

### Gmail App Password

1. Enable 2-factor authentication on your Google account
2. Go to **Google Account** → **Security** → **App passwords**
3. Generate a new app password for "Mail"
4. Use this password in `SmtpSettings.Password`

## Running in Production

### Azure Deployment

1. **Publish Backend to Azure App Service:**
```bash
cd MediChatAI_GraphQl
dotnet publish -c Release
```

2. **Publish Frontend to Azure Static Web Apps or App Service:**
```bash
cd MediChatAI_BlazorWebAssembly
dotnet publish -c Release
```

3. **Configure Azure SQL Database:**
   - Create Azure SQL Database
   - Update connection string in App Service Configuration
   - Run migrations

4. **Set Environment Variables in Azure:**
   - Go to App Service → **Configuration** → **Application settings**
   - Add all required API keys and secrets

### Docker Support

Create `Dockerfile` for containerized deployment (not included in this version).

## Troubleshooting

### Database Connection Issues
- Verify SQL Server is running: `services.msc` → **SQL Server (SQLEXPRESS)**
- Check Windows Authentication is enabled
- Ensure TCP/IP is enabled in SQL Server Configuration Manager
- Verify firewall rules allow SQL Server connections

### Migration Errors
```bash
# Reset migrations
dotnet ef database drop
dotnet ef database update
```

### Tailwind CSS Not Loading
```bash
cd MediChatAI_BlazorWebAssembly
npm install
npm run build:css
```

### SignalR Connection Failures
- Check CORS settings in `Program.cs`
- Verify frontend URL matches backend CORS policy
- Ensure JWT token is valid

### API Key Errors
- Verify all API keys are correctly set in `appsettings.json`
- Check API key quotas haven't been exceeded
- Ensure no extra spaces in configuration values

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'feat: add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contact

For questions, issues, or contributions:
- Email: kshirsagarpravin.1111@gmail.com
- GitHub Issues: [Create an issue](https://github.com/YOUR_USERNAME/MediChatAI/issues)

## Acknowledgments

- **Google Gemini AI** for powering the medical chatbot
- **Cloudinary** for media storage solutions
- **Geoapify** for location services
- The open-source community for amazing libraries and tools

---

Built with care by the MediChatAI team

using System.Net;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using System.Net.Mail;
using MediChatAI_GraphQl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediChatAI_GraphQl.Shared.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly ApplicationDbContext _context;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, ApplicationDbContext context)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    public async Task SendEmailVerificationAsync(string email, string firstName, string verificationLink)
    {
        var subject = "Verify Your MediChat.AI Account";
        var body = await LoadEmailTemplateAsync("EmailVerification", new Dictionary<string, string>
        {
            { "FirstName", firstName },
            { "VerificationLink", verificationLink }
        });
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetAsync(string email, string firstName, string resetLink)
    {
        var subject = "Reset Your MediChat.AI Password";
        var body = await LoadEmailTemplateAsync("PasswordReset", new Dictionary<string, string>
        {
            { "FirstName", firstName },
            { "ResetLink", resetLink }
        });
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendLoginAlertAsync(string email, string firstName, DateTime loginTime)
    {
        var subject = "New Login Alert - MediChat.AI";
        var body = await LoadEmailTemplateAsync("LoginAlert", new Dictionary<string, string>
        {
            { "FirstName", firstName },
            { "LoginTime", loginTime.ToString("MMM dd, yyyy 'at' hh:mm tt") + " UTC" }
        });
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendOtpAsync(string email, string firstName, string otpCode)
    {
        var subject = "Email Verification Code - MediChat.AI";
        var body = await LoadEmailTemplateAsync("OtpVerification", new Dictionary<string, string>
        {
            { "FirstName", firstName },
            { "OtpCode", otpCode }
        });
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendTwoFactorEnabledAsync(string email, string firstName)
    {
        var subject = "Two-Factor Authentication Enabled - MediChat.AI";
        var body = await LoadEmailTemplateAsync("TwoFactorEnabled", new Dictionary<string, string>
        {
            { "FirstName", firstName },
            { "DateTime", DateTime.UtcNow.ToString("MMM dd, yyyy 'at' hh:mm tt") + " UTC" }
        });
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendTwoFactorDisabledAsync(string email, string firstName)
    {
        var subject = "Two-Factor Authentication Disabled - MediChat.AI";
        var body = await LoadEmailTemplateAsync("TwoFactorDisabled", new Dictionary<string, string>
        {
            { "FirstName", firstName },
            { "DateTime", DateTime.UtcNow.ToString("MMM dd, yyyy 'at' hh:mm tt") + " UTC" }
        });
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendDoctorApprovedAsync(string email, string firstName)
    {
        var subject = "Doctor Profile Approved - MediChat.AI";
        var body = await LoadEmailTemplateAsync("DoctorApproved", new Dictionary<string, string>
        {
            { "FirstName", firstName },
            { "DateTime", DateTime.UtcNow.ToString("MMM dd, yyyy 'at' hh:mm tt") + " UTC" }
        });
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendDoctorRejectedAsync(string email, string firstName, string reason)
    {
        var subject = "Doctor Profile Application Update - MediChat.AI";
        var body = await LoadEmailTemplateAsync("DoctorRejected", new Dictionary<string, string>
        {
            { "FirstName", firstName },
            { "Reason", reason },
            { "DateTime", DateTime.UtcNow.ToString("MMM dd, yyyy 'at' hh:mm tt") + " UTC" },
            { "ApplicationId", Guid.NewGuid().ToString("N")[..8].ToUpper() }
        });
        await SendEmailAsync(email, subject, body);
    }

    public async Task SendAppointmentRequestConfirmationAsync(string email, string firstName, int requestId, string preferredDate, string symptoms)
    {
        var subject = "Appointment Request Received - MediChat.AI";

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;"">
        <h1 style=""color: white; margin: 0;"">MediChat.AI</h1>
    </div>
    <div style=""background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;"">
        <h2 style=""color: #667eea;"">Hello {firstName},</h2>
        <p>Your appointment request has been received successfully!</p>

        <div style=""background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #667eea;"">
            <h3 style=""margin-top: 0; color: #667eea;"">Request Details</h3>
            <p><strong>Request ID:</strong> #{requestId}</p>
            <p><strong>Preferred Date:</strong> {preferredDate}</p>
            <p><strong>Symptoms:</strong> {symptoms}</p>
        </div>

        <p>A doctor will review your request and get back to you within 24 hours.</p>
        <p>You will receive an email notification once your appointment is approved.</p>

        <p style=""margin-top: 30px;"">Best regards,<br><strong>The MediChat.AI Team</strong></p>
    </div>
    <div style=""text-align: center; margin-top: 20px; color: #888; font-size: 12px;"">
        <p>&copy; {DateTime.UtcNow.Year} MediChat.AI. All rights reserved.</p>
    </div>
</body>
</html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendNewAppointmentRequestNotificationAsync(string email, string doctorFirstName, string patientName, string preferredDate, string symptoms, bool isUrgent)
    {
        var subject = isUrgent ? "⚠️ URGENT Appointment Request - MediChat.AI" : "New Appointment Request - MediChat.AI";

        var urgentBadge = isUrgent ? @"<div style=""background: #ff4444; color: white; padding: 10px; border-radius: 5px; text-align: center; font-weight: bold; margin-bottom: 20px;"">⚠️ URGENT REQUEST</div>" : "";

        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;"">
        <h1 style=""color: white; margin: 0;"">MediChat.AI</h1>
    </div>
    <div style=""background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;"">
        {urgentBadge}
        <h2 style=""color: #667eea;"">Hello Dr. {doctorFirstName},</h2>
        <p>You have a new appointment request from a patient.</p>

        <div style=""background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #667eea;"">
            <h3 style=""margin-top: 0; color: #667eea;"">Patient Information</h3>
            <p><strong>Patient Name:</strong> {patientName}</p>
            <p><strong>Preferred Date:</strong> {preferredDate}</p>
            <p><strong>Symptoms:</strong> {symptoms}</p>
        </div>

        <p>Please review this request and approve or reject it as soon as possible.</p>
        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""https://medichat.ai/doctor/appointments/requests"" style=""background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;"">Review Request</a>
        </div>

        <p style=""margin-top: 30px;"">Best regards,<br><strong>The MediChat.AI Team</strong></p>
    </div>
    <div style=""text-align: center; margin-top: 20px; color: #888; font-size: 12px;"">
        <p>&copy; {DateTime.UtcNow.Year} MediChat.AI. All rights reserved.</p>
    </div>
</body>
</html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendEmailAsync(string email, string subject, string body)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            
            using var client = new SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"]!))
            {
                Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"]!)
            };

            var message = new MailMessage(smtpSettings["FromEmail"]!, email, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
        }
    }

    private async Task<string> LoadEmailTemplateAsync(string templateName, Dictionary<string, string> tokens)
    {
        try
        {
            var templatePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates", $"{templateName}.html");

            if (!System.IO.File.Exists(templatePath))
            {
                _logger.LogError("Email template not found: {TemplatePath}", templatePath);
                return $"<html><body><p>Error: Email template '{templateName}' not found.</p></body></html>";
            }

            var template = await System.IO.File.ReadAllTextAsync(templatePath);

            // Apply ALL email settings from database (colors, site name, footer, social links)
            template = await ApplyEmailSettingsAsync(template);

            // Replace dynamic tokens (FirstName, VerificationLink, etc.)
            return ReplaceTemplateTokens(template, tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load email template: {TemplateName}", templateName);
            return $"<html><body><p>Error loading email template.</p></body></html>";
        }
    }

    /// <summary>
    /// Applies all email settings from database: colors, site name, footer, social links
    /// </summary>
    private async Task<string> ApplyEmailSettingsAsync(string template)
    {
        try
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                _logger.LogWarning("System settings not found, using defaults");
                return template;
            }

            // 1. Apply theme colors
            var primaryColor = settings.EmailPrimaryColor ?? "#667eea";
            var secondaryColor = settings.EmailSecondaryColor ?? "#764ba2";
            var backgroundColor = settings.EmailBackgroundColor ?? "#f8fafc";
            var textColor = settings.EmailTextColor ?? "#333333";

            // Replace gradient colors in header (linear-gradient)
            template = System.Text.RegularExpressions.Regex.Replace(
                template,
                @"linear-gradient\(135deg,\s*#[0-9a-fA-F]{6}\s+0%,\s*#[0-9a-fA-F]{6}\s+100%\)",
                $"linear-gradient(135deg, {primaryColor} 0%, {secondaryColor} 100%)"
            );

            // Replace default primary color (#667eea) with configured primary color
            template = template.Replace("#667eea", primaryColor);

            // Replace default secondary color (#764ba2) with configured secondary color
            template = template.Replace("#764ba2", secondaryColor);

            // Replace background colors if present
            template = template.Replace("#f8fafc", backgroundColor);
            template = template.Replace("#f9fafb", backgroundColor);

            // Replace text colors if present (but preserve white text in headers)
            template = System.Text.RegularExpressions.Regex.Replace(
                template,
                @"color:\s*#333333",
                $"color: {textColor}",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // 2. Replace site name (MediChat.AI -> admin-configured name)
            var siteName = !string.IsNullOrEmpty(settings.SiteName) ? settings.SiteName : "MediChat.AI";
            template = template.Replace("MediChat.AI", siteName);

            // 3. Replace site description if present
            if (!string.IsNullOrEmpty(settings.SiteDescription))
            {
                template = System.Text.RegularExpressions.Regex.Replace(
                    template,
                    @"Healthcare Communication Platform",
                    settings.SiteDescription,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            // 4. Inject header image/logo if provided (before the <h1> site name)
            if (!string.IsNullOrEmpty(settings.EmailHeaderImageUrl))
            {
                var headerImageHtml = BuildHeaderImageHtml(settings.EmailHeaderImageUrl);

                // Find the first <h1 tag in the template and insert image before it
                var h1Index = template.IndexOf("<h1", StringComparison.OrdinalIgnoreCase);
                if (h1Index > 0)
                {
                    template = template.Insert(h1Index, headerImageHtml);
                    _logger.LogInformation("Injected header image: {ImageUrl}", settings.EmailHeaderImageUrl);
                }
            }

            // 5. Inject footer before closing </body> tag if enabled
            if (settings.EmailIncludeFooter)
            {
                var footerHtml = BuildFooterHtml(settings);
                // Find the last </div> before </body> and inject footer after it
                var bodyCloseIndex = template.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                if (bodyCloseIndex > 0)
                {
                    template = template.Insert(bodyCloseIndex, footerHtml + "\n    ");
                }
            }

            _logger.LogInformation("Applied email settings - Site: {SiteName}, Colors: {Primary}/{Secondary}, Header Image: {HasImage}, Footer: {Footer}",
                siteName, primaryColor, secondaryColor, !string.IsNullOrEmpty(settings.EmailHeaderImageUrl), settings.EmailIncludeFooter);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying email settings to template");
            return template; // Return original template if error occurs
        }
    }

    private static string ReplaceTemplateTokens(string template, Dictionary<string, string> tokens)
    {
        foreach (var token in tokens)
        {
            template = template.Replace($"{{{{{token.Key}}}}}", token.Value);
        }
        return template;
    }

    /// <summary>
    /// Builds HTML for header image/logo based on admin settings
    /// </summary>
    private string BuildHeaderImageHtml(string imageUrl)
    {
        var encodedUrl = System.Net.WebUtility.HtmlEncode(imageUrl);

        return $@"
        <!-- Header Logo/Image - From Admin Settings -->
        <div style='margin-bottom: 15px;'>
            <img src='{encodedUrl}'
                 alt='Company Logo'
                 style='max-width: 200px; max-height: 80px; height: auto; width: auto; display: block; margin: 0 auto;' />
        </div>
        ";
    }

    /// <summary>
    /// Builds HTML for email footer section based on admin settings
    /// </summary>
    private string BuildFooterHtml(Core.Entities.SystemSettings settings)
    {
        var footerHtml = $@"
        <!-- Email Footer - Auto-generated from Admin Settings -->
        <div style='background: #f8fafc; padding: 20px; margin-top: 30px; border-top: 2px solid #e5e7eb; text-align: center; border-radius: 0 0 10px 10px;'>
            <p style='font-size: 13px; color: #6b7280; margin: 0 0 10px 0; line-height: 1.6;'>{System.Net.WebUtility.HtmlEncode(settings.EmailFooterText)}</p>";

        // Add social links if enabled
        if (settings.EmailIncludeSocialLinks)
        {
            var socialLinks = BuildSocialLinksHtml(settings);
            if (!string.IsNullOrEmpty(socialLinks))
            {
                footerHtml += socialLinks;
            }
        }

        footerHtml += @"
            <p style='font-size: 11px; color: #9ca3af; margin: 15px 0 0 0;'>
                © " + DateTime.UtcNow.Year + @" MediChat.AI. All rights reserved.
            </p>
        </div>";

        return footerHtml;
    }

    /// <summary>
    /// Builds HTML for social media links based on admin settings
    /// </summary>
    private string BuildSocialLinksHtml(Core.Entities.SystemSettings settings)
    {
        var links = new List<string>();

        if (!string.IsNullOrEmpty(settings.EmailFacebookUrl))
        {
            links.Add($@"<a href='{System.Net.WebUtility.HtmlEncode(settings.EmailFacebookUrl)}'
                style='color: #667eea; text-decoration: none; margin: 0 8px; font-weight: 500;'>Facebook</a>");
        }

        if (!string.IsNullOrEmpty(settings.EmailTwitterUrl))
        {
            links.Add($@"<a href='{System.Net.WebUtility.HtmlEncode(settings.EmailTwitterUrl)}'
                style='color: #667eea; text-decoration: none; margin: 0 8px; font-weight: 500;'>Twitter</a>");
        }

        if (!string.IsNullOrEmpty(settings.EmailLinkedInUrl))
        {
            links.Add($@"<a href='{System.Net.WebUtility.HtmlEncode(settings.EmailLinkedInUrl)}'
                style='color: #667eea; text-decoration: none; margin: 0 8px; font-weight: 500;'>LinkedIn</a>");
        }

        if (!string.IsNullOrEmpty(settings.EmailInstagramUrl))
        {
            links.Add($@"<a href='{System.Net.WebUtility.HtmlEncode(settings.EmailInstagramUrl)}'
                style='color: #667eea; text-decoration: none; margin: 0 8px; font-weight: 500;'>Instagram</a>");
        }

        if (links.Count == 0)
        {
            return "";
        }

        return $@"
            <div style='text-align: center; margin: 15px 0 10px 0; padding-top: 15px; border-top: 1px solid #e5e7eb;'>
                <p style='font-size: 12px; color: #6b7280; margin: 0 0 10px 0;'>Follow us on social media:</p>
                <div style='font-size: 13px;'>
                    {string.Join(" <span style='color: #d1d5db;'>|</span> ", links)}
                </div>
            </div>";
    }
}
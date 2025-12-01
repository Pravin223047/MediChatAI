using MediChatAI_GraphQl.Shared.DTOs;
using System.Text;

namespace MediChatAI_GraphQl.Shared.Services;

public interface IPrescriptionPdfService
{
    Task<byte[]> GeneratePrescriptionPdfAsync(PrescriptionDto prescription);
    Task<string> GeneratePrescriptionHtmlAsync(PrescriptionDto prescription);
}

public class PrescriptionPdfService : IPrescriptionPdfService
{
    private readonly ILogger<PrescriptionPdfService> _logger;

    public PrescriptionPdfService(ILogger<PrescriptionPdfService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GeneratePrescriptionPdfAsync(PrescriptionDto prescription)
    {
        // TODO: Implement actual PDF generation using a library like QuestPDF or iTextSharp
        // For now, return HTML as bytes that can be converted to PDF by the frontend
        var html = await GeneratePrescriptionHtmlAsync(prescription);
        return Encoding.UTF8.GetBytes(html);
    }

    public async Task<string> GeneratePrescriptionHtmlAsync(PrescriptionDto prescription)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<title>Prescription</title>");
        sb.AppendLine("<style>");
        sb.AppendLine(@"
            body { font-family: Arial, sans-serif; margin: 40px; }
            .header { border-bottom: 2px solid #333; padding-bottom: 20px; margin-bottom: 20px; }
            .header h1 { margin: 0; color: #2563eb; }
            .info-section { margin: 20px 0; }
            .info-label { font-weight: bold; color: #666; }
            .medications { margin-top: 30px; }
            .medication-item { border: 1px solid #ddd; padding: 15px; margin: 10px 0; border-radius: 5px; }
            .medication-name { font-size: 18px; font-weight: bold; color: #1e40af; }
            .footer { margin-top: 40px; border-top: 1px solid #ddd; padding-top: 20px; }
            .signature { margin-top: 30px; }
        ");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        // Header
        sb.AppendLine("<div class='header'>");
        sb.AppendLine("<h1>Medical Prescription</h1>");
        sb.AppendLine($"<p>Date: {prescription.PrescribedDate:MMMM dd, yyyy}</p>");
        sb.AppendLine("</div>");

        // Patient Information
        sb.AppendLine("<div class='info-section'>");
        sb.AppendLine("<h2>Patient Information</h2>");
        sb.AppendLine($"<p><span class='info-label'>Name:</span> {prescription.PatientName}</p>");
        sb.AppendLine($"<p><span class='info-label'>Prescription ID:</span> {prescription.Id}</p>");
        sb.AppendLine("</div>");

        // Doctor Information
        sb.AppendLine("<div class='info-section'>");
        sb.AppendLine("<h2>Prescribing Doctor</h2>");
        sb.AppendLine($"<p><span class='info-label'>Name:</span> {prescription.DoctorName}</p>");
        sb.AppendLine("</div>");

        // Diagnosis
        if (!string.IsNullOrEmpty(prescription.Diagnosis))
        {
            sb.AppendLine("<div class='info-section'>");
            sb.AppendLine("<h2>Diagnosis</h2>");
            sb.AppendLine($"<p>{prescription.Diagnosis}</p>");
            sb.AppendLine("</div>");
        }

        // Medications
        sb.AppendLine("<div class='medications'>");
        sb.AppendLine("<h2>Prescribed Medications</h2>");

        foreach (var item in prescription.Items)
        {
            sb.AppendLine("<div class='medication-item'>");
            sb.AppendLine($"<div class='medication-name'>{item.MedicationName}</div>");

            if (!string.IsNullOrEmpty(item.GenericName))
                sb.AppendLine($"<p><span class='info-label'>Generic Name:</span> {item.GenericName}</p>");

            sb.AppendLine($"<p><span class='info-label'>Dosage:</span> {item.Dosage}</p>");
            sb.AppendLine($"<p><span class='info-label'>Frequency:</span> {item.Frequency}</p>");

            if (!string.IsNullOrEmpty(item.Route))
                sb.AppendLine($"<p><span class='info-label'>Route:</span> {item.Route}</p>");

            if (!string.IsNullOrEmpty(item.Form))
                sb.AppendLine($"<p><span class='info-label'>Form:</span> {item.Form}</p>");

            sb.AppendLine($"<p><span class='info-label'>Duration:</span> {item.DurationDays} days</p>");
            sb.AppendLine($"<p><span class='info-label'>Quantity:</span> {item.Quantity}</p>");

            if (!string.IsNullOrEmpty(item.Instructions))
            {
                sb.AppendLine($"<p><span class='info-label'>Instructions:</span></p>");
                sb.AppendLine($"<p>{item.Instructions}</p>");
            }

            if (!string.IsNullOrEmpty(item.Warnings))
            {
                sb.AppendLine($"<p style='color: #dc2626;'><span class='info-label'>Warnings:</span> {item.Warnings}</p>");
            }

            sb.AppendLine("</div>");
        }

        sb.AppendLine("</div>");

        // Doctor Notes
        if (!string.IsNullOrEmpty(prescription.DoctorNotes))
        {
            sb.AppendLine("<div class='info-section'>");
            sb.AppendLine("<h2>Doctor's Notes</h2>");
            sb.AppendLine($"<p>{prescription.DoctorNotes}</p>");
            sb.AppendLine("</div>");
        }

        // Refill Information
        sb.AppendLine("<div class='info-section'>");
        sb.AppendLine("<h2>Refill Information</h2>");
        sb.AppendLine($"<p><span class='info-label'>Refills Allowed:</span> {prescription.RefillsAllowed}</p>");
        sb.AppendLine($"<p><span class='info-label'>Refills Used:</span> {prescription.RefillsUsed}</p>");
        sb.AppendLine($"<p><span class='info-label'>Valid Until:</span> {prescription.EndDate:MMMM dd, yyyy}</p>");
        sb.AppendLine("</div>");

        // Signature
        sb.AppendLine("<div class='footer'>");
        sb.AppendLine("<div class='signature'>");
        sb.AppendLine($"<p><span class='info-label'>Doctor's Signature:</span> {(prescription.IsVerified ? "✓ Digitally Signed" : "Pending Signature")}</p>");
        sb.AppendLine($"<p>{prescription.DoctorName}</p>");
        sb.AppendLine("</div>");
        sb.AppendLine("</div>");

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return await Task.FromResult(sb.ToString());
    }
}

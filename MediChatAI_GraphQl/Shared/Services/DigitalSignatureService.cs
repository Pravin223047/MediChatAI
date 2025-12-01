using System.Security.Cryptography;
using System.Text;

namespace MediChatAI_GraphQl.Shared.Services;

public interface IDigitalSignatureService
{
    string GenerateSignature(string doctorId, string prescriptionData);
    bool VerifySignature(string signature, string doctorId, string prescriptionData);
}

public class DigitalSignatureService : IDigitalSignatureService
{
    private readonly ILogger<DigitalSignatureService> _logger;
    private const string SignatureSalt = "MediChatAI_Prescription_Signature_Salt_2025"; // In production, use environment variable

    public DigitalSignatureService(ILogger<DigitalSignatureService> logger)
    {
        _logger = logger;
    }

    public string GenerateSignature(string doctorId, string prescriptionData)
    {
        try
        {
            var dataToSign = $"{doctorId}|{prescriptionData}|{SignatureSalt}|{DateTime.UtcNow:yyyy-MM-dd}";
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
            var signature = Convert.ToBase64String(hashBytes);

            _logger.LogInformation("Generated digital signature for doctor: {DoctorId}", doctorId);
            return signature;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating digital signature");
            throw;
        }
    }

    public bool VerifySignature(string signature, string doctorId, string prescriptionData)
    {
        try
        {
            var expectedSignature = GenerateSignature(doctorId, prescriptionData);
            var isValid = signature == expectedSignature;

            _logger.LogInformation("Signature verification for doctor {DoctorId}: {IsValid}", doctorId, isValid);
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying digital signature");
            return false;
        }
    }
}

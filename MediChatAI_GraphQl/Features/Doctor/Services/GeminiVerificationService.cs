using MediChatAI_GraphQl.Shared.DTOs;
using MediChatAI_GraphQl.Core.Interfaces.Services.Shared;
using MediChatAI_GraphQl.Core.Interfaces.Services.Admin;
using MediChatAI_GraphQl.Core.Interfaces.Services.Auth;
using MediChatAI_GraphQl.Core.Interfaces.Services.Doctor;
using MediChatAI_GraphQl.Features.Admin.DTOs;
using MediChatAI_GraphQl.Features.Doctor.DTOs;
using MediChatAI_GraphQl.Features.Authentication.DTOs;
using MediChatAI_GraphQl.Features.Notifications.DTOs;
using System.Text;
using System.Text.Json;

namespace MediChatAI_GraphQl.Features.Doctor.Services;

public class GeminiVerificationService : IGeminiVerificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiVerificationService> _logger;
    private readonly string _geminiApiKey;
    private const string GEMINI_API_BASE = "https://generativelanguage.googleapis.com/v1beta";

    public GeminiVerificationService(HttpClient httpClient, ILogger<GeminiVerificationService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _geminiApiKey = configuration["GeminiApiKey"] ?? "AIzaSyAJBGZwB4PddLfRnLuaZgRery33kPKqJEk";
    }

    public async Task<AiVerificationResult> VerifyAadhaarAsync(string aadhaarNumber, string imagePath)
    {
        try
        {
            var imageData = await ConvertImageToBase64(imagePath);
            if (string.IsNullOrEmpty(imageData))
            {
                return new AiVerificationResult
                {
                    IsAadhaarValid = false,
                    AadhaarVerificationNotes = "Failed to process Aadhaar card image",
                    CombinedNotes = "Failed to process Aadhaar card image"
                };
            }

            // Normalize the Aadhaar number by removing all spaces and special characters
            var normalizedAadhaarNumber = aadhaarNumber.Replace(" ", "").Replace("-", "");

            var prompt = $@"
You are an AI assistant that verifies Aadhaar card authenticity. Please analyze the provided Aadhaar card image.

TASK:
1. Extract the Aadhaar number from the image (remove all spaces, dashes, and formatting to get only the 12 digits)
2. Compare the extracted digits with this number: {normalizedAadhaarNumber}
3. If the digits match exactly (ignoring any formatting like spaces or dashes), the number verification passes
4. Verify if the document appears to be a valid Aadhaar card — this includes both original printed and digitally generated versions issued by UIDAI
5. Check for proper formatting, UIDAI logo, government seals, and required fields (name, DOB, gender, QR code, etc.)
6. Check for signs of tampering or forgery (e.g., altered text, mismatched fonts, duplicated sections)

EXAMPLE:
- If image shows '2615 9154 4325', extract as '261591544325'
- If provided number is '261591544325', then these MATCH
- Mark isValid as true if they match and the document appears to be a valid Aadhaar card (printed or digital)

Respond with a JSON object containing:
- isValid: boolean (true if Aadhaar appears valid AND extracted digits match provided digits)
- confidence: number (0-100, confidence percentage)
- notes: string (detailed explanation: state the extracted number from image, whether it matches the provided number, and document validity assessment)
- issues: array of strings (any problems found, empty array if no issues)

Be precise and logical in your comparison. Do not reject digitally generated Aadhaar cards if they follow official formatting and contain valid information.";


            var response = await CallGeminiVisionApi(prompt, imageData);
            var verificationResult = ParseGeminiResponse(response);

            return new AiVerificationResult
            {
                IsAadhaarValid = verificationResult.isValid,
                AadhaarVerificationNotes = $"Confidence: {verificationResult.confidence}%. {verificationResult.notes}",
                CombinedNotes = $"Aadhaar Verification: {verificationResult.notes}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Aadhaar card");
            return new AiVerificationResult
            {
                IsAadhaarValid = false,
                AadhaarVerificationNotes = "Error occurred during verification",
                CombinedNotes = "Error occurred during Aadhaar verification"
            };
        }
    }

    public async Task<AiVerificationResult> VerifyMedicalCertificateAsync(string certificateSource)
    {
        try
        {
            // Check if it's a file path or URL
            bool isFilePath = File.Exists(certificateSource);

            if (isFilePath)
            {
                // Process as image file
                var imageData = await ConvertImageToBase64(certificateSource);
                if (string.IsNullOrEmpty(imageData))
                {
                    return new AiVerificationResult
                    {
                        IsMedicalCertificateValid = false,
                        MedicalCertificateVerificationNotes = "Failed to process medical certificate image",
                        CombinedNotes = "Failed to process medical certificate image"
                    };
                }

                var prompt = @"
You are an AI assistant that verifies medical certificate authenticity. Please analyze the provided medical certificate image.

Please verify:
1. Check if this document appears to be a valid medical certificate or medical practice license
2. Verify if the document appears to be from a legitimate medical institution or medical council
3. Check for proper medical certification formatting and required elements (doctor name, registration number, institution, seals)
4. Look for any signs of authenticity (official seals, signatures, proper formatting, letterhead)
5. Validate that the certificate is for medical practice authorization

Respond with a JSON object containing:
- isValid: boolean (true if certificate appears authentic and is a medical certificate)
- confidence: number (0-100, confidence percentage)
- notes: string (detailed explanation of findings including what type of document it is)
- issues: array of strings (any problems found)
- institution: string (name of issuing institution if identifiable)

Be thorough in your analysis. If this is not a medical certificate (e.g., Aadhaar card, ID card), mark as invalid.";

                var response = await CallGeminiVisionApi(prompt, imageData);
                var verificationResult = ParseGeminiResponse(response);

                return new AiVerificationResult
                {
                    IsMedicalCertificateValid = verificationResult.isValid,
                    MedicalCertificateVerificationNotes = $"Confidence: {verificationResult.confidence}%. {verificationResult.notes}",
                    CombinedNotes = $"Medical Certificate Verification: {verificationResult.notes}"
                };
            }
            else
            {
                // Process as URL (text-based analysis)
                var prompt = $@"
You are an AI assistant that verifies medical certificate authenticity. Please analyze the medical certificate from the provided URL: {certificateSource}

Please verify:
1. Check if the URL leads to a valid medical certificate document
2. Verify if the document appears to be from a legitimate medical institution
3. Check for proper medical certification formatting and required elements
4. Look for any signs of authenticity (seals, signatures, proper formatting)
5. Validate that the certificate is for medical practice authorization

Respond with a JSON object containing:
- isValid: boolean (true if certificate appears authentic)
- confidence: number (0-100, confidence percentage)
- notes: string (detailed explanation of findings)
- issues: array of strings (any problems found)
- institution: string (name of issuing institution if identifiable)

Be thorough in your analysis.";

                var response = await CallGeminiTextApi(prompt);
                var verificationResult = ParseGeminiResponse(response);

                return new AiVerificationResult
                {
                    IsMedicalCertificateValid = verificationResult.isValid,
                    MedicalCertificateVerificationNotes = $"Confidence: {verificationResult.confidence}%. {verificationResult.notes}",
                    CombinedNotes = $"Medical Certificate Verification: {verificationResult.notes}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying medical certificate");
            return new AiVerificationResult
            {
                IsMedicalCertificateValid = false,
                MedicalCertificateVerificationNotes = "Error occurred during verification",
                CombinedNotes = "Error occurred during medical certificate verification"
            };
        }
    }

    public async Task<AiVerificationResult> PerformCompleteVerificationAsync(string aadhaarNumber, string imagePath, string certificateUrl)
    {
        var aadhaarResult = await VerifyAadhaarAsync(aadhaarNumber, imagePath);
        var certificateResult = await VerifyMedicalCertificateAsync(certificateUrl);

        return new AiVerificationResult
        {
            IsAadhaarValid = aadhaarResult.IsAadhaarValid,
            IsMedicalCertificateValid = certificateResult.IsMedicalCertificateValid,
            AadhaarVerificationNotes = aadhaarResult.AadhaarVerificationNotes,
            MedicalCertificateVerificationNotes = certificateResult.MedicalCertificateVerificationNotes,
            CombinedNotes = $"{aadhaarResult.AadhaarVerificationNotes}\n\n{certificateResult.MedicalCertificateVerificationNotes}"
        };
    }

    private async Task<string> CallGeminiVisionApi(string prompt, string imageData)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = "image/jpeg",
                                data = imageData
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 1000
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"{GEMINI_API_BASE}/models/gemini-2.0-flash-exp:generateContent?key={_geminiApiKey}",
            content
        );

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> CallGeminiTextApi(string prompt)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 1000
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"{GEMINI_API_BASE}/models/gemini-2.0-flash-exp:generateContent?key={_geminiApiKey}",
            content
        );

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private (bool isValid, int confidence, string notes) ParseGeminiResponse(string response)
    {
        try
        {
            using var document = JsonDocument.Parse(response);
            var content = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(content))
                return (false, 0, "No response from AI service");

            var jsonStart = content.IndexOf('{');
            var jsonEnd = content.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                using var resultDoc = JsonDocument.Parse(jsonContent);
                var root = resultDoc.RootElement;

                var isValid = root.TryGetProperty("isValid", out var validProp) && validProp.GetBoolean();
                var confidence = root.TryGetProperty("confidence", out var confProp) ? confProp.GetInt32() : 0;
                var notes = root.TryGetProperty("notes", out var notesProp) ? notesProp.GetString() ?? "" : content;

                return (isValid, confidence, notes);
            }

            return (false, 0, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response: {Response}", response);
            return (false, 0, "Error parsing AI response");
        }
    }

    private async Task<string> ConvertImageToBase64(string imagePath)
    {
        try
        {
            byte[] imageBytes;

            // Check if it's a URL (Cloudinary) or local file path
            if (Uri.TryCreate(imagePath, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                // It's a URL - download the image from Cloudinary
                _logger.LogInformation("Downloading image from URL: {ImagePath}", imagePath);
                imageBytes = await _httpClient.GetByteArrayAsync(imagePath);
            }
            else
            {
                // It's a local file path
                if (!File.Exists(imagePath))
                {
                    _logger.LogWarning("Local file not found: {ImagePath}", imagePath);
                    return string.Empty;
                }

                imageBytes = await File.ReadAllBytesAsync(imagePath);
            }

            return Convert.ToBase64String(imageBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image to base64: {ImagePath}", imagePath);
            return string.Empty;
        }
    }
}
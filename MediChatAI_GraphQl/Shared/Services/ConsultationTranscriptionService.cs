using Microsoft.EntityFrameworkCore;
using MediChatAI_GraphQl.Core.Interfaces.Services.Consultation;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Shared.DTOs;
using System.Text;
using System.Text.Json;

namespace MediChatAI_GraphQl.Shared.Services;

public class ConsultationTranscriptionService : IConsultationTranscriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConsultationTranscriptionService> _logger;
    private readonly IConsultationRecordingService _recordingService;
    private readonly IConsultationSessionService _sessionService;
    private readonly HttpClient _httpClient;
    private readonly string _geminiApiKey;
    private const string GEMINI_API_BASE = "https://generativelanguage.googleapis.com/v1beta";

    public ConsultationTranscriptionService(
        ApplicationDbContext context,
        ILogger<ConsultationTranscriptionService> logger,
        IConsultationRecordingService recordingService,
        IConsultationSessionService sessionService,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _recordingService = recordingService;
        _sessionService = sessionService;
        _httpClient = httpClient;
        _geminiApiKey = configuration["GeminiApiKey"] ??
            throw new InvalidOperationException("GeminiApiKey not configured");
    }

    #region Transcription

    public async Task<string?> TranscribeAudioAsync(string audioUrl)
    {
        _logger.LogInformation("Transcribing audio from URL: {AudioUrl}", audioUrl);

        try
        {
            // For now, return a placeholder since Gemini requires audio file upload
            // In production, you would:
            // 1. Download the audio file from Cloudinary
            // 2. Use Google Speech-to-Text API or Gemini audio processing
            // 3. Process in chunks if necessary

            var prompt = @"This is a medical consultation recording.
Please transcribe the conversation accurately, preserving medical terminology.
Format the output with clear speaker labels (Doctor: / Patient:).";

            // Placeholder implementation - would use actual audio processing API
            _logger.LogWarning("Audio transcription not fully implemented. Returning placeholder.");

            return "Transcription feature requires integration with Google Speech-to-Text API or Gemini audio processing. " +
                   "Audio URL: " + audioUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to transcribe audio from {AudioUrl}", audioUrl);
            return null;
        }
    }

    public async Task<string?> TranscribeRecordingAsync(int recordingId)
    {
        _logger.LogInformation("Transcribing recording {RecordingId}", recordingId);

        var recording = await _recordingService.GetRecordingByIdAsync(recordingId);

        if (recording == null || string.IsNullOrEmpty(recording.RecordingUrl))
        {
            _logger.LogWarning("Recording {RecordingId} not found or has no URL", recordingId);
            return null;
        }

        var transcript = await TranscribeAudioAsync(recording.RecordingUrl);

        if (!string.IsNullOrEmpty(transcript))
        {
            // Update recording with transcript
            await _recordingService.UpdateTranscriptAsync(recordingId, transcript);
        }

        return transcript;
    }

    #endregion

    #region Summary Generation

    public async Task<string?> GenerateConsultationSummaryAsync(int sessionId)
    {
        _logger.LogInformation("Generating AI summary for consultation session {SessionId}", sessionId);

        try
        {
            var session = await _sessionService.GetConsultationSessionByIdAsync(sessionId);

            if (session == null)
            {
                _logger.LogWarning("Session {SessionId} not found", sessionId);
                return null;
            }

            // Gather all available data
            var recordings = await _recordingService.GetRecordingsBySessionAsync(sessionId);
            var transcript = recordings.FirstOrDefault()?.TranscriptText;

            // Build comprehensive prompt
            var prompt = BuildSummaryPrompt(session, transcript);

            // Call Gemini API
            var summary = await CallGeminiForSummaryAsync(prompt);

            if (!string.IsNullOrEmpty(summary))
            {
                // Update session with AI summary
                await _sessionService.UpdateAISummaryAsync(sessionId, summary);

                // Update recording summaries if they exist
                foreach (var recording in recordings)
                {
                    await _recordingService.UpdateAISummaryAsync(recording.Id, summary);
                }
            }

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate summary for session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task<string?> GenerateSummaryFromTranscriptAsync(string transcript, string? clinicalNotes = null)
    {
        _logger.LogInformation("Generating summary from transcript");

        if (string.IsNullOrEmpty(transcript))
        {
            return null;
        }

        var prompt = $@"You are a medical AI assistant. Generate a professional consultation summary from the following transcript.

{(clinicalNotes != null ? $"Clinical Notes:\n{clinicalNotes}\n\n" : "")}Transcript:
{transcript}

Please provide a structured summary with the following sections:
1. **Chief Complaint**: Primary reason for consultation
2. **Clinical Assessment**: Doctor's observations and findings
3. **Diagnosis**: Identified conditions or concerns
4. **Medications Prescribed**: List of medications with dosage
5. **Treatment Plan**: Recommended treatments and lifestyle changes
6. **Follow-up Instructions**: Next steps and follow-up requirements

Format the response in clear, professional medical language.";

        return await CallGeminiForSummaryAsync(prompt);
    }

    #endregion

    #region Clinical Data Extraction

    public async Task<ExtractedClinicalDataDto?> ExtractClinicalDataFromTranscriptAsync(string transcript)
    {
        _logger.LogInformation("Extracting clinical data from transcript");

        if (string.IsNullOrEmpty(transcript))
        {
            return null;
        }

        var prompt = $@"You are a medical AI assistant. Extract structured clinical information from the following consultation transcript.

Transcript:
{transcript}

Extract and return ONLY a JSON object with the following structure (no additional text):
{{
  ""chiefComplaint"": ""primary reason for visit"",
  ""symptoms"": [""symptom1"", ""symptom2""],
  ""diagnosis"": ""diagnosed condition"",
  ""medications"": [""medication1 dosage"", ""medication2 dosage""],
  ""treatmentPlan"": ""recommended treatment"",
  ""followUpInstructions"": [""instruction1"", ""instruction2""],
  ""nextSteps"": ""next steps for patient""
}}";

        try
        {
            var response = await CallGeminiApiAsync(prompt);

            if (string.IsNullOrEmpty(response))
            {
                return null;
            }

            // Parse JSON response
            var jsonResponse = ExtractJsonFromResponse(response);
            var clinicalData = JsonSerializer.Deserialize<ExtractedClinicalDataDto>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return clinicalData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract clinical data from transcript");
            return null;
        }
    }

    #endregion

    #region Follow-up Recommendations

    public async Task<List<string>> GenerateFollowUpRecommendationsAsync(int sessionId)
    {
        _logger.LogInformation("Generating follow-up recommendations for session {SessionId}", sessionId);

        try
        {
            var session = await _sessionService.GetConsultationSessionByIdAsync(sessionId);

            if (session == null)
            {
                return new List<string>();
            }

            var prompt = $@"Based on the following consultation information, provide 3-5 specific follow-up recommendations for the patient.

Chief Complaint: {session.ChiefComplaint}
Diagnosis: {session.Diagnosis}
Treatment Plan: {session.TreatmentPlan}

Provide actionable, clear recommendations as a JSON array of strings:
[""recommendation 1"", ""recommendation 2"", ""recommendation 3""]";

            var response = await CallGeminiApiAsync(prompt);

            if (string.IsNullOrEmpty(response))
            {
                return new List<string>();
            }

            // Extract JSON and parse
            var jsonResponse = ExtractJsonFromResponse(response);
            var recommendations = JsonSerializer.Deserialize<List<string>>(jsonResponse);

            return recommendations ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate follow-up recommendations for session {SessionId}", sessionId);
            return new List<string>();
        }
    }

    #endregion

    #region Helper Methods

    private string BuildSummaryPrompt(ConsultationSessionDto session, string? transcript)
    {
        var promptBuilder = new StringBuilder();

        promptBuilder.AppendLine("You are a medical AI assistant. Generate a comprehensive consultation summary.");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("**Consultation Information:**");
        promptBuilder.AppendLine($"- Date: {session.ScheduledStartTime:yyyy-MM-dd HH:mm}");
        promptBuilder.AppendLine($"- Duration: {session.ActualDurationMinutes ?? session.PlannedDurationMinutes} minutes");
        promptBuilder.AppendLine($"- Doctor: {session.DoctorName}");
        promptBuilder.AppendLine($"- Patient: {session.PatientName}");
        promptBuilder.AppendLine();

        if (!string.IsNullOrEmpty(session.ChiefComplaint))
        {
            promptBuilder.AppendLine($"**Chief Complaint:** {session.ChiefComplaint}");
        }

        if (!string.IsNullOrEmpty(session.DoctorObservations))
        {
            promptBuilder.AppendLine($"**Observations:** {session.DoctorObservations}");
        }

        if (!string.IsNullOrEmpty(session.Diagnosis))
        {
            promptBuilder.AppendLine($"**Diagnosis:** {session.Diagnosis}");
        }

        if (!string.IsNullOrEmpty(session.TreatmentPlan))
        {
            promptBuilder.AppendLine($"**Treatment Plan:** {session.TreatmentPlan}");
        }

        if (!string.IsNullOrEmpty(transcript))
        {
            promptBuilder.AppendLine();
            promptBuilder.AppendLine("**Consultation Transcript:**");
            promptBuilder.AppendLine(transcript);
        }

        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Generate a professional medical summary with the following sections:");
        promptBuilder.AppendLine("1. **Chief Complaint**");
        promptBuilder.AppendLine("2. **Doctor's Assessment**");
        promptBuilder.AppendLine("3. **Medications Prescribed**");
        promptBuilder.AppendLine("4. **Treatment Plan**");
        promptBuilder.AppendLine("5. **Follow-up Instructions**");
        promptBuilder.AppendLine();
        promptBuilder.AppendLine("Format the response in Markdown for readability.");

        return promptBuilder.ToString();
    }

    private async Task<string?> CallGeminiForSummaryAsync(string prompt)
    {
        return await CallGeminiApiAsync(prompt);
    }

    private async Task<string?> CallGeminiApiAsync(string prompt)
    {
        try
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
                    temperature = 0.4,
                    topK = 32,
                    topP = 1,
                    maxOutputTokens = 2048
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"{GEMINI_API_BASE}/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}";

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseBody);

            var text = responseJson.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call Gemini API");
            return null;
        }
    }

    private string ExtractJsonFromResponse(string response)
    {
        // Try to extract JSON from response (in case AI adds extra text)
        var jsonMatch = System.Text.RegularExpressions.Regex.Match(
            response,
            @"(\{.*\}|\[.*\])",
            System.Text.RegularExpressions.RegexOptions.Singleline);

        if (jsonMatch.Success)
        {
            return jsonMatch.Value;
        }

        return response;
    }

    #endregion
}

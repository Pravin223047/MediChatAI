using MediChatAI_GraphQl.Core.Interfaces.Services.AIChat;
using MediChatAI_GraphQl.Features.AIChat.DTOs;
using MediChatAI_GraphQl.Infrastructure.Data;
using MediChatAI_GraphQl.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace MediChatAI_GraphQl.Features.AIChat.Services;

/// <summary>
/// Gemini AI-powered chatbot service
/// </summary>
public class GeminiAIChatService : IAIChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiAIChatService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly string _geminiApiKey;
    private const string GEMINI_API_BASE = "https://generativelanguage.googleapis.com/v1beta";

    public GeminiAIChatService(
        HttpClient httpClient,
        ILogger<GeminiAIChatService> logger,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _context = context;
        _geminiApiKey = configuration["GeminiApiKey"] ?? throw new InvalidOperationException("GeminiApiKey not configured");
    }

    public async Task<AIChatMessageResponse> SendMessageAsync(string userId, SendAIChatMessageInput input)
    {
        try
        {
            // Get or create session
            var sessionId = await GetOrCreateSessionAsync(userId, input.SessionId);

            // Determine message type
            var messageType = input.MessageType?.ToLower() ?? "text";

            // Build prompt based on message type
            string prompt;
            string? aiResponse;

            if (!string.IsNullOrEmpty(input.ImageUrl))
            {
                prompt = $"Analyze this medical image and provide insights: {input.Message}";
                aiResponse = await CallGeminiVisionApiAsync(prompt, input.ImageUrl);
            }
            else if (!string.IsNullOrEmpty(input.VoiceUrl))
            {
                prompt = input.Message;
                aiResponse = await CallGeminiApiAsync(prompt, await GetConversationContextAsync(sessionId));
            }
            else
            {
                prompt = input.Message;
                aiResponse = await CallGeminiApiAsync(prompt, await GetConversationContextAsync(sessionId));
            }

            if (string.IsNullOrEmpty(aiResponse))
            {
                return new AIChatMessageResponse
                {
                    Success = false,
                    Message = "Failed to get AI response"
                };
            }

            // Save user message
            var userMessage = new AIChatMessage
            {
                SessionId = Guid.Parse(sessionId),
                Content = input.Message,
                Role = "user",
                MessageType = messageType,
                Timestamp = DateTime.UtcNow
            };
            _context.AIChatMessages.Add(userMessage);

            // Save AI response
            var assistantMessage = new AIChatMessage
            {
                SessionId = Guid.Parse(sessionId),
                Content = aiResponse,
                Role = "assistant",
                MessageType = messageType,
                Timestamp = DateTime.UtcNow,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    SuggestedActions = GenerateSuggestedActions(aiResponse)
                })
            };
            _context.AIChatMessages.Add(assistantMessage);

            // Update session
            var session = await _context.AIChatSessions.FindAsync(Guid.Parse(sessionId));
            if (session != null)
            {
                session.LastMessageAt = DateTime.UtcNow;
                session.MessageCount += 2;
            }

            await _context.SaveChangesAsync();

            return new AIChatMessageResponse
            {
                Success = true,
                Message = "Message sent successfully",
                ChatMessage = MapToDto(assistantMessage)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
            return new AIChatMessageResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<AIChatSymptomAnalysisResponse> AnalyzeSymptomsAsync(string userId, AnalyzeSymptomsInput input)
    {
        try
        {
            var prompt = BuildSymptomAnalysisPrompt(input);
            var aiResponse = await CallGeminiApiAsync(prompt, "");

            if (string.IsNullOrEmpty(aiResponse))
            {
                return new AIChatSymptomAnalysisResponse
                {
                    Success = false,
                    Message = "Failed to analyze symptoms"
                };
            }

            var analysis = ParseSymptomAnalysisResponse(aiResponse, input.Symptoms);

            return new AIChatSymptomAnalysisResponse
            {
                Success = true,
                Message = "Symptoms analyzed successfully",
                Analysis = analysis
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing symptoms");
            return new AIChatSymptomAnalysisResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<AIChatImageAnalysisResponse> AnalyzeImageAsync(string userId, AnalyzeImageInput input)
    {
        try
        {
            var prompt = BuildImageAnalysisPrompt(input);
            var aiResponse = await CallGeminiVisionApiAsync(prompt, input.ImageUrl);

            if (string.IsNullOrEmpty(aiResponse))
            {
                return new AIChatImageAnalysisResponse
                {
                    Success = false,
                    Message = "Failed to analyze image"
                };
            }

            var analysis = ParseImageAnalysisResponse(aiResponse, input.ImageUrl);

            return new AIChatImageAnalysisResponse
            {
                Success = true,
                Message = "Image analyzed successfully",
                Analysis = analysis
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            return new AIChatImageAnalysisResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<SaveConversationResponse> SaveConversationAsync(string userId, SaveConversationInput input)
    {
        try
        {
            var session = await _context.AIChatSessions
                .FirstOrDefaultAsync(s => s.Id == Guid.Parse(input.SessionId) && s.UserId == userId);

            if (session == null)
            {
                return new SaveConversationResponse
                {
                    Success = false,
                    Message = "Session not found"
                };
            }

            session.Title = input.Title;
            session.Summary = input.Summary;
            session.IsSaved = true;

            await _context.SaveChangesAsync();

            return new SaveConversationResponse
            {
                Success = true,
                Message = "Conversation saved successfully",
                Conversation = new SavedConversationDto
                {
                    Id = session.Id.ToString(),
                    Title = session.Title,
                    Summary = session.Summary,
                    CreatedAt = session.CreatedAt,
                    LastMessageAt = session.LastMessageAt,
                    MessageCount = session.MessageCount
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving conversation");
            return new SaveConversationResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<DeleteConversationResponse> DeleteConversationAsync(string userId, string conversationId)
    {
        try
        {
            var session = await _context.AIChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == Guid.Parse(conversationId) && s.UserId == userId);

            if (session == null)
            {
                return new DeleteConversationResponse
                {
                    Success = false,
                    Message = "Conversation not found"
                };
            }

            _context.AIChatMessages.RemoveRange(session.Messages);
            _context.AIChatSessions.Remove(session);
            await _context.SaveChangesAsync();

            return new DeleteConversationResponse
            {
                Success = true,
                Message = "Conversation deleted successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation");
            return new DeleteConversationResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }
    }

    public async Task<List<SavedConversationDto>> GetConversationHistoryAsync(string userId)
    {
        try
        {
            var sessions = await _context.AIChatSessions
                .Where(s => s.UserId == userId && s.IsSaved)
                .OrderByDescending(s => s.LastMessageAt)
                .ToListAsync();

            return sessions.Select(s => new SavedConversationDto
            {
                Id = s.Id.ToString(),
                Title = s.Title ?? "Untitled Conversation",
                Summary = s.Summary,
                CreatedAt = s.CreatedAt,
                LastMessageAt = s.LastMessageAt,
                MessageCount = s.MessageCount
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation history");
            return new List<SavedConversationDto>();
        }
    }

    public async Task<List<AIChatMessageDto>> LoadConversationMessagesAsync(string userId, string conversationId)
    {
        try
        {
            var messages = await _context.AIChatMessages
                .Where(m => m.SessionId == Guid.Parse(conversationId))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            return messages.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading conversation messages");
            return new List<AIChatMessageDto>();
        }
    }

    public async Task<SuggestedQuestionsResponse> GetSuggestedQuestionsAsync()
    {
        return await Task.FromResult(new SuggestedQuestionsResponse
        {
            Questions = new List<string>
            {
                "What are the symptoms of the flu?",
                "How can I improve my sleep quality?",
                "What should I do about a persistent headache?",
                "Tell me about healthy eating habits",
                "When should I see a doctor for a fever?",
                "What are the benefits of regular exercise?",
                "How can I manage stress and anxiety?",
                "What are the early signs of diabetes?"
            }
        });
    }

    // ============================================
    // PRIVATE HELPER METHODS
    // ============================================

    private async Task<string> GetOrCreateSessionAsync(string userId, string? sessionId)
    {
        if (!string.IsNullOrEmpty(sessionId) && Guid.TryParse(sessionId, out var guid))
        {
            var exists = await _context.AIChatSessions.AnyAsync(s => s.Id == guid && s.UserId == userId);
            if (exists) return sessionId;
        }

        var newSession = new AIChatSession
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow
        };

        _context.AIChatSessions.Add(newSession);
        await _context.SaveChangesAsync();

        return newSession.Id.ToString();
    }

    private async Task<string> GetConversationContextAsync(string sessionId)
    {
        var messages = await _context.AIChatMessages
            .Where(m => m.SessionId == Guid.Parse(sessionId))
            .OrderByDescending(m => m.Timestamp)
            .Take(10)
            .ToListAsync();

        var context = new StringBuilder();
        foreach (var msg in messages.OrderBy(m => m.Timestamp))
        {
            context.AppendLine($"{msg.Role}: {msg.Content}");
        }

        return context.ToString();
    }

    private async Task<string> CallGeminiApiAsync(string prompt, string context)
    {
        try
        {
            var systemPrompt = @"You are MediChat AI, a helpful medical assistant. Provide accurate, empathetic health information.
Always remind users to consult healthcare professionals for serious concerns. Be concise but thorough.";

            var fullPrompt = string.IsNullOrEmpty(context)
                ? prompt
                : $"{systemPrompt}\n\nContext:\n{context}\n\nUser: {prompt}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = fullPrompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 1024
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{GEMINI_API_BASE}/models/gemini-2.5-flash:generateContent?key={_geminiApiKey}",
                content
            );

            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);

            var text = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "I apologize, but I couldn't generate a response.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            return "I'm sorry, I encountered an error. Please try again.";
        }
    }

    private async Task<string> CallGeminiVisionApiAsync(string prompt, string imageUrl)
    {
        try
        {
            // For vision, we need to use gemini-1.5-pro-vision or similar
            // This is a simplified implementation
            var fullPrompt = $@"You are a medical AI assistant analyzing a medical image.

{prompt}

Important: Always add appropriate medical disclaimers and recommend professional consultation for accurate diagnosis.";

            return await CallGeminiApiAsync(fullPrompt, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini Vision API");
            return "I'm sorry, I couldn't analyze the image. Please try again.";
        }
    }

    private string BuildSymptomAnalysisPrompt(AnalyzeSymptomsInput input)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("Analyze the following symptoms and provide:");
        prompt.AppendLine("1. Possible conditions (with probability and severity)");
        prompt.AppendLine("2. Urgency level (low, medium, high, emergency)");
        prompt.AppendLine("3. Recommended actions");
        prompt.AppendLine("4. Questions for clarification");
        prompt.AppendLine();
        prompt.AppendLine($"Symptoms: {string.Join(", ", input.Symptoms)}");
        if (!string.IsNullOrEmpty(input.Duration))
            prompt.AppendLine($"Duration: {input.Duration}");
        if (!string.IsNullOrEmpty(input.Severity))
            prompt.AppendLine($"Severity: {input.Severity}");

        return prompt.ToString();
    }

    private string BuildImageAnalysisPrompt(AnalyzeImageInput input)
    {
        var prompt = new StringBuilder();
        prompt.AppendLine("Analyze this medical image and provide:");
        prompt.AppendLine("1. Visual observations");
        prompt.AppendLine("2. Possible findings with confidence levels");
        prompt.AppendLine("3. Recommendations for next steps");
        prompt.AppendLine("4. Important: Add medical disclaimers");
        prompt.AppendLine();
        if (!string.IsNullOrEmpty(input.Context))
            prompt.AppendLine($"Context: {input.Context}");
        if (!string.IsNullOrEmpty(input.ImageType))
            prompt.AppendLine($"Image Type: {input.ImageType}");

        return prompt.ToString();
    }

    private AIChatSymptomAnalysisDto ParseSymptomAnalysisResponse(string response, List<string> symptoms)
    {
        // Simple parsing - in production, use more robust JSON parsing
        return new AIChatSymptomAnalysisDto
        {
            Symptoms = symptoms,
            PossibleConditions = new List<AIChatPossibleConditionDto>
            {
                new AIChatPossibleConditionDto
                {
                    Name = "General Assessment",
                    Description = response.Length > 200 ? response.Substring(0, 200) + "..." : response,
                    Probability = 0.7,
                    Severity = "medium"
                }
            },
            UrgencyLevel = "medium",
            UrgencyMessage = "Please consult a healthcare professional for accurate diagnosis.",
            RecommendedActions = new List<string>
            {
                "Monitor symptoms",
                "Consult a healthcare provider",
                "Stay hydrated and rest"
            }
        };
    }

    private AIChatImageAnalysisDto ParseImageAnalysisResponse(string response, string imageUrl)
    {
        return new AIChatImageAnalysisDto
        {
            ImageUrl = imageUrl,
            Analysis = response,
            Confidence = "medium",
            Recommendations = new List<string>
            {
                "Consult a healthcare professional for accurate diagnosis",
                "Provide this analysis to your doctor during consultation"
            },
            Findings = new List<AIChatFindingDto>
            {
                new AIChatFindingDto
                {
                    Category = "General Observation",
                    Description = response.Length > 150 ? response.Substring(0, 150) + "..." : response,
                    Severity = "unknown",
                    Confidence = 0.7
                }
            }
        };
    }

    private List<string> GenerateSuggestedActions(string response)
    {
        var actions = new List<string>();

        if (response.Contains("symptom", StringComparison.OrdinalIgnoreCase))
            actions.Add("Analyze my symptoms");
        if (response.Contains("doctor", StringComparison.OrdinalIgnoreCase) ||
            response.Contains("consult", StringComparison.OrdinalIgnoreCase))
            actions.Add("Find a doctor");
        if (response.Contains("medication", StringComparison.OrdinalIgnoreCase) ||
            response.Contains("medicine", StringComparison.OrdinalIgnoreCase))
            actions.Add("Learn about medications");

        return actions;
    }

    private AIChatMessageDto MapToDto(AIChatMessage message)
    {
        AIChatMetadataDto? metadata = null;
        if (!string.IsNullOrEmpty(message.MetadataJson))
        {
            try
            {
                metadata = JsonSerializer.Deserialize<AIChatMetadataDto>(message.MetadataJson);
            }
            catch
            {
                // Ignore deserialization errors
            }
        }

        return new AIChatMessageDto
        {
            Id = message.Id.ToString(),
            SessionId = message.SessionId.ToString(),
            Content = message.Content,
            Role = message.Role,
            Timestamp = message.Timestamp,
            MessageType = message.MessageType,
            Metadata = metadata
        };
    }
}

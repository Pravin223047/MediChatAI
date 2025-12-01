using MediChatAI_GraphQl.Core.Interfaces.Services.Emergency;
using MediChatAI_GraphQl.Features.Emergency.DTOs;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MediChatAI_GraphQl.Features.Emergency.Services;

/// <summary>
/// Gemini AI-powered emergency chat service with context-aware responses
/// </summary>
public class GeminiEmergencyChatService : IGeminiEmergencyChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeminiEmergencyChatService> _logger;
    private readonly string _geminiApiKey;
    private const string GEMINI_API_BASE = "https://generativelanguage.googleapis.com/v1beta";

    // In-memory chat history storage (in production, use a database)
    private static readonly ConcurrentDictionary<string, List<ChatMessageDto>> _chatSessions = new();

    public GeminiEmergencyChatService(
        HttpClient httpClient,
        ILogger<GeminiEmergencyChatService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _geminiApiKey = configuration["GeminiApiKey"] ?? throw new InvalidOperationException("GeminiApiKey not configured");
    }

    public async Task<ChatResponseDto> SendMessageAsync(SendChatMessageInput input)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Sanitize and validate input
            var sanitizedMessage = SanitizeUserInput(input.Message);
            if (string.IsNullOrWhiteSpace(sanitizedMessage))
            {
                return new ChatResponseDto
                {
                    Success = false,
                    ErrorMessage = "Invalid input provided",
                    Content = "Please provide a valid message."
                };
            }

            // Check rate limiting (basic implementation)
            if (!CheckRateLimit(input.SessionId))
            {
                return new ChatResponseDto
                {
                    Success = false,
                    ErrorMessage = "Rate limit exceeded",
                    Content = "Please wait a moment before sending another message."
                };
            }

            // Generate session ID if not provided
            var sessionId = input.SessionId ?? Guid.NewGuid().ToString();

            // Store user message in history
            var userMessage = new ChatMessageDto
            {
                Content = sanitizedMessage,
                IsUserMessage = true,
                SessionId = sessionId,
                Metadata = new ChatMessageMetadata
                {
                    HasVoiceInput = input.IsVoiceInput,
                    UserRole = input.Context?.UserRole,
                    UserId = input.Context?.UserId,
                    UserLatitude = input.Context?.UserLatitude,
                    UserLongitude = input.Context?.UserLongitude
                }
            };

            AddMessageToHistory(sessionId, userMessage);

            // Build context-aware prompt
            var prompt = BuildContextAwarePrompt(sanitizedMessage, input.Context);

            // Detect language from message
            var detectedLanguage = DetectLanguage(sanitizedMessage);
            
            // Check if this is a hospital search query
            var isHospitalQuery = IsHospitalSearchQuery(sanitizedMessage);
            var foundHospital = FindHospitalByName(sanitizedMessage, input.Context?.AvailableHospitals);

            // Get conversation history for context
            var history = GetChatHistory(sessionId);
            var conversationContext = BuildConversationContext(history);

            // Call Gemini API
            var geminiResponse = await CallGeminiApiAsync(prompt, conversationContext);

            // Parse response
            var parsedResponse = ParseGeminiChatResponse(geminiResponse);

            // Extract suggested actions and hospital recommendations
            var actions = ExtractQuickActions(parsedResponse.content, input.Context);
            var hospitalRecs = await GenerateHospitalRecommendations(sanitizedMessage, input.Context);

            // Add intelligent query processing
            if (foundHospital != null)
            {
                // Add hospital-specific information to response
                parsedResponse.content += $"\n\n**{foundHospital.Name}** is {foundHospital.DistanceInKm:F1}km away and is currently {(foundHospital.IsOpen ? "open" : "closed")}. Rating: {foundHospital.Rating:F1}/5";
            }
            else if (isHospitalQuery && (input.Context?.AvailableHospitals?.Any() != true))
            {
                // Suggest hospital search if no hospitals available
                actions.Add(new QuickActionDto
                {
                    Label = "Search Hospitals",
                    Action = "search_hospitals",
                    Icon = "🔍"
                });
            }

            // Store AI response in history
            var aiMessage = new ChatMessageDto
            {
                Content = parsedResponse.content,
                IsUserMessage = false,
                SessionId = sessionId
            };
            AddMessageToHistory(sessionId, aiMessage);

            stopwatch.Stop();

            return new ChatResponseDto
            {
                MessageId = aiMessage.Id,
                Content = parsedResponse.content,
                Success = true,
                SuggestedActions = actions,
                HospitalRecommendations = hospitalRecs,
                DetectedLanguage = detectedLanguage,
                Metrics = new ChatMetrics
                {
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    ConfidenceScore = parsedResponse.confidence,
                    RequiresFollowUp = parsedResponse.requiresFollowUp,
                    SentimentAnalysis = parsedResponse.sentiment
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in emergency chat: {Message}", input.Message);
            stopwatch.Stop();

            return new ChatResponseDto
            {
                Success = false,
                ErrorMessage = "I'm having trouble processing your request right now. Please try again or call 911 for immediate emergencies.",
                Content = "I apologize, but I encountered an error. For immediate emergencies, please call 911.",
                Metrics = new ChatMetrics
                {
                    ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                }
            };
        }
    }

    public Task<ChatHistoryResponse> GetChatHistoryAsync(ChatHistoryRequest request)
    {
        try
        {
            var sessionId = request.SessionId ?? request.UserId ?? throw new ArgumentException("SessionId or UserId required");

            var messages = GetChatHistory(sessionId);

            if (request.Since.HasValue)
            {
                messages = messages.Where(m => m.Timestamp >= request.Since.Value).ToList();
            }

            messages = messages.OrderByDescending(m => m.Timestamp).Take(request.Limit).Reverse().ToList();

            return Task.FromResult(new ChatHistoryResponse
            {
                Messages = messages,
                SessionId = sessionId,
                TotalMessages = messages.Count,
                OldestMessageTimestamp = messages.FirstOrDefault()?.Timestamp,
                NewestMessageTimestamp = messages.LastOrDefault()?.Timestamp,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat history");
            return Task.FromResult(new ChatHistoryResponse
            {
                Success = false,
                ErrorMessage = "Failed to retrieve chat history"
            });
        }
    }

    public async Task<(int urgencyLevel, string reasoning)> AnalyzeUrgencyAsync(string message)
    {
        try
        {
            var prompt = $@"
You are an emergency medical AI assistant. Analyze the following emergency description and rate its urgency level.

User's message: ""{message}""

Rate the urgency from 1-5:
- 5: Life-threatening emergency (cardiac arrest, severe bleeding, not breathing, stroke symptoms)
- 4: Serious emergency (severe pain, difficulty breathing, serious injuries)
- 3: Urgent but not immediately life-threatening (moderate pain, suspected fractures)
- 2: Non-urgent medical issue (minor injuries, mild symptoms)
- 1: General health inquiry or non-emergency

Respond with a JSON object:
{{
  ""urgencyLevel"": <number 1-5>,
  ""reasoning"": ""<brief explanation of the urgency rating>""
}}";

            var response = await CallGeminiApiAsync(prompt, "");
            var parsed = ParseUrgencyResponse(response);

            return parsed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing urgency");
            return (3, "Unable to determine urgency - assuming moderate priority");
        }
    }

    public async Task<List<string>> GetEmergencyProcedureAsync(string emergencyType, string? context = null)
    {
        try
        {
            var contextInfo = !string.IsNullOrEmpty(context) ? $"\n\nAdditional context: {context}" : "";

            var prompt = $@"
You are an emergency medical AI assistant. Provide clear, step-by-step first aid instructions for: {emergencyType}{contextInfo}

IMPORTANT:
- Start with ""CALL 911 IMMEDIATELY"" if it's a life-threatening emergency
- Give 5-7 clear, actionable steps
- Use simple language
- Focus on what to do BEFORE emergency services arrive
- Include what NOT to do if important

Format your response as a JSON array of strings, where each string is one step:
[
  ""Step 1: ....."",
  ""Step 2: ....."",
  ...
]";

            var response = await CallGeminiApiAsync(prompt, "");
            var steps = ParseProcedureSteps(response);

            return steps;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emergency procedure for {EmergencyType}", emergencyType);
            return new List<string>
            {
                "CALL 911 IMMEDIATELY for any life-threatening emergency",
                "Stay calm and assess the situation",
                "Keep the person comfortable and still",
                "Monitor breathing and consciousness",
                "Wait for professional medical help to arrive"
            };
        }
    }

    public async Task<List<HospitalRecommendationDto>> RecommendHospitalsAsync(
        string emergencyType,
        List<HospitalContextDto> availableHospitals,
        (double latitude, double longitude) userLocation)
    {
        try
        {
            if (!availableHospitals.Any())
            {
                return new List<HospitalRecommendationDto>();
            }

            var hospitalsJson = JsonSerializer.Serialize(availableHospitals.Take(10).Select(h => new
            {
                h.Name,
                h.Rating,
                h.DistanceInKm,
                h.IsOpen,
                h.Type,
                h.Specializations,
                h.HasEmergencyDepartment
            }));

            var prompt = $@"
You are an emergency medical AI assistant. Recommend the best hospitals for this emergency: ""{emergencyType}""

Available hospitals:
{hospitalsJson}

User location: ({userLocation.latitude}, {userLocation.longitude})

Criteria for recommendation:
1. Type of emergency and required specializations
2. Hospital is currently open (highest priority)
3. Distance from user (closer is better for emergencies)
4. Hospital rating and quality
5. Availability of emergency department

Provide top 3 hospital recommendations as a JSON array:
[
  {{
    ""hospitalName"": ""<name>"",
    ""reasonForRecommendation"": ""<brief explanation>"",
    ""priority"": <1-3, where 1 is highest>
  }},
  ...
]";

            var response = await CallGeminiApiAsync(prompt, "");
            var recommendations = ParseHospitalRecommendations(response, availableHospitals);

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recommending hospitals");
            // Fallback: return nearest open hospitals
            return availableHospitals
                .Where(h => h.IsOpen)
                .OrderBy(h => h.DistanceInKm)
                .Take(3)
                .Select((h, index) => new HospitalRecommendationDto
                {
                    HospitalName = h.Name,
                    ReasonForRecommendation = $"Nearest available hospital ({h.DistanceInKm:F1} km away)",
                    Priority = index + 1,
                    DistanceInKm = h.DistanceInKm,
                    Rating = h.Rating,
                    PhoneNumber = h.PhoneNumber,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude
                })
                .ToList();
        }
    }

    public Task<bool> ClearChatHistoryAsync(string sessionId)
    {
        try
        {
            _chatSessions.TryRemove(sessionId, out _);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing chat history for session {SessionId}", sessionId);
            return Task.FromResult(false);
        }
    }

    // Private helper methods

    private string BuildContextAwarePrompt(string userMessage, ChatContextDto? context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are an emergency medical AI assistant integrated into a hospital locator system.");
        sb.AppendLine("Your role is to help users find appropriate medical care and provide emergency guidance.");
        sb.AppendLine();
        sb.AppendLine("IMPORTANT GUIDELINES:");
        sb.AppendLine("- For life-threatening emergencies, ALWAYS recommend calling 911 first");
        sb.AppendLine("- Provide accurate, helpful information about hospitals and emergency procedures");
        sb.AppendLine("- Be empathetic and calm in your responses");
        sb.AppendLine("- Use clear, simple language");
        sb.AppendLine("- If unsure, recommend professional medical evaluation");
        sb.AppendLine();

        if (context != null)
        {
            if (context.AvailableHospitals?.Any() == true)
            {
                sb.AppendLine($"AVAILABLE HOSPITALS ({context.AvailableHospitals.Count} nearby):");
                foreach (var hospital in context.AvailableHospitals.Take(5))
                {
                    sb.AppendLine($"- {hospital.Name}: {hospital.DistanceInKm:F1}km away, " +
                                $"Rating: {hospital.Rating:F1}, " +
                                $"{(hospital.IsOpen ? "OPEN" : "CLOSED")}");
                }
                sb.AppendLine();
            }

            if (context.UserLatitude.HasValue && context.UserLongitude.HasValue)
            {
                sb.AppendLine($"User location: ({context.UserLatitude:F6}, {context.UserLongitude:F6})");
            }
        }

        sb.AppendLine($"User's question: \"{userMessage}\"");
        sb.AppendLine();
        sb.AppendLine("Provide a helpful, compassionate response. If recommending a hospital, explain why it's suitable.");

        return sb.ToString();
    }

    private string BuildConversationContext(List<ChatMessageDto> history)
    {
        if (!history.Any()) return "";

        var sb = new StringBuilder();
        sb.AppendLine("\n\nCONVERSATION HISTORY:");

        // Include last 5 messages for context
        foreach (var msg in history.TakeLast(5))
        {
            var role = msg.IsUserMessage ? "User" : "Assistant";
            sb.AppendLine($"{role}: {msg.Content}");
        }

        return sb.ToString();
    }

    private async Task<string> CallGeminiApiAsync(string prompt, string conversationContext)
    {
        var fullPrompt = prompt + conversationContext;

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
                maxOutputTokens = 2000,
                topP = 0.95,
                topK = 40
            },
            safetySettings = new[]
            {
                new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_ONLY_HIGH" }
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

    private (string content, int confidence, bool requiresFollowUp, string sentiment) ParseGeminiChatResponse(string response)
    {
        try
        {
            using var document = JsonDocument.Parse(response);
            var content = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            // Simple heuristics for metadata
            var confidence = content.Length > 100 ? 85 : 70;
            var requiresFollowUp = content.Contains("?") || content.ToLower().Contains("let me know");
            var sentiment = DetermineSentiment(content);

            return (content, confidence, requiresFollowUp, sentiment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response");
            return ("I'm here to help. Could you provide more details about your emergency?", 50, true, "neutral");
        }
    }

    private string DetermineSentiment(string content)
    {
        var lower = content.ToLower();
        if (lower.Contains("urgent") || lower.Contains("immediately") || lower.Contains("911"))
            return "urgent";
        if (lower.Contains("calm") || lower.Contains("reassuring") || lower.Contains("don't worry"))
            return "reassuring";
        return "neutral";
    }

    private (int urgencyLevel, string reasoning) ParseUrgencyResponse(string response)
    {
        try
        {
            using var document = JsonDocument.Parse(response);
            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = text.Substring(jsonStart, jsonEnd - jsonStart + 1);
                using var urgencyDoc = JsonDocument.Parse(jsonContent);
                var root = urgencyDoc.RootElement;

                var level = root.TryGetProperty("urgencyLevel", out var levelProp) ? levelProp.GetInt32() : 3;
                var reasoning = root.TryGetProperty("reasoning", out var reasonProp) ? reasonProp.GetString() ?? "" : "";

                return (level, reasoning);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing urgency response");
        }

        return (3, "Unable to determine specific urgency level");
    }

    private List<string> ParseProcedureSteps(string response)
    {
        try
        {
            using var document = JsonDocument.Parse(response);
            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            var arrayStart = text.IndexOf('[');
            var arrayEnd = text.LastIndexOf(']');

            if (arrayStart >= 0 && arrayEnd > arrayStart)
            {
                var jsonContent = text.Substring(arrayStart, arrayEnd - arrayStart + 1);
                return JsonSerializer.Deserialize<List<string>>(jsonContent) ?? new List<string>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing procedure steps");
        }

        return new List<string>();
    }

    private List<HospitalRecommendationDto> ParseHospitalRecommendations(string response, List<HospitalContextDto> availableHospitals)
    {
        try
        {
            using var document = JsonDocument.Parse(response);
            var text = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            var arrayStart = text.IndexOf('[');
            var arrayEnd = text.LastIndexOf(']');

            if (arrayStart >= 0 && arrayEnd > arrayStart)
            {
                var jsonContent = text.Substring(arrayStart, arrayEnd - arrayStart + 1);
                var recommendations = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(jsonContent);

                if (recommendations != null)
                {
                    return recommendations.Select(rec =>
                    {
                        var hospitalName = rec.ContainsKey("hospitalName") ? rec["hospitalName"].GetString() ?? "" : "";
                        var hospital = availableHospitals.FirstOrDefault(h => h.Name == hospitalName);

                        return new HospitalRecommendationDto
                        {
                            HospitalName = hospitalName,
                            ReasonForRecommendation = rec.ContainsKey("reasonForRecommendation") ? rec["reasonForRecommendation"].GetString() ?? "" : "",
                            Priority = rec.ContainsKey("priority") ? rec["priority"].GetInt32() : 1,
                            DistanceInKm = hospital?.DistanceInKm,
                            Rating = hospital?.Rating,
                            PhoneNumber = hospital?.PhoneNumber,
                            Latitude = hospital?.Latitude,
                            Longitude = hospital?.Longitude
                        };
                    }).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing hospital recommendations");
        }

        return new List<HospitalRecommendationDto>();
    }

    private List<QuickActionDto> ExtractQuickActions(string aiResponse, ChatContextDto? context)
    {
        var actions = new List<QuickActionDto>();

        var lower = aiResponse.ToLower();

        // Suggest calling 911 for emergencies
        if (lower.Contains("911") || lower.Contains("emergency services") || lower.Contains("ambulance"))
        {
            actions.Add(new QuickActionDto
            {
                Label = "Call 911",
                Action = "call_911",
                Icon = "📞",
                PhoneNumber = "911"
            });
        }

        // Suggest viewing hospitals
        if ((lower.Contains("hospital") || lower.Contains("medical center")) && context?.AvailableHospitals?.Any() == true)
        {
            actions.Add(new QuickActionDto
            {
                Label = "View Hospitals",
                Action = "view_hospitals",
                Icon = "🏥"
            });
        }

        // Suggest getting directions
        if (lower.Contains("direction") || lower.Contains("navigate"))
        {
            actions.Add(new QuickActionDto
            {
                Label = "Get Directions",
                Action = "get_directions",
                Icon = "🗺️"
            });
        }

        return actions;
    }

    private async Task<List<HospitalRecommendationDto>> GenerateHospitalRecommendations(string userMessage, ChatContextDto? context)
    {
        if (context?.AvailableHospitals?.Any() != true)
        {
            return new List<HospitalRecommendationDto>();
        }

        try
        {
            var emergencyType = ExtractEmergencyType(userMessage);
            if (!string.IsNullOrEmpty(emergencyType))
            {
                var userLocation = (context.UserLatitude ?? 0, context.UserLongitude ?? 0);
                return await RecommendHospitalsAsync(emergencyType, context.AvailableHospitals, userLocation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hospital recommendations");
        }

        return new List<HospitalRecommendationDto>();
    }

    private string ExtractEmergencyType(string message)
    {
        var lower = message.ToLower();

        var emergencyKeywords = new Dictionary<string, string[]>
        {
            { "cardiac", new[] { "heart attack", "chest pain", "cardiac", "heart" } },
            { "trauma", new[] { "accident", "injury", "broken", "fracture", "trauma" } },
            { "stroke", new[] { "stroke", "paralysis", "face drooping" } },
            { "respiratory", new[] { "breathing", "asthma", "respiratory", "choking" } },
            { "emergency", new[] { "emergency", "urgent", "critical" } }
        };

        foreach (var (type, keywords) in emergencyKeywords)
        {
            if (keywords.Any(keyword => lower.Contains(keyword)))
            {
                return type;
            }
        }

        return "general";
    }

    private void AddMessageToHistory(string sessionId, ChatMessageDto message)
    {
        _chatSessions.AddOrUpdate(
            sessionId,
            new List<ChatMessageDto> { message },
            (key, existing) =>
            {
                existing.Add(message);
                // Keep only last 50 messages
                return existing.TakeLast(50).ToList();
            }
        );
    }

    private List<ChatMessageDto> GetChatHistory(string sessionId)
    {
        return _chatSessions.TryGetValue(sessionId, out var history) ? history : new List<ChatMessageDto>();
    }

    private string SanitizeUserInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        
        // Remove potentially harmful characters and scripts
        input = Regex.Replace(input, @"<[^>]*>", ""); // Remove HTML tags
        input = Regex.Replace(input, @"javascript:|vbscript:|onload=|onerror=", "", RegexOptions.IgnoreCase); // Remove script injections
        input = input.Replace("\0", ""); // Remove null bytes
        
        // Limit length to prevent abuse
        if (input.Length > 500)
        {
            input = input.Substring(0, 500);
        }
        
        return input.Trim();
    }

    private static readonly ConcurrentDictionary<string, DateTime> _lastMessageTimes = new();
    private const int MESSAGE_COOLDOWN_MS = 1000; // Reduced to 1 second

    private bool CheckRateLimit(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId)) return true; // Allow if no session
        
        var now = DateTime.Now;
        var lastTime = _lastMessageTimes.GetOrAdd(sessionId, DateTime.MinValue);
        
        if ((now - lastTime).TotalMilliseconds < MESSAGE_COOLDOWN_MS)
        {
            return false;
        }
        
        _lastMessageTimes[sessionId] = now;
        return true;
    }

    private bool IsHospitalSearchQuery(string message)
    {
        var msg = message.ToLowerInvariant();
        
        // Hospital-related keywords in multiple languages
        var hospitalTerms = new[] {
            "hospital", "अस्पताल", "clinic", "क्लिनिक", "medical center", "चिकित्सा केंद्र",
            "healthcare", "स्वास्थ्य सेवा", "emergency room", "आपातकालीन कक्ष"
        };
        
        var locationTerms = new[] {
            "nearby", "निकट", "close", "पास", "near me", "मेरे पास", "around", "आसपास"
        };
        
        var actionTerms = new[] {
            "find", "खोज", "search", "तलाश", "locate", "show", "दिखा", "need", "चाहिए"
        };
        
        // Check for hospital + location/action combinations
        var hasHospital = hospitalTerms.Any(term => msg.Contains(term));
        var hasLocation = locationTerms.Any(term => msg.Contains(term));
        var hasAction = actionTerms.Any(term => msg.Contains(term));
        
        return hasHospital && (hasLocation || hasAction);
    }
    
    private HospitalContextDto? FindHospitalByName(string query, List<HospitalContextDto>? availableHospitals)
    {
        if (availableHospitals == null || !availableHospitals.Any() || string.IsNullOrWhiteSpace(query)) return null;
        
        query = query.Trim();
        var queryLower = query.ToLowerInvariant();
        
        // First try exact match
        var exactMatch = availableHospitals.FirstOrDefault(h => 
            h.Name.Equals(query, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null) return exactMatch;
        
        // Then try partial match with minimum length requirement
        if (query.Length >= 3)
        {
            return availableHospitals.FirstOrDefault(h => 
                h.Name.ToLowerInvariant().Contains(queryLower) ||
                queryLower.Contains(h.Name.ToLowerInvariant()));
        }
        
        return null;
    }

    private string DetectLanguage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "en";
        
        var textLower = text.ToLowerInvariant();
        
        // Language detection patterns
        var languagePatterns = new Dictionary<string, string[]>
        {
            ["hi"] = new[] { "अस्पताल", "डॉक्टर", "दवा", "इलाज", "बीमारी", "दर्द", "सिरदर्द", "बुखार", "खांसी", "पेट", "हाथ", "पैर", "आंख", "कान", "नाक", "मुंह", "सिर", "छाती", "पीठ", "गर्दन" },
            ["mr"] = new[] { "रुग्णालय", "डॉक्टर", "औषध", "उपचार", "आजार", "वेदना", "डोकेदुखी", "ताप", "खोकला", "पोट", "हात", "पाय", "डोळा", "कान", "नाक", "तोंड", "डोके", "छाती", "पाठ", "मान" },
            ["bn"] = new[] { "হাসপাতাল", "ডাক্তার", "ওষুধ", "চিকিৎসা", "রোগ", "ব্যথা", "মাথাব্যথা", "জ্বর", "কাশি", "পেট", "হাত", "পা", "চোখ", "কান", "নাক", "মুখ", "মাথা", "বুক", "পিঠ", "গলা" },
            ["te"] = new[] { "ఆసుపత్రి", "వైద్యుడు", "మందు", "చికిత్స", "వ్యాధి", "నొప్పి", "తలనొప్పి", "జ్వరం", "దగ్గు", "కడుపు", "చేయి", "కాలు", "కన్ను", "చెవి", "ముక్కు", "నోరు", "తల", "ఛాతీ", "వీపు", "మెడ" },
            ["ta"] = new[] { "மருத்துவமனை", "மருத்துவர்", "மருந்து", "சிகிச்சை", "நோய்", "வலி", "தலைவலி", "காய்ச்சல்", "இருமல்", "வயிறு", "கை", "கால்", "கண்", "காது", "மூக்கு", "வாய்", "தலை", "மார்பு", "முதுகு", "கழுத்து" },
            ["gu"] = new[] { "હોસ્પિટલ", "ડૉક્ટર", "દવા", "સારવાર", "બીમારી", "દુખાવો", "માથાનો દુખાવો", "તાવ", "ઉધરસ", "પેટ", "હાથ", "પગ", "આંખ", "કાન", "નાક", "મોં", "માથું", "છાતી", "પીઠ", "ગરદન" },
            ["kn"] = new[] { "ಆಸ್ಪತ್ರೆ", "ವೈದ್ಯ", "ಔಷಧ", "ಚಿಕಿತ್ಸೆ", "ರೋಗ", "ನೋವು", "ತಲೆನೋವು", "ಜ್ವರ", "ಕೆಮ್ಮು", "ಹೊಟ್ಟೆ", "ಕೈ", "ಕಾಲು", "ಕಣ್ಣು", "ಕಿವಿ", "ಮೂಗು", "ಬಾಯಿ", "ತಲೆ", "ಎದೆ", "ಬೆನ್ನು", "ಕುತ್ತಿಗೆ" },
            ["ml"] = new[] { "ആശുപത്രി", "ഡോക്ടർ", "മരുന്ന്", "ചികിത്സ", "രോഗം", "വേദന", "തലവേദന", "പനി", "ചുമ", "വയറ്", "കൈ", "കാൽ", "കണ്ണ്", "ചെവി", "മൂക്ക്", "വായ", "തല", "നെഞ്ച്", "പുറം", "കഴുത്ത്" },
            ["pa"] = new[] { "ਹਸਪਤਾਲ", "ਡਾਕਟਰ", "ਦਵਾਈ", "ਇਲਾਜ", "ਬਿਮਾਰੀ", "ਦਰਦ", "ਸਿਰਦਰਦ", "ਬੁਖਾਰ", "ਖੰਘ", "ਪੇਟ", "ਹੱਥ", "ਪੈਰ", "ਅੱਖ", "ਕੰਨ", "ਨੱਕ", "ਮੂੰਹ", "ਸਿਰ", "ਛਾਤੀ", "ਪਿੱਠ", "ਗਰਦਨ" },
            ["ur"] = new[] { "ہسپتال", "ڈاکٹر", "دوا", "علاج", "بیماری", "درد", "سردرد", "بخار", "کھانسی", "پیٹ", "ہاتھ", "پاؤں", "آنکھ", "کان", "ناک", "منہ", "سر", "سینہ", "پیٹھ", "گردن" }
        };
        
        // Count matches for each language
        var scores = new Dictionary<string, int>();
        foreach (var (lang, patterns) in languagePatterns)
        {
            scores[lang] = patterns.Count(pattern => textLower.Contains(pattern.ToLowerInvariant()));
        }
        
        // Return language with highest score, default to English
        var bestMatch = scores.Where(s => s.Value > 0).OrderByDescending(s => s.Value).FirstOrDefault();
        return bestMatch.Key ?? "en";
    }
}

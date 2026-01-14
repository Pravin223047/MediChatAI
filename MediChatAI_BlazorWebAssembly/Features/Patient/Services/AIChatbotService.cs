using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Models;
using MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public class AIChatbotService : IAIChatbotService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<AIChatbotService> _logger;

    public AIChatbotService(IGraphQLService graphQLService, ILogger<AIChatbotService> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
    }

    public async Task<ChatMessage?> SendMessageAsync(string message, string? sessionId = null)
    {
        try
        {
            var mutation = @"
                mutation SendAIChatMessage($input: SendAIChatMessageInput!) {
                    sendAIChatMessage(input: $input) {
                        success
                        message
                        chatMessage {
                            id
                            sessionId
                            content
                            role
                            timestamp
                            messageType
                            metadata {
                                suggestedActions
                                urgencyLevel
                            }
                        }
                    }
                }
            ";

            var variables = new
            {
                input = new
                {
                    SessionId = sessionId,
                    Message = message,
                    MessageType = "text"
                }
            };

            var response = await _graphQLService.SendQueryAsync<ChatMessageResponseWrapper>(mutation, variables);

            if (response?.SendAIChatMessage?.Success == true && response.SendAIChatMessage.ChatMessage != null)
            {
                var chatMessage = response.SendAIChatMessage.ChatMessage;
                _logger.LogInformation("AI Response - SessionId from server: {SessionId}, Original sessionId sent: {OriginalId}", 
                    chatMessage.SessionId, sessionId);
                return MapDtoToModel(chatMessage);
            }

            _logger.LogWarning("Failed to send message: {Message}", response?.SendAIChatMessage?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending chat message");
            return null;
        }
    }

    public async Task<ChatMessage?> SendMessageWithImageAsync(string message, string imageUrl, string? sessionId = null)
    {
        try
        {
            var mutation = @"
                mutation SendAIChatMessage($input: SendAIChatMessageInput!) {
                    sendAIChatMessage(input: $input) {
                        success
                        message
                        chatMessage {
                            id
                            sessionId
                            content
                            role
                            timestamp
                            messageType
                            attachments {
                                id
                                fileName
                                fileType
                                url
                            }
                            metadata {
                                imageAnalysis {
                                    id
                                    imageUrl
                                    analysis
                                    confidence
                                    recommendations
                                    findings {
                                        category
                                        description
                                        severity
                                        confidence
                                    }
                                }
                                suggestedActions
                                urgencyLevel
                            }
                        }
                    }
                }
            ";

            var variables = new
            {
                input = new
                {
                    SessionId = sessionId,
                    Message = message,
                    ImageUrl = imageUrl,
                    MessageType = "image"
                }
            };

            var response = await _graphQLService.SendQueryAsync<ChatMessageResponseWrapper>(mutation, variables);

            if (response?.SendAIChatMessage?.Success == true && response.SendAIChatMessage.ChatMessage != null)
            {
                return MapDtoToModel(response.SendAIChatMessage.ChatMessage);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message with image");
            return null;
        }
    }

    public async Task<ChatMessage?> SendVoiceMessageAsync(string voiceUrl, string? sessionId = null)
    {
        try
        {
            var mutation = @"
                mutation SendAIChatMessage($input: SendAIChatMessageInput!) {
                    sendAIChatMessage(input: $input) {
                        success
                        message
                        chatMessage {
                            id
                            sessionId
                            content
                            role
                            timestamp
                            messageType
                            metadata {
                                suggestedActions
                            }
                        }
                    }
                }
            ";

            var variables = new
            {
                input = new
                {
                    SessionId = sessionId,
                    Message = "",
                    VoiceUrl = voiceUrl,
                    MessageType = "voice"
                }
            };

            var response = await _graphQLService.SendQueryAsync<ChatMessageResponseWrapper>(mutation, variables);

            if (response?.SendAIChatMessage?.Success == true && response.SendAIChatMessage.ChatMessage != null)
            {
                return MapDtoToModel(response.SendAIChatMessage.ChatMessage);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending voice message");
            return null;
        }
    }

    public async Task<SymptomAnalysisResult?> AnalyzeSymptomsAsync(SymptomInput input)
    {
        try
        {
            var mutation = @"
                mutation AnalyzeSymptoms($input: AnalyzeSymptomsInput!) {
                    analyzeSymptoms(input: $input) {
                        success
                        message
                        analysis {
                            id
                            symptoms
                            possibleConditions {
                                name
                                description
                                probability
                                severity
                                commonSymptoms
                                whenToSeekCare
                            }
                            urgencyLevel
                            urgencyMessage
                            recommendedActions
                            questions
                            disclaimer
                        }
                    }
                }
            ";

            var variables = new
            {
                input = new
                {
                    Symptoms = input.Symptoms,
                    Duration = input.Duration,
                    Severity = input.Severity
                }
            };

            var response = await _graphQLService.SendQueryAsync<SymptomAnalysisResponseWrapper>(mutation, variables);

            if (response?.AnalyzeSymptoms?.Success == true && response.AnalyzeSymptoms.Analysis != null)
            {
                return MapSymptomAnalysisDtoToModel(response.AnalyzeSymptoms.Analysis);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing symptoms");
            return null;
        }
    }

    public async Task<ImageAnalysisResult?> AnalyzeImageAsync(ImageAnalysisRequest request)
    {
        try
        {
            var mutation = @"
                mutation AnalyzeImage($input: AnalyzeImageInput!) {
                    analyzeImage(input: $input) {
                        success
                        message
                        analysis {
                            id
                            imageUrl
                            analysis
                            confidence
                            recommendations
                            findings {
                                category
                                description
                                severity
                                confidence
                                notes
                            }
                        }
                    }
                }
            ";

            var variables = new
            {
                input = new
                {
                    ImageUrl = request.ImageUrl,
                    ImageType = request.ImageType,
                    Context = request.Context
                }
            };

            var response = await _graphQLService.SendQueryAsync<ImageAnalysisResponseWrapper>(mutation, variables);

            if (response?.AnalyzeImage?.Success == true && response.AnalyzeImage.Analysis != null)
            {
                return MapImageAnalysisDtoToModel(response.AnalyzeImage.Analysis);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            return null;
        }
    }

    public async Task<List<string>> GetSuggestedQuestionsAsync()
    {
        try
        {
            var query = @"
                query GetSuggestedQuestions {
                    aiChatSuggestedQuestions {
                        questions
                    }
                }
            ";

            var response = await _graphQLService.SendQueryAsync<SuggestedQuestionsResponseWrapper>(query);
            return response?.AiChatSuggestedQuestions?.Questions ?? new List<string>
            {
                "What are the symptoms of the flu?",
                "How can I improve my sleep quality?",
                "What should I do about a persistent headache?",
                "Tell me about healthy eating habits",
                "When should I see a doctor for a fever?"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggested questions");
            return new List<string>
            {
                "What are the symptoms of the flu?",
                "How can I improve my sleep quality?",
                "What should I do about a persistent headache?"
            };
        }
    }

    public async Task<List<string>> GetQuickActionsAsync()
    {
        return await Task.FromResult(new List<string>
        {
            "Check Symptoms",
            "Analyze Image",
            "Find a Doctor",
            "Medication Info",
            "Book Appointment",
            "Health Tips"
        });
    }

    public async Task<bool> SaveConversationAsync(string sessionId, string title, string? summary = null)
    {
        try
        {
            var mutation = @"
                mutation SaveConversation($input: SaveConversationInput!) {
                    saveAIChatConversation(input: $input) {
                        success
                        message
                    }
                }
            ";

            var variables = new
            {
                input = new SaveConversationInput
                {
                    SessionId = sessionId,
                    Title = title,
                    Summary = summary
                }
            };

            var response = await _graphQLService.SendQueryAsync<SaveConversationResponseWrapper>(mutation, variables);
            return response?.SaveAIChatConversation?.Success ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving conversation");
            return false;
        }
    }

    public async Task<List<SavedConversationDto>> GetConversationHistoryAsync()
    {
        try
        {
            var query = @"
                query GetConversationHistory {
                    aiChatConversationHistory {
                        id
                        title
                        summary
                        createdAt
                        lastMessageAt
                        messageCount
                    }
                }
            ";

            var response = await _graphQLService.SendQueryAsync<ConversationHistoryResponseWrapper>(query);
            return response?.AiChatConversationHistory ?? new List<SavedConversationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation history");
            return new List<SavedConversationDto>();
        }
    }

    public async Task<List<ChatMessageDto>> LoadConversationAsync(string conversationId)
    {
        try
        {
            var query = @"
                query LoadConversation($conversationId: String!) {
                    aiChatConversationMessages(conversationId: $conversationId) {
                        id
                        sessionId
                        content
                        role
                        timestamp
                        messageType
                        attachments {
                            id
                            fileName
                            fileType
                            url
                        }
                        metadata {
                            suggestedActions
                            urgencyLevel
                        }
                    }
                }
            ";

            var variables = new { conversationId };

            var response = await _graphQLService.SendQueryAsync<ConversationMessagesResponseWrapper>(query, variables);
            return response?.AiChatConversationMessages ?? new List<ChatMessageDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading conversation");
            return new List<ChatMessageDto>();
        }
    }

    public async Task<bool> DeleteConversationAsync(string conversationId)
    {
        try
        {
            var mutation = @"
                mutation DeleteConversation($conversationId: String!) {
                    deleteAIChatConversation(conversationId: $conversationId) {
                        success
                        message
                    }
                }
            ";

            var variables = new { conversationId };

            var response = await _graphQLService.SendQueryAsync<DeleteConversationResponseWrapper>(mutation, variables);
            return response?.DeleteAIChatConversation?.Success ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation");
            return false;
        }
    }

    // Helper methods to map DTOs to models
    private ChatMessage MapDtoToModel(ChatMessageDto dto)
    {
        return new ChatMessage
        {
            Id = dto.Id,
            SessionId = dto.SessionId,
            Content = dto.Content,
            Role = dto.Role,
            Timestamp = dto.Timestamp,
            Type = ParseMessageType(dto.MessageType),
            Attachments = dto.Attachments?.Select(a => new ChatAttachment
            {
                Id = a.Id,
                FileName = a.FileName,
                FileType = a.FileType,
                Url = a.Url
            }).ToList(),
            Metadata = dto.Metadata != null ? new ChatMessageMetadata
            {
                SuggestedActions = dto.Metadata.SuggestedActions,
                UrgencyLevel = dto.Metadata.UrgencyLevel,
                ImageAnalysis = dto.Metadata.ImageAnalysis != null ? MapImageAnalysisDtoToModel(dto.Metadata.ImageAnalysis) : null,
                SymptomAnalysis = dto.Metadata.SymptomAnalysis != null ? MapSymptomAnalysisDtoToModel(dto.Metadata.SymptomAnalysis) : null
            } : null
        };
    }

    private SymptomAnalysisResult MapSymptomAnalysisDtoToModel(SymptomAnalysisDto dto)
    {
        return new SymptomAnalysisResult
        {
            Id = dto.Id,
            Symptoms = dto.Symptoms,
            PossibleConditions = dto.PossibleConditions.Select(c => new PossibleCondition
            {
                Name = c.Name,
                Description = c.Description,
                Probability = c.Probability,
                Severity = c.Severity
            }).ToList(),
            UrgencyLevel = dto.UrgencyLevel,
            UrgencyMessage = dto.UrgencyMessage,
            RecommendedActions = dto.RecommendedActions
        };
    }

    private ImageAnalysisResult MapImageAnalysisDtoToModel(ImageAnalysisDto dto)
    {
        return new ImageAnalysisResult
        {
            Id = dto.Id,
            ImageUrl = dto.ImageUrl,
            Analysis = dto.Analysis,
            Confidence = dto.Confidence,
            Recommendations = dto.Recommendations,
            Findings = dto.Findings.Select(f => new Finding
            {
                Category = f.Category,
                Description = f.Description,
                Severity = f.Severity,
                Confidence = f.Confidence
            }).ToList()
        };
    }

    private MessageType ParseMessageType(string? type)
    {
        return type?.ToLower() switch
        {
            "text" => MessageType.Text,
            "image" => MessageType.Image,
            "voice" => MessageType.Voice,
            "symptomanalysis" => MessageType.SymptomAnalysis,
            "imageanalysis" => MessageType.ImageAnalysis,
            "system" => MessageType.System,
            _ => MessageType.Text
        };
    }

    // Response wrapper classes for GraphQL
    private class ChatMessageResponseWrapper
    {
        public ChatMessageResponse? SendAIChatMessage { get; set; }
    }

    private class SymptomAnalysisResponseWrapper
    {
        public SymptomAnalysisResponse? AnalyzeSymptoms { get; set; }
    }

    private class SymptomAnalysisResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public SymptomAnalysisDto? Analysis { get; set; }
    }

    private class ImageAnalysisResponseWrapper
    {
        public ImageAnalysisResponse? AnalyzeImage { get; set; }
    }

    private class ImageAnalysisResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public ImageAnalysisDto? Analysis { get; set; }
    }

    private class SuggestedQuestionsResponseWrapper
    {
        public SuggestedQuestionsResponse? AiChatSuggestedQuestions { get; set; }
    }

    private class SuggestedQuestionsResponse
    {
        public List<string> Questions { get; set; } = new();
    }

    private class SaveConversationResponseWrapper
    {
        public SaveConversationResponse? SaveAIChatConversation { get; set; }
    }

    private class SaveConversationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }

    private class ConversationHistoryResponseWrapper
    {
        public List<SavedConversationDto>? AiChatConversationHistory { get; set; }
    }

    private class ConversationMessagesResponseWrapper
    {
        public List<ChatMessageDto>? AiChatConversationMessages { get; set; }
    }

    private class DeleteConversationResponseWrapper
    {
        public DeleteConversationResponse? DeleteAIChatConversation { get; set; }
    }

    private class DeleteConversationResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}

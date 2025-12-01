using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Models;
using MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

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
            _logger.LogInformation("SendMessageAsync called with message: {Message}, sessionId: {SessionId}", message, sessionId);

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
                    Message = message,
                    MessageType = "text"
                }
            };

            _logger.LogInformation("Sending GraphQL request...");
            var response = await _graphQLService.SendQueryAsync<ChatMessageResponseWrapper>(mutation, variables);

            _logger.LogInformation("Response received. IsNull: {IsNull}", response == null);
            if (response != null)
            {
                _logger.LogInformation("SendAIChatMessage.IsNull: {IsNull}", response.SendAIChatMessage == null);
                if (response.SendAIChatMessage != null)
                {
                    _logger.LogInformation("Success: {Success}, ChatMessage.IsNull: {IsNull}",
                        response.SendAIChatMessage.Success,
                        response.SendAIChatMessage.ChatMessage == null);
                }
            }

            if (response?.SendAIChatMessage?.Success == true && response.SendAIChatMessage.ChatMessage != null)
            {
                var chatMessage = MapDtoToModel(response.SendAIChatMessage.ChatMessage);
                _logger.LogInformation("Mapped message with content: {Content}, role: {Role}", chatMessage.Content, chatMessage.Role);
                return chatMessage;
            }

            _logger.LogWarning("Returning null - conditions not met");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending doctor chat message");
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
            _logger.LogError(ex, "Error sending doctor message with image");
            return null;
        }
    }

    public async Task<ChatMessage?> GetClinicalDecisionSupportAsync(string query, string? patientContext = null)
    {
        try
        {
            var mutation = @"
                mutation GetClinicalDecisionSupport($query: String!, $patientContext: String) {
                    getClinicalDecisionSupport(query: $query, patientContext: $patientContext) {
                        success
                        message
                        chatMessage {
                            id
                            content
                            role
                            timestamp
                            metadata {
                                suggestedActions
                            }
                        }
                    }
                }
            ";

            var variables = new { query, patientContext };

            var response = await _graphQLService.SendQueryAsync<ClinicalDecisionSupportResponseWrapper>(mutation, variables);

            if (response?.Data?.GetClinicalDecisionSupport?.Success == true && response.Data.GetClinicalDecisionSupport.ChatMessage != null)
            {
                return MapDtoToModel(response.Data.GetClinicalDecisionSupport.ChatMessage);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting clinical decision support");
            return null;
        }
    }

    public async Task<ChatMessage?> AnalyzePatientCaseAsync(string caseDescription, Dictionary<string, string>? patientData = null)
    {
        try
        {
            var mutation = @"
                mutation AnalyzePatientCase($caseDescription: String!, $patientData: Map) {
                    analyzePatientCase(caseDescription: $caseDescription, patientData: $patientData) {
                        success
                        message
                        chatMessage {
                            id
                            content
                            role
                            timestamp
                            metadata {
                                suggestedActions
                            }
                        }
                    }
                }
            ";

            var variables = new { caseDescription, patientData };

            var response = await _graphQLService.SendQueryAsync<PatientCaseAnalysisResponseWrapper>(mutation, variables);

            if (response?.Data?.AnalyzePatientCase?.Success == true && response.Data.AnalyzePatientCase.ChatMessage != null)
            {
                return MapDtoToModel(response.Data.AnalyzePatientCase.ChatMessage);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing patient case");
            return null;
        }
    }

    public async Task<List<string>> GetDrugInteractionInfoAsync(List<string> medications)
    {
        try
        {
            var query = @"
                query GetDrugInteractions($medications: [String!]!) {
                    getDrugInteractions(medications: $medications) {
                        interactions
                    }
                }
            ";

            var variables = new { medications };

            var response = await _graphQLService.SendQueryAsync<DrugInteractionsResponseWrapper>(query, variables);
            return response?.Data?.GetDrugInteractions?.Interactions ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drug interaction info");
            return new List<string> { "Unable to fetch drug interaction information at this time." };
        }
    }

    public async Task<List<string>> GetDifferentialDiagnosisAsync(string symptoms, string? patientHistory = null)
    {
        try
        {
            var query = @"
                query GetDifferentialDiagnosis($symptoms: String!, $patientHistory: String) {
                    getDifferentialDiagnosis(symptoms: $symptoms, patientHistory: $patientHistory) {
                        diagnoses
                    }
                }
            ";

            var variables = new { symptoms, patientHistory };

            var response = await _graphQLService.SendQueryAsync<DifferentialDiagnosisResponseWrapper>(query, variables);
            return response?.Data?.GetDifferentialDiagnosis?.Diagnoses ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting differential diagnosis");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetQuickActionsAsync()
    {
        return await Task.FromResult(new List<string>
        {
            "Clinical Decision Support",
            "Drug Interactions",
            "Differential Diagnosis",
            "Treatment Guidelines",
            "Latest Research",
            "Patient Case Analysis"
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
                input = new
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
            _logger.LogError(ex, "Error saving doctor conversation");
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
            _logger.LogError(ex, "Error getting doctor conversation history");
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
            _logger.LogError(ex, "Error loading doctor conversation");
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
            _logger.LogError(ex, "Error deleting doctor conversation");
            return false;
        }
    }

    // Helper methods
    private ChatMessage MapDtoToModel(ChatMessageDto dto)
    {
        return new ChatMessage
        {
            Id = dto.Id,
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
                ImageAnalysis = dto.Metadata.ImageAnalysis != null ? MapImageAnalysisDtoToModel(dto.Metadata.ImageAnalysis) : null
            } : null
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
            "imageanalysis" => MessageType.ImageAnalysis,
            "system" => MessageType.System,
            _ => MessageType.Text
        };
    }

    // Response wrapper classes
    private class ChatMessageResponseWrapper
    {
        public ChatMessageResponse? SendAIChatMessage { get; set; }
    }

    private class ClinicalDecisionSupportResponseWrapper
    {
        public ClinicalDecisionSupportResponseData? Data { get; set; }
    }

    private class ClinicalDecisionSupportResponseData
    {
        public ChatMessageResponse? GetClinicalDecisionSupport { get; set; }
    }

    private class PatientCaseAnalysisResponseWrapper
    {
        public PatientCaseAnalysisResponseData? Data { get; set; }
    }

    private class PatientCaseAnalysisResponseData
    {
        public ChatMessageResponse? AnalyzePatientCase { get; set; }
    }

    private class DrugInteractionsResponseWrapper
    {
        public DrugInteractionsResponseData? Data { get; set; }
    }

    private class DrugInteractionsResponseData
    {
        public DrugInteractionsResponse? GetDrugInteractions { get; set; }
    }

    private class DrugInteractionsResponse
    {
        public List<string> Interactions { get; set; } = new();
    }

    private class DifferentialDiagnosisResponseWrapper
    {
        public DifferentialDiagnosisResponseData? Data { get; set; }
    }

    private class DifferentialDiagnosisResponseData
    {
        public DifferentialDiagnosisResponse? GetDifferentialDiagnosis { get; set; }
    }

    private class DifferentialDiagnosisResponse
    {
        public List<string> Diagnoses { get; set; } = new();
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

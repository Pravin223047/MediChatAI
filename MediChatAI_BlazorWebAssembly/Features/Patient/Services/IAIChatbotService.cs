using MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Models;
using MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public interface IAIChatbotService
{
    Task<ChatMessage?> SendMessageAsync(string message, string? sessionId = null);
    Task<ChatMessage?> SendMessageWithImageAsync(string message, string imageUrl, string? sessionId = null);
    Task<ChatMessage?> SendVoiceMessageAsync(string voiceUrl, string? sessionId = null);
    Task<SymptomAnalysisResult?> AnalyzeSymptomsAsync(SymptomInput input);
    Task<ImageAnalysisResult?> AnalyzeImageAsync(ImageAnalysisRequest request);
    Task<List<string>> GetSuggestedQuestionsAsync();
    Task<List<string>> GetQuickActionsAsync();
    Task<bool> SaveConversationAsync(string sessionId, string title, string? summary = null);
    Task<List<SavedConversationDto>> GetConversationHistoryAsync();
    Task<List<ChatMessageDto>> LoadConversationAsync(string conversationId);
    Task<bool> DeleteConversationAsync(string conversationId);
}

using MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.Models;
using MediChatAI_BlazorWebAssembly.Features.Shared.AIChat.DTOs;

namespace MediChatAI_BlazorWebAssembly.Features.Doctor.Services;

public interface IAIChatbotService
{
    Task<ChatMessage?> SendMessageAsync(string message, string? sessionId = null);
    Task<ChatMessage?> SendMessageWithImageAsync(string message, string imageUrl, string? sessionId = null);
    Task<ChatMessage?> GetClinicalDecisionSupportAsync(string query, string? patientContext = null);
    Task<ChatMessage?> AnalyzePatientCaseAsync(string caseDescription, Dictionary<string, string>? patientData = null);
    Task<List<string>> GetDrugInteractionInfoAsync(List<string> medications);
    Task<List<string>> GetDifferentialDiagnosisAsync(string symptoms, string? patientHistory = null);
    Task<List<string>> GetQuickActionsAsync();
    Task<bool> SaveConversationAsync(string sessionId, string title, string? summary = null);
    Task<List<SavedConversationDto>> GetConversationHistoryAsync();
    Task<List<ChatMessageDto>> LoadConversationAsync(string conversationId);
    Task<bool> DeleteConversationAsync(string conversationId);
}

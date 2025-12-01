using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using Polly;
using System.Net.Sockets;

namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Services;

/// <summary>
/// Resilient wrapper around EmergencyChatService with Polly retry and circuit breaker policies
/// </summary>
public class ResilientEmergencyChatService : IEmergencyChatService
{
    private readonly EmergencyChatService _innerService;
    private readonly ResiliencePipeline<ChatResponseDto?> _resiliencePipeline;

    public ResilientEmergencyChatService(EmergencyChatService innerService)
    {
        _innerService = innerService;
        _resiliencePipeline = PollyPolicies.GetCombinedPolicy();
    }

    /// <summary>
    /// Sends a message with automatic retry and circuit breaker protection
    /// </summary>
    public async Task<ChatResponseDto?> SendMessageAsync(SendChatMessageInput input)
    {
        try
        {
            // Execute with Polly policies (timeout → retry → circuit breaker)
            return await _resiliencePipeline.ExecuteAsync(async cancellationToken =>
            {
                return await _innerService.SendMessageAsync(input);
            });
        }
        catch (Polly.CircuitBreaker.BrokenCircuitException)
        {
            // Circuit is open - service is down
            Console.WriteLine("🚨 Circuit breaker is open - service unavailable");
            return new ChatResponseDto
            {
                Success = false,
                ErrorMessage = "Service temporarily unavailable. Please try again in a moment.",
                Content = string.Empty
            };
        }
        catch (SocketException sockEx)
        {
            // DNS or network failure
            Console.WriteLine($"📡 Network error: {sockEx.Message}");
            var isDnsError = sockEx.SocketErrorCode == SocketError.HostNotFound ||
                            sockEx.Message.Contains("No such host is known");
            return new ChatResponseDto
            {
                Success = false,
                ErrorMessage = isDnsError
                    ? "Cannot reach the AI service. Please check your internet connection and firewall settings."
                    : "Network connection failed. Please check your internet connection.",
                Content = string.Empty
            };
        }
        catch (TimeoutException)
        {
            // Request timed out
            Console.WriteLine("⏱️ Request timed out");
            return new ChatResponseDto
            {
                Success = false,
                ErrorMessage = "Request timed out. Please try again.",
                Content = string.Empty
            };
        }
        catch (HttpRequestException httpEx)
        {
            // HTTP request failed (might contain SocketException as inner exception)
            Console.WriteLine($"🌐 HTTP request error: {httpEx.Message}");

            // Check if inner exception is SocketException
            if (httpEx.InnerException is SocketException innerSockEx)
            {
                var isDnsError = innerSockEx.SocketErrorCode == SocketError.HostNotFound ||
                                innerSockEx.Message.Contains("No such host is known");
                return new ChatResponseDto
                {
                    Success = false,
                    ErrorMessage = isDnsError
                        ? "Cannot reach the AI service. Please check your internet connection and firewall settings."
                        : "Network connection failed. Please check your internet connection.",
                    Content = string.Empty
                };
            }

            return new ChatResponseDto
            {
                Success = false,
                ErrorMessage = "Connection error. Please check your internet connection.",
                Content = string.Empty
            };
        }
        catch (Exception ex)
        {
            // Unexpected error
            Console.WriteLine($"❌ Unexpected error: {ex.Message}");
            return new ChatResponseDto
            {
                Success = false,
                ErrorMessage = $"Unexpected error: {ex.Message}",
                Content = string.Empty
            };
        }
    }

    /// <summary>
    /// Clears chat history (no retry needed for this operation)
    /// </summary>
    public async Task<bool> ClearChatHistoryAsync(string sessionId)
    {
        // No retry policy for clear - if it fails, just return false
        try
        {
            return await _innerService.ClearChatHistoryAsync(sessionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing chat history: {ex.Message}");
            return false;
        }
    }
}

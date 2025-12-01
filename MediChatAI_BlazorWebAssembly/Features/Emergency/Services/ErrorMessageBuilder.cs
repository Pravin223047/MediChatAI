using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using System.Net.Http;
using System.Net.Sockets;

namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Services;

/// <summary>
/// Builds user-friendly error messages from exceptions
/// </summary>
public class ErrorMessageBuilder
{
    /// <summary>
    /// Builds a user-friendly error message from an exception
    /// </summary>
    public static ErrorMessage BuildFromException(Exception exception, string? customMessage = null)
    {
        return exception switch
        {
            HttpRequestException httpEx => BuildNetworkError(httpEx),
            SocketException sockEx => BuildDnsOrNetworkError(sockEx),
            TaskCanceledException => BuildTimeoutError(),
            OperationCanceledException => BuildTimeoutError(),
            ArgumentException argEx => BuildValidationError(argEx.Message),
            _ => BuildGenericError(customMessage ?? exception.Message)
        };
    }

    /// <summary>
    /// Builds an error message from a failed API response
    /// </summary>
    public static ErrorMessage BuildFromApiError(string? apiErrorMessage)
    {
        if (string.IsNullOrEmpty(apiErrorMessage))
        {
            return BuildGenericError();
        }

        // Check for specific API error patterns
        if (apiErrorMessage.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
        {
            return new ErrorMessage
            {
                Message = "Too many requests. Please wait a moment and try again.",
                Severity = ErrorSeverity.Warning,
                Icon = "⏳",
                SuggestedActions = new List<ErrorAction>
                {
                    new() { Label = "Wait & Retry", ActionType = ErrorActionType.Retry },
                    new() { Label = "Dismiss", ActionType = ErrorActionType.Dismiss }
                },
                AutoDismiss = true,
                AutoDismissSeconds = 10
            };
        }

        if (apiErrorMessage.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
        {
            return new ErrorMessage
            {
                Message = "Session expired. Please reload the page.",
                Severity = ErrorSeverity.Error,
                Icon = "🔒",
                SuggestedActions = new List<ErrorAction>
                {
                    new() { Label = "Reload Page", ActionType = ErrorActionType.Reload },
                    new() { Label = "Contact Support", ActionType = ErrorActionType.ContactSupport }
                }
            };
        }

        // Generic API error
        return new ErrorMessage
        {
            Message = $"Service error: {apiErrorMessage}",
            Severity = ErrorSeverity.Error,
            Icon = "⚠️",
            SuggestedActions = new List<ErrorAction>
            {
                new() { Label = "Retry", ActionType = ErrorActionType.Retry },
                new() { Label = "Call 911 if Urgent", ActionType = ErrorActionType.Call911 }
            }
        };
    }

    private static ErrorMessage BuildNetworkError(HttpRequestException exception)
    {
        // Check if inner exception is SocketException (DNS failure)
        if (exception.InnerException is SocketException sockEx)
        {
            return BuildDnsOrNetworkError(sockEx);
        }

        // Check error message for DNS-related failures
        var exceptionMessage = exception.Message ?? "";
        if (exceptionMessage.Contains("No such host is known", StringComparison.OrdinalIgnoreCase) ||
            exceptionMessage.Contains("Name or service not known", StringComparison.OrdinalIgnoreCase) ||
            exceptionMessage.Contains("nodename nor servname provided", StringComparison.OrdinalIgnoreCase))
        {
            return BuildDnsOrNetworkError(null);
        }

        var message = "Connection lost. Please check your internet connection.";

        if (exception.StatusCode.HasValue)
        {
            message = exception.StatusCode.Value switch
            {
                System.Net.HttpStatusCode.ServiceUnavailable => "Service temporarily unavailable. Please try again in a moment.",
                System.Net.HttpStatusCode.GatewayTimeout => "Request timed out. Please try again.",
                System.Net.HttpStatusCode.BadGateway => "Server temporarily unavailable. Please try again.",
                System.Net.HttpStatusCode.InternalServerError => "Server error occurred. Our team has been notified.",
                _ => message
            };
        }

        return new ErrorMessage
        {
            Message = message,
            Severity = ErrorSeverity.Error,
            Icon = "🌐",
            SuggestedActions = new List<ErrorAction>
            {
                new() { Label = "Retry", ActionType = ErrorActionType.Retry },
                new() { Label = "Call 911 if Urgent", ActionType = ErrorActionType.Call911 },
                new() { Label = "Dismiss", ActionType = ErrorActionType.Dismiss }
            }
        };
    }

    /// <summary>
    /// Builds an error message for DNS resolution or network connectivity failures
    /// </summary>
    private static ErrorMessage BuildDnsOrNetworkError(SocketException? exception)
    {
        // Check if this is specifically a DNS failure (SocketError.HostNotFound = 11001)
        var isDnsError = exception?.SocketErrorCode == SocketError.HostNotFound ||
                        exception?.Message.Contains("No such host is known") == true;

        var message = isDnsError
            ? "Cannot reach the AI service. Please check your internet connection and firewall settings."
            : "Network connection failed. Please check your internet connection.";

        return new ErrorMessage
        {
            Message = message,
            Severity = ErrorSeverity.Error,
            Icon = "📡",
            SuggestedActions = new List<ErrorAction>
            {
                new() { Label = "Retry", ActionType = ErrorActionType.Retry },
                new() { Label = "Call 911 if Urgent", ActionType = ErrorActionType.Call911 },
                new() { Label = "Dismiss", ActionType = ErrorActionType.Dismiss }
            }
        };
    }

    private static ErrorMessage BuildTimeoutError()
    {
        return new ErrorMessage
        {
            Message = "Response taking too long. Try asking a simpler question or check your connection.",
            Severity = ErrorSeverity.Warning,
            Icon = "⏱️",
            SuggestedActions = new List<ErrorAction>
            {
                new() { Label = "Retry", ActionType = ErrorActionType.Retry },
                new() { Label = "Simplify Question", ActionType = ErrorActionType.SimplifyQuestion },
                new() { Label = "Call 911 if Urgent", ActionType = ErrorActionType.Call911 }
            }
        };
    }

    private static ErrorMessage BuildValidationError(string validationMessage)
    {
        return new ErrorMessage
        {
            Message = validationMessage,
            Severity = ErrorSeverity.Warning,
            Icon = "ℹ️",
            SuggestedActions = new List<ErrorAction>
            {
                new() { Label = "Dismiss", ActionType = ErrorActionType.Dismiss }
            },
            AutoDismiss = true,
            AutoDismissSeconds = 8
        };
    }

    private static ErrorMessage BuildGenericError(string? customMessage = null)
    {
        return new ErrorMessage
        {
            Message = customMessage ?? "Something went wrong. Please try again.",
            Severity = ErrorSeverity.Error,
            Icon = "❌",
            SuggestedActions = new List<ErrorAction>
            {
                new() { Label = "Retry", ActionType = ErrorActionType.Retry },
                new() { Label = "Clear Chat", ActionType = ErrorActionType.ClearChat },
                new() { Label = "Call 911 if Urgent", ActionType = ErrorActionType.Call911 }
            }
        };
    }

    /// <summary>
    /// Builds a critical error that requires immediate action
    /// </summary>
    public static ErrorMessage BuildCriticalError(string message)
    {
        return new ErrorMessage
        {
            Message = message,
            Severity = ErrorSeverity.Critical,
            Icon = "🚨",
            SuggestedActions = new List<ErrorAction>
            {
                new() { Label = "Call 911 Now", ActionType = ErrorActionType.Call911 },
                new() { Label = "Dismiss", ActionType = ErrorActionType.Dismiss }
            }
        };
    }

    /// <summary>
    /// Builds an info message (not actually an error)
    /// </summary>
    public static ErrorMessage BuildInfoMessage(string message, bool autoDismiss = true)
    {
        return new ErrorMessage
        {
            Message = message,
            Severity = ErrorSeverity.Info,
            Icon = "ℹ️",
            SuggestedActions = new List<ErrorAction>
            {
                new() { Label = "Got it", ActionType = ErrorActionType.Dismiss }
            },
            AutoDismiss = autoDismiss,
            AutoDismissSeconds = 5
        };
    }
}

using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace MediChatAI_BlazorWebAssembly.Features.Emergency.Services;

/// <summary>
/// Centralized Polly policies for resilient API calls
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Retry policy: 3 attempts with exponential backoff (2s, 4s, 8s)
    /// </summary>
    public static ResiliencePipeline<ChatResponseDto?> GetRetryPolicy()
    {
        return new ResiliencePipelineBuilder<ChatResponseDto?>()
            .AddRetry(new RetryStrategyOptions<ChatResponseDto?>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true, // Add randomness to prevent thundering herd
                ShouldHandle = new PredicateBuilder<ChatResponseDto?>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .Handle<TimeoutException>()
                    .HandleResult(response => response == null || !response.Success),
                OnRetry = args =>
                {
                    Console.WriteLine($"Retry attempt {args.AttemptNumber} after {args.RetryDelay.TotalSeconds}s delay");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Circuit breaker policy: Opens after 5 consecutive failures, stays open for 30s
    /// </summary>
    public static ResiliencePipeline<ChatResponseDto?> GetCircuitBreakerPolicy()
    {
        return new ResiliencePipelineBuilder<ChatResponseDto?>()
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<ChatResponseDto?>
            {
                FailureRatio = 0.5, // Open if 50% of requests fail
                MinimumThroughput = 5, // Minimum 5 requests before calculating failure ratio
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30), // Stay open for 30s
                ShouldHandle = new PredicateBuilder<ChatResponseDto?>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .Handle<TimeoutException>()
                    .HandleResult(response => response == null || !response.Success),
                OnOpened = args =>
                {
                    Console.WriteLine($"Circuit breaker opened. Will retry after {args.BreakDuration.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    Console.WriteLine("Circuit breaker closed. Normal operation resumed.");
                    return ValueTask.CompletedTask;
                },
                OnHalfOpened = args =>
                {
                    Console.WriteLine("Circuit breaker half-open. Testing if service recovered...");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Timeout policy: 30 seconds max per request
    /// </summary>
    public static ResiliencePipeline<ChatResponseDto?> GetTimeoutPolicy()
    {
        return new ResiliencePipelineBuilder<ChatResponseDto?>()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(30),
                OnTimeout = args =>
                {
                    Console.WriteLine($"Request timed out after {args.Timeout.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Combined policy: Timeout → Retry → Circuit Breaker (innermost to outermost)
    /// Execution order: Timeout wraps Retry wraps Circuit Breaker wraps actual call
    /// </summary>
    public static ResiliencePipeline<ChatResponseDto?> GetCombinedPolicy()
    {
        return new ResiliencePipelineBuilder<ChatResponseDto?>()
            // 1. Circuit Breaker (innermost - closest to actual call)
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<ChatResponseDto?>
            {
                FailureRatio = 0.5,
                MinimumThroughput = 5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = new PredicateBuilder<ChatResponseDto?>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .Handle<TimeoutException>()
                    .HandleResult(response => response == null || !response.Success),
                OnOpened = args =>
                {
                    Console.WriteLine($"⚠️ Circuit breaker OPENED - Service unavailable for {args.BreakDuration.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                },
                OnClosed = args =>
                {
                    Console.WriteLine("✅ Circuit breaker CLOSED - Service recovered");
                    return ValueTask.CompletedTask;
                }
            })
            // 2. Retry (middle layer)
            .AddRetry(new RetryStrategyOptions<ChatResponseDto?>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<ChatResponseDto?>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .Handle<TimeoutException>()
                    .Handle<BrokenCircuitException>() // Don't retry if circuit is open
                    .HandleResult(response => response == null || !response.Success),
                OnRetry = args =>
                {
                    Console.WriteLine($"🔄 Retry attempt {args.AttemptNumber} of 3 (delay: {args.RetryDelay.TotalSeconds:F1}s)");
                    return ValueTask.CompletedTask;
                }
            })
            // 3. Timeout (outermost)
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(30),
                OnTimeout = args =>
                {
                    Console.WriteLine($"⏱️ Request timed out after {args.Timeout.TotalSeconds}s");
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }
}

using Blazored.LocalStorage;
using MediChatAI_BlazorWebAssembly.Core.Models;
using MediChatAI_BlazorWebAssembly.Features.Authentication.Models;
using MediChatAI_BlazorWebAssembly.Features.Admin.Models;
using MediChatAI_BlazorWebAssembly.Features.Doctor.Models;
using MediChatAI_BlazorWebAssembly.Features.Emergency.Models;
using MediChatAI_BlazorWebAssembly.Features.Profile.Models;
using MediChatAI_BlazorWebAssembly.Features.Notifications.Models;
using MediChatAI_BlazorWebAssembly.Core.Utilities;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

public class GraphQLService : IGraphQLService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private string? _cachedToken;
    private bool _isRefreshing = false;

    public GraphQLService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
    }

    public async Task<T?> SendQueryAsync<T>(string query, object? variables = null)
    {
        try
        {
            // Use cached token if available, otherwise fetch from localStorage
            if (_cachedToken == null)
            {
                _cachedToken = await _localStorage.GetItemAsync<string>("token");
#if DEBUG
                Console.WriteLine($"Token fetched from localStorage: {(!string.IsNullOrEmpty(_cachedToken) ? "EXISTS" : "NULL/EMPTY")}");
#endif
            }

            if (!string.IsNullOrEmpty(_cachedToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _cachedToken);
            }

            // Add Origin header for CORS
            _httpClient.DefaultRequestHeaders.Remove("Origin");
            _httpClient.DefaultRequestHeaders.Add("Origin", "http://localhost:5212");

            var request = new GraphQLRequest(query, variables);
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonDateTimeConverter() }
            });

#if DEBUG
            Console.WriteLine($"GraphQL Request URL: {_httpClient.BaseAddress}/graphql");
            Console.WriteLine($"GraphQL Request: {json}");
#endif

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/graphql", content);

            var responseJson = await response.Content.ReadAsStringAsync();

#if DEBUG
            Console.WriteLine($"GraphQL Response Status: {response.StatusCode}");
            Console.WriteLine($"GraphQL Response: {responseJson}");
#endif

            // Handle 401 Unauthorized - Token expired or invalid
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
#if DEBUG
                Console.WriteLine("Received 401 Unauthorized - Attempting token refresh");
#endif
                // Try to refresh the token
                var refreshSuccessful = await TryRefreshTokenAsync();

                if (refreshSuccessful)
                {
                    // Retry the original request with new token
                    return await SendQueryAsync<T>(query, variables);
                }
                else
                {
                    // Refresh failed, logout and redirect to login
                    Console.WriteLine("Token refresh failed - logging out");
                    await HandleUnauthorizedAsync();
                    return default;
                }
            }

            if (!response.IsSuccessStatusCode)
            {
#if DEBUG
                Console.WriteLine($"GraphQL HTTP Error: {response.StatusCode} - {responseJson}");
#endif
                return default;
            }

            var graphQLResponse = JsonSerializer.Deserialize<GraphQLResponse<T>>(responseJson,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonDateTimeConverter() }
                });

#if DEBUG
            if (graphQLResponse?.Errors != null && graphQLResponse.Errors.Any())
            {
                Console.WriteLine($"GraphQL Schema/Auth Errors found: {graphQLResponse.Errors.Length}");
                foreach (var error in graphQLResponse.Errors)
                {
                    Console.WriteLine($"GraphQL Error: {error.Message}");
                    if (error.Path != null && error.Path.Length > 0)
                    {
                        Console.WriteLine($"Error Path: {string.Join(".", error.Path)}");
                    }
                }
            }
#endif

            return graphQLResponse != null ? graphQLResponse.Data : default;
        }
        catch (Exception ex)
        {
#if DEBUG
            Console.WriteLine($"GraphQL Exception: {ex.Message}");
#else
            _ = ex; // Suppress unused variable warning in Release mode
#endif
            return default;
        }
    }

    // Clear cached token (call this on logout)
    public void ClearTokenCache()
    {
        _cachedToken = null;
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        // Prevent multiple simultaneous refresh attempts
        if (_isRefreshing)
        {
            await Task.Delay(1000); // Wait for ongoing refresh
            return _cachedToken != null;
        }

        try
        {
            _isRefreshing = true;

            var token = await _localStorage.GetItemAsync<string>("token");
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                return false;

            // Temporarily clear the cached token to avoid using expired token
            var oldToken = _cachedToken;
            _cachedToken = null;

            var refreshQuery = """
                mutation RefreshToken($input: RefreshTokenInput!) {
                  refreshToken(input: $input) {
                    success
                    token
                    refreshToken
                    user {
                      id
                      email
                      firstName
                      lastName
                      role
                      emailConfirmed
                    }
                    errors
                  }
                }
                """;

            var variables = new { input = new { token, refreshToken } };
            var request = new GraphQLRequest(refreshQuery, variables);
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/graphql", content);

            if (!response.IsSuccessStatusCode)
            {
                _cachedToken = oldToken; // Restore old token
                return false;
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var graphQLResponse = JsonSerializer.Deserialize<GraphQLResponse<RefreshTokenResponse>>(responseJson,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            var authResponse = graphQLResponse?.Data?.RefreshToken;

            if (authResponse?.Success == true && authResponse.Token != null)
            {
                // Update tokens in localStorage
                await _localStorage.SetItemAsync("token", authResponse.Token);
                await _localStorage.SetItemAsync("refreshToken", authResponse.RefreshToken);
                await _localStorage.SetItemAsync("user", authResponse.User);

                // Update cached token
                _cachedToken = authResponse.Token;

                Console.WriteLine("Token refreshed successfully");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing token: {ex.Message}");
            return false;
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private async Task HandleUnauthorizedAsync()
    {
        try
        {
            // Clear all auth data
            await _localStorage.RemoveItemAsync("token");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _localStorage.RemoveItemAsync("user");
            await _localStorage.RemoveItemAsync("app_session_id");
            await _localStorage.RemoveItemAsync("sessionVersion");

            _cachedToken = null;

            // Redirect to login page - clean redirect
            _navigationManager.NavigateTo("/login", forceLoad: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling unauthorized: {ex.Message}");
        }
    }

    // Helper class for refresh token response
    private class RefreshTokenResponse
    {
        public AuthResponse? RefreshToken { get; set; }
    }
}
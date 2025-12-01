using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Profile.Services;

public class ProfileService : IProfileService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ProfileService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<string?> UploadProfileImageAsync(Stream fileStream, string fileName)
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (string.IsNullOrEmpty(token))
                return null;

            // Copy Blazor file stream to byte array to completely avoid _blazorFilesById error
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Determine content type based on file extension
            var contentType = GetContentType(fileName);

            // Create content from byte array (not from stream)
            using var content = new MultipartFormDataContent();
            var byteArrayContent = new ByteArrayContent(fileBytes);
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Add(byteArrayContent, "file", fileName);

            // Create a new HttpClient instance for this request to avoid conflicts
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpClient.Timeout = TimeSpan.FromMinutes(5); // Increase timeout for file uploads

            var response = await httpClient.PostAsync("http://localhost:5095/api/profile/upload-image", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UploadImageResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result?.ImageUrl;
            }

            // Log the error response for debugging
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Upload failed: {response.StatusCode} - {errorContent}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload exception: {ex.Message}");
            return null;
        }
    }

    private static string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };
    }

    public async Task<T?> ExecuteGraphQLAsync<T>(string query, object? variables = null)
    {
        try
        {
            var token = await _localStorage.GetItemAsync<string>("token");
            if (string.IsNullOrEmpty(token))
                return default;

            var requestBody = new
            {
                query = query,
                variables = variables
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync("http://localhost:5095/graphql", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return result;
            }

            Console.WriteLine($"GraphQL error: {responseContent}");
            return default;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GraphQL exception: {ex.Message}");
            return default;
        }
    }

    private class UploadImageResponse
    {
        public string? ImageUrl { get; set; }
    }
}
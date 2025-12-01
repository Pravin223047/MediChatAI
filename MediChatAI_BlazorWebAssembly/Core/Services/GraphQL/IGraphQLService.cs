namespace MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;

public interface IGraphQLService
{
    Task<T?> SendQueryAsync<T>(string query, object? variables = null);
    void ClearTokenCache();
}
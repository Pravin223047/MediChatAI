namespace MediChatAI_BlazorWebAssembly.Core.Models;

// GraphQL Request/Response Infrastructure
public record GraphQLRequest(string Query, object? Variables = null);

public record GraphQLResponse<T>(T? Data, GraphQLError[]? Errors);

public record GraphQLError(string Message, string[]? Path = null);
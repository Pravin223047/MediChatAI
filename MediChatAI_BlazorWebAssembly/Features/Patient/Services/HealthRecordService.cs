using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;
using MediChatAI_BlazorWebAssembly.Core.Services.GraphQL;
using System.Text.Json;

namespace MediChatAI_BlazorWebAssembly.Features.Patient.Services;

public class HealthRecordService : IHealthRecordService
{
    private readonly IGraphQLService _graphQLService;
    private readonly ILogger<HealthRecordService> _logger;

    public HealthRecordService(IGraphQLService graphQLService, ILogger<HealthRecordService> logger)
    {
        _graphQLService = graphQLService;
        _logger = logger;
    }

    public async Task<List<PatientDocumentDto>> GetMyDocumentsAsync()
    {
        try
        {
            const string query = """
                query {
                  myDocuments {
                    id
                    patientId
                    fileName
                    fileUrl
                    documentType
                    mimeType
                    fileSizeBytes
                    description
                    uploadedAt
                  }
                }
                """;

            try
            {
                var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(query);

                if (response != null && response.ContainsKey("myDocuments"))
                {
                    var json = JsonSerializer.Serialize(response["myDocuments"]);
                    var documents = JsonSerializer.Deserialize<List<PatientDocumentDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<PatientDocumentDto>();
                    _logger.LogInformation("Loaded {Count} documents from GraphQL", documents.Count);
                    return documents;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GraphQL error loading documents");
                // Return empty list instead of throwing to prevent UI crashes
                return new List<PatientDocumentDto>();
            }

            _logger.LogWarning("No documents data received from GraphQL");
            return new List<PatientDocumentDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading documents from GraphQL");
            throw;
        }
    }

    public async Task<PatientDocumentDto> UploadDocumentAsync(string fileName, string fileUrl, DocumentType documentType, string mimeType, long fileSizeBytes, string? description = null)
    {
        try
        {
            // Build mutation with enum value directly in the query string (not as a variable)
            // This is necessary because GraphQL enums need special serialization
            var descriptionValue = string.IsNullOrEmpty(description) ? "null" : $"\"{description.Replace("\"", "\\\"")}\"";

            // Convert PascalCase to UPPER_SNAKE_CASE for GraphQL
            var enumName = documentType.ToString();
            var enumValue = string.Concat(enumName.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToUpper();

            var mutation = $@"
                mutation {{
                  uploadPatientDocument(input: {{
                    fileName: ""{fileName.Replace("\"", "\\\"")}""
                    fileUrl: ""{fileUrl.Replace("\"", "\\\"")}""
                    documentType: {enumValue}
                    mimeType: ""{mimeType}""
                    fileSizeBytes: {fileSizeBytes}
                    description: {descriptionValue}
                    patientId: """"
                    uploadedById: """"
                  }}) {{
                    id
                    patientId
                    fileName
                    fileUrl
                    documentType
                    mimeType
                    fileSizeBytes
                    description
                    uploadedAt
                  }}
                }}
                ";

            _logger.LogInformation("Sending mutation for file: {FileName}, DocumentType: {DocumentType}, EnumValue: {EnumValue}", fileName, documentType, enumValue);
            _logger.LogDebug("Full mutation: {Mutation}", mutation);

            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(mutation, null);

            if (response != null && response.ContainsKey("uploadPatientDocument"))
            {
                var json = JsonSerializer.Serialize(response["uploadPatientDocument"]);
                var document = JsonSerializer.Deserialize<PatientDocumentDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (document != null)
                {
                    _logger.LogInformation("Document uploaded successfully: {FileName}", fileName);
                    return document;
                }
            }

            throw new Exception("Failed to upload document - no response data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        try
        {
            var mutation = $@"
                mutation {{
                  deletePatientDocument(documentId: {documentId})
                }}
                ";

            _logger.LogInformation("Deleting document: {DocumentId}", documentId);

            var response = await _graphQLService.SendQueryAsync<Dictionary<string, object>>(mutation, null);

            if (response != null && response.ContainsKey("deletePatientDocument"))
            {
                var deleteResult = response["deletePatientDocument"];
                bool success = false;
                
                // Handle different possible return types from GraphQL
                if (deleteResult is bool boolResult)
                {
                    success = boolResult;
                }
                else if (deleteResult is string stringResult)
                {
                    success = bool.TryParse(stringResult, out var parsed) && parsed;
                }
                else if (deleteResult is JsonElement jsonElement)
                {
                    success = jsonElement.ValueKind == JsonValueKind.True || 
                             (jsonElement.ValueKind == JsonValueKind.String && bool.TryParse(jsonElement.GetString(), out var parsed) && parsed);
                }
                else if (deleteResult != null)
                {
                    // Try to convert other types (int, etc.) - non-zero/non-null = true
                    success = !deleteResult.Equals(0) && !deleteResult.Equals(false) && !deleteResult.Equals("false");
                }
                
                _logger.LogInformation("Document {DocumentId} deleted: {Success} (raw response: {RawResponse})", documentId, success, deleteResult);
                return success;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            throw;
        }
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;
using MediChatAI_BlazorWebAssembly.Features.Patient.DTOs;

namespace MediChatAI_BlazorWebAssembly.Core.Converters;

public class DocumentTypeConverter : JsonConverter<DocumentType>
{
    public override DocumentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return DocumentType.Other;

        // Convert UPPER_SNAKE_CASE to PascalCase
        // LAB_RESULT -> LabResult
        // MEDICAL_REPORT -> MedicalReport
        var pascalCase = string.Join("", value.Split('_')
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));

        return Enum.TryParse<DocumentType>(pascalCase, true, out var result) ? result : DocumentType.Other;
    }

    public override void Write(Utf8JsonWriter writer, DocumentType value, JsonSerializerOptions options)
    {
        // Convert PascalCase to UPPER_SNAKE_CASE
        var name = value.ToString();
        var snakeCase = string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToUpper();
        writer.WriteStringValue(snakeCase);
    }
}

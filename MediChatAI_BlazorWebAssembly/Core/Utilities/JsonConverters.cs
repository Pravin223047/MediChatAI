using System.Text.Json;
using System.Text.Json.Serialization;

namespace MediChatAI_BlazorWebAssembly.Core.Utilities;

public class JsonDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (DateTime.TryParse(stringValue, out var result))
                return result;
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            // Convert to UTC if not already UTC
            // This ensures the 'Z' suffix accurately represents the time as UTC
            var utcValue = value.Value.Kind == DateTimeKind.Utc
                ? value.Value
                : value.Value.ToUniversalTime();
            writer.WriteStringValue(utcValue.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

public class CaseInsensitiveEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var intValue = reader.GetInt32();
            return (T)Enum.ToObject(typeof(T), intValue);
        }

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
            return default;

        // Try direct parse first (handles PascalCase)
        if (Enum.TryParse<T>(value, true, out var result))
            return result;

        // Convert UPPER_SNAKE_CASE to PascalCase (e.g., FOLLOW_UP -> FollowUp)
        var pascalValue = string.Concat(value.Split('_')
            .Select(word => word.Length > 0 ? char.ToUpper(word[0]) + word.Substring(1).ToLower() : ""));

        if (Enum.TryParse<T>(pascalValue, true, out result))
            return result;

        return default;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
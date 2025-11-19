using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace zoplannerservice.Serialization;

/// <summary>
/// Flexible converter for DateOnly that accepts either "yyyy-MM-dd" or a full DateTime string like "yyyy-MM-ddTHH:mm:ss".
/// Always writes as "yyyy-MM-dd".
/// </summary>
public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string OutputFormat = "yyyy-MM-dd";

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var raw = reader.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                throw new JsonException("Empty date string");
            }

            // Try pure DateOnly first
            if (DateOnly.TryParse(raw, out var d))
            {
                return d;
            }

            // Try DateTime and take the date portion
            if (DateTime.TryParse(raw, out var dt))
            {
                return DateOnly.FromDateTime(dt);
            }

            throw new JsonException($"Invalid date format: '{raw}'");
        }

        throw new JsonException($"Unexpected token parsing DateOnly. Token: {reader.TokenType}");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(OutputFormat));
    }
}

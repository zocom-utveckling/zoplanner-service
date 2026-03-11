using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace zoplannerservice.Serialization;

public class FormatTimeSpan : JsonConverter<TimeSpan>
{
    private const string Format = @"hh\:mm"; // matches "10:00"

    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (TimeSpan.TryParseExact(value, Format, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        if (TimeSpan.TryParse(value, out result))
        {
            return result;
        }

        throw new JsonException($"Unable to parse '{value}' as TimeSpan");
    }

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}

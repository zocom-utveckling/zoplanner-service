using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace zoplannerservice.Serialization;

/// <summary>
/// Sets the DateTime format in JSON body to yyyy-MM-dd HH:mm
/// </summary>
public class FormatDateTime : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd HH:mm";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();

        //accepted date/time formats
        var formats = new[]
        {
            "yyyy-MM-dd HH:mm",           
            "yyyy-MM-ddTHH:mm:ss",        
            "yyyy-MM-ddTHH:mm:ss.fff",   
            "yyyy-MM-ddTHH:mm:ssZ",       
            "yyyy-MM-ddTHH:mm:ss.fffZ"    
        };

        if (DateTime.TryParseExact(dateString, formats, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var result))
        {
            return result;
        }

        throw new JsonException($"Unable to parse '{dateString}' as DateTime");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));
}
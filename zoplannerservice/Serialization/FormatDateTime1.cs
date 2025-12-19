using System.Text.Json;
using System.Text.Json.Serialization;

namespace zoplannerservice.Serialization;

/// <summary>
/// Sets the DateTime format in JSON body to yyyy-MM-dd HH:mm
/// </summary>
public class DateTimeFormatter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd HH:mm";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Format));

}

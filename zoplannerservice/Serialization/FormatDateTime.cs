using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace zoplannerservice.Serialization
{
    public class FormatDateTime : JsonConverter<DateTime>
    {
        private static readonly string[] AcceptedFormats = new[]
        {
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.fff",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-dd"
        };

        private const string OutputFormat = "yyyy-MM-ddTHH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
                throw new JsonException("Date string is null or empty");

            if (DateTime.TryParseExact(
                dateString,
                AcceptedFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var result))
            {
                return result;
            }

            if (DateTime.TryParse(dateString, out result))
                return result;

            throw new JsonException($"Unable to parse '{dateString}' as DateTime");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(OutputFormat));
        }
    }
}
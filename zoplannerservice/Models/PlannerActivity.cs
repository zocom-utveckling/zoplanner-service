using System.Text.Json.Serialization;
using zoplannerservice.Enums;
using zoplannerservice.Serialization;

public class PlannerActivity
{
    public long Id { get; set; }

    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateOnly Date { get; set; }

    [JsonConverter(typeof(FormatTimeSpan))]
    public TimeSpan StartTime { get; set; }

    [JsonConverter(typeof(FormatTimeSpan))]
    public TimeSpan EndTime { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }

    [JsonConverter(typeof(FormatDateTime))]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("userid")]
    public long UserId { get; set; }
}

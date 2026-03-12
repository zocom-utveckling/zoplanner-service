using System.Text.Json.Serialization;

namespace zoplannerservice.Models;

/// <summary>
/// Simple direct message email event.
/// Matches the JSON format expected by the Java notification layer for DIRECT_MESSAGE.
/// </summary>
public class DirectMessageEvent
{
    [JsonPropertyName("EventType")]
    public string EventType { get; set; } = "DIRECT_MESSAGE";

    [JsonPropertyName("RecipientEmail")]
    public string RecipientEmail { get; set; } = string.Empty;

    [JsonPropertyName("Subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("Message")]
    public string Message { get; set; } = string.Empty;
}


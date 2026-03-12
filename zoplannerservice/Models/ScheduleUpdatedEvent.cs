using System;
using System.Text.Json.Serialization;

namespace zoplannerservice.Models;

/// <summary>
/// Event representing an updated schedule that should be sent to the notification service.
/// Matches the JSON format expected by the Java notification layer for SCHEDULE_UPDATED.
/// </summary>
public class ScheduleUpdatedEvent
{
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = "SCHEDULE_UPDATED";

    [JsonPropertyName("teacherId")]
    public string TeacherId { get; set; } = string.Empty;

    // Recipient of the email (can be same as TeacherEmail)
    [JsonPropertyName("recipient")]
    public string Recipient { get; set; } = string.Empty;

    [JsonPropertyName("teacherEmail")]
    public string TeacherEmail { get; set; } = string.Empty;

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("eventTime")]
    public DateTimeOffset EventTime { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = "schedule-service";

    [JsonPropertyName("preference")]
    public string Preference { get; set; } = "WEEKLY_SUMMARY";
}


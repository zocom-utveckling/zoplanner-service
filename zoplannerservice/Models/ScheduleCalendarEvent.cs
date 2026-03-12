using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace zoplannerservice.Models;

/// <summary>
/// Calendar-style schedule email summarizing multiple days.
/// Matches the JSON format expected by the Java notification layer for SCHEDULE_CALENDAR.
/// </summary>
public class ScheduleCalendarEvent
{
    [JsonPropertyName("EventType")]
    public string EventType { get; set; } = "SCHEDULE_CALENDAR";

    [JsonPropertyName("EventId")]
    public Guid EventId { get; set; }

    [JsonPropertyName("TeacherName")]
    public string TeacherName { get; set; } = string.Empty;

    [JsonPropertyName("TeacherEmail")]
    public string TeacherEmail { get; set; } = string.Empty;

    [JsonPropertyName("MonthTitle")]
    public string MonthTitle { get; set; } = string.Empty;

    [JsonPropertyName("WeekRange")]
    public string WeekRange { get; set; } = string.Empty;

    [JsonPropertyName("Days")]
    public List<ScheduleCalendarDay> Days { get; set; } = new();
}

public class ScheduleCalendarDay
{
    [JsonPropertyName("DayNumber")]
    public string DayNumber { get; set; } = string.Empty;

    [JsonPropertyName("ContentHtml")]
    public string ContentHtml { get; set; } = string.Empty;
}


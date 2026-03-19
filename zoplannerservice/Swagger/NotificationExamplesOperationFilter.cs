using System.Text.Json;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace zoplannerservice.Swagger;

/// <summary>
/// Adds request body examples for notification endpoints so Swagger shows the correct eventType and structure for each.
/// </summary>
public class NotificationExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content == null) return;

        var path = context.ApiDescription.RelativePath ?? "";
        if (!path.StartsWith("api/Notification", StringComparison.OrdinalIgnoreCase)) return;

        string? exampleJson = context.ApiDescription.HttpMethod?.ToUpperInvariant() == "POST"
            ? GetExampleJson(path)
            : null;

        if (exampleJson == null) return;

        try
        {
            using var doc = JsonDocument.Parse(exampleJson);
            var openApiExample = JsonElementToOpenApiAny(doc.RootElement);
            foreach (var contentType in operation.RequestBody.Content)
            {
                contentType.Value.Example = openApiExample;
            }
        }
        catch
        {
            foreach (var contentType in operation.RequestBody.Content)
            {
                contentType.Value.Example = new OpenApiString(exampleJson);
            }
        }
    }

    private static IOpenApiAny JsonElementToOpenApiAny(JsonElement el)
    {
        switch (el.ValueKind)
        {
            case JsonValueKind.Object:
                var obj = new OpenApiObject();
                foreach (var p in el.EnumerateObject())
                    obj[p.Name] = JsonElementToOpenApiAny(p.Value);
                return obj;
            case JsonValueKind.Array:
                var arr = new OpenApiArray();
                foreach (var item in el.EnumerateArray())
                    arr.Add(JsonElementToOpenApiAny(item));
                return arr;
            case JsonValueKind.String:
                return new OpenApiString(el.GetString() ?? "");
            case JsonValueKind.Number:
                return el.TryGetInt32(out var i) ? new OpenApiInteger(i) : new OpenApiLong(el.GetInt64());
            case JsonValueKind.True:
                return new OpenApiBoolean(true);
            case JsonValueKind.False:
                return new OpenApiBoolean(false);
            case JsonValueKind.Null:
                return new OpenApiNull();
            default:
                return new OpenApiString(el.GetRawText());
        }
    }

    private static string? GetExampleJson(string path)
    {
        if (path.Contains("send-new-assignment", StringComparison.OrdinalIgnoreCase))
            return """{"eventType":"NEW_ASSIGNMENT","eventId":"3fa85f64-5717-4562-b3fc-2c963f66afa6","timestamp":"2026-03-12T13:00:00.000Z","teacherId":"teacher-1","teacherEmail":"teacher@example.com","assignmentId":"assignment-1","assignmentDescription":"Review chapter 5","teacherName":"Jane Doe","assignmentDueDate":"2026-03-15T17:00:00.000Z"}""";

        if (path.Contains("send-schedule-updated", StringComparison.OrdinalIgnoreCase))
            return """{"eventType":"SCHEDULE_UPDATED","teacherId":"teacher-1","recipient":"teacher@example.com","teacherEmail":"teacher@example.com","subject":"Schedule update – week 12","message":"Your schedule has been updated.","eventTime":"2026-03-15T09:00:00.000Z","createdAt":"2026-03-12T13:00:00.000Z","source":"schedule-service","preference":"WEEKLY_SUMMARY"}""";

        if (path.Contains("send-direct-message", StringComparison.OrdinalIgnoreCase))
            return """{"EventType":"DIRECT_MESSAGE","RecipientEmail":"user@example.com","Subject":"Test subject","Message":"Hello, this is a test message."}""";

        if (path.Contains("send-schedule-calendar", StringComparison.OrdinalIgnoreCase))
            return """{"EventType":"SCHEDULE_CALENDAR","EventId":"f6b1b6e2-5c8c-4b9c-9a3d-9f4f9e2a1a11","TeacherName":"Jane Doe","TeacherEmail":"teacher@example.com","MonthTitle":"Mars 2026","WeekRange":"v.10-13","Days":[{"DayNumber":"1","ContentHtml":"<div>Morning session</div>"}]}""";

        return null;
    }
}

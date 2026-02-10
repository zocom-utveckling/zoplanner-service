namespace zoplannerservice.Models;
public class CreateCalendarEventRequest
{
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid UserId { get; set; }
}

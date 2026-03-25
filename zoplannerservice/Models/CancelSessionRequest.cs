using zoplannerservice.Enums;

namespace zoplannerservice.Models;

public class CancelSessionRequest
{
    public CancellationReason Reason { get; set; }
    public string? Comment { get; set; }
}

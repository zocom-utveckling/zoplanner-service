using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace zoplannerservice.Models;

public class NewAssignmentEvent
{
    public EventType EventType { get; set; }
    public Guid EventId { get; set; }
    public DateTimeOffset Timestamp { get; set; }

    // Required
    public string TeacherId { get; set; } = "";

    public string TeacherEmail { get; set; } = "";
    public string AssignmentId { get; set; } = "";
    public string AssignmentDescription { get; set; } = "";

    // Optional
    public string? TeacherName { get; set; } = "";
    public DateTime? AssignmentDueDate { get; set; }
}

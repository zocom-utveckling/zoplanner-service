using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models;

public class CreateAssignmentRequest
{
    public long? ConsultantId { get; set; }   // ❌ ta bort Required

    public long? ManagerId { get; set; }      // ✅ lägg till

    [Required(ErrorMessage = "DateStart is required")]
    public DateOnly? DateStart { get; set; }

    [Required(ErrorMessage = "DateEnd is required")]
    public DateOnly? DateEnd { get; set; }

    [Required]
    public long? CourseId { get; set; }
}
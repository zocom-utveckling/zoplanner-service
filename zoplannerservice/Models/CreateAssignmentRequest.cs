using System.ComponentModel.DataAnnotations;
namespace zoplannerservice.Models;

/// <summary>
/// DTO for creating a new assignment
///    </summary>
public class CreateAssignmentRequest
{
    public long? ConsultantId { get; set; }

    [Required(ErrorMessage = "ManagerId is required")]
    public long? ManagerId { get; set; }

    [Required(ErrorMessage = "DateStart is required")]
    public DateOnly? DateStart { get; set; }  

    [Required(ErrorMessage = "DateEnd is required")]
    public DateOnly? DateEnd { get; set; }

    public long? CourseId { get; set; }

}
using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models;

/// <summary>
/// DTO for updating an existing assignment
///   </summary>
public class UpdateAssignmentRequest
{
    [Required(ErrorMessage = "CourseName is required")]
    [MaxLength(200)]

    public long? ConsultantId { get; set; }

    [Required(ErrorMessage = "DateStart is required")]
    public DateOnly? DateStart { get; set; }  

    [Required(ErrorMessage = "DateEnd is required")]
    public DateOnly? DateEnd { get; set; }    

}
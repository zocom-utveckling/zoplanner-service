using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models;

/// <summary>
/// DTO for updating an existing assignment
///   </summary>
public class UpdateAssignmentRequest
{
    [Required(ErrorMessage = "CourseName is required")]
    [MaxLength(200)]
    public string CourseName { get; set; } = string.Empty;

    public long? ConsultantId { get; set; }

    [Required(ErrorMessage = "DateStart is required")]
    public DateTime? DateStart { get; set; }  

    [Required(ErrorMessage = "DateEnd is required")]
    public DateTime? DateEnd { get; set; }    

    public long? ClassId { get; set; }      
}
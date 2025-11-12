using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models;

/// <summary>
/// DTO for partially updating an existing assignment
///   </summary>
public class PatchAssignmentRequest
{
 
    [MaxLength(200)]
    public string CourseName { get; set; } = string.Empty;
    public long? ConsultantId { get; set; }
    public DateTime? DateStart { get; set; }    
    public DateTime? DateEnd { get; set; }    
    public long? ClassId { get; set; }      
}
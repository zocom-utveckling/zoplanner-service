using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models;

/// <summary>
/// DTO for partially updating an existing assignment
///   </summary>
public class PatchAssignmentRequest
{
 
    [MaxLength(200)]
    public long? ConsultantId { get; set; }
    public DateOnly? DateStart { get; set; }    
    public DateOnly? DateEnd { get; set; }    
}
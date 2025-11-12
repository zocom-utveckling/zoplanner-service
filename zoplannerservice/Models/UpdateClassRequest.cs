using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models;

/// <summary>
/// DTO for updating an existing class    
/// </summary>
public class UpdateClassRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer Id is required")]
    public long? CustomerId { get; set; }  // Nullable - может быть null
}

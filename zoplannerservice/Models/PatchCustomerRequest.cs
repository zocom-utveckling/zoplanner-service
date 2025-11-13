using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models;

/// <summary>
/// DTO for partially updating a Customer. All fields are optional.
/// </summary>
public class PatchCustomerRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(200)]
    public string? City { get; set; }
}

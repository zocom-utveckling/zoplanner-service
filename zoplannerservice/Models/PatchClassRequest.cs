using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models;

/// <summary>
/// DTO for partially updating a class
/// All fields are optional
/// </summary>
public class PatchClassRequest
{
    [MaxLength(200)]
    public string? Name { get; set; }

    public int? CustomerId { get; set; }
}
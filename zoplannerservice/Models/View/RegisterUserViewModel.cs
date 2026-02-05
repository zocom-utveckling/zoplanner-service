using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using zoplannerservice.Enums.UserRole;

namespace zoplannerservice.Models.View;

public class RegisterViewModel : LoginViewModel
{
    [Required]
    [Compare("Password", ErrorMessage = "Paswords do not match.")]
    public string ConfirmPassword { get; set; } = "";
    public string City { get; set; } = "";
    public string Name { get; set; } = "";
    public UserRole Role { get; set; }
}

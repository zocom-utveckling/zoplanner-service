using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace zoplannerservice.Models.View;

public class LoginViewModel
{
    [StringLength(50, ErrorMessage = "Max 50 char")]
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = "";


    [Required]
    [StringLength(50, ErrorMessage = "Max 50 char")]
    [EmailAddress]
    public string Email { get; set; } = "";


    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = "";

}


using System.ComponentModel.DataAnnotations;
namespace zoplannerservice.Models
{
    public class CreateSessionRequest
    {
        [Required(ErrorMessage = "TimeStart is required")]
        public string TimeStart { get; set; } = string.Empty;

        [Required(ErrorMessage = "TimeEnd is required")]
        public string TimeEnd { get; set; } = string.Empty;
    }
}   
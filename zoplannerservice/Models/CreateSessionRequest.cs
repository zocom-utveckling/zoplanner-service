using System.ComponentModel.DataAnnotations;
using zoplannerservice.Enums.SessionLocation;
namespace zoplannerservice.Models
{
    public class CreateSessionRequest
    {
        [Required(ErrorMessage = "TimeStart is required")]
        public string TimeStart { get; set; } = string.Empty;

        [Required(ErrorMessage = "TimeEnd is required")]
        public string TimeEnd { get; set; } = string.Empty;

        [Required]
        public SessionLocation Location { get; set; }

        public String Comment { get; set; } = string.Empty;
    }
}   
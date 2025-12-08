using System.ComponentModel.DataAnnotations;
using zoplannerservice.Enums.SessionLocation;
namespace zoplannerservice.Models
{
    public class CreateSessionRequest
    {
        [Required(ErrorMessage = "TimeStart is required")]
        public DateTime TimeStart { get; set; }

        [Required(ErrorMessage = "TimeEnd is required")]
        public DateTime TimeEnd { get; set; }

        [Required]
        public SessionLocation Location { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}

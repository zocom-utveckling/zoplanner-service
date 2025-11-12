using System.ComponentModel.DataAnnotations;
namespace zoplannerservice.Models
{
    public class CreateSessionRequest
    {
        [Required(ErrorMessage = "TimeStart is required")]
        public DateTime TimeStart { get; set; }

        [Required(ErrorMessage = "TimeEnd is required")]
        public DateTime TimeEnd { get; set; }
    }
}   
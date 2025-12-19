
using System.ComponentModel.DataAnnotations;
namespace zoplannerservice.Models
{
    public class CreateManagerRequest
    {
        [Required]
        public long UserId { get; set; }
    }
}


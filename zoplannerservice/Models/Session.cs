using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace zoplannerservice.Models
{
    public class Session
    {
        public long Id { get; set; }
        public long? AssignmentId { get; set; }

        [Required]
        public DateTime TimeStart { get; set; }

        [Required]
        public DateTime TimeEnd { get; set; } 


        [JsonIgnore]
        public Assignment? Assignment { get; set; }
    }
}
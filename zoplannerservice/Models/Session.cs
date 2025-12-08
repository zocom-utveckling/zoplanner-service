using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zoplannerservice.Enums.SessionLocation;


namespace zoplannerservice.Models
{
    public class Session
    {
        public long Id { get; set; }

        [JsonIgnore]
        public long? AssignmentId { get; set; }

        [Required]
        public DateTime TimeStart { get; set; } 

        [Required]
        public DateTime TimeEnd { get; set; }

        public string Comment { get; set; } = string.Empty;

        [Required]
        public SessionLocation Location { get; set; }
       
       // public Assignment? Assignment { get; set; } //
    }
}
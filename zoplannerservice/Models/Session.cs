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
        public string TimeStart { get; set; } = string.Empty;

        [Required]
        public string TimeEnd { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        [Required]
        public SessionLocation Location { get; set; }
       
       // public Assignment? Assignment { get; set; } //
    }
}
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


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


       
       // public Assignment? Assignment { get; set; } //
    }
}
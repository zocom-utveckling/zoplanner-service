using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace zoplannerservice.Models
{
    public class Session
    {
        public long Id { get; set; }
        public long? AssignmentId { get; set; }

        [Required]
        [JsonPropertyName("timeStart")]
        public DateTime TimeStart { get; set; }

        [Required]
        [JsonPropertyName("timeEnd")]
        public DateTime TimeEnd { get; set; }
    }
}
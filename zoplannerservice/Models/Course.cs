using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zoplannerservice.Enums.SessionLocation;


namespace zoplannerservice.Models
{
    public class Course
    {

        public long Id { get; set; }

        [Required]
        public long ClassId { get; set; }

        public string Name { get; set; } = string.Empty;

        [Required]
        public DateOnly? DateStart { get; set; }

        [Required]
        public DateOnly? DateEnd { get; set; }
    }
}
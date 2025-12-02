using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using zoplannerservice.Enums.SessionLocation;


namespace zoplannerservice.Models
{
    public class CreateCourseRequest
    {
        [Required(ErrorMessage = "ClassId is required")]
        public long ClassId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "DateStart is required")]
        public DateOnly? DateStart { get; set; }

        [Required(ErrorMessage = "DateEnd is required")]
        public DateOnly? DateEnd { get; set; }
    }
}
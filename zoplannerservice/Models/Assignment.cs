using System.ComponentModel.DataAnnotations;


namespace zoplannerservice.Models
{
    public class Assignment
    {

        public long Id { get; set; }

        [Required]
        public long? ConsultantId { get; set; }

        [Required]
        public DateOnly? DateStart { get; set; }

        [Required]
        public DateOnly? DateEnd { get; set; }

        public long CourseId { get; set; }
        public Course? Course { get; set; }

        public List<Session>? Sessions { get; set; } 
    }
}
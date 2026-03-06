using zoplannerservice.Enums.SessionLocation;

namespace zoplannerservice.DTO.Responses
{
    public class SessionDto
    {
        public long SessionId { get; set; }

        public DateTime TimeStart { get; set; }

        public DateTime TimeEnd { get; set; }

        public string? Location { get; set; }
        public string Comment { get; set; }

        public long AssignmentId { get; set; }

        public long CourseId { get; set; }

        public string CourseName { get; set; }
    }
}

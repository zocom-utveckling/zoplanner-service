namespace zoplannerservice.DTO.Responses
{
    public class ScheduleSearchRowDto
    {
        public long ConsultantId { get; set; }
        public string? City { get; set; }

        public long CourseId { get; set; }
        public string? CourseName { get; set; }

        public long SessionId { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }

        public string? Location { get; set; }
        public string? Comment { get; set; }
    }
}

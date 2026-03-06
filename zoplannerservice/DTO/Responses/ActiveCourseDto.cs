namespace zoplannerservice.DTO.Responses
{
    public class ActiveCourseDto
    {
        public long CourseId { get; set; }

        public string Name { get; set; }

        public DateOnly? DateStart { get; set; }

        public DateOnly? DateEnd { get; set; }
    }
}

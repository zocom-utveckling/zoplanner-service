namespace zoplannerservice.DTO.Responses
{
    public class ConsultantOverviewResponse
    {
        public long ConsultantId { get; set; }

        public List<ActiveCourseDto> ActiveCourses { get; set; } = new();

        public List<SessionDto> Sessions { get; set; } = new();
    }
}

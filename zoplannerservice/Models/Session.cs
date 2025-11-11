namespace zoplannerservice.Models
{
    public class Session
    {
        public long Id { get; set; }
        public long? AssignmentId { get; set; }  // Nullable 
        public string? TimeStart { get; set; }    // String to accept Spring Boot format "2025-11-16 10:00"
        public string? TimeEnd { get; set; }      // String to accept Spring Boot format "2025-11-16 12:00"
    }
}
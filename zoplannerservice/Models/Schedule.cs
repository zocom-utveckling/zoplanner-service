namespace zoplannerservice.Models
{
    public class Schedule
    {
        public long Id { get; set; }
        public long? AssignmentId { get; set; }  // Nullable - может быть null
        public DateTime? TimeStart { get; set; }  // Nullable - может быть null
        public DateTime? TimeEnd { get; set; }    // Nullable - может быть null

    }
}
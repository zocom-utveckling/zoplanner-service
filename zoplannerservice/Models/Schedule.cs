namespace zoplannerservice.Modals
{
    public class Schedule
    {
        public long Id { get; set; }
        public long AssignmentId { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }

    }
}
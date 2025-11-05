namespace zoplannerservice.Models
{
    public class Assignment
    {
       public long Id { get; set; }
        public string CourseName { get; set; }  = string.Empty;
        public long? ConsultantId { get; set; }  // Nullable - может быть null
        public DateTime? DateStart { get; set; }  // Nullable - может быть null
        public DateTime? DateEnd { get; set; }    // Nullable - может быть null
        public long? ClassId { get; set; }       // Nullable - может быть null
       
    }
}
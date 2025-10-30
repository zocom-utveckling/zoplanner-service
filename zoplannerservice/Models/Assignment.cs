namespace zoplannerservice.Models
{
    public class Assignment
    {
       public long Id { get; set; }
        public string CourseName { get; set; }  = string.Empty;
        public long ConsultantId { get; set; }
        public long DateStart { get; set; }
        public long DateEnd { get; set; }
        public long ClassId { get; set; } 
       
    }
}
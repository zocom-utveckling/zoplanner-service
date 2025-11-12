using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models
{
    public class Assignment
    {
       
        public long Id { get; set; }

        [Required]
         public string CourseName { get; set; } = string.Empty;
        public long? ConsultantId { get; set; }  
        
        [Required]
        public DateTime? DateStart { get; set; }  
        
        [Required]
        public DateTime? DateEnd { get; set; }    
        public long? ClassId { get; set; }   

        public List<Session>? Sessions { get; set; } 
    }
}
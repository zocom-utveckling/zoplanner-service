using System.ComponentModel.DataAnnotations;

namespace zoplannerservice.Models
{
    public class Class
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? CustomerId { get; set; }  // Nullable - может быть null

    }
}
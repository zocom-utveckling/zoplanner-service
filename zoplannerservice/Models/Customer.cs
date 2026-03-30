namespace zoplannerservice.Models
{
    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public long ManagerId { get; set; }
       
    }
}
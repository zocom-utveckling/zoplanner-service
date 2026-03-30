namespace zoplannerservice.Models
{
    public class UpdateCustomerRequest
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public long? ManagerId { get; set; }
    }
}

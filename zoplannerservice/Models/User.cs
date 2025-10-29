namespace zoplannerservice.Modals
{
    public class UserController
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

         public ICollection<Assignment>? Assignments { get; set; }
    }
}

    

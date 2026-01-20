using zoplannerservice.Enums.ConsultantStatus;
using zoplannerservice.Enums.UserRole;

namespace zoplannerservice.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string City { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public ConsultantStatus Availability { get; set; }
    }
}



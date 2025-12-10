using Microsoft.AspNetCore.Http.HttpResults;
using zoplannerservice.Enums.ConsultantStatus;
using zoplannerservice.Enums.UserRole;

namespace zoplannerservice.Models
{
    public class Consultant
    {
        public long Id { get; set; }
        
        public long UserId { get; set; }
        public long? ManagerId { get; set; }
        public string City { get; set; } = string.Empty;
    }
}

//  TABLE consultants (
//	    id BIGSERIAL PRIMARY KEY,
//      user_id BIGINT UNIQUE NOT NULL,
//      manager_id BIGINT NULL,
//      city VARCHAR(255) NOT NULL,
//      FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
//      FOREIGN KEY (manager_id) REFERENCES managers(id) ON DELETE SET NULL
//  );

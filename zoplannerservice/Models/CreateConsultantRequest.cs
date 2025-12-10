
using System.ComponentModel.DataAnnotations;


namespace zoplannerservice.Models
{
    public class CreateConsultantRequest
    {
        [Required]
        public long UserId { get; set; }
        [Required]
        public long ManagerId { get; set; }
        [Required]
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

namespace zoplannerservice.Models
{
    public class Manager
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public List<Consultant>? Consultants { get; set; }
        public List<Customer>? Customers { get; set; }
    }
}

//TABLE managers (
//	id BIGSERIAL PRIMARY KEY,
//    user_id BIGINT UNIQUE NOT NULL,
//    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
//);

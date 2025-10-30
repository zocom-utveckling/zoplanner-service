using zoplannerservice.Models;

namespace zoplannerservice.Services
{
    public class UserService
    {
        // list to store users (Mock)
        private List<User> users = new List<User>
        {
            new User { Id = 1, Username = "chris", Password = "pass123", Role = "Admin", City = "Oslo", Name = "Chris Hemsworth" },
            new User { Id = 2, Username = "Henry", Password = "pass456", Role = "Consultant", City = "Michigan", Name = "Henry Cavill" }
        };

        // Get all users
        public List<User> GetAllUsers()
        {
            return users;
        }

        // Get user by id
        public User GetUserById(long id)
        {
            foreach (var user in users)
            {
                if (user.Id == id)
                {
                    return user;
                }
            }
            return null;
        }

        // Create new user
        public User CreateUser(User user)
        {
            users.Add(user);
            return user;
        }

        // Update user
        public User UpdateUser(long id, User user)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    users[i] = user;
                    return user;
                }
            }
            return null;
        }

        // Delete user
        public void DeleteUser(long id)
        {
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].Id == id)
                {
                    users.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
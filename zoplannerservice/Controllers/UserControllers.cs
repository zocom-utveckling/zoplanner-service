using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Models;
using zoplannerservice.Services;

namespace zoplannerservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }
      
        [HttpGet("test")]
        public IActionResult TestEndPoint()
        {
            return Ok(new { message = "UserController test endpoint fungera!" });
        }

        // GET: api/user
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public IActionResult GetUserById(long id)
        {
            var user = _userService.GetUserById(id);
            return Ok(user);
        }

        // POST: api/user
        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            var createdUser = _userService.CreateUser(user);
            return Ok(createdUser);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateUser(long id, [FromBody] User user)
        {
            var updatedUser = _userService.UpdateUser(id, user);
            return Ok(updatedUser);
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(long id)
        {
            _userService.DeleteUser(id);
            return Ok(new { message = "User deleted" });
        }
    }
}
    


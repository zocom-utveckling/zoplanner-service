using Microsoft.AspNetCore.Mvc;

namespace zoplannerservice.Users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "UserController works!" });
        }
    }
}

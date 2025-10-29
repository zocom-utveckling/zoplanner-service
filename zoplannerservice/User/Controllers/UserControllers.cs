using Microsoft.AspNetCore.Mvc;

namespace zoplannerservice.Users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        [HttpGet("test")]
        public IActionResult TestEndPoints()
        {
            return Ok(new { message = "UserController test endpoints funkar!" });
        }
    }
}

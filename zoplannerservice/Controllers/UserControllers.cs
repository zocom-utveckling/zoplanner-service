using Microsoft.AspNetCore.Mvc;

namespace zoplannerservice.Controllers
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

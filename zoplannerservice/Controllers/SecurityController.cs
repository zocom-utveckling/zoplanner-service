using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace zoplannerservice.Controllers;

[ApiController]
[Route("api")]
public class SecurityController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var username = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new
        {
            id = userId,
            username,
            role
        });
    }

    [Authorize(Policy = "ManagerOnly")]
    [HttpGet("manager-test")]
    public IActionResult ManagerOnly()
    {
        return Ok(new
        {
            message = "Du är inloggad som MANAGER och har åtkomst till denna endpoint."
        });
    }

    [Authorize(Policy = "ManagerOrConsultant")]
    [HttpGet("staff-test")]
    public IActionResult StaffOnly()
    {
        return Ok(new
        {
            message = "MANAGER, CONSULTANT eller BOTH har åtkomst."
        });
    }
}
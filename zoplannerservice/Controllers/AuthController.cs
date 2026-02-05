using Microsoft.AspNetCore.Mvc;
using zoplannerservice.Models.View;

namespace zoplannerservice.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {

        var result = await _authService.LoginAsync(model, ct);

        if (!result.Success)
        {
            return Unauthorized();
        }
        return Ok(new { success = true, result.User!.Username, result.User.Role, token = result.Token });
    }

    // [Authorize(Policy ="Admin")] För framtiden när auktorisering behövs.
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(RegisterViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return ValidationProblem();

        var success = await _authService.RegisterAsync(model, ct);

        if (!success)
        {
            return BadRequest(new { success = false });
        }
        return StatusCode(201);
    }
}
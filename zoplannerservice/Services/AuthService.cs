using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ganss.Xss;
using Microsoft.IdentityModel.Tokens;
using zoplannerservice.Models;
using zoplannerservice.Models.View;

namespace zoplannerservice.Services;

public class AuthService : IAuthService
{

    private readonly IUserService _userService;

    private readonly string _jwtSecret;

    public AuthService(IUserService userService)
    {
        _userService = userService;
        _jwtSecret = Environment.GetEnvironmentVariable("TOKENKEY")
        ?? throw new Exception("JWT Key saknas.");
    }
    public async Task<(bool Success, string? Token, User? User)> LoginAsync(LoginViewModel model, CancellationToken ct)
    {
        var user = await _userService.GetByUsernameAsync(model.Username, ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            return (false, null, null);
        }

        var token = CreateToken(user);
        return (true, token, user);
    }

    public async Task<bool> RegisterAsync(RegisterViewModel model, CancellationToken ct)
    {
        try
        {
            var sanitizer = new HtmlSanitizer();

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var createRequest = new CreateUserRequest
            {
                Username = sanitizer.Sanitize(model.Username),
                Email = sanitizer.Sanitize(model.Email),
                Password = passwordHash,
                City = model.City,
                Name = model.Name,
                Role = model.Role,
            };


            var user = await _userService.CreateAsync(createRequest, ct);

            return user != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error on registration: {ex.Message}");
            return false;
        }
    }

    /* JWT Logik */
    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role.ToString()),

        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

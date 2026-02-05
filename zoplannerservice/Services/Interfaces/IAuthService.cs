using zoplannerservice.Models;
using zoplannerservice.Models.View;

public interface IAuthService
{
    Task<(bool Success, string? Token, User? User)> LoginAsync(LoginViewModel model, CancellationToken ct);
    Task<bool> RegisterAsync(RegisterViewModel model, CancellationToken ct);
}
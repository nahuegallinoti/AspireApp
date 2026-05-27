using AspireApp.Application.Models.Auth;
using AspireApp.Application.Models.Auth.User;
using AspireApp.Application.Models.Users;
using AspireApp.Domain.ROP;

namespace AspireApp.Application.Contracts.Auth;

public interface IAuthService
{
    Task<Result<AuthenticationResult>> LoginAsync(UserLogin login, string? ip, CancellationToken ct);

    Task<Result<AuthenticationResult>> RegisterAsync(UserRegister userRegister, string? ip, CancellationToken ct);

    Task<Result<AuthenticationResult>> RefreshAsync(RefreshTokenRequest request, string? ip, CancellationToken ct);

    Task<Result<Unit>> LogoutAsync(LogoutRequest request, string? ip, CancellationToken ct);

    Task<Result<UserDto>> GetCurrentAsync(Guid userId, CancellationToken ct);
}

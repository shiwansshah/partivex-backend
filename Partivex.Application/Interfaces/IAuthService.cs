using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult<AuthResponse>> LoginAsync(LoginCommand command);

    Task<AuthResult<AuthResponse>> RegisterAsync(RegisterCommand command);

    Task<AuthResult<UserCreatedResponse>> CreateUserAsync(CreateUserCommand command);
}

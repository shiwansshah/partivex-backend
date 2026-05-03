using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Application.Constants; // Imports role constants.
using Partivex.Domain.Entities;

namespace Partivex.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResult<AuthResponse>> LoginAsync(LoginCommand command)
    {
        var user = await _userRepository.FindByEmailAsync(command.Email);
        if (user is null || !await _userRepository.CheckPasswordAsync(user, command.Password))
        {
            return AuthResult<AuthResponse>.Unauthorized();
        }

        var roles = await _userRepository.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return AuthResult<AuthResponse>.Success(new AuthResponse(token));
    }

    public async Task<AuthResult<AuthResponse>> RegisterAsync(RegisterCommand command)
    {
        var user = CreateApplicationUser(command.FullName, command.Email);

        var errors = await _userRepository.CreateAsync(user, command.Password);
        if (errors.Count > 0)
        {
            return AuthResult<AuthResponse>.Failed(errors);
        }

        var roleErrors = await _userRepository.AddToRoleAsync(user, ApplicationRoles.Customer);
        if (roleErrors.Count > 0)
        {
            await _userRepository.DeleteAsync(user);
            return AuthResult<AuthResponse>.Failed(roleErrors);
        }

        var token = _jwtService.GenerateToken(user, [ApplicationRoles.Customer]);
        return AuthResult<AuthResponse>.Success(new AuthResponse(token));
    }

    public async Task<AuthResult<UserCreatedResponse>> CreateUserAsync(CreateUserCommand command)
    {
        var user = CreateApplicationUser(command.FullName, command.Email);

        var errors = await _userRepository.CreateAsync(user, command.Password);
        if (errors.Count > 0)
        {
            return AuthResult<UserCreatedResponse>.Failed(errors);
        }

        var roleErrors = await _userRepository.AddToRoleAsync(user, command.Role);
        if (roleErrors.Count > 0)
        {
            await _userRepository.DeleteAsync(user);
            return AuthResult<UserCreatedResponse>.Failed(roleErrors);
        }

        return AuthResult<UserCreatedResponse>.Success(new UserCreatedResponse(user.Id, user.Email!, command.Role));
    }

    private static ApplicationUser CreateApplicationUser(string fullName, string email)
    {
        return new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName
        };
    }
}

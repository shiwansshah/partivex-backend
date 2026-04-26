using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string AdminRole = "Admin";
    private const string StaffRole = "Staff";
    private const string CustomerRole = "Customer";

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(new LoginCommand(request.Email, request.Password));
        if (result.IsUnauthorized)
        {
            return Unauthorized();
        }

        return Ok(result.Value);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(new RegisterCommand(request.FullName, request.Email, request.Password));
        if (!result.Succeeded)
        {
            return ToValidationProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("create-staff")]
    [Authorize(Roles = AdminRole)]
    public async Task<ActionResult<UserCreatedResponse>> CreateStaff(CreateUserRequest request)
    {
        return await CreateUser(request, StaffRole);
    }

    [HttpPost("create-customer")]
    [Authorize(Roles = StaffRole)]
    public async Task<ActionResult<UserCreatedResponse>> CreateCustomer(CreateUserRequest request)
    {
        return await CreateUser(request, CustomerRole);
    }

    [HttpGet("profile")]
    [Authorize]
    public IActionResult Profile()
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            FullName = User.FindFirstValue(ClaimTypes.Name),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value)
        });
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = AdminRole)]
    public IActionResult AdminOnly()
    {
        return Ok(new { Message = "Admin access granted." });
    }

    [HttpGet("staff-only")]
    [Authorize(Roles = StaffRole)]
    public IActionResult StaffOnly()
    {
        return Ok(new { Message = "Staff access granted." });
    }

    private async Task<ActionResult<UserCreatedResponse>> CreateUser(CreateUserRequest request, string role)
    {
        var result = await _authService.CreateUserAsync(new CreateUserCommand(request.FullName, request.Email, request.Password, role));
        if (!result.Succeeded)
        {
            return ToValidationProblem(result);
        }

        return Ok(result.Value);
    }

    private ActionResult ToValidationProblem<T>(AuthResult<T> result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem(ModelState);
    }
}

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed class RegisterRequest
{
    [Required]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed class CreateUserRequest
{
    [Required]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

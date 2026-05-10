using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants; // Imports role constants.
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")] // Handles login.
    [AllowAnonymous] // Allows public login.
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)] // Documents login success.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents login failure.
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request) // Logs user in.
    {
        var result = await _authService.LoginAsync(new LoginCommand(request.Email, request.Password));
        if (result.IsUnauthorized)
        {
            return Unauthorized();
        }

        return Ok(result.Value);
    }

    [HttpPost("register")] // Handles registration.
    [AllowAnonymous] // Allows public registration.
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)] // Documents register success.
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documents validation failure.
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request) // Registers user.
    {
        var result = await _authService.RegisterAsync(new RegisterCommand(request.FullName, request.Email, request.Password));
        if (!result.Succeeded)
        {
            return ToValidationProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("create-staff")] // Handles staff creation.
    [Authorize(Roles = ApplicationRoles.Admin)] // Restricts staff creation.
    [ProducesResponseType(typeof(UserCreatedResponse), StatusCodes.Status200OK)] // Documents create success.
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documents validation failure.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    public async Task<ActionResult<UserCreatedResponse>> CreateStaff([FromBody] CreateUserRequest request) // Creates staff.
    {
        return await CreateUser(request, ApplicationRoles.Staff);
    }

    [HttpPost("create-customer")] // Handles customer creation.
    [Authorize(Roles = ApplicationRoles.AdminAndStaff)] // Allows admin to do everything staff can do.
    [ProducesResponseType(typeof(UserCreatedResponse), StatusCodes.Status200OK)] // Documents create success.
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // Documents validation failure.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    public async Task<ActionResult<UserCreatedResponse>> CreateCustomer([FromBody] CreateUserRequest request) // Creates customer.
    {
        return await CreateUser(request, ApplicationRoles.Customer);
    }

    [HttpGet("profile")] // Handles profile lookup.
    [Authorize] // Requires authentication.
    [ProducesResponseType(StatusCodes.Status200OK)] // Documents profile success.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    public IActionResult Profile() // Returns current profile.
    {
        return Ok(new
        {
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            FullName = User.FindFirstValue(ClaimTypes.Name),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value)
        });
    }

    [HttpGet("admin-only")] // Handles admin check.
    [Authorize(Roles = ApplicationRoles.Admin)] // Requires admin role.
    [ProducesResponseType(StatusCodes.Status200OK)] // Documents access success.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    public IActionResult AdminOnly() // Returns admin result.
    {
        return Ok(new { Message = "Admin access granted." });
    }

    [HttpGet("staff-only")] // Handles staff check.
    [Authorize(Roles = ApplicationRoles.Staff)] // Requires staff role.
    [ProducesResponseType(StatusCodes.Status200OK)] // Documents access success.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    public IActionResult StaffOnly() // Returns staff result.
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

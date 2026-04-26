using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Controllers;

[ApiController]
[Route("")]
public class AuthController : ControllerBase
{
    private const string AdminRole = "Admin";
    private const string StaffRole = "Staff";
    private const string CustomerRole = "Customer";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthController(UserManager<ApplicationUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user, roles);

        return Ok(new AuthResponse(token));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ToValidationProblem(result);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, CustomerRole);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return ToValidationProblem(roleResult);
        }

        var token = _jwtService.GenerateToken(user, [CustomerRole]);
        return Ok(new AuthResponse(token));
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
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return ToValidationProblem(result);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return ToValidationProblem(roleResult);
        }

        return Ok(new UserCreatedResponse(user.Id, user.Email!, role));
    }

    private ActionResult ToValidationProblem(IdentityResult result)
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

public sealed record AuthResponse(string Token);

public sealed record UserCreatedResponse(string UserId, string Email, string Role);

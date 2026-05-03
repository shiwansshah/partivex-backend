using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customer")]
[Authorize(Roles = ApplicationRoles.Customer)]
public class CustomerController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public CustomerController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<CustomerProfileDto>> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token.", errors = Array.Empty<string>() });
        }

        var email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
        var user = await _userRepository.FindByEmailAsync(email);

        if (user is null)
        {
            return NotFound(new { success = false, message = "Customer not found.", errors = Array.Empty<string>() });
        }

        var profile = new CustomerProfileDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.PhoneNumber
        );

        return Ok(profile);
    }
}

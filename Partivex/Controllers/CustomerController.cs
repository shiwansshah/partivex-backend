using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customer")]
[Authorize(Roles = ApplicationRoles.Customer)]
public class CustomerController : ControllerBase
{
    private const string NepaliMobilePattern = @"^(?:\+977|977)?9[78]\d{8}$";

    private readonly AppDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public CustomerController(
        AppDbContext dbContext,
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _userManager = userManager;
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

    [HttpPut("profile")]
    public async Task<ActionResult<CustomerProfileDto>> UpdateProfile([FromBody] UpdateCustomerProfileDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { success = false, message = "Invalid token.", errors = Array.Empty<string>() });
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return NotFound(new { success = false, message = "Customer not found.", errors = Array.Empty<string>() });
        }

        var fullName = request.FullName.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return BadRequest(new { success = false, message = "Full name is required.", errors = Array.Empty<string>() });
        }

        var normalizedPhone = NormalizeNepaliPhoneNumber(request.PhoneNumber);

        if (normalizedPhone is null && !string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            return BadRequest(new { success = false, message = "Enter a valid Nepali mobile number.", errors = Array.Empty<string>() });
        }

        if (!string.IsNullOrEmpty(normalizedPhone))
        {
            var phoneExists = await _dbContext.Users
                .AnyAsync(existingUser => existingUser.Id != user.Id && existingUser.PhoneNumber == normalizedPhone);

            if (phoneExists)
            {
                return Conflict(new { success = false, message = "Phone number is already in use.", errors = Array.Empty<string>() });
            }
        }

        user.FullName = fullName;
        user.PhoneNumber = normalizedPhone;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            return BadRequest(new
            {
                success = false,
                message = "Profile update failed.",
                errors = updateResult.Errors.Select(error => error.Description)
            });
        }

        var profile = new CustomerProfileDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.PhoneNumber
        );

        return Ok(profile);
    }

    private static string? NormalizeNepaliPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("977"))
        {
            digits = digits[3..];
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(digits, NepaliMobilePattern))
        {
            return null;
        }

        return $"+977{digits}";
    }
}

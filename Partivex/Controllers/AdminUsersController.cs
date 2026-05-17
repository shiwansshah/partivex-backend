using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = ApplicationRoles.Admin)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet("users-with-roles")]
    public async Task<ActionResult<IReadOnlyCollection<UserWithRoleDto>>> GetUsersWithRoles(
        CancellationToken cancellationToken)
    {
        var users = await _adminUserService.GetUsersWithRolesAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPut("users/{userId}/role")]
    public async Task<ActionResult<UserWithRoleDto>> UpdateUserRole(
        string userId,
        [FromBody] UpdateUserRoleDto dto,
        CancellationToken cancellationToken)
    {
        var user = await _adminUserService.UpdateUserRoleAsync(userId, dto, cancellationToken);

        return Ok(user);
    }
}

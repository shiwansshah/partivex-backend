using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = ApplicationRoles.Admin)]
public sealed class AdminPermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public AdminPermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet("permissions")]
    public async Task<ActionResult<IReadOnlyCollection<PermissionDto>>> GetPermissions(
        CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetPermissionsAsync(cancellationToken);

        return Ok(permissions);
    }

    [HttpGet("roles/{roleName}/permissions")]
    public async Task<ActionResult<RolePermissionsDto>> GetRolePermissions(
        string roleName,
        CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetRolePermissionsAsync(roleName, cancellationToken);

        return Ok(permissions);
    }

    [HttpPost("roles/{roleName}/permissions")]
    public async Task<ActionResult<RolePermissionsDto>> UpdateRolePermissions(
        string roleName,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.UpdateRolePermissionsAsync(
            roleName,
            new UpdateRolePermissionsCommand(request.PermissionIds),
            cancellationToken);

        return Ok(permissions);
    }
}

public sealed class UpdateRolePermissionsRequest
{
    public IReadOnlyCollection<int> PermissionIds { get; init; } = [];
}

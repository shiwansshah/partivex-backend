using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly AppDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IActivityLogService _activityLogService;

    public PermissionService(
        AppDbContext dbContext,
        RoleManager<IdentityRole> roleManager,
        IActivityLogService activityLogService)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
        _activityLogService = activityLogService;
    }

    public async Task<IReadOnlyCollection<PermissionDto>> GetPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Name)
            .ToArrayAsync(cancellationToken);

        return permissions.Select(MapPermission).ToArray();
    }

    public async Task<RolePermissionsDto> GetRolePermissionsAsync(
        string roleName,
        CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = await GetExistingRoleNameAsync(roleName, cancellationToken);

        return await BuildRolePermissionsDtoAsync(normalizedRoleName, cancellationToken);
    }

    public async Task<RolePermissionsDto> UpdateRolePermissionsAsync(
        string roleName,
        UpdateRolePermissionsCommand command,
        CancellationToken cancellationToken = default)
    {
        var normalizedRoleName = await GetExistingRoleNameAsync(roleName, cancellationToken);
        var requestedPermissionIds = command.PermissionIds.Distinct().ToArray();
        var existingPermissionIds = await _dbContext.Permissions
            .Where(permission => requestedPermissionIds.Contains(permission.Id))
            .Select(permission => permission.Id)
            .ToArrayAsync(cancellationToken);

        if (existingPermissionIds.Length != requestedPermissionIds.Length)
        {
            throw new ArgumentException("One or more selected permissions do not exist.");
        }

        var currentAssignments = await _dbContext.RolePermissions
            .Where(rolePermission => rolePermission.RoleName == normalizedRoleName)
            .ToArrayAsync(cancellationToken);

        _dbContext.RolePermissions.RemoveRange(currentAssignments);

        foreach (var permissionId in requestedPermissionIds)
        {
            await _dbContext.RolePermissions.AddAsync(
                new RolePermission
                {
                    RoleName = normalizedRoleName,
                    PermissionId = permissionId,
                    CreatedAt = DateTimeOffset.UtcNow
                },
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(
            new CreateActivityLogCommand(
                "UpdateRolePermissions",
                "RolePermission",
                normalizedRoleName,
                $"Admin updated permissions for role {normalizedRoleName}."),
            cancellationToken);

        return await BuildRolePermissionsDtoAsync(normalizedRoleName, cancellationToken);
    }

    private async Task<string> GetExistingRoleNameAsync(
        string roleName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var trimmedRoleName = roleName.Trim();

        if (string.IsNullOrWhiteSpace(trimmedRoleName))
        {
            throw new ArgumentException("Role name is required.");
        }

        var role = await _roleManager.FindByNameAsync(trimmedRoleName);

        if (role is null)
        {
            throw new KeyNotFoundException("Role was not found.");
        }

        return role.Name ?? trimmedRoleName;
    }

    private async Task<RolePermissionsDto> BuildRolePermissionsDtoAsync(
        string roleName,
        CancellationToken cancellationToken)
    {
        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.Name)
            .ToArrayAsync(cancellationToken);
        var assignedPermissionIds = await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(rolePermission => rolePermission.RoleName == roleName)
            .Select(rolePermission => rolePermission.PermissionId)
            .ToArrayAsync(cancellationToken);
        var assigned = assignedPermissionIds.ToHashSet();

        var permissionDtos = permissions
            .Select(permission => new RolePermissionDto(
                permission.Id,
                permission.Name,
                permission.Description,
                assigned.Contains(permission.Id)))
            .ToArray();

        return new RolePermissionsDto(roleName, permissionDtos);
    }

    private static PermissionDto MapPermission(Permission permission)
    {
        return new PermissionDto(
            permission.Id,
            permission.Name,
            permission.Description,
            permission.CreatedAt);
    }
}

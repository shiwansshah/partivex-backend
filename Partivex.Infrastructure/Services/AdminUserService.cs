using Microsoft.AspNetCore.Identity;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Services;

public sealed class AdminUserService : IAdminUserService
{
    private static readonly string[] ManagedRoles = [ApplicationRoles.Admin, ApplicationRoles.Staff];

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IActivityLogService _activityLogService;

    public AdminUserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ICurrentUserContext currentUserContext,
        IActivityLogService activityLogService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _currentUserContext = currentUserContext;
        _activityLogService = activityLogService;
    }

    public async Task<IReadOnlyCollection<UserWithRoleDto>> GetUsersWithRolesAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var admins = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
        var staff = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Staff);

        return admins
            .Select(user => MapToDto(user, ApplicationRoles.Admin))
            .Concat(staff.Select(user => MapToDto(user, ApplicationRoles.Staff)))
            .GroupBy(user => user.Id)
            .Select(group => group.First())
            .OrderBy(user => user.FullName)
            .ThenBy(user => user.Email)
            .ToArray();
    }

    public async Task<UserWithRoleDto> UpdateUserRoleAsync(
        string userId,
        UpdateUserRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var targetRole = NormalizeManagedRole(dto.Role);
        await EnsureRoleExistsAsync(targetRole);

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var currentManagedRole = currentRoles.FirstOrDefault(ManagedRoles.Contains);

        if (currentManagedRole is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        if (currentManagedRole == targetRole)
        {
            return MapToDto(user, targetRole);
        }

        if (currentManagedRole == ApplicationRoles.Admin && targetRole == ApplicationRoles.Staff)
        {
            await EnsureAdminCanBeDowngradedAsync(user);
        }

        var removeResult = await _userManager.RemoveFromRoleAsync(user, currentManagedRole);

        if (!removeResult.Succeeded)
        {
            throw new InvalidOperationException(ToErrorMessage(removeResult));
        }

        var addResult = await _userManager.AddToRoleAsync(user, targetRole);

        if (!addResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, currentManagedRole);

            throw new InvalidOperationException(ToErrorMessage(addResult));
        }

        await _activityLogService.LogAsync(
            new CreateActivityLogCommand(
                "ChangeUserRole",
                "ApplicationUser",
                user.Id,
                $"Admin changed role for {user.Email} from {currentManagedRole} to {targetRole}."),
            cancellationToken);

        return MapToDto(user, targetRole);
    }

    private async Task EnsureAdminCanBeDowngradedAsync(ApplicationUser user)
    {
        var admins = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
        var isOnlyAdmin = admins.Count == 1;

        if (isOnlyAdmin)
        {
            var isCurrentAdmin = user.Id == _currentUserContext.UserId;
            var message = isCurrentAdmin
                ? "You cannot remove your own Admin role because you are the only Admin."
                : "Cannot remove the last Admin account.";

            throw new InvalidOperationException(message);
        }
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            throw new InvalidOperationException($"{roleName} role is not configured.");
        }
    }

    private static string NormalizeManagedRole(string role)
    {
        var normalizedRole = role.Trim();

        if (!ManagedRoles.Contains(normalizedRole))
        {
            throw new ArgumentException("Role must be Admin or Staff.");
        }

        return normalizedRole;
    }

    private static UserWithRoleDto MapToDto(ApplicationUser user, string role)
    {
        return new UserWithRoleDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            role);
    }

    private static string ToErrorMessage(IdentityResult result)
    {
        return string.Join(" ", result.Errors.Select(error => error.Description));
    }
}

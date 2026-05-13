using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyCollection<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default);

    Task<RolePermissionsDto> GetRolePermissionsAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    Task<RolePermissionsDto> UpdateRolePermissionsAsync(
        string roleName,
        UpdateRolePermissionsCommand command,
        CancellationToken cancellationToken = default);
}

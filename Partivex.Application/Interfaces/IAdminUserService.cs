using Partivex.Application.DTOs;

namespace Partivex.Application.Interfaces;

public interface IAdminUserService
{
    Task<IReadOnlyCollection<UserWithRoleDto>> GetUsersWithRolesAsync(
        CancellationToken cancellationToken = default);

    Task<UserWithRoleDto> UpdateUserRoleAsync(
        string userId,
        UpdateUserRoleDto dto,
        CancellationToken cancellationToken = default);
}

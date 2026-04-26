using Partivex.Application.DTOs;
using Partivex.Domain.Entities;

namespace Partivex.Application.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);

    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);

    Task<IReadOnlyList<string>> GetRolesAsync(ApplicationUser user);

    Task<IReadOnlyCollection<AuthError>> CreateAsync(ApplicationUser user, string password);

    Task<IReadOnlyCollection<AuthError>> AddToRoleAsync(ApplicationUser user, string role);

    Task DeleteAsync(ApplicationUser user);
}

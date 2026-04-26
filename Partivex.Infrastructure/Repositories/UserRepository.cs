using Microsoft.AspNetCore.Identity;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        return _userManager.FindByEmailAsync(email);
    }

    public Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(ApplicationUser user)
    {
        return [.. await _userManager.GetRolesAsync(user)];
    }

    public async Task<IReadOnlyCollection<AuthError>> CreateAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return ToErrors(result);
    }

    public async Task<IReadOnlyCollection<AuthError>> AddToRoleAsync(ApplicationUser user, string role)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        return ToErrors(result);
    }

    public async Task DeleteAsync(ApplicationUser user)
    {
        await _userManager.DeleteAsync(user);
    }

    private static IReadOnlyCollection<AuthError> ToErrors(IdentityResult result)
    {
        return result.Succeeded
            ? []
            : result.Errors.Select(error => new AuthError(error.Code, error.Description)).ToArray();
    }
}

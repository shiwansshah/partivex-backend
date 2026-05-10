using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Identity;

public static class RoleSeeder
{
    private static readonly string[] Roles = ["Admin", "Staff", "Customer"];

    private static readonly SeedUser[] Users =
    [
        new("Aayush Admin", "aayush@gmail.com", "Admin@123", "Admin"),
        new("Partivex Staff", "staff111@gmail.com", "Staff@12345", "Staff")
    ];

    public static async Task SeedRolesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        foreach (var seedUser in Users)
        {
            await EnsureUserAsync(userManager, seedUser);
        }
    }

    private static async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, SeedUser seedUser)
    {
        var user = await userManager.FindByEmailAsync(seedUser.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                FullName = seedUser.FullName,
                UserName = seedUser.Email,
                Email = seedUser.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, seedUser.Password);
            ThrowIfFailed(createResult, $"create {seedUser.Role} user {seedUser.Email}");
        }
        else
        {
            user.FullName = seedUser.FullName;
            user.UserName = seedUser.Email;
            user.Email = seedUser.Email;
            user.EmailConfirmed = true;

            var updateResult = await userManager.UpdateAsync(user);
            ThrowIfFailed(updateResult, $"update {seedUser.Role} user {seedUser.Email}");

            if (!await userManager.CheckPasswordAsync(user, seedUser.Password))
            {
                var removePasswordResult = await userManager.RemovePasswordAsync(user);
                ThrowIfFailed(removePasswordResult, $"remove old password for {seedUser.Email}");

                var addPasswordResult = await userManager.AddPasswordAsync(user, seedUser.Password);
                ThrowIfFailed(addPasswordResult, $"set password for {seedUser.Email}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, seedUser.Role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, seedUser.Role);
            ThrowIfFailed(roleResult, $"assign {seedUser.Role} role to {seedUser.Email}");
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string action)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join("; ", result.Errors.Select(error => $"{error.Code}: {error.Description}"));
        throw new InvalidOperationException($"Failed to {action}. {errors}");
    }

    private sealed record SeedUser(string FullName, string Email, string Password, string Role);
}

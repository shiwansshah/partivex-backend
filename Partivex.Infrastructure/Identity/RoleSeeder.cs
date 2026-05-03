using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Partivex.Application.Constants; // Imports role constants.
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role)); // Creates missing role.

                if (!result.Succeeded) // Handles seed failure.
                {
                    throw new InvalidOperationException(ToErrorMessage(role, result)); // Stops startup on seed error.
                }
            }
        }

        await SeedUserAsync(
            userManager,
            configuration["SeedUsers:Admin:Email"] ?? "admin@partivex.local",
            configuration["SeedUsers:Admin:Password"] ?? "Admin@12345",
            configuration["SeedUsers:Admin:FullName"] ?? "Partivex Admin",
            ApplicationRoles.Admin);

        await SeedUserAsync(
            userManager,
            configuration["SeedUsers:Staff:Email"] ?? "staff@partivex.local",
            configuration["SeedUsers:Staff:Password"] ?? "Staff@12345",
            configuration["SeedUsers:Staff:FullName"] ?? "Partivex Staff",
            ApplicationRoles.Staff);
    }

    private static string ToErrorMessage(string role, IdentityResult result) // Formats role errors.
    {
        var errors = string.Join(" ", result.Errors.Select(error => error.Description)); // Joins error text.

        return $"Failed to seed role '{role}'. {errors}"; // Returns startup error.
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        var normalizedEmail = email.Trim();
        var user = await userManager.FindByEmailAsync(normalizedEmail);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = normalizedEmail,
                Email = normalizedEmail,
                EmailConfirmed = true,
                FullName = fullName.Trim()
            };

            var createResult = await userManager.CreateAsync(user, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(ToErrorMessage(role, createResult));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(ToErrorMessage(role, roleResult));
            }
        }
    }
}

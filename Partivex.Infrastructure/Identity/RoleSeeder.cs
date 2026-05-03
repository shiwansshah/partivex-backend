using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Partivex.Application.Constants; // Imports role constants.

namespace Partivex.Infrastructure.Identity;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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
    }

    private static string ToErrorMessage(string role, IdentityResult result) // Formats role errors.
    {
        var errors = string.Join(" ", result.Errors.Select(error => error.Description)); // Joins error text.

        return $"Failed to seed role '{role}'. {errors}"; // Returns startup error.
    }
}

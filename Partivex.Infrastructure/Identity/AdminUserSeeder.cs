using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Partivex.Domain.Entities;

namespace Partivex.Infrastructure.Identity;

public static class AdminUserSeeder
{
    public const string AdminId = "admin-seed-001";
    public const string AdminFullName = "Partivex Admin";
    public const string AdminEmail = "admin@partivex.com";
    public const string AdminPassword = "Admin@12345";

    public static async Task SeedAdminUserAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var existingAdmin = await userManager.FindByEmailAsync(AdminEmail);
        if (existingAdmin is not null)
        {
            existingAdmin.FullName = AdminFullName;
            existingAdmin.UserName = AdminEmail;
            existingAdmin.Email = AdminEmail;
            existingAdmin.EmailConfirmed = true;

            var updateResult = await userManager.UpdateAsync(existingAdmin);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Unable to update seeded admin user. {errors}");
            }

            var resetToken = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
            var passwordResult = await userManager.ResetPasswordAsync(existingAdmin, resetToken, AdminPassword);
            if (!passwordResult.Succeeded)
            {
                var errors = string.Join("; ", passwordResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Unable to reset seeded admin password. {errors}");
            }

            if (!await userManager.IsInRoleAsync(existingAdmin, "Admin"))
            {
                await userManager.AddToRoleAsync(existingAdmin, "Admin");
            }

            return;
        }

        var adminUser = new ApplicationUser
        {
            Id = AdminId,
            FullName = AdminFullName,
            UserName = AdminEmail,
            Email = AdminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Unable to seed admin user. {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        if (!roleResult.Succeeded)
        {
            var errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Unable to assign Admin role to seeded user. {errors}");
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Partivex.Application.Constants;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Data;

namespace Partivex.Infrastructure.Identity;

public static class PermissionSeeder
{
    public static async Task SeedPermissionsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        foreach (var permissionDefinition in PermissionNames.SeedPermissions)
        {
            var exists = await dbContext.Permissions
                .AnyAsync(permission => permission.Name == permissionDefinition.Name);

            if (!exists)
            {
                await dbContext.Permissions.AddAsync(new Permission
                {
                    Name = permissionDefinition.Name,
                    Description = permissionDefinition.Description,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync();

        var permissions = await dbContext.Permissions.ToArrayAsync();

        foreach (var permission in permissions)
        {
            var assigned = await dbContext.RolePermissions.AnyAsync(rolePermission =>
                rolePermission.RoleName == ApplicationRoles.Admin &&
                rolePermission.PermissionId == permission.Id);

            if (!assigned)
            {
                await dbContext.RolePermissions.AddAsync(new RolePermission
                {
                    RoleName = ApplicationRoles.Admin,
                    PermissionId = permission.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync();
    }
}

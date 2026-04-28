using Microsoft.Extensions.DependencyInjection;
using Partivex.Application.Interfaces;
using Partivex.Application.Services;

namespace Partivex.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInventoryService, InventoryService>();

        return services;
    }
}

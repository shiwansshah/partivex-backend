using Microsoft.Extensions.DependencyInjection;
using Partivex.Application.Interfaces;
using Partivex.Application.Services;

namespace Partivex.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<ICustomerAppointmentService, CustomerAppointmentService>();
        services.AddScoped<IPartRequestService, PartRequestService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}

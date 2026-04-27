using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Infrastructure.Authentication;
using Partivex.Infrastructure.Data;
using Partivex.Infrastructure.Repositories;
using Partivex.Infrastructure.Services; // Imports infrastructure services.

namespace Partivex.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var jwtSection = config.GetSection(JwtOptions.SectionName);
        var jwtOptions = new JwtOptions
        {
            Key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT key is missing."),
            Issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("JWT issuer is missing."),
            Audience = jwtSection["Audience"] ?? throw new InvalidOperationException("JWT audience is missing."),
            ExpiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var expiryMinutes) ? expiryMinutes : 60
        };

        services.Configure<JwtOptions>(options =>
        {
            options.Key = jwtOptions.Key;
            options.Issuer = jwtOptions.Issuer;
            options.Audience = jwtOptions.Audience;
            options.ExpiryMinutes = jwtOptions.ExpiryMinutes;
        });

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStaffService, StaffService>(); // Registers staff service.

        return services;
    }
}

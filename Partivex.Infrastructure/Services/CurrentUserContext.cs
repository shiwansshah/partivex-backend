using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Partivex.Application.Interfaces;

namespace Partivex.Infrastructure.Services;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
        GetClaim(ClaimTypes.NameIdentifier) ??
        GetClaim("sub") ??
        string.Empty;

    public string UserName =>
        GetClaim(ClaimTypes.Email) ??
        GetClaim(ClaimTypes.Name) ??
        "System";

    public string Role =>
        GetClaim(ClaimTypes.Role) ??
        "Unknown";

    public string IpAddress =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

    private string? GetClaim(string claimType)
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
    }
}

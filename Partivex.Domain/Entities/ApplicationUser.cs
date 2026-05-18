using Microsoft.AspNetCore.Identity;

namespace Partivex.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public ICollection<CustomerHistory> CustomerHistories { get; set; } = [];
}

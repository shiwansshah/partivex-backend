using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Partivex.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string? Address { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public ICollection<CustomerHistory> CustomerHistories { get; set; } = [];
}

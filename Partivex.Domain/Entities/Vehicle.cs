using System.ComponentModel.DataAnnotations;

namespace Partivex.Domain.Entities;

public class Vehicle
{
    public Guid Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public ApplicationUser Customer { get; set; } = null!;

    public ICollection<CustomerHistory> CustomerHistories { get; set; } = [];
}

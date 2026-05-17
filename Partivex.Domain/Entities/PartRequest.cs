using System.ComponentModel.DataAnnotations;
using Partivex.Domain.Enums;

namespace Partivex.Domain.Entities;

public class PartRequest
{
    public Guid Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public Guid? VehicleId { get; set; }

    [Required]
    [MaxLength(120)]
    public string PartName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? BrandModelSpecification { get; set; }

    public int Quantity { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser Customer { get; set; } = null!;

    public Vehicle? Vehicle { get; set; }
}

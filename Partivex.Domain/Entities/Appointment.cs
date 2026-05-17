using System.ComponentModel.DataAnnotations;
using Partivex.Domain.Enums;

namespace Partivex.Domain.Entities;

public class Appointment
{
    public Guid Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public Guid VehicleId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ServiceType { get; set; } = string.Empty;

    public DateTimeOffset PreferredAt { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser Customer { get; set; } = null!;

    public Vehicle Vehicle { get; set; } = null!;

    public ICollection<Review> Reviews { get; set; } = [];
}

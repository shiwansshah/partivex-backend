using System.ComponentModel.DataAnnotations;
using Partivex.Domain.Enums;

namespace Partivex.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }

    [Required]
    public string CustomerId { get; set; } = string.Empty;

    public Guid? AppointmentId { get; set; }

    public ReviewCategory Category { get; set; } = ReviewCategory.General;

    public int Rating { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser Customer { get; set; } = null!;

    public Appointment? Appointment { get; set; }
}

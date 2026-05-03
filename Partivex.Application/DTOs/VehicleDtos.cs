using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs;

public sealed record VehicleDto(
    Guid Id,
    string Name,
    string Number,
    string? ImageUrl
);

public sealed class CreateVehicleDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Number { get; init; } = string.Empty;
}

public sealed class UpdateVehicleDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Number { get; init; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs;

public sealed record VehicleDto(
    Guid Id,
    string Name,
    string Number,
    string? ImageUrl,
    string CustomerId = ""
);

public sealed class CreateVehicleDto
{
    [MaxLength(100)]
    public string? Name { get; init; }

    [MaxLength(50)]
    public string? Number { get; init; }

    [MaxLength(450)]
    public string? CustomerId { get; init; }

    [MaxLength(50)]
    public string? VehicleNumber { get; init; }

    [MaxLength(100)]
    public string? Model { get; init; }
}

public sealed class UpdateVehicleDto
{
    [MaxLength(100)]
    public string? Name { get; init; }

    [MaxLength(50)]
    public string? Number { get; init; }

    [MaxLength(50)]
    public string? VehicleNumber { get; init; }

    [MaxLength(100)]
    public string? Model { get; init; }
}

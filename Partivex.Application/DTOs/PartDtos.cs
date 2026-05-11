using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs;

public sealed class CreatePartDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string PartCode { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int Stock { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed class UpdatePartDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string PartCode { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue)]
    public int Stock { get; init; }

    public bool IsActive { get; init; }
}

public sealed class PartResponseDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string PartCode { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public int Stock { get; init; }

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
}

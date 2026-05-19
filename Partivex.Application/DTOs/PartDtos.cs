using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs;

public sealed class CreatePartDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string PartCode { get; init; } = string.Empty;

    [Required]
    public string Category { get; init; } = string.Empty;

    public string CompatibleVehicle { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; init; }

    [Range(0, int.MaxValue)]
    public int MinimumStockLevel { get; init; }

    [Range(0, int.MaxValue)]
    public int CurrentStock { get; init; }

    public string ImageUrl { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}

public sealed class UpdatePartDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string PartCode { get; init; } = string.Empty;

    [Required]
    public string Category { get; init; } = string.Empty;

    public string CompatibleVehicle { get; init; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; init; }

    [Range(0, int.MaxValue)]
    public int MinimumStockLevel { get; init; }

    [Range(0, int.MaxValue)]
    public int CurrentStock { get; init; }

    public string ImageUrl { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}

public sealed class PartResponseDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string PartCode { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string CompatibleVehicle { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int MinimumStockLevel { get; init; }

    public int CurrentStock { get; init; }

    public string StockStatus { get; init; } = string.Empty;

    public string ImageUrl { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
}

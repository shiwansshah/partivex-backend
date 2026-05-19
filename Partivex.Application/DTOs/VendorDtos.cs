using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs;

public sealed class CreateVendorDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string ContactPerson { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    [Required]
    public string Address { get; init; } = string.Empty;

    public bool IsActive { get; init; } = true;
}

public sealed class UpdateVendorDto
{
    [Required]
    public string Name { get; init; } = string.Empty;

    [Required]
    public string ContactPerson { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Phone { get; init; } = string.Empty;

    [Required]
    public string Address { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}

public sealed class VendorResponseDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string ContactPerson { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public DateTime CreatedAt { get; init; }
}

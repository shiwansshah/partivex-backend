using Partivex.Application.DTOs.Vehicles;

namespace Partivex.Application.DTOs.Customers;

public class CustomerDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<VehicleDto> Vehicles { get; set; } = [];
}

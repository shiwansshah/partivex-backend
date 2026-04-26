namespace Partivex.Domain.Entities;

public class Vehicle
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string VehicleNumber { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public int? Year { get; set; }

    public string? VehicleType { get; set; }

    public string? Notes { get; set; }

    public Customer Customer { get; set; } = null!;
}

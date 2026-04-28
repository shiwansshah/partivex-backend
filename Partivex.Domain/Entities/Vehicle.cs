namespace Partivex.Domain.Entities; // Defines entity namespace.

public sealed class Vehicle // Defines vehicle entity.
{ // Begins vehicle entity.
    public Guid Id { get; set; } // Stores vehicle identifier.

    public string CustomerId { get; set; } = string.Empty; // Stores customer user id.

    public string VehicleNumber { get; set; } = string.Empty; // Stores vehicle number.

    public string? Model { get; set; } // Stores optional model.
} // Ends vehicle entity.

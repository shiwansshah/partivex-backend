namespace Partivex.Domain.Entities; // Defines entity namespace.

public sealed class CustomerHistory // Defines customer history entity.
{ // Begins customer history entity.
    public Guid Id { get; set; } // Stores history identifier.

    public string CustomerId { get; set; } = string.Empty; // Stores customer user id.

    public string Description { get; set; } = string.Empty; // Stores history description.

    public DateTime CreatedAt { get; set; } // Stores creation timestamp.
} // Ends customer history entity.

using Partivex.Domain.Enums;

namespace Partivex.Domain.Entities; // Defines entity namespace.

public sealed class CustomerHistory // Defines customer history entity.
{ // Begins customer history entity.
    public Guid Id { get; set; } // Stores history identifier.

    public string CustomerId { get; set; } = string.Empty; // Stores customer user id.

    public Guid? VehicleId { get; set; } // Stores optional vehicle id.

    public HistoryType HistoryType { get; set; } // Stores the history type.

    public string Description { get; set; } = string.Empty; // Stores history description.

    public decimal Amount { get; set; } // Stores history amount.

    public PaymentStatus PaymentStatus { get; set; } // Stores payment status.

    public DateTime HistoryDate { get; set; } // Stores history timestamp.

    public ApplicationUser Customer { get; set; } = null!; // Stores customer navigation.

    public Vehicle? Vehicle { get; set; } // Stores vehicle navigation.
} // Ends customer history entity.

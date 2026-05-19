namespace Partivex.Domain.Entities;

public class StaffFeatureAccess
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string FeatureKey { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ApplicationUser Staff { get; set; } = null!;
}

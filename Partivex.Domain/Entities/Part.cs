namespace Partivex.Domain.Entities;

public class Part
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PartCode { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

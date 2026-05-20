namespace Partivex.Domain.Entities;

public class Notification
{
    public int Id { get; set; }

    public string TargetRole { get; set; } = string.Empty;

    public string? TargetUserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; }
}

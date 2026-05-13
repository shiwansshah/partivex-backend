namespace Partivex.Domain.Entities;

public class RolePermission
{
    public int Id { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public int PermissionId { get; set; }

    public Permission Permission { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
}

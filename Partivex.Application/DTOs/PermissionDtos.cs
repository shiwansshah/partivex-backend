namespace Partivex.Application.DTOs;

public sealed record PermissionDto(
    int Id,
    string Name,
    string Description,
    DateTimeOffset CreatedAt);

public sealed record RolePermissionDto(
    int Id,
    string Name,
    string Description,
    bool Assigned);

public sealed record RolePermissionsDto(
    string RoleName,
    IReadOnlyCollection<RolePermissionDto> Permissions);

public sealed record UpdateRolePermissionsCommand(
    IReadOnlyCollection<int> PermissionIds);

using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs;

public sealed record UserWithRoleDto(
    string Id,
    string FullName,
    string Email,
    string Role);

public sealed record UpdateUserRoleDto(
    [Required]
    string Role);

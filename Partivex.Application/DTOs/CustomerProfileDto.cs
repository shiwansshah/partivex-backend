namespace Partivex.Application.DTOs;

public sealed record CustomerProfileDto(
    string Id,
    string FullName,
    string Email,
    string? PhoneNumber
);

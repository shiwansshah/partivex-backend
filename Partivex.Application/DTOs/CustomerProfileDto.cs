namespace Partivex.Application.DTOs;

public sealed record CustomerProfileDto(
    string Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? ProfileImageUrl
);

public sealed record UpdateCustomerProfileDto(
    string FullName,
    string? PhoneNumber
);

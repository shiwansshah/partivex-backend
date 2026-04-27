namespace Partivex.Application.DTOs; // Defines DTO namespace.

public sealed record CreateStaffDto( // Defines staff creation DTO.
    string FullName, // Carries staff full name.
    string Email, // Carries staff email.
    string Password); // Carries staff password.

public sealed record UpdateStaffDto( // Defines staff update DTO.
    string FullName); // Carries updated full name.

public sealed record StaffDto( // Defines staff response DTO.
    string Id, // Carries user identifier.
    string FullName, // Carries staff full name.
    string Email, // Carries staff email.
    string Role); // Carries assigned role.

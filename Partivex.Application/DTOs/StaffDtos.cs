using System.ComponentModel.DataAnnotations; // Imports validation attributes.

namespace Partivex.Application.DTOs; // Defines DTO namespace.

public sealed record CreateStaffDto( // Defines staff creation DTO.
    [Required] // Requires full name.
    [StringLength(256)] // Limits full name length.
    string FullName, // Carries staff full name.
    [Required] // Requires email.
    [EmailAddress] // Validates email format.
    [StringLength(256)] // Limits email length.
    string Email, // Carries staff email.
    [Required] // Requires password.
    [MinLength(6)] // Enforces minimum password length.
    string Password); // Carries staff password.

public sealed record UpdateStaffDto( // Defines staff update DTO.
    [Required] // Requires full name.
    [StringLength(256)] // Limits full name length.
    string FullName); // Carries updated full name.

public sealed record StaffDto( // Defines staff response DTO.
    string Id, // Carries user identifier.
    string FullName, // Carries staff full name.
    string Email, // Carries staff email.
    string Role); // Carries assigned role.

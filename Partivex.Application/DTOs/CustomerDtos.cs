using System.ComponentModel.DataAnnotations; // Imports validation attributes.

namespace Partivex.Application.DTOs; // Defines DTO namespace.

public sealed record CustomerDto( // Defines customer list DTO.
    [Required] // Requires customer id.
    [StringLength(450)] // Limits customer id length.
    string Id, // Carries customer id.
    [Required] // Requires full name.
    [StringLength(256)] // Limits full name length.
    string FullName, // Carries customer full name.
    [Required] // Requires email.
    [EmailAddress] // Validates email format.
    [StringLength(256)] // Limits email length.
    string Email, // Carries customer email.
    [StringLength(32)] // Limits phone length.
    string? PhoneNumber); // Carries customer phone number.

public sealed record VehicleDto( // Defines vehicle DTO.
    Guid Id, // Carries vehicle id.
    [Required] // Requires customer id.
    [StringLength(450)] // Limits customer id length.
    string CustomerId, // Carries customer id.
    [Required] // Requires vehicle number.
    [StringLength(64)] // Limits vehicle number length.
    string VehicleNumber, // Carries vehicle number.
    [StringLength(128)] // Limits model length.
    string? Model); // Carries optional vehicle model.

public sealed record CreateVehicleDto( // Defines vehicle creation DTO.
    [Required] // Requires customer id.
    [StringLength(450)] // Limits customer id length.
    string CustomerId, // Carries customer id.
    [Required] // Requires vehicle number.
    [StringLength(64)] // Limits vehicle number length.
    string VehicleNumber, // Carries vehicle number.
    [StringLength(128)] // Limits model length.
    string? Model); // Carries optional vehicle model.

public sealed record UpdateVehicleDto( // Defines vehicle update DTO.
    [Required] // Requires vehicle number.
    [StringLength(64)] // Limits vehicle number length.
    string VehicleNumber, // Carries vehicle number.
    [StringLength(128)] // Limits model length.
    string? Model); // Carries optional vehicle model.

public sealed record CustomerDetailDto( // Defines customer detail DTO.
    [Required] // Requires customer id.
    [StringLength(450)] // Limits customer id length.
    string Id, // Carries customer id.
    [Required] // Requires full name.
    [StringLength(256)] // Limits full name length.
    string FullName, // Carries customer full name.
    [Required] // Requires email.
    [EmailAddress] // Validates email format.
    [StringLength(256)] // Limits email length.
    string Email, // Carries customer email.
    [StringLength(32)] // Limits phone length.
    string? PhoneNumber, // Carries customer phone number.
    [Required] // Requires vehicle list.
    List<VehicleDto> Vehicles); // Carries customer vehicles.

public sealed record CustomerHistoryDto( // Defines customer history DTO.
    [Required] // Requires customer id.
    [StringLength(450)] // Limits customer id length.
    string CustomerId, // Carries customer id.
    [Required] // Requires records list.
    List<string> Records); // Carries history records.

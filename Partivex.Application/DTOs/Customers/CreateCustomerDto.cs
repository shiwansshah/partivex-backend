using System.ComponentModel.DataAnnotations;

namespace Partivex.Application.DTOs.Customers;

public class CreateCustomerDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    public string? Email { get; set; }

    public string? Address { get; set; }
}

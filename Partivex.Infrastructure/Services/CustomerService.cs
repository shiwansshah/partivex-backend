using Microsoft.AspNetCore.Identity; // Imports Identity services.
using Microsoft.EntityFrameworkCore; // Imports EF query extensions.
using Partivex.Application.Constants; // Imports role constants.
using Partivex.Application.DTOs; // Imports customer DTOs.
using Partivex.Application.Interfaces; // Imports service contract.
using Partivex.Domain.Entities; // Imports domain entities.
using Partivex.Infrastructure.Data; // Imports database context.

namespace Partivex.Infrastructure.Services; // Defines service namespace.

public sealed class CustomerService : ICustomerService // Implements customer service.
{
    private readonly AppDbContext _dbContext; // Stores database context.
    private readonly UserManager<ApplicationUser> _userManager; // Stores user manager.

    public CustomerService( // Defines constructor.
        AppDbContext dbContext, // Receives database context.
        UserManager<ApplicationUser> userManager) // Receives user manager.
    {
        _dbContext = dbContext; // Assigns database context.
        _userManager = userManager; // Assigns user manager.
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync() // Gets customer list.
    {
        var customers = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Customer); // Loads customer users.

        return customers.Select(MapToCustomerDto); // Maps users to DTOs.
    }

    public async Task<CustomerDetailDto?> GetCustomerByIdAsync(string id) // Gets customer detail.
    {
        var customer = await FindCustomerOrThrowAsync(id); // Loads customer user.

        var vehicleRows = await _dbContext.Vehicles // Starts vehicle query.
            .AsNoTracking() // Disables tracking.
            .Where(vehicle => vehicle.CustomerId == customer.Id) // Filters by customer id.
            .ToListAsync(); // Executes vehicle query.

        var vehicles = vehicleRows.Select(MapToVehicleDto).ToList(); // Maps vehicle DTOs.

        return new CustomerDetailDto( // Creates detail DTO.
            customer.Id, // Maps customer id.
            NormalizeText(customer.FullName), // Maps full name.
            NormalizeText(customer.Email), // Maps email safely.
            NormalizeOptionalText(customer.PhoneNumber), // Maps phone number.
            vehicles); // Maps vehicles.
    }

    public async Task<CustomerHistoryDto?> GetCustomerHistoryAsync(string id) // Gets customer history.
    {
        var customer = await FindCustomerOrThrowAsync(id); // Loads customer user.

        var historyRows = await _dbContext.CustomerHistories // Starts history query.
            .AsNoTracking() // Disables tracking.
            .Where(history => history.CustomerId == customer.Id) // Filters by customer id.
            .OrderByDescending(history => history.CreatedAt) // Orders latest first.
            .ToListAsync(); // Executes history query.

        var records = historyRows.Select(history => NormalizeText(history.Description)).ToList(); // Maps descriptions.

        return new CustomerHistoryDto(customer.Id, records); // Returns history DTO.
    }

    private async Task<ApplicationUser> FindCustomerOrThrowAsync(string id) // Finds customer user.
    {
        var customerId = NormalizeText(id); // Normalizes customer id.

        if (string.IsNullOrWhiteSpace(customerId)) // Checks blank id.
        {
            throw new KeyNotFoundException("Customer was not found."); // Rejects blank id.
        }

        var user = await _userManager.FindByIdAsync(customerId); // Loads identity user.

        if (user is null) // Handles missing user.
        {
            throw new KeyNotFoundException("Customer was not found."); // Rejects missing user.
        }

        var isCustomer = await _userManager.IsInRoleAsync(user, ApplicationRoles.Customer); // Checks customer role.

        if (!isCustomer) // Handles non-customer user.
        {
            throw new KeyNotFoundException("Customer was not found."); // Rejects non-customer user.
        }

        return user; // Returns customer user.
    }

    private static CustomerDto MapToCustomerDto(ApplicationUser customer) // Maps customer DTO.
    {
        return new CustomerDto( // Creates customer DTO.
            customer.Id, // Maps customer id.
            NormalizeText(customer.FullName), // Maps full name.
            NormalizeText(customer.Email), // Maps email safely.
            NormalizeOptionalText(customer.PhoneNumber)); // Maps phone number.
    }

    private static VehicleDto MapToVehicleDto(Vehicle vehicle) // Maps vehicle DTO.
    {
        return new VehicleDto( // Creates vehicle DTO.
            vehicle.Id, // Maps vehicle id.
            NormalizeText(vehicle.Name), // Maps vehicle name.
            NormalizeText(vehicle.Number), // Maps vehicle number.
            NormalizeOptionalText(vehicle.ImageUrl), // Maps vehicle image.
            NormalizeText(vehicle.CustomerId)); // Maps customer id.
    }

    private static string NormalizeText(string? value) // Normalizes required text.
    {
        return value?.Trim() ?? string.Empty; // Returns safe text.
    }

    private static string? NormalizeOptionalText(string? value) // Normalizes optional text.
    {
        var normalizedValue = value?.Trim(); // Trims optional text.

        return string.IsNullOrWhiteSpace(normalizedValue) ? null : normalizedValue; // Returns optional text.
    }
}

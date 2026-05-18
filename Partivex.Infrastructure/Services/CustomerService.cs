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
            NormalizeOptionalText(customer.Address), // Maps address.
            vehicles); // Maps vehicles.
    }

    public async Task<CustomerDetailDto> UpdateAsync(string id, UpdateCustomerDto dto) // Updates customer.
    {
        var customer = await FindCustomerOrThrowAsync(id); // Loads customer user.

        var fullName = NormalizeRequiredText(dto.FullName, nameof(dto.FullName)); // Normalizes full name.
        var email = NormalizeRequiredText(dto.Email, nameof(dto.Email)); // Normalizes email.
        var address = NormalizeRequiredText(dto.Address, nameof(dto.Address)); // Normalizes address.
        var phoneNumber = NormalizeOptionalText(dto.PhoneNumber); // Normalizes phone number.

        if (!string.IsNullOrWhiteSpace(phoneNumber)) // Checks for duplicate phone.
        {
            var duplicatePhoneExists = await _userManager.Users // Starts user query.
                .AnyAsync(user => user.Id != customer.Id && user.PhoneNumber == phoneNumber); // Looks for another phone.

            if (duplicatePhoneExists) // Handles duplicate phone.
            {
                throw new InvalidOperationException("Phone number is already in use."); // Rejects duplicate phone.
            }
        }

        var duplicateEmailExists = await _userManager.Users // Starts user query.
            .AnyAsync(user => user.Id != customer.Id && user.Email == email); // Looks for another email.

        if (duplicateEmailExists) // Handles duplicate email.
        {
            throw new InvalidOperationException("Email address is already in use."); // Rejects duplicate email.
        }

        customer.FullName = fullName; // Updates full name.
        customer.Email = email; // Updates email.
        customer.UserName = email; // Keeps username aligned with email.
        customer.PhoneNumber = phoneNumber; // Updates phone number.
        customer.Address = address; // Updates address.

        var result = await _userManager.UpdateAsync(customer); // Persists changes.

        if (!result.Succeeded) // Handles identity validation failures.
        {
            throw new ArgumentException(string.Join(" ", result.Errors.Select(error => error.Description))); // Raises validation error.
        }

        return await GetCustomerByIdAsync(customer.Id) ?? throw new InvalidOperationException("Customer could not be loaded after update.");
    }

    public async Task<IEnumerable<CustomerDto>> SearchAsync(string term) // Searches customers.
    {
        var normalizedTerm = NormalizeRequiredText(term, nameof(term)); // Normalizes search term.
        var customers = await _userManager.GetUsersInRoleAsync(ApplicationRoles.Customer); // Loads customers.
        var matches = new Dictionary<string, ApplicationUser>(); // Prevents duplicates.

        foreach (var customer in customers) // Scans customers.
        {
            if (Matches(customer.Id, normalizedTerm) // Matches id.
                || Matches(customer.FullName, normalizedTerm) // Matches full name.
                || Matches(customer.Email, normalizedTerm) // Matches email.
                || Matches(customer.PhoneNumber, normalizedTerm) // Matches phone.
                || Matches(customer.Address, normalizedTerm)) // Matches address.
            {
                matches[customer.Id] = customer; // Adds customer match.
            }
        }

        var vehicleCustomerIds = await _dbContext.Vehicles // Starts vehicle query.
            .AsNoTracking() // Disables tracking.
            .Where(vehicle => vehicle.Number.Contains(normalizedTerm)) // Filters by vehicle number.
            .Select(vehicle => vehicle.CustomerId) // Selects matching customer ids.
            .Distinct() // Avoids duplicate ids.
            .ToListAsync(); // Executes vehicle query.

        foreach (var vehicleCustomerId in vehicleCustomerIds) // Adds customers matched by vehicles.
        {
            var customer = customers.FirstOrDefault(user => user.Id == vehicleCustomerId); // Finds customer.

            if (customer is not null) // Ensures customer exists.
            {
                matches[customer.Id] = customer; // Adds customer once.
            }
        }

        return matches.Values // Projects results.
            .OrderBy(customer => customer.FullName) // Sorts alphabetically.
            .Select(MapToCustomerDto) // Maps to DTO.
            .ToArray(); // Returns array.
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
            NormalizeOptionalText(customer.PhoneNumber), // Maps phone number.
            NormalizeOptionalText(customer.Address)); // Maps address.
    }

    private static bool Matches(string? value, string term) // Checks string match.
    {
        return !string.IsNullOrWhiteSpace(value) && value.Contains(term, StringComparison.OrdinalIgnoreCase); // Uses case-insensitive contains.
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

    private static string NormalizeRequiredText(string? value, string fieldName) // Normalizes required text.
    {
        var normalizedValue = NormalizeText(value); // Trims value.

        if (string.IsNullOrWhiteSpace(normalizedValue)) // Checks blank.
        {
            throw new ArgumentException($"{fieldName} is required."); // Rejects blank value.
        }

        return normalizedValue; // Returns normalized text.
    }

    private static string? NormalizeOptionalText(string? value) // Normalizes optional text.
    {
        var normalizedValue = value?.Trim(); // Trims optional text.

        return string.IsNullOrWhiteSpace(normalizedValue) ? null : normalizedValue; // Returns optional text.
    }
}

using System.Security.Claims; // Imports claim types.
using Microsoft.AspNetCore.Http; // Imports HTTP context access.
using Microsoft.AspNetCore.Identity; // Imports Identity services.
using Microsoft.EntityFrameworkCore; // Imports EF query extensions.
using Partivex.Application.Constants; // Imports role constants.
using Partivex.Application.DTOs; // Imports vehicle DTOs.
using Partivex.Application.Interfaces; // Imports service contract.
using Partivex.Domain.Entities; // Imports domain entities.
using Partivex.Infrastructure.Data; // Imports database context.

namespace Partivex.Infrastructure.Services; // Defines service namespace.

public sealed class VehicleService : IVehicleService // Implements vehicle service.
{
    private readonly AppDbContext _dbContext; // Stores database context.
    private readonly IHttpContextAccessor _httpContextAccessor; // Stores HTTP context accessor.
    private readonly UserManager<ApplicationUser> _userManager; // Stores user manager.

    public VehicleService( // Defines constructor.
        AppDbContext dbContext, // Receives database context.
        IHttpContextAccessor httpContextAccessor, // Receives HTTP context accessor.
        UserManager<ApplicationUser> userManager) // Receives user manager.
    {
        _dbContext = dbContext; // Assigns database context.
        _httpContextAccessor = httpContextAccessor; // Assigns HTTP context accessor.
        _userManager = userManager; // Assigns user manager.
    }

    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto) // Creates vehicle.
    {
        var customerId = NormalizeRequired(dto.CustomerId, nameof(dto.CustomerId)); // Normalizes customer id.

        EnsureCanWrite(customerId); // Validates write access.

        await EnsureCustomerExistsAsync(customerId); // Validates customer owner.

        var vehicle = new Vehicle // Creates vehicle entity.
        {
            Id = Guid.NewGuid(), // Sets vehicle id.
            CustomerId = customerId, // Sets customer id.
            VehicleNumber = NormalizeRequired(dto.VehicleNumber, nameof(dto.VehicleNumber)), // Sets vehicle number.
            Model = NormalizeOptional(dto.Model) // Sets vehicle model.
        };

        await _dbContext.Vehicles.AddAsync(vehicle); // Adds vehicle row.

        await _dbContext.SaveChangesAsync(); // Saves vehicle row.

        return MapToVehicleDto(vehicle); // Returns vehicle DTO.
    }

    public async Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerAsync(string customerId) // Gets vehicles.
    {
        var normalizedCustomerId = NormalizeRequired(customerId, nameof(customerId)); // Normalizes customer id.

        EnsureCanRead(normalizedCustomerId); // Validates read access.

        await EnsureCustomerExistsAsync(normalizedCustomerId); // Validates customer owner.

        var vehicles = await _dbContext.Vehicles // Starts vehicle query.
            .AsNoTracking() // Disables tracking.
            .Where(vehicle => vehicle.CustomerId == normalizedCustomerId) // Filters by customer id.
            .ToListAsync(); // Executes vehicle query.

        return vehicles.Select(MapToVehicleDto); // Maps vehicle DTOs.
    }

    public async Task<VehicleDto> UpdateVehicleAsync(Guid id, UpdateVehicleDto dto) // Updates vehicle.
    {
        var vehicle = await FindVehicleOrThrowAsync(id); // Loads vehicle row.

        EnsureCanWrite(vehicle.CustomerId); // Validates write access.

        vehicle.VehicleNumber = NormalizeRequired(dto.VehicleNumber, nameof(dto.VehicleNumber)); // Updates vehicle number.

        vehicle.Model = NormalizeOptional(dto.Model); // Updates vehicle model.

        await _dbContext.SaveChangesAsync(); // Saves vehicle update.

        return MapToVehicleDto(vehicle); // Returns vehicle DTO.
    }

    public async Task DeleteVehicleAsync(Guid id) // Deletes vehicle.
    {
        var vehicle = await FindVehicleOrThrowAsync(id); // Loads vehicle row.

        EnsureCanWrite(vehicle.CustomerId); // Validates write access.

        _dbContext.Vehicles.Remove(vehicle); // Removes vehicle row.

        await _dbContext.SaveChangesAsync(); // Saves vehicle deletion.
    }

    private async Task<Vehicle> FindVehicleOrThrowAsync(Guid id) // Finds vehicle row.
    {
        var vehicle = await _dbContext.Vehicles.FindAsync(id); // Loads vehicle by id.

        if (vehicle is null) // Handles missing vehicle.
        {
            throw new KeyNotFoundException("Vehicle was not found."); // Rejects missing vehicle.
        }

        return vehicle; // Returns vehicle row.
    }

    private async Task EnsureCustomerExistsAsync(string customerId) // Validates customer owner.
    {
        var user = await _userManager.FindByIdAsync(customerId); // Loads identity user.

        if (user is null) // Handles missing user.
        {
            throw new KeyNotFoundException("Customer was not found."); // Rejects missing customer.
        }

        var isCustomer = await _userManager.IsInRoleAsync(user, ApplicationRoles.Customer); // Checks customer role.

        if (!isCustomer) // Handles non-customer user.
        {
            throw new KeyNotFoundException("Customer was not found."); // Rejects invalid owner.
        }
    }

    private void EnsureCanRead(string customerId) // Validates read access.
    {
        if (IsAdmin() || IsStaff()) // Allows privileged read.
        {
            return; // Stops access check.
        }

        if (IsCustomer() && CurrentUserId() == customerId) // Allows owner read.
        {
            return; // Stops access check.
        }

        throw new UnauthorizedAccessException("Vehicle access denied."); // Rejects read access.
    }

    private void EnsureCanWrite(string customerId) // Validates write access.
    {
        if (IsAdmin()) // Allows admin write.
        {
            return; // Stops access check.
        }

        if (IsCustomer() && CurrentUserId() == customerId) // Allows owner write.
        {
            return; // Stops access check.
        }

        throw new UnauthorizedAccessException("Vehicle access denied."); // Rejects write access.
    }

    private string? CurrentUserId() // Gets current user id.
    {
        return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier); // Reads id claim.
    }

    private bool IsAdmin() // Checks admin role.
    {
        return HasRole(ApplicationRoles.Admin); // Returns admin status.
    }

    private bool IsStaff() // Checks staff role.
    {
        return HasRole(ApplicationRoles.Staff); // Returns staff status.
    }

    private bool IsCustomer() // Checks customer role.
    {
        return HasRole(ApplicationRoles.Customer); // Returns customer status.
    }

    private bool HasRole(string role) // Checks current role.
    {
        return _httpContextAccessor.HttpContext?.User.IsInRole(role) == true; // Reads role claim.
    }

    private static VehicleDto MapToVehicleDto(Vehicle vehicle) // Maps vehicle DTO.
    {
        return new VehicleDto( // Creates vehicle DTO.
            vehicle.Id, // Maps vehicle id.
            NormalizeRequired(vehicle.CustomerId, nameof(vehicle.CustomerId)), // Maps customer id.
            NormalizeRequired(vehicle.VehicleNumber, nameof(vehicle.VehicleNumber)), // Maps vehicle number.
            NormalizeOptional(vehicle.Model)); // Maps vehicle model.
    }

    private static string NormalizeRequired(string? value, string fieldName) // Normalizes required text.
    {
        if (string.IsNullOrWhiteSpace(value)) // Checks blank text.
        {
            throw new InvalidOperationException($"{fieldName} is required."); // Rejects blank text.
        }

        return value.Trim(); // Returns trimmed text.
    }

    private static string? NormalizeOptional(string? value) // Normalizes optional text.
    {
        var normalizedValue = value?.Trim(); // Trims optional text.

        return string.IsNullOrWhiteSpace(normalizedValue) ? null : normalizedValue; // Returns optional text.
    }
}

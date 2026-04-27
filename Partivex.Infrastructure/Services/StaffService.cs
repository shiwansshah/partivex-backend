using Microsoft.AspNetCore.Identity; // Imports Identity services.
using Partivex.Application.DTOs; // Imports staff DTOs.
using Partivex.Application.Interfaces; // Imports service contract.
using Partivex.Domain.Entities; // Imports application user.

namespace Partivex.Infrastructure.Services; // Defines service namespace.

public sealed class StaffService : IStaffService // Implements staff service.
{
    private const string StaffRole = "Staff"; // Defines staff role.

    private readonly UserManager<ApplicationUser> _userManager; // Stores user manager.
    private readonly RoleManager<IdentityRole> _roleManager; // Stores role manager.

    public StaffService( // Defines constructor.
        UserManager<ApplicationUser> userManager, // Receives user manager.
        RoleManager<IdentityRole> roleManager) // Receives role manager.
    {
        _userManager = userManager; // Assigns user manager.
        _roleManager = roleManager; // Assigns role manager.
    }

    public async Task<StaffDto> CreateStaffAsync(CreateStaffDto dto) // Creates staff user.
    {
        await EnsureStaffRoleExistsAsync(); // Validates staff role.

        var existingUser = await _userManager.FindByEmailAsync(dto.Email); // Checks duplicate email.

        if (existingUser is not null) // Handles existing email.
        {
            throw new InvalidOperationException("Email is already registered."); // Stops duplicate user.
        }

        var user = new ApplicationUser // Builds identity user.
        {
            UserName = dto.Email, // Sets username.
            Email = dto.Email, // Sets email.
            FullName = dto.FullName // Sets full name.
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password); // Creates identity user.

        if (!createResult.Succeeded) // Handles create failure.
        {
            throw new InvalidOperationException(ToErrorMessage(createResult)); // Reports identity errors.
        }

        var roleResult = await _userManager.AddToRoleAsync(user, StaffRole); // Assigns staff role.

        if (!roleResult.Succeeded) // Handles role failure.
        {
            await _userManager.DeleteAsync(user); // Rolls back user.

            throw new InvalidOperationException(ToErrorMessage(roleResult)); // Reports role errors.
        }

        return MapToStaffDto(user); // Returns staff DTO.
    }

    public async Task<IEnumerable<StaffDto>> GetAllStaffAsync() // Gets all staff users.
    {
        var users = await _userManager.GetUsersInRoleAsync(StaffRole); // Loads staff users.

        return users.Select(MapToStaffDto); // Maps users to DTOs.
    }

    public async Task<StaffDto> GetStaffByIdAsync(string id) // Gets staff by id.
    {
        var user = await FindStaffUserAsync(id); // Loads staff user.

        return MapToStaffDto(user); // Returns staff DTO.
    }

    public async Task UpdateStaffAsync(string id, UpdateStaffDto dto) // Updates staff user.
    {
        var user = await FindStaffUserAsync(id); // Loads staff user.

        user.FullName = dto.FullName; // Updates full name.

        var result = await _userManager.UpdateAsync(user); // Persists user update.

        if (!result.Succeeded) // Handles update failure.
        {
            throw new InvalidOperationException(ToErrorMessage(result)); // Reports identity errors.
        }
    }

    public async Task DeleteStaffAsync(string id) // Deletes staff user.
    {
        var user = await FindStaffUserAsync(id); // Loads staff user.

        var result = await _userManager.DeleteAsync(user); // Deletes identity user.

        if (!result.Succeeded) // Handles delete failure.
        {
            throw new InvalidOperationException(ToErrorMessage(result)); // Reports identity errors.
        }
    }

    private async Task EnsureStaffRoleExistsAsync() // Validates role exists.
    {
        var exists = await _roleManager.RoleExistsAsync(StaffRole); // Checks role store.

        if (!exists) // Handles missing role.
        {
            throw new InvalidOperationException("Staff role is not configured."); // Stops invalid setup.
        }
    }

    private async Task<ApplicationUser> FindStaffUserAsync(string id) // Finds staff user.
    {
        var user = await _userManager.FindByIdAsync(id); // Loads identity user.

        if (user is null) // Handles missing user.
        {
            throw new KeyNotFoundException("Staff user was not found."); // Stops missing user.
        }

        var isStaff = await _userManager.IsInRoleAsync(user, StaffRole); // Checks staff role.

        if (!isStaff) // Handles non-staff user.
        {
            throw new KeyNotFoundException("Staff user was not found."); // Hides non-staff users.
        }

        return user; // Returns staff user.
    }

    private static StaffDto MapToStaffDto(ApplicationUser user) // Maps user to DTO.
    {
        return new StaffDto( // Creates response DTO.
            user.Id, // Maps user id.
            user.FullName, // Maps full name.
            user.Email ?? string.Empty, // Maps email safely.
            StaffRole); // Maps staff role.
    }

    private static string ToErrorMessage(IdentityResult result) // Formats identity errors.
    {
        return string.Join(" ", result.Errors.Select(error => error.Description)); // Joins error text.
    }
}

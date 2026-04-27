using Microsoft.AspNetCore.Authorization; // Imports authorization attributes.
using Microsoft.AspNetCore.Mvc; // Imports MVC controller types.
using Partivex.Application.DTOs; // Imports staff DTOs.
using Partivex.Application.Interfaces; // Imports staff service contract.

namespace Partivex.Controllers; // Defines API namespace.

[ApiController] // Enables API conventions.
[Route("api/staff")] // Sets staff route.
public sealed class StaffController : ControllerBase // Defines staff controller.
{
    private const string AdminRole = "Admin"; // Defines admin role.
    private const string StaffRole = "Staff"; // Defines staff role.
    private const string ReadRoles = AdminRole + "," + StaffRole; // Defines read roles.

    private readonly IStaffService _staffService; // Stores staff service.

    public StaffController(IStaffService staffService) // Defines constructor.
    {
        _staffService = staffService; // Assigns staff service.
    }

    [HttpPost] // Handles staff creation.
    [Authorize(Roles = AdminRole)] // Restricts create to admin.
    public async Task<ActionResult<StaffDto>> CreateStaff(CreateStaffDto dto) // Creates staff endpoint.
    {
        try // Handles service outcomes.
        {
            var staff = await _staffService.CreateStaffAsync(dto); // Creates staff user.

            return CreatedAtAction(nameof(GetStaffById), new { id = staff.Id }, staff); // Returns created staff.
        }
        catch (InvalidOperationException exception) // Handles validation errors.
        {
            return BadRequest(new { Error = exception.Message }); // Returns bad request.
        }
    }

    [HttpGet] // Handles staff listing.
    [Authorize(Roles = ReadRoles)] // Allows admin and staff.
    public async Task<ActionResult<IEnumerable<StaffDto>>> GetAllStaff() // Lists staff endpoint.
    {
        var staff = await _staffService.GetAllStaffAsync(); // Gets staff users.

        return Ok(staff); // Returns staff list.
    }

    [HttpGet("{id}")] // Handles staff lookup.
    [Authorize(Roles = ReadRoles)] // Allows admin and staff.
    public async Task<ActionResult<StaffDto>> GetStaffById(string id) // Gets staff endpoint.
    {
        try // Handles service outcomes.
        {
            var staff = await _staffService.GetStaffByIdAsync(id); // Gets staff user.

            return Ok(staff); // Returns staff data.
        }
        catch (KeyNotFoundException exception) // Handles missing staff.
        {
            return NotFound(new { Error = exception.Message }); // Returns not found.
        }
    }

    [HttpPut("{id}")] // Handles staff update.
    [Authorize(Roles = AdminRole)] // Restricts update to admin.
    public async Task<IActionResult> UpdateStaff(string id, UpdateStaffDto dto) // Updates staff endpoint.
    {
        try // Handles service outcomes.
        {
            await _staffService.UpdateStaffAsync(id, dto); // Updates staff user.

            return NoContent(); // Returns success without body.
        }
        catch (KeyNotFoundException exception) // Handles missing staff.
        {
            return NotFound(new { Error = exception.Message }); // Returns not found.
        }
        catch (InvalidOperationException exception) // Handles validation errors.
        {
            return BadRequest(new { Error = exception.Message }); // Returns bad request.
        }
    }

    [HttpDelete("{id}")] // Handles staff deletion.
    [Authorize(Roles = AdminRole)] // Restricts delete to admin.
    public async Task<IActionResult> DeleteStaff(string id) // Deletes staff endpoint.
    {
        try // Handles service outcomes.
        {
            await _staffService.DeleteStaffAsync(id); // Deletes staff user.

            return NoContent(); // Returns success without body.
        }
        catch (KeyNotFoundException exception) // Handles missing staff.
        {
            return NotFound(new { Error = exception.Message }); // Returns not found.
        }
        catch (InvalidOperationException exception) // Handles validation errors.
        {
            return BadRequest(new { Error = exception.Message }); // Returns bad request.
        }
    }
}

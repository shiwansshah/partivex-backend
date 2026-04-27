using Microsoft.AspNetCore.Authorization; // Imports authorization attributes.
using Microsoft.AspNetCore.Mvc; // Imports MVC controller types.
using Partivex.Application.Constants; // Imports role constants.
using Partivex.Application.DTOs; // Imports staff DTOs.
using Partivex.Application.Interfaces; // Imports staff service contract.

namespace Partivex.Controllers; // Defines API namespace.

[ApiController] // Enables API conventions.
[Route("api/staff")] // Sets staff route.
public sealed class StaffController : ControllerBase // Defines staff controller.
{
    private readonly IStaffService _staffService; // Stores staff service.

    public StaffController(IStaffService staffService) // Defines constructor.
    {
        _staffService = staffService; // Assigns staff service.
    }

    [HttpPost] // Handles staff creation.
    [Authorize(Roles = ApplicationRoles.Admin)] // Restricts create to admin.
    [ProducesResponseType(typeof(StaffDto), StatusCodes.Status201Created)] // Documents created response.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Documents bad request.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    public async Task<ActionResult<StaffDto>> CreateStaff([FromBody] CreateStaffDto dto) // Creates staff endpoint.
    {
        try // Handles service outcomes.
        {
            var staff = await _staffService.CreateStaffAsync(dto); // Creates staff user.

            return CreatedAtAction(nameof(GetStaffById), new { id = staff.Id }, staff); // Returns created staff.
        }
        catch (InvalidOperationException exception) // Handles validation errors.
        {
            return BadRequest(new ApiErrorResponse(exception.Message)); // Returns bad request.
        }
    }

    [HttpGet] // Handles staff listing.
    [Authorize(Roles = ApplicationRoles.AdminAndStaff)] // Allows admin and staff.
    [ProducesResponseType(typeof(IEnumerable<StaffDto>), StatusCodes.Status200OK)] // Documents success response.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    public async Task<ActionResult<IEnumerable<StaffDto>>> GetAllStaff() // Lists staff endpoint.
    {
        var staff = await _staffService.GetAllStaffAsync(); // Gets staff users.

        return Ok(staff); // Returns staff list.
    }

    [HttpGet("{id}")] // Handles staff lookup.
    [Authorize(Roles = ApplicationRoles.AdminAndStaff)] // Allows admin and staff.
    [ProducesResponseType(typeof(StaffDto), StatusCodes.Status200OK)] // Documents success response.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Documents missing staff.
    public async Task<ActionResult<StaffDto>> GetStaffById([FromRoute] string id) // Gets staff endpoint.
    {
        try // Handles service outcomes.
        {
            var staff = await _staffService.GetStaffByIdAsync(id); // Gets staff user.

            return Ok(staff); // Returns staff data.
        }
        catch (KeyNotFoundException exception) // Handles missing staff.
        {
            return NotFound(new ApiErrorResponse(exception.Message)); // Returns not found.
        }
    }

    [HttpPut("{id}")] // Handles staff update.
    [Authorize(Roles = ApplicationRoles.Admin)] // Restricts update to admin.
    [ProducesResponseType(StatusCodes.Status204NoContent)] // Documents update success.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Documents bad request.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Documents missing staff.
    public async Task<IActionResult> UpdateStaff([FromRoute] string id, [FromBody] UpdateStaffDto dto) // Updates staff endpoint.
    {
        try // Handles service outcomes.
        {
            await _staffService.UpdateStaffAsync(id, dto); // Updates staff user.

            return NoContent(); // Returns success without body.
        }
        catch (KeyNotFoundException exception) // Handles missing staff.
        {
            return NotFound(new ApiErrorResponse(exception.Message)); // Returns not found.
        }
        catch (InvalidOperationException exception) // Handles validation errors.
        {
            return BadRequest(new ApiErrorResponse(exception.Message)); // Returns bad request.
        }
    }

    [HttpDelete("{id}")] // Handles staff deletion.
    [Authorize(Roles = ApplicationRoles.Admin)] // Restricts delete to admin.
    [ProducesResponseType(StatusCodes.Status204NoContent)] // Documents delete success.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Documents bad request.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    [ProducesResponseType(StatusCodes.Status403Forbidden)] // Documents denied role.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Documents missing staff.
    public async Task<IActionResult> DeleteStaff([FromRoute] string id) // Deletes staff endpoint.
    {
        try // Handles service outcomes.
        {
            await _staffService.DeleteStaffAsync(id); // Deletes staff user.

            return NoContent(); // Returns success without body.
        }
        catch (KeyNotFoundException exception) // Handles missing staff.
        {
            return NotFound(new ApiErrorResponse(exception.Message)); // Returns not found.
        }
        catch (InvalidOperationException exception) // Handles validation errors.
        {
            return BadRequest(new ApiErrorResponse(exception.Message)); // Returns bad request.
        }
    }
}

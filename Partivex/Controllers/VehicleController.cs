using Microsoft.AspNetCore.Authorization; // Imports authorization attributes.
using Microsoft.AspNetCore.Mvc; // Imports MVC controller types.
using Partivex.Application.Constants; // Imports role constants.
using Partivex.Application.DTOs; // Imports vehicle DTOs.
using Partivex.Application.Interfaces; // Imports vehicle service contract.

namespace Partivex.Controllers; // Defines API namespace.

[ApiController] // Enables API conventions.
[Route("api/vehicles")] // Sets vehicle route.
[Authorize(Roles = ApplicationRoles.AdminStaffAndCustomer)] // Restricts vehicle endpoints.
public sealed class VehicleController : ControllerBase // Defines vehicle controller.
{
    private readonly IVehicleService _vehicleService; // Stores vehicle service.

    public VehicleController(IVehicleService vehicleService) // Defines constructor.
    {
        _vehicleService = vehicleService; // Assigns vehicle service.
    }

    [HttpPost] // Handles vehicle creation.
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status201Created)] // Documents created response.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Documents bad request.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)] // Documents denied access.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Documents missing customer.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    public async Task<ActionResult<VehicleDto>> CreateVehicle([FromBody] CreateVehicleDto dto) // Creates vehicle.
    {
        try // Handles service outcomes.
        {
            var vehicle = await _vehicleService.CreateVehicleAsync(dto); // Creates vehicle DTO.

            return CreatedAtAction(nameof(GetVehiclesByCustomer), new { customerId = vehicle.CustomerId }, vehicle); // Returns created vehicle.
        }
        catch (InvalidOperationException exception) // Handles validation errors.
        {
            return BadRequest(new ApiErrorResponse(exception.Message)); // Returns bad request.
        }
        catch (UnauthorizedAccessException exception) // Handles denied access.
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(exception.Message)); // Returns forbidden.
        }
        catch (KeyNotFoundException exception) // Handles missing customer.
        {
            return NotFound(new ApiErrorResponse(exception.Message)); // Returns not found.
        }
    }

    [HttpGet("customer/{customerId}")] // Handles customer vehicle listing.
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)] // Documents success response.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Documents bad request.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)] // Documents denied access.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Documents missing customer.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehiclesByCustomer([FromRoute] string customerId) // Gets customer vehicles.
    {
        try // Handles service outcomes.
        {
            var vehicles = await _vehicleService.GetVehiclesByCustomerAsync(customerId); // Gets vehicle DTOs.

            return Ok(vehicles); // Returns vehicles.
        }
        catch (InvalidOperationException exception) // Handles validation errors.
        {
            return BadRequest(new ApiErrorResponse(exception.Message)); // Returns bad request.
        }
        catch (UnauthorizedAccessException exception) // Handles denied access.
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(exception.Message)); // Returns forbidden.
        }
        catch (KeyNotFoundException exception) // Handles missing customer.
        {
            return NotFound(new ApiErrorResponse(exception.Message)); // Returns not found.
        }
    }

    [HttpPut("{id:guid}")] // Handles vehicle update.
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)] // Documents success response.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)] // Documents bad request.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)] // Documents denied access.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Documents missing vehicle.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    public async Task<ActionResult<VehicleDto>> UpdateVehicle([FromRoute] Guid id, [FromBody] UpdateVehicleDto dto) // Updates vehicle.
    {
        try // Handles service outcomes.
        {
            var vehicle = await _vehicleService.UpdateVehicleAsync(id, dto); // Updates vehicle DTO.

            return Ok(vehicle); // Returns updated vehicle.
        }
        catch (InvalidOperationException exception) // Handles validation errors.
        {
            return BadRequest(new ApiErrorResponse(exception.Message)); // Returns bad request.
        }
        catch (UnauthorizedAccessException exception) // Handles denied access.
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(exception.Message)); // Returns forbidden.
        }
        catch (KeyNotFoundException exception) // Handles missing vehicle.
        {
            return NotFound(new ApiErrorResponse(exception.Message)); // Returns not found.
        }
    }

    [HttpDelete("{id:guid}")] // Handles vehicle deletion.
    [ProducesResponseType(StatusCodes.Status204NoContent)] // Documents delete success.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)] // Documents denied access.
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)] // Documents missing vehicle.
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] // Documents missing auth.
    public async Task<IActionResult> DeleteVehicle([FromRoute] Guid id) // Deletes vehicle.
    {
        try // Handles service outcomes.
        {
            await _vehicleService.DeleteVehicleAsync(id); // Deletes vehicle.

            return NoContent(); // Returns delete success.
        }
        catch (UnauthorizedAccessException exception) // Handles denied access.
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ApiErrorResponse(exception.Message)); // Returns forbidden.
        }
        catch (KeyNotFoundException exception) // Handles missing vehicle.
        {
            return NotFound(new ApiErrorResponse(exception.Message)); // Returns not found.
        }
    }
}

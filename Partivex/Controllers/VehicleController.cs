using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize(Roles = ApplicationRoles.AdminStaffAndCustomer)]
public class VehicleController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehicleController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet("my")]
    [Authorize(Roles = ApplicationRoles.Customer)]
    public async Task<ActionResult<IReadOnlyList<VehicleDto>>> GetMyVehicles()
    {
        var customerId = GetCustomerId();
        var vehicles = await _vehicleService.GetCustomerVehiclesAsync(customerId);

        return Ok(vehicles);
    }

    [HttpGet("customer/{customerId}")]
    [Authorize(Roles = ApplicationRoles.AdminAndStaff)]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehiclesByCustomer([FromRoute] string customerId)
    {
        try
        {
            var vehicles = await _vehicleService.GetVehiclesByCustomerAsync(customerId);

            return Ok(vehicles);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ApiErrorResponse(exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ApiErrorResponse(exception.Message));
        }
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.Customer)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<VehicleDto>> AddVehicle(
        [FromForm] CreateVehicleDto dto,
        IFormFile? image)
    {
        var customerId = GetCustomerId();
        var vehicle = await _vehicleService.AddVehicleAsync(customerId, dto, image);

        return Ok(vehicle);
    }

    [HttpPost]
    [Authorize(Roles = ApplicationRoles.AdminAndStaff)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VehicleDto>> CreateVehicle([FromBody] CreateVehicleDto dto)
    {
        try
        {
            var vehicle = await _vehicleService.CreateVehicleAsync(dto);

            return CreatedAtAction(nameof(GetVehiclesByCustomer), new { customerId = vehicle.CustomerId }, vehicle);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ApiErrorResponse(exception.Message));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.Customer)]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<VehicleDto>> UpdateVehicle(
        Guid id,
        [FromForm] UpdateVehicleDto dto,
        IFormFile? image)
    {
        var customerId = GetCustomerId();
        var vehicle = await _vehicleService.UpdateVehicleAsync(id, customerId, dto, image);

        return Ok(vehicle);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.AdminAndStaff)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VehicleDto>> UpdateVehicle([FromRoute] Guid id, [FromBody] UpdateVehicleDto dto)
    {
        try
        {
            var vehicle = await _vehicleService.UpdateVehicleAsync(id, dto);

            return Ok(vehicle);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ApiErrorResponse(exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ApiErrorResponse(exception.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ApplicationRoles.AdminAndStaff)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteVehicle([FromRoute] Guid id)
    {
        try
        {
            await _vehicleService.DeleteVehicleAsync(id);

            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ApiErrorResponse(exception.Message));
        }
    }

    private string GetCustomerId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
    }
}

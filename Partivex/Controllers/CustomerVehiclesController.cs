using Microsoft.AspNetCore.Mvc;
using Partivex.Application.DTOs.Vehicles;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customers/{customerId:int}/vehicles")]
public class CustomerVehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public CustomerVehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VehicleDto>>> GetByCustomerId(int customerId)
    {
        try
        {
            var vehicles = await _vehicleService.GetByCustomerIdAsync(customerId);
            return Ok(vehicles);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> AddVehicle(int customerId, CreateVehicleDto dto)
    {
        try
        {
            var vehicle = await _vehicleService.AddVehicleAsync(customerId, dto);
            return CreatedAtAction(nameof(GetByCustomerId), new { customerId }, vehicle);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

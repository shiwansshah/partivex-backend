using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize(Roles = "Customer")]
public class VehicleController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehicleController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet("my")]
    public async Task<ActionResult<IReadOnlyList<VehicleDto>>> GetMyVehicles()
    {
        var customerId = GetCustomerId();
        var vehicles = await _vehicleService.GetCustomerVehiclesAsync(customerId);
        return Ok(vehicles);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<VehicleDto>> AddVehicle(
        [FromForm] CreateVehicleDto dto,
        IFormFile? image)
    {
        var customerId = GetCustomerId();
        var vehicle = await _vehicleService.AddVehicleAsync(customerId, dto, image);
        return Ok(vehicle);
    }

    [HttpPut("{id:guid}")]
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

    private string GetCustomerId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
    }
}

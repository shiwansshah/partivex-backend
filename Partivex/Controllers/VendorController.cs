using Microsoft.AspNetCore.Mvc;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/vendor")]
public class VendorController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VendorResponseDto>>> GetVendors()
    {
        var vendors = await _vendorService.GetAllAsync();
        return Ok(vendors);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VendorResponseDto>> GetVendorById(int id)
    {
        var vendor = await _vendorService.GetByIdAsync(id);
        if (vendor is null)
        {
            return NotFound();
        }

        return Ok(vendor);
    }

    [HttpPost]
    public async Task<ActionResult<VendorResponseDto>> CreateVendor(CreateVendorDto vendorDto)
    {
        try
        {
            var vendor = await _vendorService.CreateAsync(vendorDto);
            return CreatedAtAction(nameof(GetVendorById), new { id = vendor.Id }, vendor);
        }
        catch (ArgumentException error)
        {
            return BadRequest(new { message = error.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VendorResponseDto>> UpdateVendor(int id, UpdateVendorDto vendorDto)
    {
        try
        {
            var vendor = await _vendorService.UpdateAsync(id, vendorDto);
            if (vendor is null)
            {
                return NotFound();
            }

            return Ok(vendor);
        }
        catch (ArgumentException error)
        {
            return BadRequest(new { message = error.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVendor(int id)
    {
        var deleted = await _vendorService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}

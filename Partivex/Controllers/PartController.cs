using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/parts")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class PartController : ControllerBase
{
    private readonly IPartService _partService;

    public PartController(IPartService partService)
    {
        _partService = partService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PartResponseDto>>> GetParts(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDirection = "asc")
    {
        var parts = await _partService.GetAllAsync(searchTerm, pageNumber, pageSize, sortBy, sortDirection);
        return Ok(parts);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartResponseDto>> GetPartById(int id)
    {
        var part = await _partService.GetByIdAsync(id);
        if (part is null || !part.IsActive)
        {
            return NotFound();
        }

        return Ok(part);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PartResponseDto>> CreatePart([FromForm] CreatePartDto partDto, IFormFile? image)
    {
        try
        {
            var part = await _partService.CreateAsync(partDto, image);
            return CreatedAtAction(nameof(GetPartById), new { id = part.Id }, part);
        }
        catch (ArgumentException error)
        {
            return BadRequest(new { message = error.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<PartResponseDto>> UpdatePart(int id, [FromForm] UpdatePartDto partDto, IFormFile? image)
    {
        try
        {
            var part = await _partService.UpdateAsync(id, partDto, image);
            if (part is null)
            {
                return NotFound();
            }

            return Ok(part);
        }
        catch (ArgumentException error)
        {
            return BadRequest(new { message = error.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePart(int id)
    {
        try
        {
            var deleted = await _partService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException error)
        {
            return BadRequest(new { message = error.Message });
        }
    }
}

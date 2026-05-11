using Microsoft.AspNetCore.Mvc;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/parts")]
public class PartController : ControllerBase
{
    private readonly IPartService _partService;

    public PartController(IPartService partService)
    {
        _partService = partService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PartResponseDto>>> GetParts()
    {
        var parts = await _partService.GetAllAsync();
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
    public async Task<ActionResult<PartResponseDto>> CreatePart(CreatePartDto partDto)
    {
        try
        {
            var part = await _partService.CreateAsync(partDto);
            return CreatedAtAction(nameof(GetPartById), new { id = part.Id }, part);
        }
        catch (ArgumentException error)
        {
            return BadRequest(new { message = error.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PartResponseDto>> UpdatePart(int id, UpdatePartDto partDto)
    {
        try
        {
            var part = await _partService.UpdateAsync(id, partDto);
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

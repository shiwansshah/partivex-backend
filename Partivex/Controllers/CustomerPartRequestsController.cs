using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customer/part-requests")]
[Authorize(Roles = ApplicationRoles.Customer)]
public sealed class CustomerPartRequestsController : ControllerBase
{
    private readonly IPartRequestService _partRequestService;

    public CustomerPartRequestsController(IPartRequestService partRequestService)
    {
        _partRequestService = partRequestService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PartRequestListDto>>> GetPartRequests(CancellationToken cancellationToken)
    {
        var requests = await _partRequestService.GetPartRequestsAsync(GetCustomerId(), cancellationToken);

        return Ok(requests);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PartRequestDetailDto>> GetPartRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partRequestService.GetPartRequestAsync(id, GetCustomerId(), cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(ToErrorResponse(result, "Part request not found."));
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<PartRequestDetailDto>> CreatePartRequest(
        [FromBody] CreatePartRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _partRequestService.CreatePartRequestAsync(GetCustomerId(), dto, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(ToErrorResponse(result, "Part request could not be created."));
        }

        return CreatedAtAction(nameof(GetPartRequest), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPatch("{id:guid}/cancel")]
    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<PartRequestDetailDto>> CancelPartRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _partRequestService.CancelPartRequestAsync(id, GetCustomerId(), cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(ToErrorResponse(result, "Part request not found."));
        }

        if (!result.Succeeded)
        {
            return BadRequest(ToErrorResponse(result, "Part request could not be cancelled."));
        }

        return Ok(result.Value);
    }

    private string GetCustomerId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
    }

    private static object ToErrorResponse<T>(CustomerPortalResult<T> result, string fallbackMessage)
    {
        var errors = result.Errors.Select(error => error.Description).ToArray();

        return new
        {
            success = false,
            message = result.Message ?? errors.FirstOrDefault() ?? fallbackMessage,
            errors
        };
    }
}

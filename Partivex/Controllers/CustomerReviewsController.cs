using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customer/reviews")]
[Authorize(Roles = ApplicationRoles.Customer)]
public sealed class CustomerReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public CustomerReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReviewListDto>>> GetReviews(CancellationToken cancellationToken)
    {
        var reviews = await _reviewService.GetReviewsAsync(GetCustomerId(), cancellationToken);

        return Ok(reviews);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReviewDetailDto>> GetReview(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reviewService.GetReviewAsync(id, GetCustomerId(), cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(ToErrorResponse(result, "Review not found."));
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDetailDto>> CreateReview(
        [FromBody] CreateReviewDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _reviewService.CreateReviewAsync(GetCustomerId(), dto, cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(ToErrorResponse(result, "Appointment not found."));
        }

        if (!result.Succeeded)
        {
            return BadRequest(ToErrorResponse(result, "Review could not be submitted."));
        }

        return CreatedAtAction(nameof(GetReview), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReviewDetailDto>> UpdateReview(
        Guid id,
        [FromBody] UpdateReviewDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _reviewService.UpdateReviewAsync(id, GetCustomerId(), dto, cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(ToErrorResponse(result, "Review not found."));
        }

        if (!result.Succeeded)
        {
            return BadRequest(ToErrorResponse(result, "Review could not be updated."));
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReview(Guid id, CancellationToken cancellationToken)
    {
        var result = await _reviewService.DeleteReviewAsync(id, GetCustomerId(), cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(ToErrorResponse(result, "Review not found."));
        }

        return NoContent();
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

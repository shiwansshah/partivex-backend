using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/staff/part-requests")]
[Authorize(Roles = ApplicationRoles.Staff)]
public sealed class StaffPartRequestsController : ControllerBase
{
    private readonly IPartRequestRepository _partRequestRepository;
    private readonly ICustomerPartPurchaseService _partPurchaseService;

    public StaffPartRequestsController(
        IPartRequestRepository partRequestRepository,
        ICustomerPartPurchaseService partPurchaseService)
    {
        _partRequestRepository = partRequestRepository;
        _partPurchaseService = partPurchaseService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffPartRequestDto>>> GetPending(CancellationToken cancellationToken)
    {
        var requests = await _partRequestRepository.GetPendingAsync(cancellationToken);
        return Ok(requests.Select(request => new StaffPartRequestDto(
            request.Id,
            request.Customer.FullName,
            request.Customer.Email ?? string.Empty,
            request.Vehicle?.Name,
            request.Vehicle?.Number,
            request.PartId,
            request.PartName,
            request.BrandModelSpecification,
            request.Quantity,
            request.Reason,
            request.Status.ToString(),
            request.CreatedAt)));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<StaffPartRequestApprovalResultDto>> Approve(
        Guid id,
        ApprovePartRequestDto request,
        CancellationToken cancellationToken)
    {
        var staffName = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "Staff";
        var result = await _partPurchaseService.ApprovePartRequestAsync(id, request, staffName, cancellationToken);
        if (!result.Succeeded)
        {
            if (result.IsNotFound)
            {
                return NotFound(new { message = result.Message ?? "Part request not found." });
            }

            return BadRequest(new
            {
                message = result.Message ?? result.Errors.FirstOrDefault()?.Description ?? "Part request could not be approved.",
                errors = result.Errors.Select(error => error.Description).ToArray()
            });
        }

        return Ok(result.Value);
    }
}

public sealed record StaffPartRequestDto(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    string? VehicleName,
    string? VehicleNumber,
    int? PartId,
    string PartName,
    string? BrandModelSpecification,
    int Quantity,
    string? Reason,
    string Status,
    DateTimeOffset CreatedAt);

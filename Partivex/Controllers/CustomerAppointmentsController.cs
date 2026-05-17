using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customer/appointments")]
[Authorize(Roles = ApplicationRoles.Customer)]
public sealed class CustomerAppointmentsController : ControllerBase
{
    private readonly ICustomerAppointmentService _appointmentService;

    public CustomerAppointmentsController(ICustomerAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet("service-options")]
    public ActionResult<IReadOnlyList<ServiceOptionDto>> GetServiceOptions()
    {
        return Ok(_appointmentService.GetServiceOptions());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentListDto>>> GetAppointments(CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetAppointmentsAsync(GetCustomerId(), cancellationToken);

        return Ok(appointments);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetAppointment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.GetAppointmentAsync(id, GetCustomerId(), cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(ToErrorResponse(result, "Appointment not found."));
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDetailDto>> CreateAppointment(
        [FromBody] CreateAppointmentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CreateAppointmentAsync(GetCustomerId(), dto, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(ToErrorResponse(result, "Appointment could not be created."));
        }

        return CreatedAtAction(nameof(GetAppointment), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPatch("{id:guid}/cancel")]
    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<AppointmentDetailDto>> CancelAppointment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CancelAppointmentAsync(id, GetCustomerId(), cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(ToErrorResponse(result, "Appointment not found."));
        }

        if (!result.Succeeded)
        {
            return BadRequest(ToErrorResponse(result, "Appointment could not be cancelled."));
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

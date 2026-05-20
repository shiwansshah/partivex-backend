using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/appointment-invoices")]
[Authorize(Roles = ApplicationRoles.AdminAndStaff)]
public sealed class AppointmentInvoicesController : ControllerBase
{
    private readonly IAppointmentInvoiceService _invoiceService;

    public AppointmentInvoicesController(IAppointmentInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet("appointments")]
    public async Task<ActionResult<IReadOnlyList<StaffAppointmentListDto>>> GetAppointments(CancellationToken cancellationToken)
    {
        return Ok(await _invoiceService.GetAppointmentsAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentInvoiceDto>>> GetInvoices(CancellationToken cancellationToken)
    {
        return Ok(await _invoiceService.GetAllInvoicesAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentInvoiceDto>> CreateInvoice(CreateAppointmentInvoiceDto dto, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.CreateInvoiceAsync(dto, GetStaffIdentifier(), cancellationToken);
        if (!result.Succeeded)
        {
            return result.IsNotFound ? NotFound(ToError(result, "Appointment not found.")) : BadRequest(ToError(result, "Appointment invoice could not be created."));
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:int}/payment-status")]
    public async Task<ActionResult<AppointmentInvoiceDto>> UpdatePaymentStatus(int id, UpdateAppointmentInvoicePaymentDto dto, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.UpdatePaymentStatusAsync(id, dto, null, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : BadRequest(ToError(result, "Payment status could not be updated."));
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GenerateInvoicePdfAsync(id, null, cancellationToken);
        if (!result.Succeeded) return NotFound(new { message = "Appointment invoice not found." });
        return File(result.Value!, "application/pdf", $"appointment-invoice-{id}.pdf");
    }

    [HttpPost("{id:int}/email")]
    public async Task<ActionResult<AppointmentInvoiceEmailResult>> EmailInvoice(
        int id,
        [FromBody] SendAppointmentInvoiceEmailDto? request,
        CancellationToken cancellationToken)
    {
        var result = await _invoiceService.SendInvoiceEmailAsync(id, request?.Email, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { message = result.Message ?? "Appointment invoice not found." });
    }

    [HttpPost("overdue-reminders")]
    public async Task<ActionResult<OverdueAppointmentInvoiceEmailResult>> SendOverdueReminders(CancellationToken cancellationToken)
    {
        return Ok(await _invoiceService.SendOverdueReminderEmailsAsync(cancellationToken));
    }

    private string GetStaffIdentifier()
    {
        return User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "Staff";
    }

    private static object ToError<T>(CustomerPortalResult<T> result, string fallback)
    {
        var errors = result.Errors.Select(error => error.Description).ToArray();
        return new { success = false, message = result.Message ?? errors.FirstOrDefault() ?? fallback, errors };
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customer/appointment-invoices")]
[Authorize(Roles = ApplicationRoles.Customer)]
public sealed class CustomerAppointmentInvoicesController : ControllerBase
{
    private readonly IAppointmentInvoiceService _invoiceService;

    public CustomerAppointmentInvoicesController(IAppointmentInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentInvoiceDto>>> GetMyInvoices(CancellationToken cancellationToken)
    {
        return Ok(await _invoiceService.GetCustomerInvoicesAsync(GetCustomerId(), cancellationToken));
    }

    [HttpPatch("{id:int}/pay")]
    public async Task<ActionResult<AppointmentInvoiceDto>> PayInvoice(int id, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.UpdatePaymentStatusAsync(id, new UpdateAppointmentInvoicePaymentDto { PaymentStatus = "Paid" }, GetCustomerId(), cancellationToken);
        if (!result.Succeeded)
        {
            return result.IsNotFound ? NotFound(new { message = "Appointment invoice not found." }) : BadRequest(new { message = result.Message ?? "Invoice could not be paid." });
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id, CancellationToken cancellationToken)
    {
        var result = await _invoiceService.GenerateInvoicePdfAsync(id, GetCustomerId(), cancellationToken);
        if (!result.Succeeded) return NotFound(new { message = "Appointment invoice not found." });
        return File(result.Value!, "application/pdf", $"appointment-invoice-{id}.pdf");
    }

    private string GetCustomerId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
    }
}

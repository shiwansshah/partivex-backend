using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customer-part-purchase-invoices")]
[Authorize(Roles = ApplicationRoles.AdminAndStaff)]
public sealed class CustomerPartPurchaseInvoicesController : ControllerBase
{
    private readonly ICustomerPartPurchaseService _partPurchaseService;

    public CustomerPartPurchaseInvoicesController(ICustomerPartPurchaseService partPurchaseService)
    {
        _partPurchaseService = partPurchaseService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerPartInvoiceDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _partPurchaseService.GetAllInvoicesAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerPartInvoiceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _partPurchaseService.GetInvoiceAsync(id, null, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : NotFound(new { message = "Customer part purchase invoice not found." });
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> DownloadPdf(int id, CancellationToken cancellationToken)
    {
        var result = await _partPurchaseService.GenerateInvoicePdfAsync(id, null, cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound(new { message = "Customer part purchase invoice not found." });
        }

        return File(result.Value!, "application/pdf", $"customer-part-invoice-{id}.pdf");
    }

    [HttpPost("{id:int}/email")]
    public async Task<ActionResult<CustomerPartInvoiceEmailResult>> EmailInvoice(
        int id,
        SendCustomerPartInvoiceEmailDto request,
        CancellationToken cancellationToken)
    {
        var result = await _partPurchaseService.SendInvoiceEmailAsync(id, request.Email, cancellationToken);
        if (!result.Succeeded)
        {
            return result.IsNotFound
                ? NotFound(new { message = result.Message ?? "Customer part purchase invoice not found." })
                : BadRequest(new { message = result.Message ?? "Invoice email could not be sent." });
        }

        return Ok(result.Value);
    }
}

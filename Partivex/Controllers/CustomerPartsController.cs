using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/customer/parts")]
[Authorize(Roles = ApplicationRoles.AdminStaffAndCustomer)]
public sealed class CustomerPartsController : ControllerBase
{
    private readonly ICustomerPartPurchaseService _partPurchaseService;

    public CustomerPartsController(ICustomerPartPurchaseService partPurchaseService)
    {
        _partPurchaseService = partPurchaseService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerPartCatalogDto>>> GetCatalog(CancellationToken cancellationToken)
    {
        return Ok(await _partPurchaseService.GetCatalogAsync(cancellationToken));
    }

    [HttpPost("checkout")]
    [Authorize(Roles = ApplicationRoles.Customer)]
    public async Task<ActionResult<CustomerPartInvoiceDto>> Checkout(
        CustomerPartCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _partPurchaseService.CheckoutAsync(
            GetCustomerId(),
            request,
            request.Items.Count == 1 ? "BuyNow" : "CartCheckout",
            cancellationToken);

        if (!result.Succeeded)
        {
            return ToProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpGet("invoices")]
    [Authorize(Roles = ApplicationRoles.Customer)]
    public async Task<ActionResult<IReadOnlyList<CustomerPartInvoiceDto>>> GetMyInvoices(CancellationToken cancellationToken)
    {
        return Ok(await _partPurchaseService.GetCustomerInvoicesAsync(GetCustomerId(), cancellationToken));
    }

    [HttpGet("invoices/{id:int}/pdf")]
    [Authorize(Roles = ApplicationRoles.Customer)]
    public async Task<IActionResult> DownloadMyInvoicePdf(int id, CancellationToken cancellationToken)
    {
        var result = await _partPurchaseService.GenerateInvoicePdfAsync(id, GetCustomerId(), cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound(new { message = "Customer part purchase invoice not found." });
        }

        return File(result.Value!, "application/pdf", $"customer-part-invoice-{id}.pdf");
    }

    private string GetCustomerId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Invalid token.");
    }

    private ActionResult ToProblem<T>(CustomerPartPurchaseResult<T> result)
    {
        if (result.IsNotFound)
        {
            return NotFound(new { message = result.Message ?? "Resource not found." });
        }

        return BadRequest(new
        {
            success = false,
            message = result.Message ?? result.Errors.FirstOrDefault()?.Description ?? "Request could not be completed.",
            errors = result.Errors.Select(error => error.Description).ToArray()
        });
    }
}

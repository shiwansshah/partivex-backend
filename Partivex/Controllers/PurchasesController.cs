using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;

    public PurchasesController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PurchaseInvoiceDto>>> GetAll(CancellationToken cancellationToken)
    {
        var invoices = await _purchaseService.GetAllAsync(cancellationToken);
        return Ok(invoices);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PurchaseInvoiceDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _purchaseService.GetByIdAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            return result.IsNotFound ? NotFound(new { message = "Purchase invoice not found." }) : BadRequest();
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseInvoiceDto>> Create(
        CreatePurchaseInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _purchaseService.CreateAsync(request.ToCommand(), cancellationToken);
        if (!result.Succeeded)
        {
            return ToProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<ActionResult<PurchaseInvoiceDto>> Confirm(int id, CancellationToken cancellationToken)
    {
        var result = await _purchaseService.ConfirmAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            return ToProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _purchaseService.DeleteAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            return ToProblem(result);
        }

        return Ok(new { id = result.Value });
    }

    private ActionResult ToProblem<T>(PurchaseResult<T> result)
    {
        if (result.IsNotFound)
        {
            return NotFound(new { message = "Purchase invoice not found." });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem(ModelState);
    }
}

public sealed class CreatePurchaseInvoiceRequest
{
    [Required]
    [MaxLength(40)]
    public string InvoiceNumber { get; init; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string VendorName { get; init; } = string.Empty;

    public DateTimeOffset InvoiceDate { get; init; } = DateTimeOffset.UtcNow;

    [Required]
    [MaxLength(120)]
    public string CreatedBy { get; init; } = string.Empty;

    [MaxLength(500)]
    public string Notes { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<PurchaseLineRequest> Lines { get; init; } = [];

    public CreatePurchaseInvoiceCommand ToCommand()
    {
        return new CreatePurchaseInvoiceCommand(
            InvoiceNumber,
            VendorName,
            InvoiceDate,
            CreatedBy,
            Notes,
            Lines.Select(l => new PurchaseInvoiceLineCommand(l.InventoryItemId, l.Quantity, l.UnitCost)).ToArray());
    }
}

public sealed class PurchaseLineRequest
{
    [Range(1, int.MaxValue)]
    public int InventoryItemId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    [Range(typeof(decimal), "0", "9999999999")]
    public decimal UnitCost { get; init; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Partivex.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = ApplicationRoles.Admin)]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<InventoryMonitoringDto>> GetMonitoring(CancellationToken cancellationToken)
    {
        var monitoring = await _inventoryService.GetMonitoringAsync(cancellationToken);
        return Ok(monitoring);
    }

    [HttpGet("items")]
    public async Task<ActionResult<IReadOnlyCollection<InventoryItemDto>>> GetItems(CancellationToken cancellationToken)
    {
        var items = await _inventoryService.GetItemsAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("changes")]
    public async Task<ActionResult<IReadOnlyCollection<InventoryStockChangeDto>>> GetStockChanges(CancellationToken cancellationToken)
    {
        var changes = await _inventoryService.GetRecentStockChangesAsync(cancellationToken);
        return Ok(changes);
    }

    [HttpPost("stock")]
    public async Task<ActionResult<PurchaseInvoiceDto>> AddStock(
        AddStockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.AddStockAsync(request.ToCommand(), cancellationToken);
        if (!result.Succeeded)
        {
            return ToResultProblem(result);
        }

        return Ok(result.Value);
    }

    private ActionResult ToResultProblem<T>(InventoryResult<T> result)
    {
        if (result.IsNotFound)
        {
            return NotFound(new { message = "Active vendor or part not found." });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem(ModelState);
    }
}

public sealed class AddStockRequest
{
    [Range(1, int.MaxValue)]
    public int VendorId { get; init; }

    [Range(1, int.MaxValue)]
    public int PartId { get; init; }

    [Range(1, int.MaxValue)]
    public int PurchaseQuantity { get; init; }

    public DateTimeOffset PurchaseDate { get; init; } = DateTimeOffset.UtcNow;

    [MaxLength(40)]
    public string InvoiceNumber { get; init; } = string.Empty;

    [MaxLength(120)]
    public string ChangedBy { get; init; } = string.Empty;

    [MaxLength(500)]
    public string Remarks { get; init; } = string.Empty;

    public AddStockCommand ToCommand()
    {
        return new AddStockCommand(
            VendorId,
            PartId,
            PurchaseQuantity,
            PurchaseDate,
            InvoiceNumber,
            ChangedBy,
            Remarks);
    }
}

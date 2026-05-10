using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

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

    [HttpPost]
    public async Task<ActionResult<InventoryItemDto>> CreateItem(
        UpsertInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.CreateItemAsync(request.ToCommand(), cancellationToken);
        if (!result.Succeeded)
        {
            return ToResultProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InventoryItemDto>> UpdateItem(
        int id,
        UpsertInventoryItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _inventoryService.UpdateItemAsync(id, request.ToCommand(), cancellationToken);
        if (!result.Succeeded)
        {
            return ToResultProblem(result);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<InventoryDeletedResponse>> DeleteItem(int id, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.DeleteItemAsync(id, cancellationToken);
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
            return NotFound(new { message = "Inventory item not found." });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem(ModelState);
    }
}

public sealed class UpsertInventoryItemRequest
{
    [Required]
    [MaxLength(40)]
    public string PartNumber { get; init; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string Category { get; init; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string VendorName { get; init; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string StorageLocation { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; init; }

    [Range(0, int.MaxValue)]
    public int ReorderLevel { get; init; }

    [Range(typeof(decimal), "0", "9999999999")]
    public decimal UnitCost { get; init; }

    [Required]
    [MaxLength(120)]
    public string ChangedBy { get; init; } = string.Empty;

    [MaxLength(40)]
    public string ReferenceCode { get; init; } = string.Empty;

    [MaxLength(240)]
    public string Notes { get; init; } = string.Empty;

    [MaxLength(40)]
    public string StockChangeType { get; init; } = string.Empty;

    public UpsertInventoryItemCommand ToCommand()
    {
        return new UpsertInventoryItemCommand(
            PartNumber,
            Name,
            Category,
            VendorName,
            StorageLocation,
            QuantityInStock,
            ReorderLevel,
            UnitCost,
            ChangedBy,
            ReferenceCode,
            Notes,
            StockChangeType);
    }
}

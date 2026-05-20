using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Partivex.Application.Constants;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;

namespace Partivex.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;
    private readonly INotificationService _notificationService;
    private readonly IInventoryRepository _inventoryRepository;

    public SalesController(ISalesService salesService, INotificationService notificationService, IInventoryRepository inventoryRepository)
    {
        _salesService = salesService;
        _notificationService = notificationService;
        _inventoryRepository = inventoryRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SaleDto>>> GetAll(CancellationToken cancellationToken)
    {
        var sales = await _salesService.GetAllAsync(cancellationToken);
        return Ok(sales);
    }

    [HttpGet("reports")]
    public async Task<ActionResult<SalesReportSummaryDto>> GetReports(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var reports = await _salesService.GetReportSummaryAsync(from, to, cancellationToken);
        return Ok(reports);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SaleDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _salesService.GetByIdAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            return result.IsNotFound ? NotFound(new { message = "Sale not found." }) : BadRequest();
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<SaleDto>> Create(CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var result = await _salesService.CreateAsync(request.ToCommand(), cancellationToken);
        if (!result.Succeeded)
        {
            return ToProblem(result);
        }

        var sale = result.Value!;
        var invoiceNumber = sale.Invoice?.InvoiceNumber ?? $"Sale #{sale.Id}";

        _ = Task.Run(async () =>
        {
            try
            {
                await _notificationService.CreateAsync(
                    ApplicationRoles.AdminAndStaff, null,
                    "New Sale Created",
                    $"Invoice {invoiceNumber} — {request.CustomerName} ({request.VehicleNo}) · Rs. {sale.TotalAmount:N2}",
                    "NewSale");

                var soldIds = request.Lines.Select(l => l.InventoryItemId).Distinct().ToArray();
                var items = await _inventoryRepository.GetItemsByIdsAsync(soldIds, CancellationToken.None);
                foreach (var item in items.Where(i => i.QuantityInStock < 10))
                {
                    await _notificationService.CreateAsync(
                        ApplicationRoles.Admin, null,
                        "Low Stock Alert",
                        $"{item.Name} (#{item.PartNumber}) has only {item.QuantityInStock} unit(s) remaining.",
                        "LowStock");
                }
            }
            catch { /* non-critical */ }
        });

        return Ok(sale);
    }

    private ActionResult ToProblem<T>(SalesResult<T> result)
    {
        if (result.IsNotFound)
        {
            return NotFound(new { message = "Sale not found." });
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Code, error.Description);
        }

        return ValidationProblem(ModelState);
    }
}

public sealed class CreateSaleRequest
{
    [Required]
    [MaxLength(120)]
    public string CustomerName { get; init; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string VehicleNo { get; init; } = string.Empty;

    [Required]
    [MaxLength(120)]
    public string SoldBy { get; init; } = string.Empty;

    public DateOnly SaleDate { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [MaxLength(500)]
    public string Notes { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CreateSaleLineRequest> Lines { get; init; } = [];

    public CreateSaleCommand ToCommand()
    {
        return new CreateSaleCommand(
            CustomerName,
            VehicleNo,
            SoldBy,
            SaleDate,
            Notes,
            Lines.Select(line => new CreateSaleLineCommand(
                line.InventoryItemId,
                line.Quantity,
                line.UnitPrice)).ToArray());
    }
}

public sealed class CreateSaleLineRequest
{
    [Range(1, int.MaxValue)]
    public int InventoryItemId { get; init; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; init; }

    [Range(typeof(decimal), "0.01", "9999999999")]
    public decimal UnitPrice { get; init; }
}

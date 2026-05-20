using System.Text;
using Partivex.Application.DTOs;
using Partivex.Application.Interfaces;
using Partivex.Domain.Entities;
using Partivex.Domain.Enums;

namespace Partivex.Application.Services;

public sealed class CustomerPartPurchaseService : ICustomerPartPurchaseService
{
    private const decimal LoyaltyThreshold = 5000m;
    private const decimal LoyaltyDiscountRate = 0.05m;
    private readonly ICustomerPartPurchaseRepository _invoiceRepository;
    private readonly IPartRepository _partRepository;
    private readonly IPartRequestRepository _partRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICustomerInvoiceEmailService _emailService;

    public CustomerPartPurchaseService(
        ICustomerPartPurchaseRepository invoiceRepository,
        IPartRepository partRepository,
        IPartRequestRepository partRequestRepository,
        IUserRepository userRepository,
        ICustomerInvoiceEmailService emailService)
    {
        _invoiceRepository = invoiceRepository;
        _partRepository = partRepository;
        _partRequestRepository = partRequestRepository;
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<IReadOnlyList<CustomerPartCatalogDto>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var parts = await _partRepository.GetActiveCatalogAsync(cancellationToken);
        return parts.Select(MapCatalog).ToArray();
    }

    public Task<CustomerPartPurchaseResult<CustomerPartInvoiceDto>> CheckoutAsync(
        string customerId,
        CustomerPartCheckoutRequest request,
        string source,
        CancellationToken cancellationToken = default)
    {
        return CreateInvoiceAsync(customerId, request.Items, source, "Customer checkout", null, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerPartInvoiceDto>> GetAllInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetAllAsync(cancellationToken);
        return invoices.Select(MapInvoice).ToArray();
    }

    public async Task<IReadOnlyList<CustomerPartInvoiceDto>> GetCustomerInvoicesAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return invoices.Select(MapInvoice).ToArray();
    }

    public async Task<CustomerPartPurchaseResult<CustomerPartInvoiceDto>> GetInvoiceAsync(
        int id,
        string? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice is null || (!string.IsNullOrWhiteSpace(customerId) && invoice.CustomerId != customerId))
        {
            return CustomerPartPurchaseResult<CustomerPartInvoiceDto>.NotFound("Customer part purchase invoice not found.");
        }

        return CustomerPartPurchaseResult<CustomerPartInvoiceDto>.Success(MapInvoice(invoice));
    }

    public async Task<CustomerPartPurchaseResult<StaffPartRequestApprovalResultDto>> ApprovePartRequestAsync(
        Guid partRequestId,
        ApprovePartRequestDto request,
        string staffIdentifier,
        CancellationToken cancellationToken = default)
    {
        var partRequest = await _partRequestRepository.GetByIdAsync(partRequestId, cancellationToken);
        if (partRequest is null)
        {
            return CustomerPartPurchaseResult<StaffPartRequestApprovalResultDto>.NotFound("Part request not found.");
        }

        if (partRequest.Status != PartRequestStatus.Pending)
        {
            return CustomerPartPurchaseResult<StaffPartRequestApprovalResultDto>.Failed(
            [
                new CustomerPartPurchaseError(nameof(partRequest.Status), "Only pending customer part requests can be approved.")
            ],
            "Part request could not be approved.");
        }

        var selectedPartId = request.PartId ?? partRequest.PartId;
        if (!selectedPartId.HasValue)
        {
            return CustomerPartPurchaseResult<StaffPartRequestApprovalResultDto>.Failed(
            [
                new CustomerPartPurchaseError(nameof(request.PartId), "Select a catalog part to sell for this request.")
            ]);
        }

        var invoiceResult = await CreateInvoiceAsync(
            partRequest.CustomerId,
            [new CustomerPartCheckoutLineRequest { PartId = selectedPartId.Value, Quantity = partRequest.Quantity }],
            "RequestApproval",
            staffIdentifier,
            partRequest.Id,
            cancellationToken);

        if (!invoiceResult.Succeeded)
        {
            if (invoiceResult.IsNotFound)
            {
                return CustomerPartPurchaseResult<StaffPartRequestApprovalResultDto>.NotFound(
                    invoiceResult.Message ?? "Invoice could not be created.");
            }

            return CustomerPartPurchaseResult<StaffPartRequestApprovalResultDto>.Failed(
                invoiceResult.Errors,
                invoiceResult.Message ?? "Invoice could not be created.");
        }

        partRequest.PartId = selectedPartId.Value;
        partRequest.Status = PartRequestStatus.Approved;
        partRequest.UpdatedAt = DateTimeOffset.UtcNow;
        await _partRequestRepository.SaveChangesAsync(cancellationToken);

        CustomerPartInvoiceEmailResult? emailResult = null;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var sendResult = await SendInvoiceEmailAsync(invoiceResult.Value!.Id, request.Email.Trim(), cancellationToken);
            emailResult = sendResult.Value;
        }

        return CustomerPartPurchaseResult<StaffPartRequestApprovalResultDto>.Success(
            new StaffPartRequestApprovalResultDto(invoiceResult.Value!, emailResult));
    }

    public async Task<CustomerPartPurchaseResult<CustomerPartInvoiceEmailResult>> SendInvoiceEmailAsync(
        int id,
        string email,
        CancellationToken cancellationToken = default)
    {
        var invoiceResult = await GetInvoiceAsync(id, null, cancellationToken);
        if (!invoiceResult.Succeeded)
        {
            return CustomerPartPurchaseResult<CustomerPartInvoiceEmailResult>.NotFound("Customer part purchase invoice not found.");
        }

        var pdfResult = await GenerateInvoicePdfAsync(id, null, cancellationToken);
        var emailResult = await _emailService.SendInvoiceAsync(invoiceResult.Value!, email.Trim(), pdfResult.Value!, cancellationToken);

        return CustomerPartPurchaseResult<CustomerPartInvoiceEmailResult>.Success(emailResult);
    }

    public async Task<CustomerPartPurchaseResult<byte[]>> GenerateInvoicePdfAsync(
        int id,
        string? customerId = null,
        CancellationToken cancellationToken = default)
    {
        var invoiceResult = await GetInvoiceAsync(id, customerId, cancellationToken);
        if (!invoiceResult.Succeeded)
        {
            return CustomerPartPurchaseResult<byte[]>.NotFound("Customer part purchase invoice not found.");
        }

        return CustomerPartPurchaseResult<byte[]>.Success(BuildInvoicePdf(invoiceResult.Value!));
    }

    private async Task<CustomerPartPurchaseResult<CustomerPartInvoiceDto>> CreateInvoiceAsync(
        string customerId,
        IReadOnlyList<CustomerPartCheckoutLineRequest> requestedItems,
        string source,
        string createdBy,
        Guid? partRequestId,
        CancellationToken cancellationToken)
    {
        var customer = await _userRepository.FindByIdAsync(customerId);
        if (customer is null)
        {
            return CustomerPartPurchaseResult<CustomerPartInvoiceDto>.NotFound("Customer not found.");
        }

        var errors = ValidateCheckout(requestedItems);
        if (errors.Count > 0)
        {
            return CustomerPartPurchaseResult<CustomerPartInvoiceDto>.Failed(errors, "Checkout could not be completed.");
        }

        var groupedLines = requestedItems
            .GroupBy(item => item.PartId)
            .Select(group => new CustomerPartCheckoutLineRequest
            {
                PartId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToArray();

        var parts = new List<(Part Part, int Quantity)>();
        foreach (var line in groupedLines)
        {
            var part = await _partRepository.GetActiveByIdAsync(line.PartId, cancellationToken);
            if (part is null)
            {
                errors.Add(new CustomerPartPurchaseError(nameof(line.PartId), "One of the selected parts is no longer available."));
                continue;
            }

            if (part.CurrentStock <= 0)
            {
                errors.Add(new CustomerPartPurchaseError(part.PartCode, $"{part.Name} is out of stock."));
                continue;
            }

            if (line.Quantity > part.CurrentStock)
            {
                errors.Add(new CustomerPartPurchaseError(part.PartCode, $"Only {part.CurrentStock} units of {part.Name} are available."));
                continue;
            }

            parts.Add((part, line.Quantity));
        }

        if (errors.Count > 0)
        {
            return CustomerPartPurchaseResult<CustomerPartInvoiceDto>.Failed(errors, "Checkout could not be completed.");
        }

        var subTotal = parts.Sum(item => item.Part.UnitPrice * item.Quantity);
        var discountAmount = subTotal > LoyaltyThreshold ? Math.Round(subTotal * LoyaltyDiscountRate, 2) : 0m;
        var invoice = new CustomerPartPurchaseInvoice
        {
            InvoiceNumber = await CreateInvoiceNumberAsync(cancellationToken),
            CustomerId = customer.Id,
            CustomerName = string.IsNullOrWhiteSpace(customer.FullName) ? customer.Email ?? "Customer" : customer.FullName,
            CustomerEmail = customer.Email ?? string.Empty,
            InvoiceDate = DateTimeOffset.UtcNow,
            Source = source,
            Status = "Paid",
            SubTotal = subTotal,
            DiscountAmount = discountAmount,
            TotalAmount = subTotal - discountAmount,
            CreatedBy = createdBy,
            PartRequestId = partRequestId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var line in parts)
        {
            line.Part.CurrentStock -= line.Quantity;
            line.Part.UpdatedAt = DateTime.UtcNow;

            invoice.Items.Add(new CustomerPartPurchaseInvoiceItem
            {
                PartId = line.Part.Id,
                PartCode = line.Part.PartCode,
                PartName = line.Part.Name,
                Quantity = line.Quantity,
                UnitPrice = line.Part.UnitPrice,
                SubTotal = line.Part.UnitPrice * line.Quantity
            });
        }

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        var saved = await _invoiceRepository.GetByIdAsync(invoice.Id, cancellationToken);
        return CustomerPartPurchaseResult<CustomerPartInvoiceDto>.Success(MapInvoice(saved!));
    }

    private async Task<string> CreateInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        string invoiceNumber;
        do
        {
            invoiceNumber = $"CPI-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
        }
        while (await _invoiceRepository.InvoiceNumberExistsAsync(invoiceNumber, cancellationToken));

        return invoiceNumber;
    }

    private static List<CustomerPartPurchaseError> ValidateCheckout(IReadOnlyList<CustomerPartCheckoutLineRequest> items)
    {
        var errors = new List<CustomerPartPurchaseError>();
        if (items.Count == 0)
        {
            errors.Add(new CustomerPartPurchaseError(nameof(items), "Add at least one part to checkout."));
        }

        if (items.Any(item => item.PartId <= 0 || item.Quantity <= 0))
        {
            errors.Add(new CustomerPartPurchaseError(nameof(items), "Every checkout item must have a valid part and quantity."));
        }

        return errors;
    }

    private static CustomerPartCatalogDto MapCatalog(Part part)
    {
        return new CustomerPartCatalogDto(
            part.Id,
            part.Name,
            part.PartCode,
            part.Category,
            part.CompatibleVehicle,
            part.UnitPrice,
            part.CurrentStock,
            part.MinimumStockLevel,
            GetStockStatus(part.CurrentStock, part.MinimumStockLevel),
            part.ImageUrl);
    }

    private static CustomerPartInvoiceDto MapInvoice(CustomerPartPurchaseInvoice invoice)
    {
        var items = invoice.Items
            .Select(item => new CustomerPartInvoiceItemDto(
                item.Id,
                item.PartId,
                item.PartCode,
                item.PartName,
                item.Quantity,
                item.UnitPrice,
                item.SubTotal))
            .ToArray();

        return new CustomerPartInvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.CustomerId,
            invoice.CustomerName,
            invoice.CustomerEmail,
            invoice.InvoiceDate,
            invoice.Source,
            invoice.Status,
            invoice.SubTotal,
            invoice.DiscountAmount,
            invoice.TotalAmount,
            invoice.CreatedBy,
            invoice.PartRequestId,
            invoice.CreatedAt,
            items);
    }

    private static string GetStockStatus(int currentStock, int minimumStockLevel)
    {
        if (currentStock == 0) return "Out of Stock";
        return currentStock <= minimumStockLevel ? "Low Stock" : "In Stock";
    }

    private static byte[] BuildInvoicePdf(CustomerPartInvoiceDto invoice)
    {
        var lines = new List<string>
        {
            "Partivex",
            $"Customer Part Purchase Invoice {invoice.InvoiceNumber}",
            $"Date: {invoice.InvoiceDate:yyyy-MM-dd HH:mm}",
            $"Customer: {invoice.CustomerName}",
            $"Email: {invoice.CustomerEmail}",
            $"Status: {invoice.Status}",
            "",
            "Items"
        };

        lines.AddRange(invoice.Items.Select(item =>
            $"{item.PartCode}  {item.PartName}  Qty {item.Quantity}  Unit NPR {item.UnitPrice:0.00}  Total NPR {item.SubTotal:0.00}"));
        lines.Add("");
        lines.Add($"Subtotal: NPR {invoice.SubTotal:0.00}");
        lines.Add($"Loyalty Discount: NPR {invoice.DiscountAmount:0.00}");
        lines.Add($"Total: NPR {invoice.TotalAmount:0.00}");

        return SimplePdf(lines);
    }

    private static byte[] SimplePdf(IReadOnlyList<string> lines)
    {
        var objects = new List<string>();
        var content = new StringBuilder("BT\n/F1 11 Tf\n50 790 Td\n14 TL\n");
        foreach (var line in lines)
        {
            content.Append('(').Append(EscapePdfText(line)).Append(") Tj\nT*\n");
        }
        content.Append("ET");

        objects.Add("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");
        objects.Add("2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n");
        objects.Add("3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\nendobj\n");
        objects.Add("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n");
        objects.Add($"5 0 obj\n<< /Length {Encoding.ASCII.GetByteCount(content.ToString())} >>\nstream\n{content}\nendstream\nendobj\n");

        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int> { 0 };
        foreach (var obj in objects)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append(obj);
        }

        var xrefOffset = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append("xref\n0 6\n0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++)
        {
            pdf.Append(offsets[i].ToString("0000000000")).Append(" 00000 n \n");
        }
        pdf.Append("trailer\n<< /Root 1 0 R /Size 6 >>\nstartxref\n")
            .Append(xrefOffset)
            .Append("\n%%EOF");

        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static string EscapePdfText(string value)
    {
        return value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}

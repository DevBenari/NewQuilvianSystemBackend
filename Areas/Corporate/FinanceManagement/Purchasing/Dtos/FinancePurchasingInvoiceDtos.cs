namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;

public sealed class PurchasingInvoiceItemRequestDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class PurchasingInvoiceItemResponse
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class PurchasingInvoiceResponse
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid InvoiceExchangeId { get; set; }
    public Guid SupplierId { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PPNAmount { get; set; }
    public decimal DownPaymentAmount { get; set; }
    public decimal OtherDeductionAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ApprovalTier { get; set; } = string.Empty;
    public Guid RequestedByUserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class PurchasingInvoiceDetailResponse : PurchasingInvoiceResponse
{
    public List<PurchasingInvoiceItemResponse> Items { get; set; } = [];
}

public sealed class CreatePurchasingInvoiceRequest
{
    public Guid InvoiceExchangeId { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PPNAmount { get; set; }
    public decimal DownPaymentAmount { get; set; }
    public decimal OtherDeductionAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchasingInvoiceItemRequestDto> Items { get; set; } = [];
}

public sealed class UpdatePurchasingInvoiceRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PPNAmount { get; set; }
    public decimal DownPaymentAmount { get; set; }
    public decimal OtherDeductionAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public List<PurchasingInvoiceItemRequestDto> Items { get; set; } = [];
}

public sealed class PurchasingInvoiceRowVersionRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

public sealed class RejectPurchasingInvoiceRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

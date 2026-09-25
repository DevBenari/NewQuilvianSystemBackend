namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Dtos;

public sealed class SupplierPayableItemResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}

public sealed class PayableAdjustmentResponse
{
    public Guid Id { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public string PayableType { get; set; } = string.Empty;
    public Guid? SupplierPayableId { get; set; }
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid RowVersion { get; set; }
}

public class SupplierPayableResponse
{
    public Guid Id { get; set; }
    public string PayableNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly SupplierInvoiceDate { get; set; }
    public string? Description { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal AdjustedAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public int PaymentTermDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RowVersion { get; set; }
}

public sealed class SupplierPayableDetailResponse : SupplierPayableResponse
{
    public List<SupplierPayableItemResponse> Items { get; set; } = [];
    public List<PayableAdjustmentResponse> Adjustments { get; set; } = [];
}

public sealed class SupplierPayableItemRequestDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class CreateSupplierPayableRequest
{
    public Guid SupplierId { get; set; }
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly SupplierInvoiceDate { get; set; }
    public string? Description { get; set; }
    public List<SupplierPayableItemRequestDto> Items { get; set; } = [];
}

public sealed class RequestPayableAdjustmentRequest
{
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class DecidePayableAdjustmentRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string? RejectionReason { get; set; }
}

public sealed class CancelSupplierPayableRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

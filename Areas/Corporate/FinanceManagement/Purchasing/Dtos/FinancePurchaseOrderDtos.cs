namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;

public sealed class PurchaseOrderItemRequestDto
{
    public string ProductCategory { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class PurchaseOrderItemResponse
{
    public Guid Id { get; set; }
    public string ProductCategory { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class PurchaseOrderResponse
{
    public Guid Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string ApprovalTier { get; set; } = string.Empty;
    public Guid RequestedByUserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class PurchaseOrderDetailResponse : PurchaseOrderResponse
{
    public List<PurchaseOrderItemResponse> Items { get; set; } = [];
}

public sealed class CreatePurchaseOrderRequest
{
    public Guid SupplierId { get; set; }
    public List<PurchaseOrderItemRequestDto> Items { get; set; } = [];
}

public sealed class UpdatePurchaseOrderRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public List<PurchaseOrderItemRequestDto> Items { get; set; } = [];
}

public sealed class PurchaseOrderRowVersionRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

public sealed class RejectPurchaseOrderRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

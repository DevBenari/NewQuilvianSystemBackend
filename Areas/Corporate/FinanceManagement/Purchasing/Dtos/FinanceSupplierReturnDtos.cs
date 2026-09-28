using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;

public class SupplierReturnItemRequestDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }

    /// <summary>Dipakai service untuk menghitung LineTotal — FinSupplierReturnItem tidak
    /// menyimpan UnitPrice sendiri (hanya Description/Quantity/LineTotal).</summary>
    public decimal UnitPrice { get; set; }
}

public sealed class CreateSupplierReturnRequest
{
    public Guid PurchasingInvoiceId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<SupplierReturnItemRequestDto> Items { get; set; } = new();
}

public sealed class SupplierReturnRowVersionRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

public class SupplierReturnItemResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class SupplierReturnResponse
{
    public Guid Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public Guid PurchasingInvoiceId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RowVersion { get; set; }
}

public sealed class SupplierReturnDetailResponse : SupplierReturnResponse
{
    public List<SupplierReturnItemResponse> Items { get; set; } = new();
    public SupplierReturnDepositResponse? Deposit { get; set; }
}

public sealed class SupplierReturnDepositResponse
{
    public Guid Id { get; set; }
    public Guid SupplierId { get; set; }
    public Guid SourceReturnId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RowVersion { get; set; }
}

public sealed class SupplierReturnDepositQuery
{
    public Guid? SupplierId { get; set; }
    public string? Status { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

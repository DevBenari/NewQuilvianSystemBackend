using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;

public class InvoiceExchangeResponse
{
    public Guid Id { get; set; }
    public string ExchangeNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? GoodsReceiptId { get; set; }
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly SupplierInvoiceDate { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public DateOnly EstimatedDueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RowVersion { get; set; }
}

public sealed class InvoiceExchangeDetailResponse : InvoiceExchangeResponse
{
}

/// <summary>
/// Sengaja TIDAK punya field EstimatedDueDate — dihitung backend dari ReceivedDate +
/// MstSupplier.PaymentTermDays (FR-FIN-082, FIN-DEC-051), nilai kiriman client tidak pernah
/// ada tempatnya untuk diterima, apalagi dipakai.
/// </summary>
public sealed class CreateInvoiceExchangeRequest
{
    public Guid SupplierId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? GoodsReceiptId { get; set; }
    public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly SupplierInvoiceDate { get; set; }
    public DateOnly ReceivedDate { get; set; }
}

public sealed class InvoiceExchangeRowVersionRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

/// <summary>GET / — daftar Tukar Faktur berpaging (FIN-API-1.1 §B.3, kolom Request
/// `InvoiceExchangeQuery`). Filter persis sesuai kontrak: supplier, status.</summary>
public sealed class InvoiceExchangeQuery
{
    public Guid? SupplierId { get; set; }
    public string? Status { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

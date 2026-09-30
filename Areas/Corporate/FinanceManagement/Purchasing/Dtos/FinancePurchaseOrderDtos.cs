using System.ComponentModel.DataAnnotations;

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

    /// <summary>Riwayat GR anak PO ini, ringkas — delta kontrak BE-FIN-032 (lihat laporan task):
    /// api-contract.md §B.1 tidak mendaftarkan field ini, ditambahkan karena tanpanya GR yang
    /// sudah tercatat pada sesi sebelumnya tidak dapat ditampilkan kembali lewat GET /{id}.</summary>
    public List<PurchaseOrderGoodsReceiptSummaryResponse> GoodsReceipts { get; set; } = [];
}

/// <summary>Ringkasan GR untuk ditampilkan di dalam rincian PO induknya — bukan bentuk penuh
/// GoodsReceiptResponse (tidak menyertakan Items baris GR, cukup untuk daftar riwayat).</summary>
public sealed class PurchaseOrderGoodsReceiptSummaryResponse
{
    public Guid Id { get; set; }
    public string GRNumber { get; set; } = string.Empty;
    public DateOnly ReceivedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>GET / — daftar PO berpaging (FIN-API-1.1 §B.1, kolom Request `PurchaseOrderQuery`).
/// Filter persis sesuai kontrak: supplier, status, tanggal (RequestedAt) — bukan filter tambahan
/// yang tidak diminta.</summary>
public sealed class PurchaseOrderQuery
{
    public Guid? SupplierId { get; set; }
    public string? Status { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
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

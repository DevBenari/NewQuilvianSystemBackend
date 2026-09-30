using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;

public sealed class GoodsReceiptItemRequestDto
{
    public Guid PurchaseOrderItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public string? Notes { get; set; }
}

public sealed class GoodsReceiptItemResponse
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderItemId { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class GoodsReceiptResponse
{
    public Guid Id { get; set; }
    public string GRNumber { get; set; } = string.Empty;
    public Guid PurchaseOrderId { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid RowVersion { get; set; }
}

public sealed class GoodsReceiptDetailResponse : GoodsReceiptResponse
{
    public List<GoodsReceiptItemResponse> Items { get; set; } = [];
}

public sealed class CreateGoodsReceiptRequest
{
    public Guid PurchaseOrderId { get; set; }
    public DateOnly ReceivedDate { get; set; }
    public List<GoodsReceiptItemRequestDto> Items { get; set; } = [];
}

public sealed class GoodsReceiptRowVersionRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

/// <summary>GET / — daftar GR berpaging (FIN-API-1.1 §B.2, kolom Request `GoodsReceiptQuery`).
/// SupplierId disaring lewat join ke PurchaseOrder.SupplierId — FinGoodsReceipt sendiri tidak
/// punya kolom SupplierId (PurchaseOrderId wajib terisi, lihat FinGoodsReceipt.cs).</summary>
public sealed class GoodsReceiptQuery
{
    public Guid? PurchaseOrderId { get; set; }
    public Guid? SupplierId { get; set; }
    public string? Status { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

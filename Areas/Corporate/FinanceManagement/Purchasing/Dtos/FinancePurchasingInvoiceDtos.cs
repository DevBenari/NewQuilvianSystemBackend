using System.ComponentModel.DataAnnotations;

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

    /// <summary>Bea materai (BE-FIN-123). Komponen penambah total.</summary>
    public decimal StampDutyAmount { get; set; }

    /// <summary>Ongkos kirim (BE-FIN-123). Komponen penambah total.</summary>
    public decimal ShippingCostAmount { get; set; }

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

    /// <summary>Opsional; bernilai 0 bila tidak dikirim (kompatibel dengan klien lama).</summary>
    public decimal StampDutyAmount { get; set; }

    /// <summary>Opsional; bernilai 0 bila tidak dikirim (kompatibel dengan klien lama).</summary>
    public decimal ShippingCostAmount { get; set; }

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

    /// <summary>Opsional; bernilai 0 bila tidak dikirim (kompatibel dengan klien lama).</summary>
    public decimal StampDutyAmount { get; set; }

    /// <summary>Opsional; bernilai 0 bila tidak dikirim (kompatibel dengan klien lama).</summary>
    public decimal ShippingCostAmount { get; set; }

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

/// <summary>GET / — daftar Purchasing Invoice berpaging (FIN-API-1.1 §B.4, kolom Request
/// `PurchasingInvoiceQuery`, BE-FIN-122, filter jatuh tempo BE-FIN-127). Filter: supplier, status,
/// dan rentang jatuh tempo AKTUAL.
///
/// Jatuh tempo aktual adalah <c>FinSupplierPayable.DueDate</c> milik utang yang dibentuk invoice
/// itu (<c>SourcePurchasingInvoiceId</c>); Purchasing Invoice sendiri tidak menyimpan tanggal.
/// Invoice yang belum membentuk utang (DRAFT, PENDING_APPROVAL, REJECTED, CANCELLED) TIDAK
/// ikut tersaring begitu salah satu <c>DueDateFrom</c>/<c>DueDateTo</c> diisi, dan utang berstatus
/// CANCELLED tidak dihitung. Estimasi jatuh tempo Tukar Faktur tidak dipakai menyaring; ia hanya
/// ditampilkan terpisah pada respons.</summary>
public sealed class PurchasingInvoiceQuery
{
    public Guid? SupplierId { get; set; }
    public string? Status { get; set; }

    /// <summary>Batas bawah (inklusif) jatuh tempo aktual utang dari invoice ini.</summary>
    public DateOnly? DueDateFrom { get; set; }

    /// <summary>Batas atas (inklusif) jatuh tempo aktual utang dari invoice ini.</summary>
    public DateOnly? DueDateTo { get; set; }

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>
/// Baris daftar Purchasing Invoice (BE-FIN-127): seluruh field <see cref="PurchasingInvoiceResponse"/>
/// ditambah informasi jatuh tempo. <see cref="ActualDueDate"/> kosong selama invoice belum membentuk
/// utang; <see cref="EstimatedDueDate"/> berasal dari Tukar Faktur dan tidak pernah menggantikan tanggal aktual.
/// </summary>
public sealed class PurchasingInvoiceListItemResponse : PurchasingInvoiceResponse
{
    /// <summary>Estimasi jatuh tempo pada Tukar Faktur sumber (tanggal dokumen diterima + termin).</summary>
    public DateOnly? EstimatedDueDate { get; set; }

    /// <summary>Jatuh tempo utang yang dibentuk invoice ini. Kosong bila belum ada utang.</summary>
    public DateOnly? ActualDueDate { get; set; }

    /// <summary>SUPPLIER_INVOICE_DATE atau GOODS_RECEIVED_DATE; kosong untuk utang tanpa basis tercatat.</summary>
    public string? DueDateBasis { get; set; }

    /// <summary>Status utang yang dibentuk invoice ini (OUTSTANDING, PARTIAL, PAID, CANCELLED); kosong bila belum ada.</summary>
    public string? PayableStatus { get; set; }
}

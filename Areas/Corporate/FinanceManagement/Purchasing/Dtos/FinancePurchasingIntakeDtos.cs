using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;

/// <summary>
/// Kategori antrean Penerimaan Invoice (P02). Antrean ini PROYEKSI dari dokumen yang sudah ada —
/// bukan tabel baru dan bukan status baru pada dokumen mana pun.
/// </summary>
public static class PurchasingIntakeCategories
{
    /// <summary>GR aktif yang belum punya Tukar Faktur aktif: barang sudah datang, faktur belum (SJ/faktur menyusul).</summary>
    public const string AwaitingInvoice = "AWAITING_INVOICE";

    /// <summary>Tukar Faktur RECEIVED yang belum memiliki Purchasing Invoice: siap disusun invoice.</summary>
    public const string ReadyForInvoice = "READY_FOR_INVOICE";

    /// <summary>Purchasing Invoice DRAFT atau PENDING_APPROVAL: sedang diverifikasi.</summary>
    public const string InVerification = "IN_VERIFICATION";

    /// <summary>Tukar Faktur RECEIVED yang invoice-nya hanya tersisa yang DIBATALKAN/DITOLAK: tidak bisa
    /// disusun ulang pada Tukar Faktur yang sama (invoice lama masih mengikat), harus dicatat ulang.</summary>
    public const string NeedsRebuild = "NEEDS_REBUILD";

    public static readonly IReadOnlyList<string> All =
        [AwaitingInvoice, ReadyForInvoice, InVerification, NeedsRebuild];
}

/// <summary>Tindak lanjut yang disarankan untuk satu baris antrean; klien memetakannya ke tombol.</summary>
public static class PurchasingIntakeNextActions
{
    public const string RecordInvoiceExchange = "RECORD_INVOICE_EXCHANGE";
    public const string ComposeInvoice = "COMPOSE_INVOICE";
    public const string ContinueVerification = "CONTINUE_VERIFICATION";
    public const string RebuildExchange = "REBUILD_EXCHANGE";
}

public sealed class PurchasingIntakeQuery
{
    /// <summary>Salah satu <see cref="PurchasingIntakeCategories"/>. Kosong = AWAITING_INVOICE.</summary>
    public string? Category { get; set; }
    public Guid? SupplierId { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class PurchasingIntakeRow
{
    public string Category { get; set; } = string.Empty;

    /// <summary>GOODS_RECEIPT, INVOICE_EXCHANGE, atau PURCHASING_INVOICE (<see cref="PurchasingDocumentTypes"/>).</summary>
    public string DocumentType { get; set; } = string.Empty;
    public Guid DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public DateOnly DocumentDate { get; set; }
    public string Status { get; set; } = string.Empty;

    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;

    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public Guid? GoodsReceiptId { get; set; }

    /// <summary>Hanya terisi untuk baris Tukar Faktur.</summary>
    public string? SupplierInvoiceNumber { get; set; }
    public DateOnly? EstimatedDueDate { get; set; }

    /// <summary>Lama menunggu sejak tanggal dokumen (hari, tidak negatif).</summary>
    public int AgeDays { get; set; }

    public string NextAction { get; set; } = string.Empty;
}

public sealed class PurchasingIntakeSummaryItem
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class PurchasingIntakeSummaryResponse
{
    public List<PurchasingIntakeSummaryItem> Items { get; set; } = [];

    /// <summary>Kategori yang disembunyikan karena pemanggil tidak berhak membaca dokumen sumbernya.</summary>
    public List<string> RestrictedCategories { get; set; } = [];
}

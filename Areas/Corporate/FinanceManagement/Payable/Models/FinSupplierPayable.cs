using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

/// <summary>
/// Aggregate root utang supplier (FIN-DES-015 bagian supplier, FIN-DEC-015). Diinput manual staf
/// AP karena modul Purchasing belum ada — tidak ada PO yang bisa dicocokkan, sehingga pasangan
/// (SupplierId, SupplierInvoiceNumber) adalah satu-satunya penjaga terhadap invoice supplier
/// yang terinput dua kali (state-transition-matrix.md §5).
///
/// Invariant: OriginalAmount = OutstandingAmount + PaidAmount + AdjustedAmount — pola yang sama
/// dengan FinReceivable (FIN-VAL-011), diterapkan di sini walau kontrak tidak menyebutnya
/// eksplisit untuk Payable (lihat laporan BE-FIN-019 bagian 1.4 untuk transparansi ekstrapolasi
/// ini). PaidAmount MUST tetap nol sampai BE-FIN-020 (FinPayment/FinPaymentAllocation) membangun
/// jalur pembayarannya — satu-satunya penulis FinSupplierPayable pada task ini adalah
/// FinanceSupplierPayableService (input manual dan koreksi saja).
/// </summary>
[Table("FinSupplierPayable", Schema = "public")]
public sealed class FinSupplierPayable : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string PayableNumber { get; set; } = string.Empty;

    /// <summary>Milik Administrator (FIN-DEC-014) — Finance MUST NOT membuat master Supplier baru.</summary>
    public Guid SupplierId { get; set; }
    public MstSupplier? Supplier { get; set; }

    [Required, MaxLength(100)] public string SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly SupplierInvoiceDate { get; set; }

    [MaxLength(300)] public string? Description { get; set; }

    public decimal OriginalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0m;
    public decimal AdjustedAmount { get; set; } = 0m;

    public DateOnly DueDate { get; set; }

    /// <summary>
    /// Tanggal acuan yang dipakai menghitung <see cref="DueDate"/> (BE-FIN-124, AP-DEC-03):
    /// <see cref="FinSupplierPayableDueDateBases"/>. NULL untuk baris yang dibuat sebelum kolom ini
    /// ada dan untuk saldo awal hasil migrasi (jatuh tempo dibawa dari berkas migrasi) — basisnya
    /// tidak tercatat, bukan berarti kosong.
    /// </summary>
    [MaxLength(30)] public string? DueDateBasis { get; set; }

    /// <summary>Disalin dari MstSupplier.PaymentTermDays saat dibuat — perubahan termin supplier setelahnya MUST NOT mengubah utang yang sudah tercatat.</summary>
    public int PaymentTermDays { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinSupplierPayableStatuses.Outstanding;

    /// <summary>
    /// BE-FIN-030, FIN-DES-040: terisi otomatis saat baris ini dibuat dari FinPurchasingInvoice
    /// Approved (BE-FIN-034). NULL untuk baris lama dan baris yang tetap diinput manual — jalur
    /// input manual TIDAK dihapus, tetap fallback yang sah (FIN-DEC-045).
    /// </summary>
    public Guid? SourcePurchasingInvoiceId { get; set; }
    public FinPurchasingInvoice? SourcePurchasingInvoice { get; set; }

    /// <summary>BE-FIN-079, FIN-DES-093: terisi hanya untuk item migrasi utang lama. Nol kolom lain berubah — SupplierId tetap wajib (R14.10).</summary>
    public Guid? OpeningItemBatchId { get; set; }
    public FinOpeningItemBatch? OpeningItemBatch { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinSupplierPayableItem> Items { get; set; } = new List<FinSupplierPayableItem>();
    public ICollection<FinPayableAdjustment> Adjustments { get; set; } = new List<FinPayableAdjustment>();

    /// <summary>BE-FIN-058, FIN-DES-079: Buku mutasi perubahan saldo utang supplier.</summary>
    public ICollection<FinSupplierPayableMovement> Movements { get; set; } = new List<FinSupplierPayableMovement>();
}

/// <summary>Dasar perhitungan jatuh tempo utang supplier (BE-FIN-124).</summary>
public static class FinSupplierPayableDueDateBases
{
    /// <summary>Tanggal faktur supplier + termin. Jalur input manual dan invoice tanpa GR.</summary>
    public const string SupplierInvoiceDate = "SUPPLIER_INVOICE_DATE";

    /// <summary>Tanggal barang diterima (GR) + termin. Invoice yang Tukar Fakturnya merujuk GR (default BLM).</summary>
    public const string GoodsReceivedDate = "GOODS_RECEIVED_DATE";
}

public static class FinSupplierPayableStatuses
{
    public const string Outstanding = "OUTSTANDING";
    public const string Partial = "PARTIAL";
    public const string Paid = "PAID";
    public const string Cancelled = "CANCELLED";
}

using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
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

    /// <summary>Disalin dari MstSupplier.PaymentTermDays saat dibuat — perubahan termin supplier setelahnya MUST NOT mengubah utang yang sudah tercatat.</summary>
    public int PaymentTermDays { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinSupplierPayableStatuses.Outstanding;

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinSupplierPayableItem> Items { get; set; } = new List<FinSupplierPayableItem>();
    public ICollection<FinPayableAdjustment> Adjustments { get; set; } = new List<FinPayableAdjustment>();
}

public static class FinSupplierPayableStatuses
{
    public const string Outstanding = "OUTSTANDING";
    public const string Partial = "PARTIAL";
    public const string Paid = "PAID";
    public const string Cancelled = "CANCELLED";
}

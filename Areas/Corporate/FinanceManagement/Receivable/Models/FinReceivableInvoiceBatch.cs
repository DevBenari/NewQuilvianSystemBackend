using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Aggregate root Batch Tagihan AR (FIN-DES-041, FIN-DEC-048, erd/data-dictionary.md §C.13-C.14).
/// Menggabungkan beberapa FinReceivable milik satu penjamin (DebtorReferenceId sama) menjadi satu
/// dokumen tagihan resmi ke penjamin. BUKAN pengganti FinReceivable — murni lapisan penagihan di
/// atasnya; status batch mengikuti/meringkas status anggotanya, tidak pernah menjadi sumber
/// kebenaran baru untuk pelunasan (FinanceReceivableService tetap satu-satunya penulis
/// OutstandingAmount).
/// </summary>
[Table("FinReceivableInvoiceBatch", Schema = "public")]
public sealed class FinReceivableInvoiceBatch : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string BatchNumber { get; set; } = string.Empty;

    /// <summary>Tetap PAYER pada rilis ini (FIN-DEC-048).</summary>
    [Required, MaxLength(30)] public string DebtorType { get; set; } = FinReceivableInvoiceBatchDebtorTypes.Payer;

    /// <summary>Kunci pengelompokan — sama dengan FinReceivable.DebtorReferenceId anggotanya.</summary>
    public Guid DebtorReferenceId { get; set; }

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }

    /// <summary>Tanggal pembuatan invoice yang ditentukan pengguna. Null berarti tanggal batch dibuat (hari ini WIB).</summary>
    public DateOnly? InvoiceDate { get; set; }

    /// <summary>Tanggal jatuh tempo batch tagihan (InvoiceDate + PaymentTermDays). Backend-owned.</summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>Tenor jatuh tempo dalam hari yang disnapshot saat batch dibuat.</summary>
    public int? PaymentTermDays { get; set; }

    /// <summary>Catatan atau keterangan dokumen batch tagihan.</summary>
    [MaxLength(500)] public string? Note { get; set; }

    /// <summary>Jumlah OriginalAmount seluruh FinReceivable anggota.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Snapshot kode kunjungan (IP / OP).</summary>
    [MaxLength(10)] public string? VisitCode { get; set; }

    /// <summary>Total diskon invoice yang disepakati/diterapkan.</summary>
    public decimal TotalDiscount { get; set; } = 0m;

    /// <summary>Rujukan akun COA diskon (AccChartOfAccount).</summary>
    public Guid? DiscountChartOfAccountId { get; set; }

    /// <summary>Jenis diskon: PERCENT atau NOMINAL.</summary>
    [MaxLength(20)] public string? DiscountType { get; set; }

    /// <summary>Persentase diskon bila jenis diskon adalah PERCENT.</summary>
    public decimal? DiscountPercent { get; set; }

    /// <summary>Keterangan atau alasan diskon invoice.</summary>
    [MaxLength(500)] public string? DiscountNote { get; set; }

    /// <summary>Nominal penerimaan tambahan lainnya di luar tagihan pokok.</summary>
    public decimal OtherReceiptAmount { get; set; } = 0m;

    /// <summary>Rujukan akun COA penerimaan lainnya (AccChartOfAccount).</summary>
    public Guid? OtherReceiptChartOfAccountId { get; set; }

    /// <summary>Keterangan penerimaan lainnya.</summary>
    [MaxLength(500)] public string? OtherReceiptNote { get; set; }

    [Required, MaxLength(20)] public string Status { get; set; } = FinReceivableInvoiceBatchStatuses.Draft;

    /// <summary>Terisi saat ISSUED.</summary>
    public DateTimeOffset? IssuedAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    // ------------------------------------------------------------------------------------
    // Sumbu klaim penjamin (BE-FIN-052, FIN-DEC-097, FIN-DES-070). Sumbu KEDUA, terpisah dari
    // Status di atas — Status menjawab "apakah dokumen terbit dan apakah lunas" (ditulis Sistem
    // untuk pelunasan), ClaimStatus menjawab "apa kata penjamin" (ditulis petugas AR, kecuali
    // SUBMITTED yang ditulis Sistem saat IssueAsync). Keduanya MUST NOT saling menulis kolom
    // satu sama lain — lihat FIN-DES-070/071 dan state-transition-matrix.md §D.1.
    // ------------------------------------------------------------------------------------

    /// <summary>Null berarti batch belum diterbitkan ke penjamin. Lihat FinReceivableInvoiceBatchClaimStatuses.</summary>
    public string? ClaimStatus { get; set; }

    /// <summary>Nominal yang disetujui penjamin. MUST NOT dipakai sebagai dasar pelunasan (FIN-DES-070).</summary>
    public decimal? ApprovedAmount { get; set; }

    public string? PayerClaimReference { get; set; }
    public string? ClaimNote { get; set; }
    public DateTimeOffset? PayerVerifiedAt { get; set; }
    public DateTimeOffset? ClaimApprovedAt { get; set; }
    public DateTimeOffset? ClaimClosedAt { get; set; }

    /// <summary>Alasan pembatalan batch tagihan.</summary>
    [MaxLength(500)] public string? CancelReason { get; set; }

    /// <summary>Snapshot jenis layanan (RANAP, RAJAL, IGD, OTC).</summary>
    [MaxLength(20)] public string? ServiceType { get; set; }

    /// <summary>Referensi batch lama yang dibatalkan bila batch ini adalah hasil reissue.</summary>
    public Guid? ReissuedFromBatchId { get; set; }

    /// <summary>Referensi batch baru pengganti bila batch ini telah dibatalkan dan dibuat ulang.</summary>
    public Guid? ReissuedToBatchId { get; set; }

    public ICollection<FinReceivableInvoiceBatchItem> Items { get; set; } = new List<FinReceivableInvoiceBatchItem>();
}

public static class FinReceivableInvoiceBatchStatuses
{
    public const string Draft = "DRAFT";
    public const string Issued = "ISSUED";
    public const string PartiallyPaid = "PARTIALLY_PAID";
    public const string Paid = "PAID";
    public const string Cancelled = "CANCELLED";
}

/// <summary>Sumbu klaim penjamin (BE-FIN-052, FIN-DEC-097). Lihat state-transition-matrix.md §D.1.</summary>
public static class FinReceivableInvoiceBatchClaimStatuses
{
    public const string Submitted = "SUBMITTED";
    public const string PayerVerified = "PAYER_VERIFIED";
    public const string Approved = "APPROVED";
    public const string Closed = "CLOSED";
}

public static class FinReceivableInvoiceBatchDebtorTypes
{
    public const string Payer = "PAYER";
}

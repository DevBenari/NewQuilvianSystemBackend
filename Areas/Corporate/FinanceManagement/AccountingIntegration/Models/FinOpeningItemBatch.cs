using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

/// <summary>
/// Satu unggahan migrasi tagihan lama (piutang atau utang supplier) lewat spreadsheet, beserta
/// hasil validasi dan rekonsiliasinya terhadap saldo awal Accounting yang dinyatakan petugas
/// (R14.6, FIN-DES-089, FIN-DES-090, FIN-DEC-129). Item piutang/utang baru lahir saat batch
/// berpindah ke APPROVED (BE-FIN-082) — selama DRAFT/VALIDATED tidak ada baris piutang atau utang
/// yang tercipta.
///
/// Satu ItemKind per batch — piutang dan utang MUST NOT dicampur, supaya totalnya dapat
/// dibandingkan langsung terhadap satu angka DeclaredAccountingOpeningAmount.
/// </summary>
[Table("FinOpeningItemBatch", Schema = "public")]
public sealed class FinOpeningItemBatch : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string BatchNumber { get; set; } = string.Empty;

    [Required, MaxLength(30)] public string ItemKind { get; set; } = string.Empty;

    [Required, MaxLength(20)] public string Status { get; set; } = FinOpeningItemBatchStatuses.Draft;

    public DateOnly CutoverDate { get; set; }

    public int TotalItemCount { get; set; } = 0;

    public decimal TotalOutstandingAmount { get; set; } = 0m;

    /// <summary>Angka yang dinyatakan petugas dari dokumen saldo awal Accounting (FIN-DES-090) — batch MUST NOT APPROVED bila berbeda dari total sisa itemnya.</summary>
    public decimal DeclaredAccountingOpeningAmount { get; set; }

    [Required, MaxLength(200)] public string AccountingReferenceDocument { get; set; } = string.Empty;

    [Required, MaxLength(260)] public string UploadedFileName { get; set; } = string.Empty;

    /// <summary>
    /// FIN-DES-093 (revisi 15): CSV atau XLSX, ditetapkan dari tipe media/ekstensi saat unggah —
    /// bukan dari ruas yang diisi pengguna. Dicatat supaya cacat paritas antar format dapat ditelusuri.
    /// </summary>
    [Required, MaxLength(10)] public string SourceFormat { get; set; } = string.Empty;

    /// <summary>Sensitif — dapat memuat nama debitur atau supplier dari hasil validasi per baris.</summary>
    public string? ValidationSummaryJson { get; set; }

    [MaxLength(500)] public string? RejectionReason { get; set; }

    public Guid? ApprovedBy { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? LockedAt { get; set; }

    [ConcurrencyCheck]
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinOpeningItemBatchItemKinds
{
    public const string Receivable = "RECEIVABLE";
    public const string SupplierPayable = "SUPPLIER_PAYABLE";

    public static readonly string[] All = [Receivable, SupplierPayable];
}

public static class FinOpeningItemBatchStatuses
{
    public const string Draft = "DRAFT";
    public const string Validated = "VALIDATED";
    public const string Approved = "APPROVED";
    public const string Locked = "LOCKED";
    public const string Rejected = "REJECTED";

    public static readonly string[] All = [Draft, Validated, Approved, Locked, Rejected];
}

using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

/// <summary>
/// Aggregate root penerimaan Finance (FIN-DES-010/011, 02-backend-architecture.md §4.7). Mencatat
/// uang yang benar-benar sudah diterima rumah sakit — dari tender Billing (`BILLING_TENDER`),
/// pelunasan piutang (`AR_COLLECTION`, belum ada pemanggil sampai BE-FIN-017/018), atau manual.
///
/// Amount MUST disalin apa adanya dari BilTender.Amount, tidak pernah dihitung ulang. Satu
/// SourceTenderId hanya boleh melahirkan satu receipt (index unik parsial, FIN-DES-010) — baris
/// pembalik memakai SourceTenderId kosong dan menunjuk baris asli lewat ReversalOfReceiptId,
/// supaya identitas idempotensi tender asli tidak pernah dipakai ulang. Receipt MUST NOT
/// menciptakan piutang (FIN-DES-011) — FinReceivable hanya lahir dari BilArHandoff.
/// </summary>
[Table("FinReceipt", Schema = "public")]
public sealed class FinReceipt : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string ReceiptNumber { get; set; } = string.Empty;

    [Required, MaxLength(30)] public string SourceType { get; set; } = string.Empty;

    /// <summary>Kunci idempotensi (FIN-DES-010). Kosong untuk baris pembalik dan penerimaan manual.</summary>
    public Guid? SourceTenderId { get; set; }

    /// <summary>Rujukan BilCollectionHandoff asal — bukan FK, lintas bounded context.</summary>
    public Guid? SourceCollectionHandoffId { get; set; }

    /// <summary>Rujukan BilSettlement — bukan FK, lintas bounded context.</summary>
    public Guid? SettlementId { get; set; }

    /// <summary>Rujukan BilInvoice — bukan FK, lintas bounded context.</summary>
    public Guid? InvoiceId { get; set; }

    /// <summary>Rujukan MstPaymentMethod — bukan FK, lintas bounded context.</summary>
    public Guid? PaymentMethodId { get; set; }

    public Guid? PaymentMethodAccountId { get; set; }

    /// <summary>Disalin apa adanya dari BilTender.Amount. MUST NOT dihitung ulang.</summary>
    public decimal Amount { get; set; }

    public decimal AllocatedAmount { get; set; } = 0m;

    /// <summary>Amount - AllocatedAmount. Diisi Amount penuh sampai BE-FIN-017 mengalokasikannya.</summary>
    public decimal UnallocatedAmount { get; set; }

    [MaxLength(50)] public string? KwitansiNumber { get; set; }

    /// <summary>Wajib untuk penerimaan tunai (FR-FIN-033) — bukan FK, lintas bounded context.</summary>
    public Guid? CashierShiftId { get; set; }

    [MaxLength(150)] public string? ProviderReference { get; set; }

    [MaxLength(100)] public string? ProviderEventId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Status BilInvoice saat penerimaan terjadi — penentu HELD_FOR_FINALIZATION (FR-FIN-034).</summary>
    [MaxLength(30)] public string? SourceInvoiceStatus { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinReceiptStatuses.Received;

    /// <summary>Terisi pada baris pembalik — menunjuk penerimaan asli yang dinetralkan.</summary>
    public Guid? ReversalOfReceiptId { get; set; }

    /// <summary>Diwarisi dari BilCollectionHandoff — rantai telusur ujung ke ujung.</summary>
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }

    /// <summary>Optimistic concurrency (FIN-DES-005).</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinReceiptAllocation> Allocations { get; set; } = new List<FinReceiptAllocation>();
}

public static class FinReceiptSourceTypes
{
    public const string BillingTender = "BILLING_TENDER";
    public const string ArCollection = "AR_COLLECTION";
    public const string Manual = "MANUAL";
}

public static class FinReceiptStatuses
{
    public const string Received = "RECEIVED";
    public const string Allocated = "ALLOCATED";
    public const string Reconciled = "RECONCILED";
    public const string Reversed = "REVERSED";
}

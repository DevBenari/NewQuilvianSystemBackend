using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

/// <summary>
/// Buku mutasi utang supplier (FIN-DES-079, FIN-DEC-123).
/// Mencatat setiap perubahan sisa utang supplier (OutstandingAmount) beserta tanggal bisnisnya (WIB),
/// menjadi satu-satunya dasar perhitungan posisi utang per tanggal.
/// Invariant: BalanceAfter = BalanceBefore + Amount (ditegakkan CK_FinSupplierPayableMovement_Balance di DB).
/// </summary>
[Table("FinSupplierPayableMovement", Schema = "public")]
public sealed class FinSupplierPayableMovement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SupplierPayableId { get; set; }
    public FinSupplierPayable? SupplierPayable { get; set; }

    [Required, MaxLength(30)]
    public string MovementType { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public DateOnly BusinessDate { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Rujukan ke FinPayment untuk pembayaran dokumen.</summary>
    public Guid? PaymentId { get; set; }

    /// <summary>Rujukan ke FinPaymentAllocation (satu baris per alokasi).</summary>
    public Guid? PaymentAllocationId { get; set; }

    [MaxLength(30)]
    public string? PaymentMethodCode { get; set; }

    [MaxLength(30)]
    public string? FundingSourceType { get; set; }

    public Guid? FundingSourceId { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    public Guid? ProofId { get; set; }

    public Guid? OpeningItemBatchId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public static class FinSupplierPayableMovementTypes
{
    public const string Pengakuan = "PENGAKUAN";
    public const string PembukaanMigrasi = "PEMBUKAAN-MIGRASI";
    public const string PembayaranDokumen = "PEMBAYARAN-DOKUMEN";
    public const string PembayaranLangsung = "PEMBAYARAN-LANGSUNG";
    public const string Penyesuaian = "PENYESUAIAN";
}

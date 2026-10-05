using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Buku mutasi piutang (FIN-DES-079, FIN-DEC-123).
/// Mencatat setiap perubahan sisa piutang (OutstandingAmount) beserta tanggal bisnisnya (WIB),
/// menjadi satu-satunya dasar perhitungan posisi piutang per tanggal.
/// Invariant: BalanceAfter = BalanceBefore + Amount (ditegakkan CK_FinReceivableMovement_Balance di DB).
/// </summary>
[Table("FinReceivableMovement", Schema = "public")]
public sealed class FinReceivableMovement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }

    [Required, MaxLength(30)]
    public string MovementType { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }

    public DateOnly BusinessDate { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Rujukan ke FinReceiptAllocation bila mutasi berasal dari alokasi atau potongan.</summary>
    public Guid? SourceAllocationId { get; set; }

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

public static class FinReceivableMovementTypes
{
    public const string Pengakuan = "PENGAKUAN";
    public const string PembukaanMigrasi = "PEMBUKAAN-MIGRASI";
    public const string AlokasiPenerimaan = "ALOKASI-PENERIMAAN";
    public const string PembalikanAlokasi = "PEMBALIKAN-ALOKASI";
    public const string Potongan = "POTONGAN";
    public const string PembalikanPotongan = "PEMBALIKAN-POTONGAN";
    public const string Penyesuaian = "PENYESUAIAN";
    public const string Penghapusan = "PENGHAPUSAN";
    public const string PembayaranLangsung = "PEMBAYARAN-LANGSUNG";
}

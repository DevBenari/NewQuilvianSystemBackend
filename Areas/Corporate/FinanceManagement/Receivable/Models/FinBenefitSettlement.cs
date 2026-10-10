using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Satu penutupan berkala atas piutang porsi manfaat yang ditanggung rumah sakit, untuk satu
/// periode akuntansi dan satu penjamin internal (FIN-DES-102, FIN-DEC-178). Entity tersendiri —
/// BUKAN FinReceipt (berarti uang diterima) dan BUKAN penghapusan buku.
/// </summary>
[Table("FinBenefitSettlement", Schema = "public")]
public sealed class FinBenefitSettlement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string SettlementNumber { get; set; } = string.Empty;

    /// <summary>Periode yang ditutup, bentuk YYYY-MM.</summary>
    [Required, MaxLength(7)] public string AccountingPeriodCode { get; set; } = string.Empty;

    /// <summary>Penjamin internal yang ditutup — bukan FK, master milik Administrator (FIN-DES-104).</summary>
    public Guid DebtorReferenceId { get; set; }

    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinBenefitSettlementStatuses.Draf;

    /// <summary>Wajib terisi bila DITERBITKAN (CK_FinBenefitSettlement_Posted).</summary>
    public Guid? PostedBy { get; set; }
    public DateTimeOffset? PostedAt { get; set; }

    [MaxLength(500)] public string? CancelReason { get; set; }

    /// <summary>Kejadian yang dititipkan ke Accounting. Tetap kosong sampai FIN-OQ-103 terjawab.</summary>
    public Guid? AccountingEventId { get; set; }
    public FinAccountingEventOutbox? AccountingEvent { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinBenefitSettlementItem> Items { get; set; } = new List<FinBenefitSettlementItem>();
}

public static class FinBenefitSettlementStatuses
{
    public const string Draf = "DRAF";
    public const string Diterbitkan = "DITERBITKAN";
    public const string Dibatalkan = "DIBATALKAN";
}

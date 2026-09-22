using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;

/// <summary>
/// Rekapitulasi kas harian per tanggal (FIN-DES-020, FIN-DES-022).
/// Mengikat formula: ClosingBalance = OpeningBalance + CashReceiptAmount + OtherReceiptAmount - DisbursementAmount - BankDepositAmount.
/// Kas kecil TIDAK ikut dihitung (FIN-DEC-020).
/// </summary>
[Table("FinDailyCashSnapshot", Schema = "public")]
public sealed class FinDailyCashSnapshot : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Tanggal kas harian, unik (satu baris per tanggal).</summary>
    public DateOnly CashDate { get; set; }

    /// <summary>Saldo awal = ClosingBalance hari sebelumnya.</summary>
    public decimal OpeningBalance { get; set; } = 0m;

    /// <summary>Penerimaan kas tunai hari ini (dari FinReceipt tunai).</summary>
    public decimal CashReceiptAmount { get; set; } = 0m;

    /// <summary>Penerimaan kas lain-lain yang disahkan.</summary>
    public decimal OtherReceiptAmount { get; set; } = 0m;

    /// <summary>Pengeluaran kas hari ini.</summary>
    public decimal DisbursementAmount { get; set; } = 0m;

    /// <summary>Setoran kas ke bank hari ini yang berstatus POSTED (dari FinBankDeposit).</summary>
    public decimal BankDepositAmount { get; set; } = 0m;

    /// <summary>Saldo akhir kas = Opening + CashReceipt + OtherReceipt - Disbursement - BankDeposit.</summary>
    public decimal ClosingBalance { get; set; } = 0m;

    /// <summary>OPEN atau CLOSED.</summary>
    [Required, MaxLength(30)] public string Status { get; set; } = FinDailyCashSnapshotStatuses.Open;

    public Guid? ClosedBy { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinDailyCashSnapshotStatuses
{
    public const string Open = "OPEN";
    public const string Closed = "CLOSED";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>([Open, Closed], StringComparer.OrdinalIgnoreCase);
}

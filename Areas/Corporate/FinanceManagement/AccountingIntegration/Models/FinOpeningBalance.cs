using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

/// <summary>
/// Saldo awal cutover per kelompok saldo (R14.5, FIN-DES-088, FIN-DEC-128).
/// Menjadi titik awal perhitungan posisi saldo subledger sejak tanggal cutover.
/// Invariant: Kelompok PIUTANG, UTANG-SUPPLIER, dan UTANG-JASA-MEDIS wajib bernilai 0.00 (item migrasi).
/// Status: DRAFT -> APPROVED -> LOCKED. Baris LOCKED tidak dapat diubah lagi oleh service mana pun.
/// </summary>
[Table("FinOpeningBalance", Schema = "public")]
public sealed class FinOpeningBalance : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)]
    public string BalanceGroup { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateOnly CutoverDate { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = FinOpeningBalanceStatuses.Draft;

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string AccountingReferenceDocument { get; set; } = string.Empty;

    public Guid? ApprovedBy { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public DateTimeOffset? LockedAt { get; set; }

    [ConcurrencyCheck]
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinOpeningBalanceStatuses
{
    public const string Draft = "DRAFT";
    public const string Approved = "APPROVED";
    public const string Locked = "LOCKED";

    public static readonly string[] All = [Draft, Approved, Locked];
}

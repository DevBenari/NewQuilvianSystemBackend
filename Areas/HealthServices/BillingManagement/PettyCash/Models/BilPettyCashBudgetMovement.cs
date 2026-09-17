using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;

/// <summary>
/// Ledger append-only yang menjelaskan KENAPA saldo kolam bergerak (PC-DES-004).
/// Baris MUST NOT disunting atau dihapus; koreksi memakai baris ADJUSTMENT baru.
/// Unique index parsial pada (VoucherId) untuk MovementType = 'DISBURSEMENT'
/// menegakkan invariant "satu voucher paling banyak satu pengurangan saldo".
/// </summary>
[Table("BilPettyCashBudgetMovement", Schema = "public")]
public sealed class BilPettyCashBudgetMovement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BudgetId { get; set; }

    [Required, MaxLength(30)] public string MovementType { get; set; } = string.Empty;

    /// <summary>Selalu positif; arahnya ditentukan MovementType.</summary>
    public decimal Amount { get; set; }

    public decimal BalanceBefore { get; set; }

    /// <summary>MUST NOT negatif.</summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>Terisi hanya untuk DISBURSEMENT.</summary>
    public Guid? VoucherId { get; set; }

    /// <summary>SENSITIF. Wajib untuk TOP_UP dan ADJUSTMENT; kosong untuk DISBURSEMENT.</summary>
    [MaxLength(500)] public string? Reason { get; set; }

    /// <summary>Sumber dana penambahan anggaran (TRANSFER atau CASH). Diisi hanya untuk
    /// MovementType = TOP_UP. Null untuk seluruh jenis pergerakan lain.
    /// Historical records pra-task ini sah dengan nilai null (PC-DES-026).</summary>
    [MaxLength(30)] public string? FundingSourceType { get; set; }

    /// <summary>Nomor referensi transfer. Diisi hanya bila FundingSourceType = TRANSFER.
    /// Null untuk CASH dan untuk seluruh jenis pergerakan selain TOP_UP.</summary>
    [MaxLength(100)] public string? TransferReference { get; set; }

    public Guid ActorUserId { get; set; }

    public Guid? IdempotencyKey { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public BilPettyCashBudget Budget { get; set; } = null!;

    public BilPettyCashVoucher? Voucher { get; set; }
}

public static class PettyCashBudgetMovementTypes
{
    public const string TopUp = "TOP_UP";
    public const string Disbursement = "DISBURSEMENT";
    public const string Adjustment = "ADJUSTMENT";

    /// <summary>Sisa uang yang dikembalikan penerima. VoucherId wajib; boleh berkali-kali
    /// per voucher selama totalnya tidak melampaui Amount (PC-DES-019, PC-DES-020).</summary>
    public const string Return = "RETURN";

    /// <summary>Pencairan yang seharusnya tidak terjadi dibalik. VoucherId wajib; paling
    /// banyak sekali per voucher (PC-DES-019, PC-DES-020).</summary>
    public const string Reversal = "REVERSAL";

    /// <summary>Sisa saldo periode yang ditutup, dicatat pada periode itu sendiri sampai
    /// nol. Selalu berpasangan dengan satu baris CarryForwardIn pada periode penerus dalam
    /// transaction yang sama (PC-DES-018).</summary>
    public const string CarryForwardOut = "CARRY_FORWARD_OUT";

    /// <summary>Sisa saldo yang diterima dari periode yang ditutup. Selalu berpasangan
    /// dengan satu baris CarryForwardOut pada periode asal (PC-DES-018).</summary>
    public const string CarryForwardIn = "CARRY_FORWARD_IN";
}

/// <summary>Sumber dana penambahan anggaran kas kecil (PC-DES-026).
/// Hanya berlaku untuk movement berjenis TOP_UP.
/// Nilai ini adalah kode internal; label tampilan dikelola frontend.</summary>
public static class PettyCashFundingSourceTypes
{
    /// <summary>Dana masuk melalui transfer bank. TransferReference wajib diisi.</summary>
    public const string Transfer = "TRANSFER";

    /// <summary>Dana masuk secara tunai/langsung. TransferReference tidak diwajibkan.</summary>
    public const string Cash = "CASH";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>([Transfer, Cash], StringComparer.OrdinalIgnoreCase);
}

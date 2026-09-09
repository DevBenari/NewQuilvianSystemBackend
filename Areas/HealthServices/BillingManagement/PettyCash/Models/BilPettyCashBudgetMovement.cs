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
}

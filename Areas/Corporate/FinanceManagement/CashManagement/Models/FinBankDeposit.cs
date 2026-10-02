using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;

/// <summary>
/// Setoran kas ke bank milik Finance (FIN-DES-018, FIN-DES-021).
/// Divalidasi terhadap saldo kas tersedia saat posting (FIN-VAL-060).
/// </summary>
[Table("FinBankDeposit", Schema = "public")]
public sealed class FinBankDeposit : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nomor setoran bank, mis. DEP-20260921-0001. Unik.</summary>
    [Required, MaxLength(50)] public string DepositNumber { get; set; } = string.Empty;

    public DateOnly DepositDate { get; set; }

    public Guid BankAccountId { get; set; }
    public MstBankAccount? BankAccount { get; set; }

    /// <summary>Nominal setoran, selalu positif, tidak boleh melebihi saldo tersedia saat posting.</summary>
    public decimal Amount { get; set; }

    /// <summary>Rujukan ke shift kasir di Billing (FIN-CAP-006). Finance MUST NOT menulis shift.</summary>
    public Guid? CashierShiftId { get; set; }

    /// <summary>Nomor bukti setor dari bank (slip setoran).</summary>
    [MaxLength(100)] public string? DepositSlipNumber { get; set; }

    /// <summary>DRAFT, POSTED, VERIFIED, CANCELLED.</summary>
    [Required, MaxLength(30)] public string Status { get; set; } = FinBankDepositStatuses.Draft;

    public Guid? PostedBy { get; set; }
    public DateTimeOffset? PostedAt { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinBankDepositStatuses
{
    public const string Draft = "DRAFT";
    public const string Posted = "POSTED";
    public const string Verified = "VERIFIED";
    public const string Cancelled = "CANCELLED";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>([Draft, Posted, Verified, Cancelled], StringComparer.OrdinalIgnoreCase);
}

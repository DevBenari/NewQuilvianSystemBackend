using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;

/// <summary>
/// Buku mutasi kas (FIN-DES-081, FIN-DEC-124..127, 132..133).
/// Menjadi satu-satunya sumbu perhitungan posisi Kas Kasir per tanggal.
/// Idempotensi dijaga oleh unique index gabungan (SourceReferenceType, SourceReferenceId, MovementType).
/// Direction: IN / OUT; Amount selalu positif (> 0).
/// </summary>
[Table("FinCashMovement", Schema = "public")]
public sealed class FinCashMovement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(40)]
    public string MovementType { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string Direction { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateOnly BusinessDate { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    [Required, MaxLength(40)]
    public string SourceReferenceType { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string SourceReferenceId { get; set; } = string.Empty;

    /// <summary>Rujukan ke BilCashierShift milik modul Billing (bukan FK, lintas bounded context).</summary>
    public Guid? CashierShiftId { get; set; }

    [MaxLength(30)]
    public string? PaymentMethodCode { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public static class FinCashMovementTypes
{
    public const string SaldoAwal = "SALDO-AWAL";
    public const string KasShift = "KAS-SHIFT";
    public const string PenerimaanTunaiLangsung = "PENERIMAAN-TUNAI-LANGSUNG";
    public const string PembayaranTunaiLangsung = "PEMBAYARAN-TUNAI-LANGSUNG";
    public const string PembayaranTunaiDokumen = "PEMBAYARAN-TUNAI-DOKUMEN";
    public const string SetoranBank = "SETORAN-BANK";
    public const string PembalikanSetoranBank = "PEMBALIKAN-SETORAN-BANK";
}

public static class FinCashMovementDirections
{
    public const string In = "IN";
    public const string Out = "OUT";
}

public static class FinCashMovementSourceReferenceTypes
{
    public const string CashierShift = "CASHIER_SHIFT";
    public const string BankDeposit = "BANK_DEPOSIT";
    public const string ReceivableMovement = "RECEIVABLE_MOVEMENT";
    public const string PayableMovement = "PAYABLE_MOVEMENT";
    public const string Payment = "PAYMENT";
    public const string OpeningBalance = "OPENING_BALANCE";
}

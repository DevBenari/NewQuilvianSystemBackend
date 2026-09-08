using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;

/// <summary>
/// Jejak audit append-only setiap perintah yang pernah dijalankan atas sebuah voucher
/// (PC-DES-009). Bentuknya meniru BilCashierShiftCommand yang sudah ada.
/// Baris di sini MUST NOT pernah disunting atau dihapus — inilah dasar PC-DEC-003.
/// </summary>
[Table("BilPettyCashVoucherCommand", Schema = "public")]
public sealed class BilPettyCashVoucherCommand : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VoucherId { get; set; }

    [Required, MaxLength(40)] public string CommandType { get; set; } = string.Empty;

    public Guid ActorUserId { get; set; }

    [Required, MaxLength(150)] public string ActorRole { get; set; } = string.Empty;

    /// <summary>Nilai RowVersion voucher sebelum perintah dijalankan.</summary>
    public Guid EntityVersion { get; set; }

    /// <summary>Kosong hanya untuk SUBMIT.</summary>
    [MaxLength(30)] public string? StatusBefore { get; set; }

    [Required, MaxLength(30)] public string StatusAfter { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    /// <summary>SENSITIF. Wajib untuk REJECT dan CANCEL.</summary>
    [MaxLength(500)] public string? Reason { get; set; }

    public Guid? IdempotencyKey { get; set; }

    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;

    public Guid CorrelationId { get; set; }

    public Guid CausationId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>SENSITIF. Dipakai memutar ulang permintaan ber-Idempotency-Key sama.</summary>
    [Required] public string ResponseJson { get; set; } = "{}";

    public BilPettyCashVoucher Voucher { get; set; } = null!;
}

public static class PettyCashVoucherCommandTypes
{
    public const string Submit = "SUBMIT";
    public const string Approve = "APPROVE";
    public const string Reject = "REJECT";
    public const string Cancel = "CANCEL";
    public const string Disburse = "DISBURSE";
    public const string AttachProof = "ATTACH_PROOF";
    public const string ProofCorrected = "PROOF_CORRECTED";
}

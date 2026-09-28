using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Models;

/// <summary>
/// Satu pintu masuk seluruh fakta dari Billing (AR/AP/penerimaan/koreksi, ditambah mutasi deposit,
/// kelebihan bayar, pengembalian, dan pengesahan selisih kas shift), beserta jejak ACK dan
/// penanganan gagal (FIN-DES-008). SourceHandoffId sengaja bukan FK — Finance hanya menyimpan
/// Id-nya agar tidak mengunci tabel milik modul lain (BE-FIN-005).
///
/// Empat jenis yang ditambahkan BE-FIN-022 (FIN-DES-029) berhenti di status CONSUMED, bukan
/// ACKNOWLEDGED: sumbernya tidak punya kolom status handoff yang bisa ditandai, dan Finance
/// dilarang menulis ke tabel Billing (FIN-STATE-1.1 bagian 1). Jalur sinkronisasinya sendiri
/// adalah BE-FIN-025, yang belum dikerjakan.
/// </summary>
[Table("FinBillingHandoffIntake", Schema = "public")]
public sealed class FinBillingHandoffIntake : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)] public string HandoffType { get; set; } = string.Empty;

    /// <summary>Id baris handoff milik Billing — bukan FK (lihat ringkasan kelas).</summary>
    public Guid SourceHandoffId { get; set; }

    /// <summary>Kunci idempotensi milik Billing, disalin dari BilArHandoff.HandoffKey (FIN-DES-009).</summary>
    public Guid SourceHandoffKey { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinBillingHandoffIntakeStatuses.New;

    /// <summary>Piutang/penerimaan yang lahir — kosong selama masih NEW atau ERROR.</summary>
    public Guid? TargetEntityId { get; set; }

    public DateTimeOffset? ConsumedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }

    public int RetryCount { get; set; } = 0;

    [MaxLength(1000)] public string? ErrorMessage { get; set; }

    public Guid CorrelationId { get; set; }

    /// <summary>Optimistic concurrency (FIN-DES-005) — di-set ulang Guid.NewGuid() setiap perubahan.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinBillingHandoffTypes
{
    public const string Ar = "AR";
    public const string Ap = "AP";
    public const string Collection = "COLLECTION";
    public const string Adjustment = "ADJUSTMENT";

    /// <summary>Mutasi deposit pasien (TOP_UP/ALLOCATION/RELEASE/REVERSAL) dari BilDepositMovement.
    /// Kunci idempotensinya memakai IdempotencyKey milik Billing (FIN-DES-029).</summary>
    public const string DepositMovement = "DEPOSIT_MOVEMENT";

    /// <summary>Pengakuan kelebihan bayar dari BilRefundableCredit ber-SourceType ALLOCATION_EXCESS.
    /// Sumbernya tidak punya kunci idempotensi sendiri, sehingga SourceHandoffKey memakai Id baris
    /// itu (FIN-DES-029).</summary>
    public const string RefundableCredit = "REFUNDABLE_CREDIT";

    /// <summary>Pengembalian uang ke pasien dari BilRefundCase berstatus EXECUTED, khusus yang
    /// bersumber ALLOCATION_EXCESS (FIN-DEC-041).</summary>
    public const string RefundCase = "REFUND_CASE";

    /// <summary>Pengesahan selisih kas shift dari BilCashVarianceReview. Sumbernya tidak punya
    /// kunci idempotensi sendiri, sehingga SourceHandoffKey memakai Id baris itu (FIN-DES-036).</summary>
    public const string CashVarianceReview = "CASH_VARIANCE_REVIEW";
}

public static class FinBillingHandoffIntakeStatuses
{
    public const string New = "NEW";
    public const string Consumed = "CONSUMED";
    public const string Acknowledged = "ACKNOWLEDGED";
    public const string Error = "ERROR";
}

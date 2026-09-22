using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Models;

/// <summary>
/// Satu pintu masuk seluruh fakta dari Billing (AR/AP/penerimaan/koreksi), beserta jejak ACK dan
/// penanganan gagal (FIN-DES-008). SourceHandoffId sengaja bukan FK — Finance hanya menyimpan
/// Id-nya agar tidak mengunci tabel milik modul lain (BE-FIN-005).
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
}

public static class FinBillingHandoffIntakeStatuses
{
    public const string New = "NEW";
    public const string Consumed = "CONSUMED";
    public const string Acknowledged = "ACKNOWLEDGED";
    public const string Error = "ERROR";
}

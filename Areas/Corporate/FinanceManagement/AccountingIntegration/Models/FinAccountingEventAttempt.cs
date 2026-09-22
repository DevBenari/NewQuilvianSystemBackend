using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

/// <summary>
/// Jejak percobaan kirim satu baris FinAccountingEventOutbox ke Accounting. Append-only — diisi
/// worker pengiriman (di luar lingkup task ini). ResponseBody dipotong agar tabel tidak
/// menggelembung.
/// </summary>
[Table("FinAccountingEventAttempt", Schema = "public")]
public sealed class FinAccountingEventAttempt : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OutboxId { get; set; }
    public FinAccountingEventOutbox? Outbox { get; set; }

    public int AttemptNumber { get; set; }

    public DateTimeOffset AttemptedAt { get; set; }

    /// <summary>Kosong bila gagal sebelum sempat terkirim.</summary>
    public int? ResponseCode { get; set; }

    [MaxLength(2000)] public string? ResponseBody { get; set; }

    public int? DurationMs { get; set; }

    [MaxLength(1000)] public string? ErrorMessage { get; set; }
}

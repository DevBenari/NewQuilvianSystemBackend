using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilApHandoff", Schema = "public")]
public sealed class BilApHandoff : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public Guid FinalizationRecordId { get; set; }
    public Guid DoctorId { get; set; }
    public decimal Amount { get; set; }
    [Required, MaxLength(30)] public string ReadinessStatus { get; set; } = BillingApReadinessStatuses.NotReady;
    [Required, MaxLength(30)] public string Status { get; set; } = BillingHandoffStatuses.Created;
    public Guid HandoffKey { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReadyAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice Invoice { get; set; } = null!;
    public BilFinalizationRecord FinalizationRecord { get; set; } = null!;
}

public static class BillingApReadinessStatuses
{
    public const string NotReady = "NOT_READY";
    public const string Ready = "READY";
}

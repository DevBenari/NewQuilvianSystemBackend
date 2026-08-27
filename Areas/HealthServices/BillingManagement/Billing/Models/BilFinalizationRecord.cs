using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilFinalizationRecord", Schema = "public")]
public sealed class BilFinalizationRecord : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public int CalculationVersion { get; set; }
    public decimal OutstandingAtFinalization { get; set; }
    public bool IsDepartureException { get; set; }
    [MaxLength(30)] public string? DepartureReason { get; set; }
    [MaxLength(200)] public string? DebtorIdentity { get; set; }
    [MaxLength(100)] public string? DebtorRelationship { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid IdempotencyKey { get; set; }
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset FinalizedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice Invoice { get; set; } = null!;
}

public static class BillingDepartureReasons
{
    public const string Death = "DEATH";
    public const string EmergencyTransfer = "EMERGENCY_TRANSFER";
    public const string Dama = "DAMA";
}

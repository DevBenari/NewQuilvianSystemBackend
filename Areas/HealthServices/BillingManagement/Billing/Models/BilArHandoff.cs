using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilArHandoff", Schema = "public")]
public sealed class BilArHandoff : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public Guid FinalizationRecordId { get; set; }
    [Required, MaxLength(30)] public string DebtorType { get; set; } = BillingArDebtorTypes.Payer;
    public Guid? DebtorReferenceId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingHandoffStatuses.Created;
    public Guid HandoffKey { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilInvoice Invoice { get; set; } = null!;
    public BilFinalizationRecord FinalizationRecord { get; set; } = null!;
}

public static class BillingArDebtorTypes
{
    public const string PatientGuarantor = "PATIENT_GUARANTOR";
    public const string Payer = "PAYER";
}

public static class BillingHandoffStatuses
{
    public const string Created = "CREATED";
    public const string Acknowledged = "ACKNOWLEDGED";
}

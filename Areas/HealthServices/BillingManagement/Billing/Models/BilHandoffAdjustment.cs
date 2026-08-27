using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilHandoffAdjustment", Schema = "public")]
public sealed class BilHandoffAdjustment : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ArHandoffId { get; set; }
    public Guid? ApHandoffId { get; set; }
    public Guid? SourceAdjustmentId { get; set; }
    public Guid? SourceWriteOffCaseId { get; set; }
    [Required, MaxLength(10)] public string Direction { get; set; } = BillingAdjustmentDirections.Credit;
    public decimal Amount { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public BilArHandoff? ArHandoff { get; set; }
    public BilApHandoff? ApHandoff { get; set; }
}

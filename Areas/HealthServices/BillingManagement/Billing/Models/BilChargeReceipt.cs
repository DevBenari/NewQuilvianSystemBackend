using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilChargeReceipt", Schema = "public")]
public sealed class BilChargeReceipt : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid IdempotencyKey { get; set; }
    public Guid InvoiceItemId { get; set; }
    [Required, MaxLength(50)] public string SourceDomain { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string SourceDetailId { get; set; } = string.Empty;
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public BilInvoiceItem InvoiceItem { get; set; } = null!;
}

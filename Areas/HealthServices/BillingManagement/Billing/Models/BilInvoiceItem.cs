using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilInvoiceItem", Schema = "public")]
public sealed class BilInvoiceItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    [Required, MaxLength(50)] public string SourceDomain { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string SourceDetailId { get; set; } = string.Empty;
    public long SourceVersion { get; set; }
    [Required, MaxLength(30)] public string SourceContractVersion { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string SourceStatus { get; set; } = string.Empty;
    public DateTimeOffset SourceOccurredAt { get; set; }
    public Guid CategoryId { get; set; }
    [Required, MaxLength(250)] public string DescriptionSnapshot { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DoctorShare { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingInvoiceItemStatuses.Active;
    [MaxLength(500)] public string? VoidReason { get; set; }
    public Guid LastIdempotencyKey { get; set; }
    public Guid LastCorrelationId { get; set; }
    public Guid LastCausationId { get; set; }
    [Required, MaxLength(64)] public string SourcePayloadHash { get; set; } = string.Empty;
    public BilInvoice Invoice { get; set; } = null!;
    public MstTariffCategory Category { get; set; } = null!;
}

public static class BillingInvoiceItemStatuses
{
    public const string Active = "ACTIVE";
    public const string Voided = "VOIDED";
}

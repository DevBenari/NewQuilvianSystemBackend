using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilCalculationVersion", Schema = "public")]
public sealed class BilCalculationVersion : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    public int VersionNo { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal AdministrationFeeAmount { get; set; }
    public decimal ItemDiscount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PatientAmount { get; set; }
    public decimal PrimaryAmount { get; set; }
    public decimal ExcessAmount { get; set; }
    public decimal UnresolvedCoverageAmount { get; set; }
    public decimal RoundingAmount { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset CalculatedAt { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    [Required] public string BreakdownSnapshot { get; set; } = "{}";
    public BilInvoice Invoice { get; set; } = null!;
}

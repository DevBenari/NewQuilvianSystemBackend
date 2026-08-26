using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilRefundableCredit", Schema = "public")]
public sealed class BilRefundableCredit : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid InvoiceId { get; set; }
    [Required, MaxLength(30)] public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = BillingRefundableCreditStatuses.Available;
    public DateTimeOffset RecognizedAt { get; set; }
}

public static class BillingRefundableCreditSourceTypes
{
    public const string AllocationExcess = "ALLOCATION_EXCESS";
    public const string Settlement = "SETTLEMENT";
}

public static class BillingRefundableCreditStatuses
{
    public const string Available = "AVAILABLE";
    public const string Exhausted = "EXHAUSTED";
}

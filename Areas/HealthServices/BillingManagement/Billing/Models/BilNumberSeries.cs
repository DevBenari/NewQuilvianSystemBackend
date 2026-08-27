using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;

[Table("BilNumberSeries", Schema = "public")]
public sealed class BilNumberSeries : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(50)] public string SequenceKey { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string ScopeKey { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string ResetPolicy { get; set; } = string.Empty;
    public long CurrentValue { get; set; }
    public DateTimeOffset LastAllocatedAt { get; set; }
}

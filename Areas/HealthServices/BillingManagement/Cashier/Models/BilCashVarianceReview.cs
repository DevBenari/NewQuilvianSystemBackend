using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;

[Table("BilCashVarianceReview", Schema = "public")]
public sealed class BilCashVarianceReview : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShiftId { get; set; }
    public Guid ReviewerId { get; set; }
    public decimal Variance { get; set; }
    [Required, MaxLength(500)] public string Resolution { get; set; } = string.Empty;
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public DateTimeOffset ReviewedAt { get; set; }
    public Guid? ReopenAuthorizedBy { get; set; }
    public BilCashierShift Shift { get; set; } = null!;
}

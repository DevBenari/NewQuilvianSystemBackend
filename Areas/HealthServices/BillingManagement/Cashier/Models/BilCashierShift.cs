using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;

[Table("BilCashierShift", Schema = "public")]
public sealed class BilCashierShift : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(40)] public string ShiftNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public Guid RegisterId { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal SystemCash { get; set; }
    public decimal PhysicalCash { get; set; }
    public decimal Variance { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = CashierShiftStatuses.Open;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();
    public ICollection<BilCashVarianceReview> VarianceReviews { get; set; } = [];
}

public static class CashierShiftStatuses
{
    public const string Open = "OPEN";
    public const string HandedOver = "HANDED_OVER";
    public const string Closed = "CLOSED";
    public const string ClosedWithVariance = "CLOSED_WITH_VARIANCE";
    public const string Reviewed = "REVIEWED";
    public const string Reopened = "REOPENED";

    public static bool IsActive(string status) =>
        status is Open or Reopened;
}

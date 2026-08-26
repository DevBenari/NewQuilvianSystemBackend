using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;

[Table("BilCashierShiftHandover", Schema = "public")]
public sealed class BilCashierShiftHandover : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SourceShiftId { get; set; }
    public Guid? ReceivingShiftId { get; set; }
    public Guid OutgoingCashierId { get; set; }
    public Guid IncomingCashierId { get; set; }
    [Required, MaxLength(30)] public string Status { get; set; } = CashierShiftHandoverStatuses.Pending;
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public Guid RowVersion { get; set; } = Guid.NewGuid();
    public BilCashierShift SourceShift { get; set; } = null!;
    public BilCashierShift? ReceivingShift { get; set; }
}

public static class CashierShiftHandoverStatuses
{
    public const string Pending = "PENDING";
    public const string Confirmed = "CONFIRMED";
}

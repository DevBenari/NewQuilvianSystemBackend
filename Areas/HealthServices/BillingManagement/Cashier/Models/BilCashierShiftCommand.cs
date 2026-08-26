using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;

[Table("BilCashierShiftCommand", Schema = "public")]
public sealed class BilCashierShiftCommand : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ShiftId { get; set; }
    public Guid CashierId { get; set; }
    public Guid RegisterId { get; set; }
    public Guid EntityVersion { get; set; }
    [Required, MaxLength(40)] public string CommandType { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    [Required, MaxLength(150)] public string ActorRole { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Authority { get; set; } = string.Empty;
    public Guid? IdempotencyKey { get; set; }
    [Required, MaxLength(64)] public string PayloadHash { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    [MaxLength(30)] public string? StatusBefore { get; set; }
    [Required, MaxLength(30)] public string StatusAfter { get; set; } = string.Empty;
    public decimal OpeningCash { get; set; }
    public decimal SystemCash { get; set; }
    public decimal? PhysicalCash { get; set; }
    public decimal? Variance { get; set; }
    public decimal? Amount { get; set; }
    [MaxLength(500)] public string? Reason { get; set; }
    [MaxLength(40)] public string? SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    [Required] public string ResponseJson { get; set; } = "{}";
    public BilCashierShift Shift { get; set; } = null!;
}

public static class CashierShiftCommandTypes
{
    public const string Open = "OPEN";
    public const string HandoverInitiated = "HANDOVER_INITIATED";
    public const string HandoverConfirmed = "HANDOVER_CONFIRMED";
    public const string Close = "CLOSE";
    public const string ReviewVariance = "REVIEW_VARIANCE";
    public const string Reopen = "REOPEN";
    public const string CashReceipt = "CASH_RECEIPT";
}

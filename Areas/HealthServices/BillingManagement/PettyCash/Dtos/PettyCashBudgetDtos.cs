using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;

public static class PettyCashBudgetAdjustmentDirections
{
    public const string Increase = "INCREASE";
    public const string Decrease = "DECREASE";
    public static readonly IReadOnlySet<string> All = new HashSet<string>([Increase, Decrease], StringComparer.OrdinalIgnoreCase);
}

public sealed class PettyCashBudgetMovementQuery
{
    public string? MovementType { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class PettyCashBudgetTopUpRequest
{
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid ExpectedRowVersion { get; set; }
}

public sealed class PettyCashBudgetAdjustmentRequest : PettyCashBudgetTopUpRequest
{
    [Required, MaxLength(20)] public string Direction { get; set; } = string.Empty;
}

public sealed class PettyCashBudgetResponse
{
    public Guid Id { get; set; }
    public string PoolCode { get; set; } = string.Empty;
    public string PoolName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal ReservedAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public decimal TotalTopUpAmount { get; set; }
    public decimal TotalDisbursedAmount { get; set; }
    public DateTimeOffset? LastMovementAt { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class PettyCashBudgetMovementResponse
{
    public Guid Id { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? VoucherId { get; set; }
    public string? VoucherNumber { get; set; }
    public string? Reason { get; set; }
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

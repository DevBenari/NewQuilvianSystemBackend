using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;

public static class RoomChargePolicyValues
{
    public const string CeilingPeriod = "CEILING_PERIOD";
    public const string Proportional = "PROPORTIONAL";
    public const string WholePeriods = "WHOLE_PERIODS";
    public const string PeriodStart = "PERIOD_START";
    public const string OccupancyStart = "OCCUPANCY_START";
    public const string IncludeLeave = "INCLUDE_LEAVE";
    public const string ExcludeLeave = "EXCLUDE_LEAVE";

    public static readonly IReadOnlySet<string> RemainderRoundings = Set(CeilingPeriod, Proportional, WholePeriods);
    public static readonly IReadOnlySet<string> TariffMoments = Set(PeriodStart, OccupancyStart);
    public static readonly IReadOnlySet<string> LeaveRules = Set(IncludeLeave, ExcludeLeave);

    private static IReadOnlySet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
}

public sealed class RoomChargePolicyQuery
{
    public DateTimeOffset? EffectiveAt { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class CreateRoomChargePolicyRequest
{
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Range(1, int.MaxValue)] public int MinimumMinutes { get; set; }
    [Range(1, int.MaxValue)] public int PeriodMinutes { get; set; }
    [Required, MaxLength(30)] public string RemainderRounding { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string TariffMoment { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string LeaveRule { get; set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UpdateRoomChargePolicyRequest : CreateRoomChargePolicyRequest;

public sealed class RoomChargePolicyResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MinimumMinutes { get; set; }
    public int PeriodMinutes { get; set; }
    public string RemainderRounding { get; set; } = string.Empty;
    public string TariffMoment { get; set; } = string.Empty;
    public string LeaveRule { get; set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
}

public sealed class RoomChargePolicyDeleteResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDelete { get; set; }
}

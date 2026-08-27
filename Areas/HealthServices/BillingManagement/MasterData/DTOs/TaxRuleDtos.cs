using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;

public static class TaxRuleValues
{
    public const string HalfUp = "HALF_UP";
    public const string HalfEven = "HALF_EVEN";
    public const string Up = "UP";
    public const string Down = "DOWN";
    public const string Proportional = "PROPORTIONAL";
    public const string Patient = "PATIENT";
    public const string Guarantor = "GUARANTOR";

    public static readonly IReadOnlySet<string> RoundingModes = Set(HalfUp, HalfEven, Up, Down);
    public static readonly IReadOnlySet<string> AllocationRules = Set(Proportional, Patient, Guarantor);

    private static IReadOnlySet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
}

public sealed class TaxRuleQuery
{
    public DateTimeOffset? EffectiveAt { get; set; }
    public string? TaxableCategory { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class CreateTaxRuleRequest
{
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string TaxableCategory { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.000001", "100.000000",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)] public decimal Rate { get; set; }
    [Required, MaxLength(30)] public string RoundingMode { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string AllocationRule { get; set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UpdateTaxRuleRequest : CreateTaxRuleRequest;

public sealed class TaxRuleResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TaxableCategory { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string RoundingMode { get; set; } = string.Empty;
    public string AllocationRule { get; set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
}

public sealed class TaxRuleDeleteResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDelete { get; set; }
}

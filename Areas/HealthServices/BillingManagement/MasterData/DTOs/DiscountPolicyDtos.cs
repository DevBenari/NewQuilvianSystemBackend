using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;

public static class DiscountPolicyValues
{
    public const string PromoTotal = "PROMO_TOTAL";
    public const string PromoItem = "PROMO_ITEM";
    public const string Doctor = "DOCTOR";
    public const string PatientPortion = "PATIENT_PORTION";
    public const string InvoiceItem = "INVOICE_ITEM";
    public const string DoctorShare = "DOCTOR_SHARE";
    public const string Percentage = "PERCENTAGE";
    public const string FixedAmount = "FIXED_AMOUNT";
    public const string DoctorApprover = "DOCTOR";

    public static readonly IReadOnlySet<string> DiscountTypes = Set(PromoTotal, PromoItem, Doctor);
    public static readonly IReadOnlySet<string> TargetComponents = Set(PatientPortion, InvoiceItem, DoctorShare);
    public static readonly IReadOnlySet<string> ValueTypes = Set(Percentage, FixedAmount);

    private static IReadOnlySet<string> Set(params string[] values) =>
        new HashSet<string>(values, StringComparer.OrdinalIgnoreCase);
}

public sealed class DiscountPolicyQuery
{
    public DateTimeOffset? EffectiveAt { get; set; }
    public string? DiscountType { get; set; }
    public string? TargetComponent { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class CreateDiscountPolicyRequest
{
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string DiscountType { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string TargetComponent { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string ValueType { get; set; } = string.Empty;
    [Range(typeof(decimal),"0.01","9999999999999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Value { get; set; }

    [Range(typeof(decimal),"0.01","9999999999999999.99",ParseLimitsInInvariantCulture = true,ConvertValueInInvariantCulture = true)]
    public decimal? Limit { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UpdateDiscountPolicyRequest : CreateDiscountPolicyRequest;

public sealed class DiscountPolicyResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public string TargetComponent { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? Limit { get; set; }
    public bool RequiresApproval { get; set; }
    public string? ApproverRole { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
}

public sealed class DiscountPolicyDeleteResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDelete { get; set; }
}

public sealed class DiscountPolicyOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public string TargetComponent { get; set; } = string.Empty;
    public string ValueType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public bool IsActive { get; set; }
}

public sealed class DiscountPolicySummaryResponse
{
    public int TotalPolicy { get; set; }
    public int ActivePolicy { get; set; }
    public int InactivePolicy { get; set; }
    public int PromoTotalPolicy { get; set; }
    public int PromoItemPolicy { get; set; }
    public int DoctorPolicy { get; set; }
}

public sealed class DiscountPolicyDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public string? DiscountType { get; set; }
    public string? TargetComponent { get; set; }
    public string? Search { get; set; }
    public DateTimeOffset? EffectiveAt { get; set; }
}

public sealed class DiscountPolicyFilterMetadataResponse
{
    public DiscountPolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<int> PageSizeOptions { get; set; } = new();
    public List<string> DiscountTypes { get; set; } = new();
    public List<string> TargetComponents { get; set; } = new();
    public List<string> ValueTypes { get; set; } = new();
}

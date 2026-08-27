using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;

public static class AdministrationFeeServiceTypes
{
    public const string Rajal = "RAJAL";
    public const string Igd = "IGD";
    public const string Otc = "OTC";
    public const string Ranap = "RANAP";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Rajal, Igd, Otc, Ranap
    };
}

public sealed class AdministrationFeePolicyQuery
{
    public DateTimeOffset? EffectiveAt { get; set; }
    public string? ServiceType { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class CreateAdministrationFeePolicyRequest
{
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string ServiceType { get; set; } = string.Empty;
    
    [Range(typeof(decimal), "0", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)] 
    public decimal Amount { get; set; }
    public bool Coverable { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UpdateAdministrationFeePolicyRequest : CreateAdministrationFeePolicyRequest;

public sealed class DeactivatePolicyRequest
{
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class AdministrationFeePolicyResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool OncePerPatientLocalDay { get; set; }
    public int ReplacementPriority { get; set; }
    public bool Coverable { get; set; }
    public bool Discountable { get; set; }
    public string BusinessTimeZone { get; set; } = "Asia/Jakarta";
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
}

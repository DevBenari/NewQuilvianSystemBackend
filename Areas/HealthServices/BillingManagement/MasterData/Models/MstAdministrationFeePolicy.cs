using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

[Table("MstAdministrationFeePolicy", Schema = "public")]
public sealed class MstAdministrationFeePolicy : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string ServiceType { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    [Column(TypeName = "numeric(5,2)")]
    public decimal? Percentage { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal? CapAmount { get; set; }

    [Required, MaxLength(30)]
    public string CalculationType { get; set; } = AdministrationFeeCalculationTypes.Flat;

    public bool OncePerPatientLocalDay { get; set; } = true;

    public int ReplacementPriority { get; set; }

    public bool Coverable { get; set; }

    public bool Discountable { get; set; }

    public DateTimeOffset EffectiveFrom { get; set; }

    public DateTimeOffset? EffectiveTo { get; set; }

    public bool IsActive { get; set; }
}

public static class AdministrationFeeCalculationTypes
{
    public const string Flat = "FLAT";
    public const string PercentageWithCap = "PERCENTAGE_WITH_CAP";
}

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

    public bool OncePerPatientLocalDay { get; set; } = true;

    public int ReplacementPriority { get; set; }

    public bool Coverable { get; set; }

    public bool Discountable { get; set; }

    public DateTimeOffset EffectiveFrom { get; set; }

    public DateTimeOffset? EffectiveTo { get; set; }

    public bool IsActive { get; set; }
}

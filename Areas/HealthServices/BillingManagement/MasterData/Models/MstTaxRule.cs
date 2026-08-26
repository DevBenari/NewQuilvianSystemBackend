using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

[Table("MstTaxRule", Schema = "public")]
public sealed class MstTaxRule : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string TaxableCategory { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    [Required, MaxLength(30)] public string RoundingMode { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string AllocationRule { get; set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

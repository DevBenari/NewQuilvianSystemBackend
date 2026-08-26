using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

[Table("MstDiscountPolicy", Schema = "public")]
public sealed class MstDiscountPolicy : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string DiscountType { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string TargetComponent { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string ValueType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? Limit { get; set; }
    public bool RequiresApproval { get; set; }
    [MaxLength(50)] public string? ApproverRole { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

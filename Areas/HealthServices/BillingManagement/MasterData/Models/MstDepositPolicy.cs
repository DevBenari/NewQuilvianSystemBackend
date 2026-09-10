using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

[Table("MstDepositPolicy", Schema = "public")]
public sealed class MstDepositPolicy : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    public Guid? GuarantorId { get; set; }
    public Guid? PatientClassId { get; set; }
    public bool IsRequired { get; set; } = true;
    public decimal MinimumAmount { get; set; }
    public int FollowUpIntervalDays { get; set; } = 3;
    [MaxLength(500)] public string? Description { get; set; }
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}

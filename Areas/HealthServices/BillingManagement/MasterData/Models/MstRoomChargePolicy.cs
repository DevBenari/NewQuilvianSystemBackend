using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

[Table("MstRoomChargePolicy", Schema = "public")]
public sealed class MstRoomChargePolicy : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(30)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    public int MinimumMinutes { get; set; }
    public int PeriodMinutes { get; set; }
    [Required, MaxLength(30)] public string RemainderRounding { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string TariffMoment { get; set; } = string.Empty;
    [Required, MaxLength(50)] public string LeaveRule { get; set; } = string.Empty;
    public DateTimeOffset EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
}

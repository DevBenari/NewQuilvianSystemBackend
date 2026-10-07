using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
public class PrescribeInpatientDietRequest
{
    public Guid? InstructingDoctorId { get; set; }
    public Guid DietTypeId { get; set; }
    public Guid FoodFormId { get; set; }
    [Range(1, 10000)] public int? EnergyRequirementKcal { get; set; }
    [MaxLength(1000)] public string? Instruction { get; set; }
    public DateTime? EffectiveStartAt { get; set; }
    [MaxLength(1000)] public string? ChangeReason { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}
public class StopInpatientDietRequest : StopGzDietRequest { public Guid? InstructingDoctorId { get; set; } }

using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("MstStaffingRatio", Schema = "public")]
    public class MstStaffingRatio : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid StaffingStandardId { get; set; }

        public Guid? ShiftId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? CompetencyId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RatioBasisCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RatioBasisName { get; set; } = string.Empty;

        public decimal WorkforceQuantity { get; set; } = 1m;
        public decimal WorkloadQuantity { get; set; } = 1m;

        [MaxLength(50)]
        public string WorkloadUnit { get; set; } = "Unit";

        public decimal? MinimumRatio { get; set; }
        public decimal? TargetRatio { get; set; }
        public decimal? MaximumRatio { get; set; }
        public decimal MinimumHeadcount { get; set; } = 0m;

        [MaxLength(30)]
        public string RoundingMethod { get; set; } = "Ceiling";
        // Ceiling, Floor, Nearest, NoRounding.

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int PriorityOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstStaffingStandard? StaffingStandard { get; set; }
        public MstShift? Shift { get; set; }
        public MstPosition? Position { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstCompetency? Competency { get; set; }
    }
}

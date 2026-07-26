using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("MstShiftSkillRequirement", Schema = "public")]
    public class MstShiftSkillRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RequirementName { get; set; } = string.Empty;

        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        public Guid ShiftId { get; set; }

        public Guid? ShiftGroupId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? CompetencyId { get; set; }

        [MaxLength(50)]
        public string? MinimumCompetencyLevel { get; set; }

        public decimal MinimumQualifiedHeadcount { get; set; } = 1m;
        public decimal TargetQualifiedHeadcount { get; set; } = 1m;
        public bool IsMandatory { get; set; } = true;
        public bool AllowSubstitution { get; set; } = false;

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int PriorityOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstShift? Shift { get; set; }
        public MstShiftGroup? ShiftGroup { get; set; }
        public MstPosition? Position { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstCompetency? Competency { get; set; }
    }
}

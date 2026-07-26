using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("TrxDailyStaffingRequirement", Schema = "public")]
    public class TrxDailyStaffingRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime StaffingDate { get; set; }

        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        public Guid HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        public Guid ShiftId { get; set; }

        public Guid? PositionId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? CompetencyId { get; set; }
        public Guid? StaffingStandardId { get; set; }
        public Guid? ShiftSkillRequirementId { get; set; }
        public Guid? StaffingGapAnalysisId { get; set; }

        public decimal MinimumRequiredHeadcount { get; set; } = 0m;
        public decimal TargetRequiredHeadcount { get; set; } = 0m;
        public decimal MaximumRequiredHeadcount { get; set; } = 0m;
        public decimal AvailableHeadcount { get; set; } = 0m;
        public decimal AllocatedHeadcount { get; set; } = 0m;
        public decimal GapHeadcount { get; set; } = 0m;

        [MaxLength(30)]
        public string RequirementStatus { get; set; } = "Draft";
        // Draft, Calculated, Confirmed, Published, Fulfilled, Shortage, Closed.

        [MaxLength(30)]
        public string GenerationSource { get; set; } = "Manual";

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public bool IsLocked { get; set; } = false;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstShift? Shift { get; set; }
        public MstPosition? Position { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstCompetency? Competency { get; set; }
        public MstStaffingStandard? StaffingStandard { get; set; }
        public MstShiftSkillRequirement? ShiftSkillRequirement { get; set; }
        public TrxStaffingGapAnalysis? StaffingGapAnalysis { get; set; }

        public ICollection<TrxWorkforceAllocation> WorkforceAllocations { get; set; }
            = new List<TrxWorkforceAllocation>();
    }
}

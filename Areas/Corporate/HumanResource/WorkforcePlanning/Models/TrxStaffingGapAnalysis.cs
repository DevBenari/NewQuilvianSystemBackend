using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("TrxStaffingGapAnalysis", Schema = "public")]
    public class TrxStaffingGapAnalysis : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string AnalysisNumber { get; set; } = string.Empty;

        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }

        [Required]
        public Guid LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? StaffingStandardId { get; set; }
        public Guid? StaffingRatioId { get; set; }
        public Guid? PositionHeadcountPlanId { get; set; }

        public decimal RequiredHeadcount { get; set; } = 0m;
        public decimal AvailableHeadcount { get; set; } = 0m;
        public decimal AssignedHeadcount { get; set; } = 0m;
        public decimal OnLeaveHeadcount { get; set; } = 0m;
        public decimal AbsentHeadcount { get; set; } = 0m;
        public decimal QualifiedHeadcount { get; set; } = 0m;
        public decimal GapHeadcount { get; set; } = 0m;
        public decimal GapPercentage { get; set; } = 0m;

        [MaxLength(20)]
        public string GapSeverity { get; set; } = "None";
        // None, Low, Medium, High, Critical.

        [MaxLength(30)]
        public string AnalysisSource { get; set; } = "Manual";
        // Manual, ScheduledJob, Roster, ManpowerPlan, StaffingStandard.

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public bool RequiresAction { get; set; } = false;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }

        [MaxLength(1000)]
        public string? RecommendedAction { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstShift? Shift { get; set; }
        public MstStaffingStandard? StaffingStandard { get; set; }
        public MstStaffingRatio? StaffingRatio { get; set; }
        public MstPositionHeadcountPlan? PositionHeadcountPlan { get; set; }
    }
}

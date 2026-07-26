using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("MstStaffingStandard", Schema = "public")]
    public class MstStaffingStandard : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string StandardCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string StandardName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string StandardType { get; set; } = "FixedHeadcount";
        // FixedHeadcount, WorkloadBased, RatioBased, ServiceCoverage, Custom.

        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkforceTypeId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? EmployeeGradeId { get; set; }

        public decimal MinimumHeadcount { get; set; } = 0m;
        public decimal TargetHeadcount { get; set; } = 0m;
        public decimal? MaximumHeadcount { get; set; }
        public decimal? StandardWorkloadValue { get; set; }

        [MaxLength(50)]
        public string? WorkloadUnit { get; set; }

        public decimal? CoverageHoursPerDay { get; set; }
        public bool IsShiftBased { get; set; } = false;
        public bool IsSkillMixRequired { get; set; } = false;

        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int PriorityOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstWorkforceType? WorkforceType { get; set; }
        public MstEmployeeCategory? EmployeeCategory { get; set; }
        public MstEmploymentType? EmploymentType { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }

        public ICollection<MstStaffingRatio> StaffingRatios { get; set; }
            = new List<MstStaffingRatio>();
    }
}

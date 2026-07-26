using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("MstWorkforceRequirement", Schema = "public")]
    public class MstWorkforceRequirement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public UserType UserType { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequirementCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RequirementCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;
        public bool IsMultipleAllowed { get; set; } = false;
        public bool IsFileRequired { get; set; } = true;
        public bool IsNumberRequired { get; set; } = false;
        public bool IsIssueDateRequired { get; set; } = false;
        public bool IsExpiredDateRequired { get; set; } = false;
        public bool IsVerificationRequired { get; set; } = true;
        public bool IsProfileRequired { get; set; } = false;

        [MaxLength(100)]
        public string? TargetEntityName { get; set; }

        [MaxLength(50)]
        public string RequirementScopeType { get; set; } = "Global";
        // Global, LegalEntity, HospitalSite, OrganizationUnit, Department,
        // Position, WorkforceType, EmployeeCategory, Profession, Shift.

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
        public Guid? WorkScheduleId { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? CompetencyId { get; set; }

        public decimal? MinimumQuantity { get; set; }
        public decimal? TargetQuantity { get; set; }
        public decimal? MaximumQuantity { get; set; }

        [MaxLength(50)]
        public string? MeasurementUnit { get; set; }

        public int? MinimumExperienceMonths { get; set; }

        [MaxLength(50)]
        public string? RequiredCompetencyLevel { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;
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
        public MstWorkSchedule? WorkSchedule { get; set; }
        public MstShift? Shift { get; set; }
        public MstCompetency? Competency { get; set; }
    }
}

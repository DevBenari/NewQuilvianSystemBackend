using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Performance.Models
{
    [Table("MstPerformanceTemplate", Schema = "public")]
    public class MstPerformanceTemplate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? PerformanceCycleId { get; set; }

        [Required]
        public Guid RatingScaleId { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        public Guid? ProfessionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TemplateCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TemplateName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TemplateType { get; set; } = "EmployeePerformance";
        // EmployeePerformance, Probation, Leadership, Clinical, Project, Custom

        public decimal TotalWeight { get; set; } = 100m;

        public decimal? MinimumPassingScore { get; set; }

        public bool IsSelfAssessmentRequired { get; set; } = true;

        public bool IsManagerAssessmentRequired { get; set; } = true;

        public bool IsPeerAssessmentAllowed { get; set; } = false;

        public bool IsSubordinateAssessmentAllowed { get; set; } = false;

        public bool IsCalibrationRequired { get; set; } = false;

        [MaxLength(2000)]
        public string? EmployeeInstructions { get; set; }

        [MaxLength(2000)]
        public string? ReviewerInstructions { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsDefault { get; set; } = false;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstPerformanceCycle? PerformanceCycle { get; set; }

        public MstPerformanceRatingScale? RatingScale { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstDepartment? Department { get; set; }

        public MstPosition? Position { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }

        public MstProfession? Profession { get; set; }

        public ICollection<MstPerformanceTemplateDetail> Details { get; set; }
            = new List<MstPerformanceTemplateDetail>();
    }
}

using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models
{
    [Table("MstOnboardingTemplate", Schema = "public")]
    public class MstOnboardingTemplate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(50)] public string TemplateCode { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string TemplateName { get; set; } = string.Empty;
        [MaxLength(50)] public string TemplateType { get; set; } = "NewHire";
        public int Version { get; set; } = 1;
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkforceTypeId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public int TargetCompletionDays { get; set; } = 30;
        public bool AutoCreateChecklist { get; set; } = true;
        public bool RequiresProbationReview { get; set; } = true;
        public int? ProbationReviewAfterDays { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsDefault { get; set; }
        [MaxLength(1500)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstWorkforceType? WorkforceType { get; set; }
        public MstEmployeeCategory? EmployeeCategory { get; set; }
        public MstEmploymentType? EmploymentType { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ICollection<MstOnboardingTemplateTask> Tasks { get; set; } = new List<MstOnboardingTemplateTask>();
    }
}

using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models
{
    [Table("MstApprovalMatrix", Schema = "public")]
    public class MstApprovalMatrix : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkflowDefinitionId { get; set; }

        [Required]
        public Guid WorkflowStepId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApprovalMatrixCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ApprovalMatrixName { get; set; } = string.Empty;

        public Guid? LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? RequesterPositionId { get; set; }

        public Guid? EmployeeCategoryId { get; set; }

        public Guid? EmploymentTypeId { get; set; }

        public decimal? MinimumAmount { get; set; }

        public decimal? MaximumAmount { get; set; }

        [MaxLength(10)]
        public string? CurrencyCode { get; set; } = "IDR";

        public decimal? MinimumDurationHours { get; set; }

        public decimal? MaximumDurationHours { get; set; }

        public int? MinimumDurationDays { get; set; }

        public int? MaximumDurationDays { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApproverSourceType { get; set; } = "RequesterManager";
        // RequesterManager, ManagerLevel, Position, OrganizationUnit,
        // Role, SpecificUser.

        public Guid? ApproverPositionId { get; set; }

        public Guid? ApproverOrganizationUnitId { get; set; }

        public Guid? SpecificApproverUserId { get; set; }

        [MaxLength(100)]
        public string? ApproverRoleCode { get; set; }

        public int? ManagerLevel { get; set; }

        public int Priority { get; set; } = 0;

        public bool IsFallback { get; set; } = false;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? ConditionDefinitionJson { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkflowDefinition? WorkflowDefinition { get; set; }

        public MstWorkflowStep? WorkflowStep { get; set; }

        public MstLegalEntity? LegalEntity { get; set; }

        public MstHospitalSite? HospitalSite { get; set; }

        public MstOrganizationUnit? OrganizationUnit { get; set; }

        public MstDepartment? Department { get; set; }

        public MstPosition? RequesterPosition { get; set; }

        public MstEmployeeCategory? EmployeeCategory { get; set; }

        public MstEmploymentType? EmploymentType { get; set; }

        public MstPosition? ApproverPosition { get; set; }

        public MstOrganizationUnit? ApproverOrganizationUnit { get; set; }
    }
}

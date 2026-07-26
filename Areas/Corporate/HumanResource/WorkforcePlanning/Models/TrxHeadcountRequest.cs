using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models
{
    [Table("TrxHeadcountRequest", Schema = "public")]
    public class TrxHeadcountRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string RequestNumber { get; set; } = string.Empty;

        public Guid? AnnualManpowerPlanId { get; set; }
        public Guid? ManpowerPlanDetailId { get; set; }

        [Required]
        public Guid LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public Guid? EmployeeGradeId { get; set; }
        public Guid? WorkforceTypeId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }
        public Guid? CostCenterId { get; set; }

        [Required]
        [MaxLength(30)]
        public string RequestType { get; set; } = "Additional";
        // Additional, Replacement, Temporary, Project, Seasonal.

        public decimal RequestedHeadcount { get; set; } = 1m;
        public decimal ApprovedHeadcount { get; set; } = 0m;
        public DateTime RequiredDate { get; set; }

        [MaxLength(20)]
        public string PriorityLevel { get; set; } = "Normal";

        public Guid? RequestReasonId { get; set; }

        [MaxLength(1000)]
        public string? BusinessJustification { get; set; }

        public decimal? EstimatedMonthlyCost { get; set; }

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [MaxLength(30)]
        public string RequestStatus { get; set; } = "Draft";
        // Draft, Submitted, WaitingApproval, Approved, Rejected, Cancelled, Fulfilled.

        public Guid? RequestedByWorkforceProfileId { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? FulfilledAt { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxAnnualManpowerPlan? AnnualManpowerPlan { get; set; }
        public TrxManpowerPlanDetail? ManpowerPlanDetail { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
        public MstWorkforceType? WorkforceType { get; set; }
        public MstEmployeeCategory? EmployeeCategory { get; set; }
        public MstEmploymentType? EmploymentType { get; set; }
        public MstContractType? ContractType { get; set; }
        public MstProfession? Profession { get; set; }
        public MstSpecialization? Specialization { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstWorkforceProfile? RequestedByWorkforceProfile { get; set; }
        public ApplicationUser? RequestedByUser { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}

using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models
{
    [Table("TrxJobOffer", Schema = "public")]
    public class TrxJobOffer : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string OfferNumber { get; set; } = string.Empty;

        [Required]
        public Guid CandidateApplicationId { get; set; }
        public Guid? JobRequisitionId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public Guid? EmployeeGradeId { get; set; }
        public Guid? SalaryGradeId { get; set; }
        public Guid? SalaryStructureId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? CostCenterId { get; set; }

        public decimal? BaseSalaryAmount { get; set; }

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public string? AllowanceConfigurationJson { get; set; }
        public string? BenefitConfigurationJson { get; set; }
        public DateTime ProposedStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public int? ProbationMonths { get; set; }
        public DateTime OfferDate { get; set; }
        public DateTime ValidUntil { get; set; }

        [MaxLength(30)]
        public string OfferStatus { get; set; } = "Draft";
        // Draft, WaitingApproval, Approved, Sent, Accepted, Rejected, Expired, Withdrawn, Cancelled.

        public DateTime? SentAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public Guid? RejectionReasonId { get; set; }

        [MaxLength(1000)]
        public string? CandidateResponseNotes { get; set; }

        [MaxLength(500)]
        public string? OfferDocumentPath { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public TrxCandidateApplication? CandidateApplication { get; set; }
        public TrxJobRequisition? JobRequisition { get; set; }
        public MstPosition? Position { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
        public MstSalaryGrade? SalaryGrade { get; set; }
        public MstSalaryStructure? SalaryStructure { get; set; }
        public MstEmploymentType? EmploymentType { get; set; }
        public MstContractType? ContractType { get; set; }
        public MstEmployeeCategory? EmployeeCategory { get; set; }
        public MstWorkLocation? WorkLocation { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}

using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    [Table("TrxBenefitClaim", Schema = "public")]
    public class TrxBenefitClaim : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmployeeBenefitEnrollmentId { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid BenefitPlanId { get; set; }

        public Guid? BenefitTypeId { get; set; }

        public Guid? PayrollPeriodId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string ClaimStatus { get; set; } = "Draft";

        public DateTime ClaimDate { get; set; }

        public DateTime? ServiceStartDate { get; set; }

        public DateTime? ServiceEndDate { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal ClaimedAmount { get; set; } = 0m;

        public decimal EligibleAmount { get; set; } = 0m;

        public decimal NonEligibleAmount { get; set; } = 0m;

        public decimal ApprovedAmount { get; set; } = 0m;

        public decimal PaidAmount { get; set; } = 0m;

        [MaxLength(200)]
        public string? ProviderName { get; set; }

        [MaxLength(100)]
        public string? ProviderReferenceNumber { get; set; }

        [MaxLength(100)]
        public string? ClaimCategory { get; set; }

        public string? EnrollmentSnapshotJson { get; set; }

        public string? LimitUsageSnapshotJson { get; set; }

        public string? ValidationResultJson { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? PaidAt { get; set; }

        public Guid? PaidByUserId { get; set; }

        [MaxLength(100)]
        public string? PaymentReferenceNumber { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxEmployeeBenefitEnrollment? EmployeeBenefitEnrollment { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstBenefitPlan? BenefitPlan { get; set; }
        public MstBenefitType? BenefitType { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? VerifiedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? PaidByUser { get; set; }

        public ICollection<TrxBenefitClaimItem> Items { get; set; } = new List<TrxBenefitClaimItem>();
        public ICollection<TrxBenefitClaimDocument> Documents { get; set; } = new List<TrxBenefitClaimDocument>();
        public ICollection<TrxBenefitClaimApproval> Approvals { get; set; } = new List<TrxBenefitClaimApproval>();
    }
}

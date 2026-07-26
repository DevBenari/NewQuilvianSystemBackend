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
    [Table("TrxEmployeeBenefitEnrollment", Schema = "public")]
    public class TrxEmployeeBenefitEnrollment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }

        public Guid? OrganizationAssignmentId { get; set; }

        public Guid BenefitPlanId { get; set; }

        public Guid? BenefitEligibilityRuleId { get; set; }

        public Guid? WorkflowDefinitionId { get; set; }

        public Guid? PayrollPeriodId { get; set; }

        [Required]
        [MaxLength(50)]
        public string EnrollmentNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string EnrollmentStatus { get; set; } = "Draft";
        // Draft, Submitted, PendingVerification, Approved, Active, Suspended, Cancelled, Expired, Rejected.

        [Required]
        [MaxLength(50)]
        public string CoverageLevel { get; set; } = "Individual";

        public DateTime EnrollmentDate { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal CoverageLimitAmount { get; set; } = 0m;

        public decimal UsedAmount { get; set; } = 0m;

        public decimal RemainingAmount { get; set; } = 0m;

        public decimal EmployerContributionAmount { get; set; } = 0m;

        public decimal EmployeeContributionAmount { get; set; } = 0m;

        public int MaximumDependents { get; set; } = 0;

        public int ActiveDependentCount { get; set; } = 0;

        public bool IsEligible { get; set; } = false;

        [MaxLength(1000)]
        public string? EligibilityReason { get; set; }

        public string? EligibilityResultJson { get; set; }

        public string? PlanSnapshotJson { get; set; }

        public DateTime? SubmittedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ActivatedAt { get; set; }

        public Guid? ActivatedByUserId { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstBenefitPlan? BenefitPlan { get; set; }
        public MstBenefitEligibilityRule? BenefitEligibilityRule { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? ActivatedByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }

        public ICollection<TrxEmployeeBenefitDependent> Dependents { get; set; } = new List<TrxEmployeeBenefitDependent>();
        public ICollection<TrxEmployeeInsuranceEnrollment> InsuranceEnrollments { get; set; } = new List<TrxEmployeeInsuranceEnrollment>();
        public ICollection<TrxBenefitClaim> Claims { get; set; } = new List<TrxBenefitClaim>();
    }
}

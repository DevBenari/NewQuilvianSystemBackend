using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.ExpenseManagement.Models
{
    [Table("TrxExpenseClaim", Schema = "public")]
    public class TrxExpenseClaim : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string ClaimNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public Guid? CostCenterId { get; set; }

        public Guid? ReimbursementPolicyId { get; set; }
        public Guid? BenefitPlanId { get; set; }
        public Guid? PaymentSettlementMethodId { get; set; }
        public Guid? BusinessTravelRequestId { get; set; }

        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        public Guid? PayrollPeriodId { get; set; }
        public Guid? FinancePaymentId { get; set; }
        public Guid? GlHeaderId { get; set; }

        [Required, MaxLength(40)]
        public string ClaimType { get; set; } = "General";
        // General, Medical, Transportation, Communication, Training,
        // WorkPurchase, Travel, Benefit, Other.

        [Required, MaxLength(250)]
        public string ClaimTitle { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ClaimDescription { get; set; }

        public DateOnly ClaimDate { get; set; }
        public DateOnly? PeriodStartDate { get; set; }
        public DateOnly? PeriodEndDate { get; set; }

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal TotalClaimedAmount { get; set; } = 0m;
        public decimal TotalEligibleAmount { get; set; } = 0m;
        public decimal TotalNonEligibleAmount { get; set; } = 0m;
        public decimal TotalApprovedAmount { get; set; } = 0m;
        public decimal TotalPaidAmount { get; set; } = 0m;
        public decimal TotalReversedAmount { get; set; } = 0m;
        public decimal OutstandingAmount { get; set; } = 0m;

        public decimal? PolicyTransactionLimitSnapshot { get; set; }
        public decimal? PolicyPeriodLimitSnapshot { get; set; }
        public decimal? BenefitLimitSnapshot { get; set; }
        public decimal PolicyUsedBeforeAmount { get; set; } = 0m;
        public decimal BenefitUsedBeforeAmount { get; set; } = 0m;

        public bool IsPolicyActive { get; set; } = false;
        public bool IsEmployeeGradeEligible { get; set; } = false;
        public bool IsTransactionDateValid { get; set; } = false;
        public bool IsCostCenterValid { get; set; } = false;
        public bool IsWithinPolicyLimit { get; set; } = false;
        public bool IsWithinBenefitLimit { get; set; } = false;
        public bool HasDuplicateReceipt { get; set; } = false;
        public bool IsValidationPassed { get; set; } = false;

        [Column(TypeName = "jsonb")]
        public string? ValidationResultJson { get; set; }

        [Required, MaxLength(40)]
        public string ClaimStatus { get; set; } = "Draft";
        // Draft, Submitted, WaitingSupervisorApproval, WaitingManagerApproval,
        // WaitingHrVerification, WaitingFinanceVerification, Approved, Rejected,
        // NeedRevision, PaymentProcessing, PartiallyPaid, Paid, Reversed, Cancelled.

        public int CurrentApprovalStep { get; set; } = 0;
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? PaymentCompletedAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public Guid? CancelledByUserId { get; set; }

        [MaxLength(2000)]
        public string? ApprovalNotes { get; set; }

        [MaxLength(2000)]
        public string? RejectionNotes { get; set; }

        [MaxLength(2000)]
        public string? CancellationReason { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstEmployee? Employee { get; set; }
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public MstPosition? Position { get; set; }
        public MstEmployeeGrade? EmployeeGrade { get; set; }
        public MstCostCenter? CostCenter { get; set; }
        public MstReimbursementPolicy? ReimbursementPolicy { get; set; }
        public MstBenefitPlan? BenefitPlan { get; set; }
        public MstPaymentSettlementMethod? PaymentSettlementMethod { get; set; }
        public TrxBusinessTravelRequest? BusinessTravelRequest { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public MstPayrollPeriod? PayrollPeriod { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? RejectedByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }

        public ICollection<TrxExpenseClaimItem> Items { get; set; } = new List<TrxExpenseClaimItem>();
        public ICollection<TrxExpenseReceipt> Receipts { get; set; } = new List<TrxExpenseReceipt>();
        public ICollection<TrxExpenseApproval> Approvals { get; set; } = new List<TrxExpenseApproval>();
        public ICollection<TrxExpenseVerification> Verifications { get; set; } = new List<TrxExpenseVerification>();
        public ICollection<TrxExpensePayment> Payments { get; set; } = new List<TrxExpensePayment>();
        public ICollection<TrxExpenseReversal> Reversals { get; set; } = new List<TrxExpenseReversal>();
    }
}

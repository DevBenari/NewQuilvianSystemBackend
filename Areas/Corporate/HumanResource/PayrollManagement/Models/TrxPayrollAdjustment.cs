using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models
{
    [Table("TrxPayrollAdjustment", Schema = "public")]
    public class TrxPayrollAdjustment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PayrollRunId { get; set; }

        public Guid? PayrollRunEmployeeId { get; set; }
        public Guid? PayrollComponentId { get; set; }
        public Guid? RequestReasonId { get; set; }
        public Guid? RejectionReasonId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        [Required, MaxLength(50)]
        public string AdjustmentNumber { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string AdjustmentType { get; set; } = "Add";
        // Add, Deduct, Replace, Correction, Reversal.

        [Required, MaxLength(30)]
        public string AdjustmentStatus { get; set; } = "Draft";
        // Draft, Submitted, Approved, Rejected, Applied, Cancelled.

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        public decimal Quantity { get; set; } = 1m;
        public decimal Rate { get; set; } = 0m;
        public decimal Amount { get; set; } = 0m;

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SourceType { get; set; }

        public Guid? SourceId { get; set; }

        public DateTime? SubmittedAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public DateTime? AppliedAt { get; set; }
        public Guid? AppliedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ApprovalNotes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPayrollRun? PayrollRun { get; set; }
        public TrxPayrollRunEmployee? PayrollRunEmployee { get; set; }
        public MstPayrollComponent? PayrollComponent { get; set; }
        public MstRequestReason? RequestReason { get; set; }
        public MstRejectionReason? RejectionReason { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? SubmittedByUser { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
        public ApplicationUser? AppliedByUser { get; set; }
    }
}

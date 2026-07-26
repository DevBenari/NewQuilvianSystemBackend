using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxCompensatoryLeave", Schema = "public")]
    public class TrxCompensatoryLeave : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string CompensatoryLeaveNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        public Guid? LeaveBalanceId { get; set; }
        public Guid? LeaveEntitlementId { get; set; }
        public Guid? BalanceTransactionId { get; set; }
        public Guid? WorkflowDefinitionId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }

        [Required, MaxLength(50)]
        public string SourceType { get; set; } = "Overtime";

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(100)]
        public string? SourceReferenceNumber { get; set; }

        public DateOnly EarnedDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }

        public decimal SourceHours { get; set; } = 0;
        public decimal EarnedDays { get; set; } = 0;
        public decimal UsedDays { get; set; } = 0;
        public decimal RemainingDays { get; set; } = 0;

        [Required, MaxLength(30)]
        public string CompensatoryLeaveStatus { get; set; } = "Draft";
        // Draft, Submitted, Approved, Rejected, Posted, PartiallyUsed, Used, Expired, Cancelled

        public Guid? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PostedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public WfpLeaveBalance? LeaveBalance { get; set; }
        public TrxLeaveEntitlement? LeaveEntitlement { get; set; }
        public TrxLeaveBalanceTransaction? BalanceTransaction { get; set; }
        public MstWorkflowDefinition? WorkflowDefinition { get; set; }
        public ApplicationUser? ApprovedByUser { get; set; }
    }
}

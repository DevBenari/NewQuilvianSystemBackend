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
    [Table("TrxLeaveEntitlement", Schema = "public")]
    public class TrxLeaveEntitlement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string EntitlementNumber { get; set; } = string.Empty;

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        public Guid? LeavePolicyId { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public Guid? LeaveBalanceId { get; set; }

        public int EntitlementYear { get; set; }
        public DateOnly PeriodStartDate { get; set; }
        public DateOnly PeriodEndDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }

        public decimal BaseEntitlementDays { get; set; } = 0;
        public decimal ProratedEntitlementDays { get; set; } = 0;
        public decimal AdditionalEntitlementDays { get; set; } = 0;
        public decimal CarryForwardEntitlementDays { get; set; } = 0;
        public decimal TotalEntitlementDays { get; set; } = 0;

        public bool IsProrated { get; set; } = false;
        public int ServiceMonthsAtGrant { get; set; } = 0;

        [Required, MaxLength(30)]
        public string EntitlementStatus { get; set; } = "Draft";
        // Draft, Generated, Posted, Adjusted, Expired, Cancelled

        [MaxLength(50)]
        public string SourceType { get; set; } = "Policy";

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(100)]
        public string? SourceReferenceNumber { get; set; }

        public DateTime? GeneratedAt { get; set; }
        public Guid? GeneratedByUserId { get; set; }
        public DateTime? PostedAt { get; set; }
        public Guid? PostedByUserId { get; set; }

        public string? CalculationDetailJson { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public MstLeavePolicy? LeavePolicy { get; set; }
        public MstLeaveEntitlementPolicy? LeaveEntitlementPolicy { get; set; }
        public WfpLeaveBalance? LeaveBalance { get; set; }
        public ApplicationUser? GeneratedByUser { get; set; }
        public ApplicationUser? PostedByUser { get; set; }

        public ICollection<TrxLeaveAccrual> Accruals { get; set; } = new List<TrxLeaveAccrual>();
        public ICollection<TrxLeaveBalanceTransaction> BalanceTransactions { get; set; } = new List<TrxLeaveBalanceTransaction>();
    }
}

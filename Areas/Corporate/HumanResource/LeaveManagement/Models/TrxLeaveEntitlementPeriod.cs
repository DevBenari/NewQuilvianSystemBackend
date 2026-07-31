using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveEntitlementPeriod", Schema = "public")]
    public class TrxLeaveEntitlementPeriod : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? LeaveTypeId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string PeriodCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PeriodName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string PeriodBasis { get; set; }
            = LeaveValueConstants.PeriodBasis.CalendarYear;

        public int PeriodYear { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string PeriodStatus { get; set; }
            = LeaveValueConstants.PeriodStatus.Open;

        public bool IsLocked { get; set; } = false;

        public DateTime? ProcessingStartedAt { get; set; }
        public Guid? ProcessingStartedByUserId { get; set; }

        public DateTime? ClosedAt { get; set; }
        public Guid? ClosedByUserId { get; set; }

        [MaxLength(1000)]
        public string? CloseReason { get; set; }

        public DateTime? ReopenedAt { get; set; }
        public Guid? ReopenedByUserId { get; set; }

        [MaxLength(1000)]
        public string? ReopenReason { get; set; }

        public int ReopenCount { get; set; } = 0;
        public DateTime? LastReconciledAt { get; set; }
        public string? ValidationSnapshotJson { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public MstLeaveType? LeaveType { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }

        public ApplicationUser? ProcessingStartedByUser { get; set; }
        public ApplicationUser? ClosedByUser { get; set; }
        public ApplicationUser? ReopenedByUser { get; set; }

        public ICollection<WfpLeaveBalance> LeaveBalances { get; set; }
            = new List<WfpLeaveBalance>();

        public ICollection<TrxLeaveEntitlement> Entitlements { get; set; }
            = new List<TrxLeaveEntitlement>();

        public ICollection<TrxLeaveBalanceTransaction> BalanceTransactions { get; set; }
            = new List<TrxLeaveBalanceTransaction>();

        public ICollection<TrxLeaveAdjustment> Adjustments { get; set; }
            = new List<TrxLeaveAdjustment>();

        public ICollection<TrxLeaveAccrualRun> AccrualRuns { get; set; }
            = new List<TrxLeaveAccrualRun>();

        public ICollection<TrxLeaveCarryForwardRun> SourceCarryForwardRuns { get; set; }
            = new List<TrxLeaveCarryForwardRun>();

        public ICollection<TrxLeaveCarryForwardRun> DestinationCarryForwardRuns { get; set; }
            = new List<TrxLeaveCarryForwardRun>();

        public ICollection<TrxLeaveCarryForward> SourceCarryForwards { get; set; }
            = new List<TrxLeaveCarryForward>();

        public ICollection<TrxLeaveCarryForward> DestinationCarryForwards { get; set; }
            = new List<TrxLeaveCarryForward>();
    }
}

using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveAccrualRun", Schema = "public")]
    public class TrxLeaveAccrualRun : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LeaveEntitlementPeriodId { get; set; }

        public Guid? LeaveTypeId { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required, MaxLength(50)]
        public string RunNumber { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string RunMode { get; set; }
            = LeaveValueConstants.BatchRunMode.Scheduled;

        [Required, MaxLength(30)]
        public string RunStatus { get; set; }
            = LeaveValueConstants.BatchRunStatus.Draft;

        public DateOnly ScheduledAccrualDate { get; set; }
        public DateOnly AccrualPeriodStartDate { get; set; }
        public DateOnly AccrualPeriodEndDate { get; set; }

        public bool IsDryRun { get; set; } = false;
        public bool ForceReprocess { get; set; } = false;

        public int RetryCount { get; set; } = 0;
        public int MaximumRetryCount { get; set; } = 3;

        public int TargetCount { get; set; } = 0;
        public int CalculatedCount { get; set; } = 0;
        public int PostedCount { get; set; } = 0;
        public int SkippedCount { get; set; } = 0;
        public int FailedCount { get; set; } = 0;

        public decimal TotalCalculatedDays { get; set; } = 0;
        public decimal TotalPostedDays { get; set; } = 0;

        [MaxLength(150)]
        public string? IdempotencyKey { get; set; }

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public Guid? TriggeredByUserId { get; set; }
        public Guid? CancelledByUserId { get; set; }

        public string? ParametersJson { get; set; }
        public string? ResultSummaryJson { get; set; }

        [MaxLength(4000)]
        public string? ErrorSummary { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxLeaveEntitlementPeriod? LeaveEntitlementPeriod { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public MstLeaveEntitlementPolicy? LeaveEntitlementPolicy { get; set; }
        public MstLegalEntity? LegalEntity { get; set; }
        public MstHospitalSite? HospitalSite { get; set; }
        public MstOrganizationUnit? OrganizationUnit { get; set; }
        public MstDepartment? Department { get; set; }
        public ApplicationUser? TriggeredByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }

        public ICollection<TrxLeaveAccrual> Accruals { get; set; }
            = new List<TrxLeaveAccrual>();
    }
}

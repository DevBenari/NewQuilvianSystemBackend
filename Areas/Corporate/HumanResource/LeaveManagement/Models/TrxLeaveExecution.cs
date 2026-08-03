using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveExecution", Schema = "public")]
    public class TrxLeaveExecution : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(60)]
        public string ExecutionNumber { get; set; } = string.Empty;

        [Required]
        public Guid LeaveRequestId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        public Guid? LeaveBalanceId { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public decimal RequestedDays { get; set; }
        public decimal ExecutedDays { get; set; }

        [Required, MaxLength(30)]
        public string ExecutionStatus { get; set; } = "Scheduled";

        [Required, MaxLength(30)]
        public string AttendanceIntegrationStatus { get; set; } = "Pending";

        [Required, MaxLength(30)]
        public string BalanceExecutionStatus { get; set; } = "Pending";

        public int ExpectedAttendanceDayCount { get; set; }
        public int AppliedAttendanceDayCount { get; set; }
        public int ConflictAttendanceDayCount { get; set; }
        public int FailedAttendanceDayCount { get; set; }

        public int TotalScheduledMinutes { get; set; }
        public int TotalPayableLeaveMinutes { get; set; }

        public DateTime? StartedAt { get; set; }
        public Guid? StartedByUserId { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedByUserId { get; set; }
        public DateTime? ReversedAt { get; set; }
        public Guid? ReversedByUserId { get; set; }

        public DateTime? LastAttemptAt { get; set; }
        public int RetryCount { get; set; }

        [MaxLength(120)]
        public string? CorrelationId { get; set; }

        [MaxLength(160)]
        public string? IdempotencyKey { get; set; }

        public string? ExecutionSnapshotJson { get; set; }
        public string? ResultSnapshotJson { get; set; }

        [MaxLength(4000)]
        public string? ErrorSummary { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public WfpLeaveRequest? LeaveRequest { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public WfpLeaveBalance? LeaveBalance { get; set; }
        public ApplicationUser? StartedByUser { get; set; }
        public ApplicationUser? CompletedByUser { get; set; }
        public ApplicationUser? ReversedByUser { get; set; }

        public ICollection<TrxLeaveAttendanceIntegration> AttendanceIntegrations { get; set; }
            = new List<TrxLeaveAttendanceIntegration>();
    }
}

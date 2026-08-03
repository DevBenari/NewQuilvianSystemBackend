using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models
{
    [Table("TrxLeaveAttendanceIntegration", Schema = "public")]
    public class TrxLeaveAttendanceIntegration : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid LeaveExecutionId { get; set; }

        [Required]
        public Guid LeaveRequestId { get; set; }

        public Guid? AttendanceDailyId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid LeaveTypeId { get; set; }

        public DateOnly LeaveDate { get; set; }
        public decimal RequestedLeaveDays { get; set; }
        public int? RequestedMinutes { get; set; }
        public bool IsHalfDay { get; set; }
        public bool IsHourly { get; set; }
        public bool IsPaidLeave { get; set; }

        public int ScheduledMinutes { get; set; }
        public int PayableLeaveMinutes { get; set; }

        [Required, MaxLength(30)]
        public string IntegrationStatus { get; set; } = "Pending";

        [MaxLength(50)]
        public string? AttendanceStatusBefore { get; set; }

        [MaxLength(50)]
        public string? AttendanceStatusAfter { get; set; }

        [MaxLength(30)]
        public string? ProcessingStatusBefore { get; set; }

        [MaxLength(30)]
        public string? ProcessingStatusAfter { get; set; }

        public DateTime? AppliedAt { get; set; }
        public Guid? AppliedByUserId { get; set; }
        public DateTime? ReversedAt { get; set; }
        public Guid? ReversedByUserId { get; set; }

        [MaxLength(160)]
        public string? IdempotencyKey { get; set; }

        public string? ScheduleSnapshotJson { get; set; }
        public string? ResultSnapshotJson { get; set; }

        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxLeaveExecution? LeaveExecution { get; set; }
        public WfpLeaveRequest? LeaveRequest { get; set; }
        public TrxAttendanceDaily? AttendanceDaily { get; set; }
        public MstWorkforceProfile? WorkforceProfile { get; set; }
        public MstLeaveType? LeaveType { get; set; }
        public ApplicationUser? AppliedByUser { get; set; }
        public ApplicationUser? ReversedByUser { get; set; }
    }
}

using QuilvianSystemBackend.Areas.Corporate.HumanResource.Attendance.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models
{
    [Table("WfpOvertimeRequest", Schema = "public")]
    public class WfpOvertimeRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        public Guid? AttendanceId { get; set; }

        public Guid? WorkScheduleAssignmentId { get; set; }

        public Guid? WorkScheduleId { get; set; }

        [Required]
        public DateTime OvertimeDate { get; set; }

        public TimeOnly? ScheduledStartTime { get; set; }

        public TimeOnly? ScheduledEndTime { get; set; }

        public DateTime? ActualCheckInAt { get; set; }

        public DateTime? ActualCheckOutAt { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }

        public bool IsOvernight { get; set; } = false;

        [Required]
        public int TotalMinutes { get; set; } = 0;

        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public OvertimeApprovalStatus ApprovalStatus { get; set; } = OvertimeApprovalStatus.PendingApproval;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public Guid? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(250)]
        public string? ApprovalNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public DateTime? RejectedAt { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public DateTime? CancelledAt { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        public bool IsPayrollProcessed { get; set; } = false;

        public DateTime? PayrollProcessedAt { get; set; }

        public Guid? PayrollProcessedByUserId { get; set; }

        [MaxLength(50)]
        public string? PayrollPeriodCode { get; set; }

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(100)]
        public string? AttachmentContentType { get; set; }

        public bool IsActive { get; set; } = true;

        public MstWorkforceProfile? WorkforceProfile { get; set; }

        public EmpAttendance? Attendance { get; set; }

        public WfpWorkScheduleAssignment? WorkScheduleAssignment { get; set; }

        public MstWorkSchedule? WorkSchedule { get; set; }

        public ApplicationUser? ApprovedByUser { get; set; }

        public ApplicationUser? RejectedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }

        public ApplicationUser? PayrollProcessedByUser { get; set; }
    }
}

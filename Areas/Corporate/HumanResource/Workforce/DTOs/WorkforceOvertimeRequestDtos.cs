using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceOvertimeRequestResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? AttendanceId { get; set; }

        public Guid? WorkScheduleAssignmentId { get; set; }

        public Guid? WorkScheduleId { get; set; }

        public string? WorkScheduleCode { get; set; }

        public string? WorkScheduleName { get; set; }

        public DateTime OvertimeDate { get; set; }

        public TimeOnly? ScheduledStartTime { get; set; }

        public TimeOnly? ScheduledEndTime { get; set; }

        public DateTime? ActualCheckInAt { get; set; }

        public DateTime? ActualCheckOutAt { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public bool IsOvernight { get; set; }

        public int TotalMinutes { get; set; }

        public decimal TotalHours { get; set; }

        public string Reason { get; set; } = string.Empty;

        public OvertimeApprovalStatus ApprovalStatus { get; set; }

        public DateTime RequestedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }

        public string? ApprovedByUserName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? ApprovalNote { get; set; }

        public Guid? RejectedByUserId { get; set; }

        public string? RejectedByUserName { get; set; }

        public DateTime? RejectedAt { get; set; }

        public string? RejectedReason { get; set; }

        public Guid? CancelledByUserId { get; set; }

        public string? CancelledByUserName { get; set; }

        public DateTime? CancelledAt { get; set; }

        public string? CancelReason { get; set; }

        public bool IsPayrollProcessed { get; set; }

        public DateTime? PayrollProcessedAt { get; set; }

        public Guid? PayrollProcessedByUserId { get; set; }

        public string? PayrollProcessedByUserName { get; set; }

        public string? PayrollPeriodCode { get; set; }

        public string? AttachmentPath { get; set; }

        public string? AttachmentContentType { get; set; }

        public bool HasAttachment { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceOvertimeRequestListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int PendingData { get; set; }

        public int ApprovedData { get; set; }

        public int RejectedData { get; set; }

        public int CancelledData { get; set; }

        public int PayrollProcessedData { get; set; }

        public int TotalMinutes { get; set; }

        public int ApprovedMinutes { get; set; }

        public int PendingMinutes { get; set; }

        public List<WorkforceOvertimeRequestResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceOvertimeRequestRequest
    {
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

        public int? TotalMinutes { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(100)]
        public string? AttachmentContentType { get; set; }
    }

    public class UpdateWorkforceOvertimeRequestRequest
    {
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

        public int? TotalMinutes { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(100)]
        public string? AttachmentContentType { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ApproveWorkforceOvertimeRequestRequest
    {
        [MaxLength(250)]
        public string? ApprovalNote { get; set; }
    }

    public class RejectWorkforceOvertimeRequestRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class CancelWorkforceOvertimeRequestRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }

    public class UpdateWorkforceOvertimePayrollStatusRequest
    {
        public bool IsPayrollProcessed { get; set; }

        [MaxLength(50)]
        public string? PayrollPeriodCode { get; set; }
    }
}

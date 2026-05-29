using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceLeaveRequestResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Guid? LeaveBalanceId { get; set; }

        public LeaveType LeaveType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal TotalDays { get; set; }

        public bool IsHalfDay { get; set; }

        public bool IsDeductBalance { get; set; }

        public string Reason { get; set; } = string.Empty;

        public LeaveApprovalStatus ApprovalStatus { get; set; }

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

        public string? AttachmentPath { get; set; }

        public string? AttachmentContentType { get; set; }

        public bool HasAttachment { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceLeaveRequestListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int PendingData { get; set; }

        public int ApprovedData { get; set; }

        public int RejectedData { get; set; }

        public int CancelledData { get; set; }

        public decimal TotalRequestedDays { get; set; }

        public decimal ApprovedDays { get; set; }

        public decimal PendingDays { get; set; }

        public List<WorkforceLeaveRequestResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceLeaveRequestRequest
    {
        [Required]
        public LeaveType LeaveType { get; set; } = LeaveType.AnnualLeave;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public decimal? TotalDays { get; set; }

        public bool IsHalfDay { get; set; } = false;

        public bool IsDeductBalance { get; set; } = true;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        [MaxLength(100)]
        public string? AttachmentContentType { get; set; }
    }

    public class ApproveWorkforceLeaveRequestRequest
    {
        [MaxLength(250)]
        public string? ApprovalNote { get; set; }
    }

    public class RejectWorkforceLeaveRequestRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class CancelWorkforceLeaveRequestRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}

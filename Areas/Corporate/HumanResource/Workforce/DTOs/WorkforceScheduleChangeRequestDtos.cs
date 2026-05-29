using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceScheduleChangeRequestResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }

        public Guid? CurrentWorkScheduleAssignmentId { get; set; }
        public DateTime? CurrentScheduleDate { get; set; }
        public Guid? CurrentWorkScheduleId { get; set; }
        public string? CurrentWorkScheduleCode { get; set; }
        public string? CurrentWorkScheduleName { get; set; }

        public DateTime RequestedScheduleDate { get; set; }
        public Guid? RequestedWorkScheduleId { get; set; }
        public string? RequestedWorkScheduleCode { get; set; }
        public string? RequestedWorkScheduleName { get; set; }

        public ScheduleChangeRequestType RequestType { get; set; }
        public string Reason { get; set; } = string.Empty;
        public ScheduleRequestApprovalStatus ApprovalStatus { get; set; }
        public DateTime RequestedAt { get; set; }

        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByUserName { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectedReason { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceScheduleChangeRequestListResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int PendingData { get; set; }
        public int ApprovedData { get; set; }
        public int RejectedData { get; set; }
        public int CancelledData { get; set; }
        public List<WorkforceScheduleChangeRequestResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceScheduleChangeRequest
    {
        public Guid? CurrentWorkScheduleAssignmentId { get; set; }

        [Required]
        public DateTime RequestedScheduleDate { get; set; }

        public Guid? RequestedWorkScheduleId { get; set; }

        [Required]
        public ScheduleChangeRequestType RequestType { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceScheduleChangeRequest
    {
        public Guid? CurrentWorkScheduleAssignmentId { get; set; }

        [Required]
        public DateTime RequestedScheduleDate { get; set; }

        public Guid? RequestedWorkScheduleId { get; set; }

        [Required]
        public ScheduleChangeRequestType RequestType { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceScheduleChangeStatusRequest
    {
        [Required]
        public ScheduleRequestApprovalStatus ApprovalStatus { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ApproveWorkforceScheduleChangeRequest
    {
        public bool ApplyToScheduleAssignment { get; set; } = false;

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class RejectWorkforceScheduleChangeRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class CancelWorkforceScheduleChangeRequest
    {
        [MaxLength(250)]
        public string? CancelReason { get; set; }
    }
}

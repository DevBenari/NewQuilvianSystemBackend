using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceShiftSwapRequestResponse
    {
        public Guid Id { get; set; }

        public Guid RequesterWorkforceProfileId { get; set; }
        public string RequesterProfileCode { get; set; } = string.Empty;
        public string RequesterDisplayName { get; set; } = string.Empty;
        public UserType RequesterUserType { get; set; }

        public Guid TargetWorkforceProfileId { get; set; }
        public string TargetProfileCode { get; set; } = string.Empty;
        public string TargetDisplayName { get; set; } = string.Empty;
        public UserType TargetUserType { get; set; }

        public Guid RequesterScheduleAssignmentId { get; set; }
        public DateTime? RequesterScheduleDate { get; set; }
        public Guid? RequesterWorkScheduleId { get; set; }
        public string? RequesterWorkScheduleCode { get; set; }
        public string? RequesterWorkScheduleName { get; set; }

        public Guid TargetScheduleAssignmentId { get; set; }
        public DateTime? TargetScheduleDate { get; set; }
        public Guid? TargetWorkScheduleId { get; set; }
        public string? TargetWorkScheduleCode { get; set; }
        public string? TargetWorkScheduleName { get; set; }

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

    public class WorkforceShiftSwapRequestListResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int PendingData { get; set; }
        public int ApprovedData { get; set; }
        public int RejectedData { get; set; }
        public int CancelledData { get; set; }
        public List<WorkforceShiftSwapRequestResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceShiftSwapRequest
    {
        [Required]
        public Guid RequesterWorkforceProfileId { get; set; }

        [Required]
        public Guid TargetWorkforceProfileId { get; set; }

        [Required]
        public Guid RequesterScheduleAssignmentId { get; set; }

        [Required]
        public Guid TargetScheduleAssignmentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceShiftSwapRequest
    {
        [Required]
        public Guid RequesterWorkforceProfileId { get; set; }

        [Required]
        public Guid TargetWorkforceProfileId { get; set; }

        [Required]
        public Guid RequesterScheduleAssignmentId { get; set; }

        [Required]
        public Guid TargetScheduleAssignmentId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceShiftSwapStatusRequest
    {
        [Required]
        public ScheduleRequestApprovalStatus ApprovalStatus { get; set; }

        [MaxLength(250)]
        public string? RejectedReason { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ApproveWorkforceShiftSwapRequest
    {
        public bool ApplyToScheduleAssignment { get; set; } = false;

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class RejectWorkforceShiftSwapRequest
    {
        [Required]
        [MaxLength(250)]
        public string RejectedReason { get; set; } = string.Empty;
    }

    public class CancelWorkforceShiftSwapRequest
    {
        [MaxLength(250)]
        public string? CancelReason { get; set; }
    }
}

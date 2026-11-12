using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    /// <summary>
    /// DTO permohonan izin pulang cepat administratif non-deductible (BE-HRD-108, HRD-DEC-107, HRD-Q-47).
    /// </summary>
    public class CreateEarlyLeavePermissionRequest
    {
        [Required]
        public Guid AttendanceDailyId { get; set; }

        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public DateTime RequestedEarlyLeaveAt { get; set; }

        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateEarlyLeavePermissionRequest
    {
        [Required]
        public DateTime RequestedEarlyLeaveAt { get; set; }

        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class ApproveEarlyLeavePermissionRequest
    {
        [Required]
        public Guid ApproverWorkforceProfileId { get; set; }

        [MaxLength(500)]
        public string? ApprovalNote { get; set; }
    }

    public class RejectEarlyLeavePermissionRequest
    {
        [Required]
        public Guid ApproverWorkforceProfileId { get; set; }

        [Required, MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class EarlyLeavePermissionQueryRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? WorkforceProfileId { get; set; }
        public string? Status { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class EarlyLeavePermissionResponse
    {
        public Guid Id { get; set; }
        public Guid AttendanceDailyId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNik { get; set; } = string.Empty;
        public string? DepartmentName { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public DateTime? ScheduledCheckInAt { get; set; }
        public DateTime? ScheduledCheckOutAt { get; set; }
        public DateTime RequestedEarlyLeaveAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? ApprovedByWorkforceId { get; set; }
        public string? ApproverName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsDeductible { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreateDateTime { get; set; }
    }

    public class EarlyLeavePermissionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<EarlyLeavePermissionResponse> Items { get; set; } = new();
    }
}

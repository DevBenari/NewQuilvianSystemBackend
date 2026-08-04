using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs
{
    public class LeaveTypeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int PaidLeaveData { get; set; }
        public int BalanceDeductedData { get; set; }
    }

    public class LeaveTypeResponse
    {
        public Guid Id { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveCategory { get; set; } = string.Empty;
        public bool IsPaidLeave { get; set; }
        public bool IsBalanceDeducted { get; set; }
        public bool AllowHalfDay { get; set; }
        public bool AllowHourly { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool RequiresMedicalCertificate { get; set; }
        public int? AttachmentRequiredAfterDays { get; set; }
        public int DefaultMinimumNoticeDays { get; set; }
        public int? DefaultMaximumConsecutiveDays { get; set; }
        public string? ColorCode { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int LeavePolicyCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeaveTypeDetailResponse : LeaveTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeaveTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveCategory { get; set; } = string.Empty;
        public bool IsPaidLeave { get; set; }
        public bool IsBalanceDeducted { get; set; }
    }

    public class LeaveTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveTypeOptionResponse> Items { get; set; } = new();
    }

    public class LeaveTypeFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public LeaveTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LeaveTypeStringOptionResponse> LeaveCategoryOptions { get; set; } = new();
        public List<LeaveTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LeaveTypeDefaultFilterResponse
    {
        public string? LeaveCategory { get; set; }
        public bool? IsPaidLeave { get; set; }
        public bool? IsBalanceDeducted { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "leaveTypeName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveTypeStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateLeaveTypeRequest
    {
        [Required, MaxLength(150)]
        public string LeaveTypeName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LeaveCategory { get; set; } = "Annual";

        public bool IsPaidLeave { get; set; } = true;
        public bool IsBalanceDeducted { get; set; } = true;
        public bool AllowHalfDay { get; set; }
        public bool AllowHourly { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool RequiresMedicalCertificate { get; set; }

        [Range(1, int.MaxValue)]
        public int? AttachmentRequiredAfterDays { get; set; }

        [Range(0, int.MaxValue)]
        public int DefaultMinimumNoticeDays { get; set; }

        [Range(1, int.MaxValue)]
        public int? DefaultMaximumConsecutiveDays { get; set; }

        [MaxLength(20)]
        public string? ColorCode { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateLeaveTypeRequest : CreateLeaveTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLeaveTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteLeaveTypeRequest
    {
        [MaxLength(500)]
        public string? DeleteReason { get; set; }
    }

    public class LeaveTypeCreateResponse
    {
        public Guid Id { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeaveTypeUpdateResponse
    {
        public Guid Id { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeaveCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeaveTypeDeleteResponse
    {
        public Guid Id { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}

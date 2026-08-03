using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs
{
    public class LeaveAdjustmentReasonSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int OpeningBalanceAllowedData { get; set; }
        public int ApprovalRequiredData { get; set; }
    }

    public class LeaveAdjustmentReasonResponse
    {
        public Guid Id { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string? LeaveTypeCode { get; set; }
        public string? LeaveTypeName { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string ReasonCategory { get; set; } = string.Empty;
        public string AllowedDirection { get; set; } = string.Empty;
        public bool AllowOpeningBalance { get; set; }
        public bool AllowManualAdjustment { get; set; }
        public bool AllowCorrection { get; set; }
        public bool AllowReversal { get; set; }
        public decimal? MaximumAdjustmentDays { get; set; }
        public bool RequiresComment { get; set; }
        public bool RequiresAttachment { get; set; }
        public bool RequiresApproval { get; set; }
        public string? ApprovalWorkflowCode { get; set; }
        public int SortOrder { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeaveAdjustmentReasonDetailResponse : LeaveAdjustmentReasonResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeaveAdjustmentReasonOptionResponse
    {
        public Guid Id { get; set; }
        public Guid? LeaveTypeId { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string ReasonCategory { get; set; } = string.Empty;
        public string AllowedDirection { get; set; } = string.Empty;
        public bool RequiresApproval { get; set; }
    }

    public class LeaveAdjustmentReasonOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveAdjustmentReasonOptionResponse> Items { get; set; } = new();
    }

    public class LeaveAdjustmentReasonFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public LeaveAdjustmentReasonDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LeaveAdjustmentReasonSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LeaveAdjustmentReasonDefaultFilterResponse
    {
        public Guid? LeaveTypeId { get; set; }
        public string? ReasonCategory { get; set; }
        public string? AllowedDirection { get; set; }
        public bool? AllowOpeningBalance { get; set; }
        public bool? RequiresApproval { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveAdjustmentReasonSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateLeaveAdjustmentReasonRequest
    {
        public Guid? LeaveTypeId { get; set; }
        [Required, MaxLength(200)]
        public string ReasonName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string ReasonCategory { get; set; } = "ManualAdjustment";
        [Required, MaxLength(20)]
        public string AllowedDirection { get; set; } = "Both";
        public bool AllowOpeningBalance { get; set; }
        public bool AllowManualAdjustment { get; set; } = true;
        public bool AllowCorrection { get; set; } = true;
        public bool AllowReversal { get; set; } = true;
        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? MaximumAdjustmentDays { get; set; }
        public bool RequiresComment { get; set; } = true;
        public bool RequiresAttachment { get; set; }
        public bool RequiresApproval { get; set; } = true;
        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }
        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class UpdateLeaveAdjustmentReasonRequest : CreateLeaveAdjustmentReasonRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLeaveAdjustmentReasonStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteLeaveAdjustmentReasonRequest
    {
        [MaxLength(1000)]
        public string? DeleteReason { get; set; }
    }

    public class LeaveAdjustmentReasonCreateResponse
    {
        public Guid Id { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string ReasonCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeaveAdjustmentReasonUpdateResponse
    {
        public Guid Id { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public string ReasonCategory { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeaveAdjustmentReasonDeleteResponse
    {
        public Guid Id { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs
{
    public class LeavePolicySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int DefaultData { get; set; }
        public int FallbackData { get; set; }
    }

    public class LeavePolicyResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeCode { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public Guid? LegalEntityId { get; set; }
        public string? LegalEntityName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? PositionId { get; set; }
        public string? PositionName { get; set; }
        public Guid? WorkLocationId { get; set; }
        public string? WorkLocationName { get; set; }
        public Guid? WorkforceTypeId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? EmploymentStatusId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public string LeavePolicyCode { get; set; } = string.Empty;
        public string LeavePolicyName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsFallback { get; set; }
        public int MinimumServiceMonths { get; set; }
        public int MinimumNoticeDays { get; set; }
        public int? MaximumRequestDays { get; set; }
        public int? MinimumRequestMinutes { get; set; }
        public bool AllowDuringProbation { get; set; }
        public bool AllowNegativeBalance { get; set; }
        public decimal? NegativeBalanceLimitDays { get; set; }
        public bool AllowBackdatedRequest { get; set; }
        public int BackdatedLimitDays { get; set; }
        public bool AllowFutureDatedRequest { get; set; }
        public int? MaximumAdvanceRequestDays { get; set; }
        public string DayCalculationMethod { get; set; } = string.Empty;
        public bool ExcludeHoliday { get; set; }
        public bool ExcludeWeeklyOff { get; set; }
        public string ReservationTiming { get; set; } = string.Empty;
        public string DeductionTiming { get; set; } = string.Empty;
        public bool RequireAttachment { get; set; }
        public int? AttachmentRequiredAfterDays { get; set; }
        public bool RequireReplacementEmployee { get; set; }
        public bool RequireManagerApproval { get; set; }
        public bool RequireHrVerification { get; set; }
        public string? ApprovalWorkflowCode { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int EntitlementPolicyCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeavePolicyDetailResponse : LeavePolicyResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeavePolicyOptionResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public string LeavePolicyCode { get; set; } = string.Empty;
        public string LeavePolicyName { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool IsFallback { get; set; }
        public bool IsDefault { get; set; }
    }

    public class LeavePolicyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeavePolicyOptionResponse> Items { get; set; } = new();
    }

    public class LeavePolicyFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public LeavePolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LeavePolicySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LeavePolicyDefaultFilterResponse
    {
        public Guid? LeaveTypeId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public bool? IsFallback { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "priority";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeavePolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateLeavePolicyRequest
    {
        [Required]
        public Guid LeaveTypeId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? WorkforceTypeId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? EmploymentStatusId { get; set; }
        public Guid? ContractTypeId { get; set; }

        [Required, MaxLength(150)]
        public string LeavePolicyName { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Priority { get; set; }
        public bool IsFallback { get; set; }

        [Range(0, int.MaxValue)]
        public int MinimumServiceMonths { get; set; }
        [Range(0, int.MaxValue)]
        public int MinimumNoticeDays { get; set; }
        [Range(1, int.MaxValue)]
        public int? MaximumRequestDays { get; set; }
        [Range(1, int.MaxValue)]
        public int? MinimumRequestMinutes { get; set; }

        public bool AllowDuringProbation { get; set; }
        public bool AllowNegativeBalance { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? NegativeBalanceLimitDays { get; set; }
        public bool AllowBackdatedRequest { get; set; }
        [Range(0, int.MaxValue)]
        public int BackdatedLimitDays { get; set; }
        public bool AllowFutureDatedRequest { get; set; } = true;
        [Range(1, int.MaxValue)]
        public int? MaximumAdvanceRequestDays { get; set; }

        [Required, MaxLength(50)]
        public string DayCalculationMethod { get; set; } = "ScheduledWorkDays";
        public bool ExcludeHoliday { get; set; } = true;
        public bool ExcludeWeeklyOff { get; set; } = true;

        [Required, MaxLength(30)]
        public string ReservationTiming { get; set; } = "OnSubmit";
        [Required, MaxLength(30)]
        public string DeductionTiming { get; set; } = "OnApproval";

        public bool RequireAttachment { get; set; }
        [Range(1, int.MaxValue)]
        public int? AttachmentRequiredAfterDays { get; set; }
        public bool RequireReplacementEmployee { get; set; }
        public bool RequireManagerApproval { get; set; } = true;
        public bool RequireHrVerification { get; set; }

        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateLeavePolicyRequest : CreateLeavePolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLeavePolicyStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsFallback { get; set; }
    }

    public class DeleteLeavePolicyRequest
    {
        [MaxLength(500)]
        public string? DeleteReason { get; set; }
    }

    public class LeavePolicyCreateResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeavePolicyCode { get; set; } = string.Empty;
        public string LeavePolicyName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsFallback { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeavePolicyUpdateResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeavePolicyCode { get; set; } = string.Empty;
        public string LeavePolicyName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsFallback { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeavePolicyDeleteResponse
    {
        public Guid Id { get; set; }
        public string LeavePolicyCode { get; set; } = string.Empty;
        public string LeavePolicyName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}

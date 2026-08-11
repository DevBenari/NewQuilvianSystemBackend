using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs
{
    public class LeaveEntitlementPolicySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int DefaultData { get; set; }
        public int WithCarryForwardData { get; set; }
    }

    public class LeaveEntitlementPolicyResponse
    {
        public Guid Id { get; set; }
        public Guid LeavePolicyId { get; set; }
        public string LeavePolicyCode { get; set; } = string.Empty;
        public string LeavePolicyName { get; set; } = string.Empty;
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public string EntitlementPolicyCode { get; set; } = string.Empty;
        public string EntitlementPolicyName { get; set; } = string.Empty;
        public string EntitlementMethod { get; set; } = string.Empty;
        public string PeriodBasis { get; set; } = string.Empty;
        public string GrantTiming { get; set; } = string.Empty;
        public decimal AnnualEntitlementDays { get; set; }
        public string AccrualFrequency { get; set; } = string.Empty;
        public string AccrualTiming { get; set; } = string.Empty;
        public decimal AccrualAmountDays { get; set; }
        public int? AccrualStartMonth { get; set; }
        public int? AccrualStartDay { get; set; }
        public int? AccrualDayOfMonth { get; set; }
        public string FirstAccrualRule { get; set; } = string.Empty;
        public string FinalAccrualRule { get; set; } = string.Empty;
        public decimal? AccrualMaximumPerPeriodDays { get; set; }
        public bool IsProratedOnJoin { get; set; }
        public bool IsProratedOnSeparation { get; set; }
        public int MinimumServiceMonths { get; set; }
        public decimal? MaximumBalanceDays { get; set; }
        public int? ResetMonth { get; set; }
        public int? ResetDay { get; set; }
        public string RoundingMethod { get; set; } = string.Empty;
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int CarryForwardPolicyCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeaveEntitlementPolicyDetailResponse : LeaveEntitlementPolicyResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeaveEntitlementPolicyOptionResponse
    {
        public Guid Id { get; set; }
        public Guid LeavePolicyId { get; set; }
        public string LeavePolicyName { get; set; } = string.Empty;
        public string EntitlementPolicyCode { get; set; } = string.Empty;
        public string EntitlementPolicyName { get; set; } = string.Empty;
        public string EntitlementMethod { get; set; } = string.Empty;
        public decimal AnnualEntitlementDays { get; set; }
        public bool IsDefault { get; set; }
    }

    public class LeaveEntitlementPolicyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveEntitlementPolicyOptionResponse> Items { get; set; } = new();
    }

    public class LeaveEntitlementPolicyFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public LeaveEntitlementPolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LeaveEntitlementPolicyCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<LeaveEntitlementPolicySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LeaveEntitlementPolicyDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? LeavePolicyId { get; set; }
        public string? EntitlementMethod { get; set; }
        public string? PeriodBasis { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "entitlementPolicyName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveEntitlementPolicyCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveEntitlementPolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateLeaveEntitlementPolicyRequest
    {
        [Required]
        public Guid LeavePolicyId { get; set; }
        [Required, MaxLength(150)]
        public string EntitlementPolicyName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string EntitlementMethod { get; set; } = "AnnualGrant";
        [Required, MaxLength(50)]
        public string PeriodBasis { get; set; } = "CalendarYear";
        [Required, MaxLength(50)]
        public string GrantTiming { get; set; } = "StartOfPeriod";
        [Range(typeof(decimal), "0", "999999999")]
        public decimal AnnualEntitlementDays { get; set; }
        [Required, MaxLength(50)]
        public string AccrualFrequency { get; set; } = "Annual";
        [Required, MaxLength(50)]
        public string AccrualTiming { get; set; } = "EndOfPeriod";
        [Range(typeof(decimal), "0", "999999999")]
        public decimal AccrualAmountDays { get; set; }
        [Range(1, 12)]
        public int? AccrualStartMonth { get; set; }
        [Range(1, 31)]
        public int? AccrualStartDay { get; set; }
        [Range(1, 31)]
        public int? AccrualDayOfMonth { get; set; }
        [Required, MaxLength(50)]
        public string FirstAccrualRule { get; set; } = "Prorated";
        [Required, MaxLength(50)]
        public string FinalAccrualRule { get; set; } = "Prorated";
        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? AccrualMaximumPerPeriodDays { get; set; }
        public bool IsProratedOnJoin { get; set; } = true;
        public bool IsProratedOnSeparation { get; set; } = true;
        [Range(0, int.MaxValue)]
        public int MinimumServiceMonths { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? MaximumBalanceDays { get; set; }
        [Range(1, 12)]
        public int? ResetMonth { get; set; }
        [Range(1, 31)]
        public int? ResetDay { get; set; }
        [Required, MaxLength(50)]
        public string RoundingMethod { get; set; } = "None";
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateLeaveEntitlementPolicyRequest : CreateLeaveEntitlementPolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLeaveEntitlementPolicyStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }

    public class DeleteLeaveEntitlementPolicyRequest
    {
        [MaxLength(500)]
        public string? DeleteReason { get; set; }
    }

    public class LeaveEntitlementPolicyCreateResponse
    {
        public Guid Id { get; set; }
        public Guid LeavePolicyId { get; set; }
        public string EntitlementPolicyCode { get; set; } = string.Empty;
        public string EntitlementPolicyName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeaveEntitlementPolicyUpdateResponse
    {
        public Guid Id { get; set; }
        public Guid LeavePolicyId { get; set; }
        public string EntitlementPolicyCode { get; set; } = string.Empty;
        public string EntitlementPolicyName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeaveEntitlementPolicyDeleteResponse
    {
        public Guid Id { get; set; }
        public string EntitlementPolicyCode { get; set; } = string.Empty;
        public string EntitlementPolicyName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}

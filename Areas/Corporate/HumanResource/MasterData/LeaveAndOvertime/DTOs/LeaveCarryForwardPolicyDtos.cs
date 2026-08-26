using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs
{
    public class LeaveCarryForwardPolicySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int EnabledData { get; set; }
        public int DefaultData { get; set; }
        public int PayoutAllowedData { get; set; }
    }

    public class LeaveCarryForwardPolicyResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveEntitlementPolicyId { get; set; }
        public string LeaveEntitlementPolicyCode { get; set; } = string.Empty;
        public string LeaveEntitlementPolicyName { get; set; } = string.Empty;
        public Guid LeavePolicyId { get; set; }
        public string LeavePolicyName { get; set; } = string.Empty;
        public Guid? DestinationLeaveTypeId { get; set; }
        public string? DestinationLeaveTypeName { get; set; }
        public string CarryForwardPolicyCode { get; set; } = string.Empty;
        public string CarryForwardPolicyName { get; set; } = string.Empty;
        public bool IsCarryForwardEnabled { get; set; }
        public decimal? MinimumCarryForwardDays { get; set; }
        public decimal? MaximumCarryForwardDays { get; set; }
        public int? MaximumCarryForwardPeriods { get; set; }
        public decimal CarryForwardPercentage { get; set; }
        public string CarryForwardExecutionTiming { get; set; } = string.Empty;
        public string RoundingMethod { get; set; } = string.Empty;
        public string ExpiryMethod { get; set; } = string.Empty;
        public int? ExpiryMonths { get; set; }
        public int? ExpiryMonth { get; set; }
        public int? ExpiryDay { get; set; }
        public bool IsPayoutAllowed { get; set; }
        public decimal? PayoutMaximumDays { get; set; }
        public string ExcessBalanceAction { get; set; } = string.Empty;
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeaveCarryForwardPolicyDetailResponse : LeaveCarryForwardPolicyResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeaveCarryForwardPolicyOptionResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveEntitlementPolicyId { get; set; }
        public string LeaveEntitlementPolicyName { get; set; } = string.Empty;
        public string CarryForwardPolicyCode { get; set; } = string.Empty;
        public string CarryForwardPolicyName { get; set; } = string.Empty;
        public bool IsCarryForwardEnabled { get; set; }
        public bool IsDefault { get; set; }
    }

    public class LeaveCarryForwardPolicyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<LeaveCarryForwardPolicyOptionResponse> Items { get; set; } = new();
    }

    public class LeaveCarryForwardPolicyFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public LeaveCarryForwardPolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<LeaveCarryForwardPolicyCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<LeaveCarryForwardPolicySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class LeaveCarryForwardPolicyDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? LeaveEntitlementPolicyId { get; set; }
        public Guid? DestinationLeaveTypeId { get; set; }
        public bool? IsCarryForwardEnabled { get; set; }
        public bool? IsPayoutAllowed { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "carryForwardPolicyName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class LeaveCarryForwardPolicyCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class LeaveCarryForwardPolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateLeaveCarryForwardPolicyRequest
    {
        [Required]
        public Guid LeaveEntitlementPolicyId { get; set; }
        public Guid? DestinationLeaveTypeId { get; set; }
        [Required, MaxLength(150)]
        public string CarryForwardPolicyName { get; set; } = string.Empty;
        public bool IsCarryForwardEnabled { get; set; } = true;
        [Range(typeof(decimal), "0", "999999999")]
        public decimal? MinimumCarryForwardDays { get; set; }
        [Range(typeof(decimal), "0", "999999999")]
        public decimal? MaximumCarryForwardDays { get; set; }
        [Range(1, int.MaxValue)]
        public int? MaximumCarryForwardPeriods { get; set; } = 1;
        [Range(typeof(decimal), "0", "100")]
        public decimal CarryForwardPercentage { get; set; } = 100;
        [Required, MaxLength(50)]
        public string CarryForwardExecutionTiming { get; set; } = "PeriodClose";
        [Required, MaxLength(50)]
        public string RoundingMethod { get; set; } = "None";
        [Required, MaxLength(50)]
        public string ExpiryMethod { get; set; } = "MonthsAfterCarryForward";
        [Range(1, int.MaxValue)]
        public int? ExpiryMonths { get; set; }
        [Range(1, 12)]
        public int? ExpiryMonth { get; set; }
        [Range(1, 31)]
        public int? ExpiryDay { get; set; }
        public bool IsPayoutAllowed { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")]
        public decimal? PayoutMaximumDays { get; set; }
        [Required, MaxLength(50)]
        public string ExcessBalanceAction { get; set; } = "Forfeit";
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateLeaveCarryForwardPolicyRequest : CreateLeaveCarryForwardPolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateLeaveCarryForwardPolicyStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsCarryForwardEnabled { get; set; }
    }

    public class DeleteLeaveCarryForwardPolicyRequest
    {
        [MaxLength(500)]
        public string? DeleteReason { get; set; }
    }

    public class LeaveCarryForwardPolicyCreateResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveEntitlementPolicyId { get; set; }
        public string CarryForwardPolicyCode { get; set; } = string.Empty;
        public string CarryForwardPolicyName { get; set; } = string.Empty;
        public bool IsCarryForwardEnabled { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class LeaveCarryForwardPolicyUpdateResponse
    {
        public Guid Id { get; set; }
        public Guid LeaveEntitlementPolicyId { get; set; }
        public string CarryForwardPolicyCode { get; set; } = string.Empty;
        public string CarryForwardPolicyName { get; set; } = string.Empty;
        public bool IsCarryForwardEnabled { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class LeaveCarryForwardPolicyDeleteResponse
    {
        public Guid Id { get; set; }
        public string CarryForwardPolicyCode { get; set; } = string.Empty;
        public string CarryForwardPolicyName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}

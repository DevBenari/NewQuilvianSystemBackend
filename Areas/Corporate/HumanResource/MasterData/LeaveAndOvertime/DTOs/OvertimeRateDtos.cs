using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs
{
    public class OvertimeRateSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int WorkdayData { get; set; }
        public int RestDayData { get; set; }
        public int HolidayData { get; set; }
    }

    public class OvertimeRateResponse
    {
        public Guid Id { get; set; }
        public Guid OvertimePolicyId { get; set; }
        public string OvertimePolicyCode { get; set; } = string.Empty;
        public string OvertimePolicyName { get; set; } = string.Empty;
        public string OvertimeRateCode { get; set; } = string.Empty;
        public string OvertimeRateName { get; set; } = string.Empty;
        public string DayType { get; set; } = string.Empty;
        public string TimeBand { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public decimal RateMultiplier { get; set; }
        public decimal? FixedAmount { get; set; }
        public int StartMinute { get; set; }
        public int? EndMinute { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public int MinimumEligibleMinutes { get; set; }
        public int? MaximumEligibleMinutes { get; set; }
        public int Priority { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class OvertimeRateDetailResponse : OvertimeRateResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class OvertimeRateOptionResponse
    {
        public Guid Id { get; set; }
        public Guid OvertimePolicyId { get; set; }
        public string OvertimeRateCode { get; set; } = string.Empty;
        public string OvertimeRateName { get; set; } = string.Empty;
        public string DayType { get; set; } = string.Empty;
        public string TimeBand { get; set; } = string.Empty;
        public string CalculationMethod { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class OvertimeRateOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<OvertimeRateOptionResponse> Items { get; set; } = new();
    }

    public class OvertimeRateFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public OvertimeRateDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<OvertimeRateCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<OvertimeRateStringOptionResponse> DayTypeOptions { get; set; } = new();
        public List<OvertimeRateStringOptionResponse> TimeBandOptions { get; set; } = new();
        public List<OvertimeRateStringOptionResponse> CalculationMethodOptions { get; set; } = new();
        public List<OvertimeRateSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class OvertimeRateDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? OvertimePolicyId { get; set; }
        public string? DayType { get; set; }
        public string? TimeBand { get; set; }
        public string? CalculationMethod { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "priority";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class OvertimeRateCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OvertimeRateStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OvertimeRateSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateOvertimeRateRequest
    {
        [Required]
        public Guid OvertimePolicyId { get; set; }
        [Required, MaxLength(150)]
        public string OvertimeRateName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string DayType { get; set; } = "Workday";
        [Required, MaxLength(50)]
        public string TimeBand { get; set; } = "AllDay";
        [Required, MaxLength(50)]
        public string CalculationMethod { get; set; } = "Multiplier";
        [Range(typeof(decimal), "0", "999999999")]
        public decimal RateMultiplier { get; set; } = 1;
        [Range(typeof(decimal), "0", "999999999999999")]
        public decimal? FixedAmount { get; set; }
        [Range(0, int.MaxValue)]
        public int StartMinute { get; set; }
        [Range(1, int.MaxValue)]
        public int? EndMinute { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        [Range(0, int.MaxValue)]
        public int MinimumEligibleMinutes { get; set; }
        [Range(1, int.MaxValue)]
        public int? MaximumEligibleMinutes { get; set; }
        [Range(0, int.MaxValue)]
        public int Priority { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateOvertimeRateRequest : CreateOvertimeRateRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateOvertimeRateStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteOvertimeRateRequest
    {
        [MaxLength(500)]
        public string? DeleteReason { get; set; }
    }

    public class OvertimeRateCreateResponse
    {
        public Guid Id { get; set; }
        public Guid OvertimePolicyId { get; set; }
        public string OvertimeRateCode { get; set; } = string.Empty;
        public string OvertimeRateName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class OvertimeRateUpdateResponse
    {
        public Guid Id { get; set; }
        public Guid OvertimePolicyId { get; set; }
        public string OvertimeRateCode { get; set; } = string.Empty;
        public string OvertimeRateName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class OvertimeRateDeleteResponse
    {
        public Guid Id { get; set; }
        public string OvertimeRateCode { get; set; } = string.Empty;
        public string OvertimeRateName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}

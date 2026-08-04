using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.DTOs
{
    public class OvertimePolicySummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int DefaultData { get; set; }
        public int PreApprovalRequiredData { get; set; }
        public int AttendanceMatchRequiredData { get; set; }
    }

    public class OvertimePolicyResponse
    {
        public Guid Id { get; set; }
        public Guid? LegalEntityId { get; set; }
        public string? LegalEntityName { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public string OvertimePolicyCode { get; set; } = string.Empty;
        public string OvertimePolicyName { get; set; } = string.Empty;
        public bool RequirePreApproval { get; set; }
        public bool RequirePostVerification { get; set; }
        public bool RequireAttendanceMatch { get; set; }
        public int MinimumOvertimeMinutes { get; set; }
        public int? MaximumOvertimeMinutesPerDay { get; set; }
        public int? MaximumOvertimeMinutesPerWeek { get; set; }
        public int? MaximumOvertimeMinutesPerMonth { get; set; }
        public int OvertimeThresholdMinutes { get; set; }
        public int RoundingIntervalMinutes { get; set; }
        public string RoundingMethod { get; set; } = string.Empty;
        public bool DeductBreakMinutes { get; set; }
        public int BreakDeductionMinutes { get; set; }
        public bool AllowBeforeShift { get; set; }
        public bool AllowAfterShift { get; set; }
        public bool AllowRestDay { get; set; }
        public bool AllowHoliday { get; set; }
        public bool AllowDuringLeave { get; set; }
        public int AttendanceToleranceMinutes { get; set; }
        public string? ApprovalWorkflowCode { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int OvertimeRateCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class OvertimePolicyDetailResponse : OvertimePolicyResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class OvertimePolicyOptionResponse
    {
        public Guid Id { get; set; }
        public string OvertimePolicyCode { get; set; } = string.Empty;
        public string OvertimePolicyName { get; set; } = string.Empty;
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public bool IsDefault { get; set; }
    }

    public class OvertimePolicyOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<OvertimePolicyOptionResponse> Items { get; set; } = new();
    }

    public class OvertimePolicyFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public OvertimePolicyDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<OvertimePolicyStringOptionResponse> RoundingMethodOptions { get; set; } = new();
        public List<OvertimePolicySortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class OvertimePolicyDefaultFilterResponse
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public bool? RequirePreApproval { get; set; }
        public bool? RequireAttendanceMatch { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "overtimePolicyName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class OvertimePolicyStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OvertimePolicySortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateOvertimePolicyRequest
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        [Required, MaxLength(150)]
        public string OvertimePolicyName { get; set; } = string.Empty;
        public bool RequirePreApproval { get; set; } = true;
        public bool RequirePostVerification { get; set; } = true;
        public bool RequireAttendanceMatch { get; set; } = true;
        [Range(0, int.MaxValue)]
        public int MinimumOvertimeMinutes { get; set; } = 30;
        [Range(1, int.MaxValue)]
        public int? MaximumOvertimeMinutesPerDay { get; set; }
        [Range(1, int.MaxValue)]
        public int? MaximumOvertimeMinutesPerWeek { get; set; }
        [Range(1, int.MaxValue)]
        public int? MaximumOvertimeMinutesPerMonth { get; set; }
        [Range(0, int.MaxValue)]
        public int OvertimeThresholdMinutes { get; set; }
        [Range(1, int.MaxValue)]
        public int RoundingIntervalMinutes { get; set; } = 30;
        [Required, MaxLength(50)]
        public string RoundingMethod { get; set; } = "Down";
        public bool DeductBreakMinutes { get; set; }
        [Range(0, int.MaxValue)]
        public int BreakDeductionMinutes { get; set; }
        public bool AllowBeforeShift { get; set; }
        public bool AllowAfterShift { get; set; } = true;
        public bool AllowRestDay { get; set; } = true;
        public bool AllowHoliday { get; set; } = true;
        public bool AllowDuringLeave { get; set; }
        [Range(0, int.MaxValue)]
        public int AttendanceToleranceMinutes { get; set; } = 15;
        [MaxLength(100)]
        public string? ApprovalWorkflowCode { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UpdateOvertimePolicyRequest : CreateOvertimePolicyRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateOvertimePolicyStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }

    public class DeleteOvertimePolicyRequest
    {
        [MaxLength(500)]
        public string? DeleteReason { get; set; }
    }

    public class OvertimePolicyCreateResponse
    {
        public Guid Id { get; set; }
        public string OvertimePolicyCode { get; set; } = string.Empty;
        public string OvertimePolicyName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class OvertimePolicyUpdateResponse
    {
        public Guid Id { get; set; }
        public string OvertimePolicyCode { get; set; } = string.Empty;
        public string OvertimePolicyName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class OvertimePolicyDeleteResponse
    {
        public Guid Id { get; set; }
        public string OvertimePolicyCode { get; set; } = string.Empty;
        public string OvertimePolicyName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}

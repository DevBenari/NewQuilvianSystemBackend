using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs
{
    public class WorkforceRequirementSummaryResponse
    {
        public int TotalRequirement { get; set; }
        public int ActiveRequirement { get; set; }
        public int InactiveRequirement { get; set; }
        public int RequiredRequirement { get; set; }
        public int OptionalRequirement { get; set; }
        public int MultipleAllowedRequirement { get; set; }
        public int FileRequiredRequirement { get; set; }
        public int NumberRequiredRequirement { get; set; }
        public int IssueDateRequiredRequirement { get; set; }
        public int ExpiredDateRequiredRequirement { get; set; }
        public int VerificationRequiredRequirement { get; set; }
        public int ProfileRequiredRequirement { get; set; }
        public int EmployeeRequirement { get; set; }
        public int PermanentDoctorRequirement { get; set; }
        public int GuestDoctorRequirement { get; set; }
        public int ExternalUserRequirement { get; set; }
    }

    public class WorkforceRequirementResponse
    {
        public Guid Id { get; set; }

        public UserType UserType { get; set; }
        public string UserTypeName { get; set; } = string.Empty;

        public string RequirementCategory { get; set; } = string.Empty;
        public string RequirementCode { get; set; } = string.Empty;
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; }
        public bool IsMultipleAllowed { get; set; }
        public bool IsFileRequired { get; set; }
        public bool IsNumberRequired { get; set; }
        public bool IsIssueDateRequired { get; set; }
        public bool IsExpiredDateRequired { get; set; }
        public bool IsVerificationRequired { get; set; }
        public bool IsProfileRequired { get; set; }

        public string? TargetEntityName { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceRequirementDetailResponse : WorkforceRequirementResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class WorkforceRequirementOptionResponse
    {
        public Guid Id { get; set; }

        public UserType UserType { get; set; }
        public string UserTypeName { get; set; } = string.Empty;

        public string RequirementCategory { get; set; } = string.Empty;
        public string RequirementCode { get; set; } = string.Empty;
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; }
        public bool IsMultipleAllowed { get; set; }
        public bool IsFileRequired { get; set; }
        public bool IsNumberRequired { get; set; }
        public bool IsIssueDateRequired { get; set; }
        public bool IsExpiredDateRequired { get; set; }
        public bool IsVerificationRequired { get; set; }
        public bool IsProfileRequired { get; set; }

        public string? TargetEntityName { get; set; }
        public int SortOrder { get; set; }
    }

    public class WorkforceRequirementOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceRequirementOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceRequirementFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public WorkforceRequirementDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceRequirementCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceRequirementSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceRequirementUserTypeOptionResponse> UserTypes { get; set; } = new();
        public List<string> RequirementCategories { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class WorkforceRequirementDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }

        public UserType? UserType { get; set; }
        public string? RequirementCategory { get; set; }

        public bool? IsActive { get; set; }
        public string? Search { get; set; }

        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceRequirementCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceRequirementSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceRequirementUserTypeOptionResponse
    {
        public UserType Value { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWorkforceRequirementRequest
    {
        [Required]
        public UserType UserType { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequirementCategory { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string RequirementName { get; set; } = string.Empty;

        public bool IsRequired { get; set; } = true;
        public bool IsMultipleAllowed { get; set; } = false;
        public bool IsFileRequired { get; set; } = true;
        public bool IsNumberRequired { get; set; } = false;
        public bool IsIssueDateRequired { get; set; } = false;
        public bool IsExpiredDateRequired { get; set; } = false;
        public bool IsVerificationRequired { get; set; } = true;
        public bool IsProfileRequired { get; set; } = false;

        [MaxLength(100)]
        public string? TargetEntityName { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceRequirementRequest : CreateWorkforceRequirementRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceRequirementStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class DeleteWorkforceRequirementRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class WorkforceRequirementCreateResponse
    {
        public Guid Id { get; set; }
        public UserType UserType { get; set; }
        public string UserTypeName { get; set; } = string.Empty;
        public string RequirementCategory { get; set; } = string.Empty;
        public string RequirementCode { get; set; } = string.Empty;
        public string RequirementName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

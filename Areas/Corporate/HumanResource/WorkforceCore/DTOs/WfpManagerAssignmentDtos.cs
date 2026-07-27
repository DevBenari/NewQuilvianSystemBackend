using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpManagerAssignmentSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int PrimaryManagerData { get; set; }
        public int ApprovalEnabledData { get; set; }
    }

    public class WfpManagerAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid ManagerWorkforceProfileId { get; set; }
        public string ManagerProfileCode { get; set; } = string.Empty;
        public string ManagerDisplayName { get; set; } = string.Empty;
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? ManagerPositionId { get; set; }
        public string? ManagerPositionName { get; set; }
        public string ManagerType { get; set; } = "Direct";
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimaryManager { get; set; }
        public bool CanApproveRequests { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpManagerCandidateOptionResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public class WfpManagerAssignmentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpManagerAssignmentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpManagerAssignmentStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpManagerAssignmentStringOptionResponse> ManagerTypeOptions { get; set; } = new();
        public List<WfpManagerAssignmentStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpManagerAssignmentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? ManagerWorkforceProfileId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? ManagerType { get; set; }
        public bool? IsPrimaryManager { get; set; }
        public bool? CanApproveRequests { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "effectiveStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpManagerAssignmentStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpManagerAssignmentRequest
    {
        [Required]
        public Guid ManagerWorkforceProfileId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerPositionId { get; set; }

        [Required, MaxLength(50)]
        public string ManagerType { get; set; } = "Direct";

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimaryManager { get; set; } = true;
        public bool CanApproveRequests { get; set; } = true;
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateWfpManagerAssignmentRequest : CreateWfpManagerAssignmentRequest { }

    public class UpdateWfpManagerAssignmentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetWfpManagerAssignmentPrimaryRequest
    {
        public bool IsPrimaryManager { get; set; } = true;
    }
}

using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpOrganizationAssignmentSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int PrimaryData { get; set; }
        public int ManagerialData { get; set; }
    }

    public class WfpOrganizationAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public string AssignmentType { get; set; } = "Primary";
        public bool IsPrimary { get; set; }
        public bool IsManagerialAssignment { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? AssignmentNumber { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpOrganizationAssignmentOptionResponse
    {
        public Guid Id { get; set; }
        public string AssignmentType { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }

    public class WfpOrganizationAssignmentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpOrganizationAssignmentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpOrganizationAssignmentStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpOrganizationAssignmentStringOptionResponse> AssignmentTypeOptions { get; set; } = new();
        public List<WfpOrganizationAssignmentStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpOrganizationAssignmentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public string? AssignmentType { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsManagerialAssignment { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "effectiveStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpOrganizationAssignmentStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpOrganizationAssignmentRequest
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public Guid? CostCenterId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? EmployeeGradeId { get; set; }

        [Required, MaxLength(50)]
        public string AssignmentType { get; set; } = "Primary";

        public bool IsPrimary { get; set; }
        public bool IsManagerialAssignment { get; set; }

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(100)]
        public string? AssignmentNumber { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpOrganizationAssignmentRequest : CreateWfpOrganizationAssignmentRequest { }

    public class UpdateWfpOrganizationAssignmentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetWfpOrganizationAssignmentPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }
}

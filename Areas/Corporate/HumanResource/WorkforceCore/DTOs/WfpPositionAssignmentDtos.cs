using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpPositionAssignmentSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int PrimaryData { get; set; }
        public int ActingData { get; set; }
    }

    public class WfpPositionAssignmentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public Guid? JobFamilyId { get; set; }
        public Guid? JobLevelId { get; set; }
        public Guid? EmployeeGradeId { get; set; }
        public string AssignmentType { get; set; } = "Substantive";
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActing { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpPositionAssignmentOptionResponse
    {
        public Guid Id { get; set; }
        public Guid PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string AssignmentType { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsActing { get; set; }
        public bool IsActive { get; set; }
    }

    public class WfpPositionAssignmentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpPositionAssignmentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpPositionAssignmentStringOptionResponse> CustomPeriods { get; set; } = new();
        public List<WfpPositionAssignmentStringOptionResponse> AssignmentTypeOptions { get; set; } = new();
        public List<WfpPositionAssignmentStringOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpPositionAssignmentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public string? AssignmentType { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsActing { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "effectiveStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpPositionAssignmentStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpPositionAssignmentRequest
    {
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public Guid? JobFamilyId { get; set; }
        public Guid? JobLevelId { get; set; }
        public Guid? EmployeeGradeId { get; set; }

        [Required, MaxLength(50)]
        public string AssignmentType { get; set; } = "Substantive";

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActing { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateWfpPositionAssignmentRequest : CreateWfpPositionAssignmentRequest { }

    public class UpdateWfpPositionAssignmentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetWfpPositionAssignmentPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }
}

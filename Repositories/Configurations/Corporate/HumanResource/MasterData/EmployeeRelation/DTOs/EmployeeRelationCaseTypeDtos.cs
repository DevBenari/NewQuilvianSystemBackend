using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.DTOs
{
    public class EmployeeRelationCaseTypeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int UsedData { get; set; }
    }

    public class EmployeeRelationCaseTypeResponse
    {
        public Guid Id { get; set; }
        public string CaseTypeCode { get; set; } = string.Empty;
        public string CaseTypeName { get; set; } = string.Empty;
        public string CaseCategory { get; set; } = string.Empty;
        public bool RequiresInvestigation { get; set; }
        public bool RequiresHearing { get; set; }
        public bool DefaultConfidential { get; set; }
        public int? TargetResolutionDays { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class EmployeeRelationCaseTypeDetailResponse : EmployeeRelationCaseTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class EmployeeRelationCaseTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class EmployeeRelationCaseTypeFilterMetadataResponse
    {
        public EmployeeRelationCaseTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<EmployeeRelationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class EmployeeRelationCaseTypeDefaultFilterResponse
    {
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateEmployeeRelationCaseTypeRequest
    {
        [Required]
        [MaxLength(200)]
        public string CaseTypeName { get; set; } = string.Empty;
        public string CaseCategory { get; set; } = string.Empty;
        public bool RequiresInvestigation { get; set; }
        public bool RequiresHearing { get; set; }
        public bool DefaultConfidential { get; set; }
        public int? TargetResolutionDays { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateEmployeeRelationCaseTypeRequest : CreateEmployeeRelationCaseTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeRelationCaseTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

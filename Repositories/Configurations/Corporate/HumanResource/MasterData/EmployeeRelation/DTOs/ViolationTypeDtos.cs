using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.DTOs
{
    public class ViolationTypeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int UsedData { get; set; }
    }

    public class ViolationTypeResponse
    {
        public Guid Id { get; set; }
        public string ViolationTypeCode { get; set; } = string.Empty;
        public string ViolationTypeName { get; set; } = string.Empty;
        public string ViolationCategory { get; set; } = string.Empty;
        public string SeverityLevel { get; set; } = string.Empty;
        public bool RequiresInvestigation { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class ViolationTypeDetailResponse : ViolationTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class ViolationTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class ViolationTypeFilterMetadataResponse
    {
        public ViolationTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<EmployeeRelationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ViolationTypeDefaultFilterResponse
    {
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateViolationTypeRequest
    {
        [Required]
        [MaxLength(200)]
        public string ViolationTypeName { get; set; } = string.Empty;
        public string ViolationCategory { get; set; } = string.Empty;
        public string SeverityLevel { get; set; } = string.Empty;
        public bool RequiresInvestigation { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateViolationTypeRequest : CreateViolationTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateViolationTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.DTOs
{
    public class SanctionTypeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int UsedData { get; set; }
    }

    public class SanctionTypeResponse
    {
        public Guid Id { get; set; }
        public string SanctionTypeCode { get; set; } = string.Empty;
        public string SanctionTypeName { get; set; } = string.Empty;
        public string SanctionLevel { get; set; } = string.Empty;
        public int? DefaultDurationDays { get; set; }
        public bool IsFinalSanction { get; set; }
        public bool AllowsAppeal { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class SanctionTypeDetailResponse : SanctionTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class SanctionTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class SanctionTypeFilterMetadataResponse
    {
        public SanctionTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<EmployeeRelationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<EmployeeRelationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class SanctionTypeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "name";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateSanctionTypeRequest
    {
        [Required]
        [MaxLength(200)]
        public string SanctionTypeName { get; set; } = string.Empty;
        public string SanctionLevel { get; set; } = string.Empty;
        public int? DefaultDurationDays { get; set; }
        public bool IsFinalSanction { get; set; }
        public bool AllowsAppeal { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateSanctionTypeRequest : CreateSanctionTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSanctionTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

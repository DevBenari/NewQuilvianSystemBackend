using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.DTOs
{
    public class DisciplinaryActionTypeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int UsedData { get; set; }
    }

    public class DisciplinaryActionTypeResponse
    {
        public Guid Id { get; set; }
        public string ActionTypeCode { get; set; } = string.Empty;
        public string ActionTypeName { get; set; } = string.Empty;
        public string? DefaultActionLevel { get; set; }
        public int? DefaultEffectiveDays { get; set; }
        public bool RequiresApproval { get; set; }
        public bool AllowsAppeal { get; set; }
        public bool IsConfidential { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DisciplinaryActionTypeDetailResponse : DisciplinaryActionTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DisciplinaryActionTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class DisciplinaryActionTypeFilterMetadataResponse
    {
        public DisciplinaryActionTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<EmployeeRelationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<EmployeeRelationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class DisciplinaryActionTypeDefaultFilterResponse
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

    public class CreateDisciplinaryActionTypeRequest
    {
        [Required]
        [MaxLength(200)]
        public string ActionTypeName { get; set; } = string.Empty;
        public string? DefaultActionLevel { get; set; }
        public int? DefaultEffectiveDays { get; set; }
        public bool RequiresApproval { get; set; }
        public bool AllowsAppeal { get; set; }
        public bool IsConfidential { get; set; }
        [MaxLength(1000)]
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateDisciplinaryActionTypeRequest : CreateDisciplinaryActionTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDisciplinaryActionTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

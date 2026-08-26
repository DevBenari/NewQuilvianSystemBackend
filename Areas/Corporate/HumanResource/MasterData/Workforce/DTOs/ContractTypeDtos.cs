using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs
{
    public class ContractTypeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int RenewableData { get; set; }
        public int ProbationApplicableData { get; set; }
    }

    public class ContractTypeResponse
    {
        public Guid Id { get; set; }
        public string ContractTypeCode { get; set; } = string.Empty;
        public string ContractTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? DefaultDurationMonths { get; set; }
        public bool IsRenewable { get; set; }
        public bool RequiresEndDate { get; set; }
        public bool IsProbationApplicable { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class ContractTypeDetailResponse : ContractTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class ContractTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string ContractTypeCode { get; set; } = string.Empty;
        public string ContractTypeName { get; set; } = string.Empty;
        public int? DefaultDurationMonths { get; set; }
        public bool IsRenewable { get; set; }
    }

    public class ContractTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<ContractTypeOptionResponse> Items { get; set; } = new();
    }

    public class ContractTypeFilterMetadataResponse
    {
        public ContractTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<ContractTypeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<ContractTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class ContractTypeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsRenewable { get; set; }
        public bool? IsProbationApplicable { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "contractTypeName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class ContractTypeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class ContractTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateContractTypeRequest
    {
        [Required, MaxLength(150)]
        public string ContractTypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int? DefaultDurationMonths { get; set; }

        public bool IsRenewable { get; set; } = true;
        public bool RequiresEndDate { get; set; } = true;
        public bool IsProbationApplicable { get; set; }

        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }
    }

    public class UpdateContractTypeRequest : CreateContractTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateContractTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

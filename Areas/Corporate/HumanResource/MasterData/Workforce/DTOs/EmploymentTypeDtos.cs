using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs
{
    public class EmploymentTypeSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int PermanentData { get; set; }
        public int ContractBasedData { get; set; }
    }

    public class EmploymentTypeResponse
    {
        public Guid Id { get; set; }
        public string EmploymentTypeCode { get; set; } = string.Empty;
        public string EmploymentTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPermanent { get; set; }
        public bool IsContractBased { get; set; }
        public bool RequiresContractEndDate { get; set; }
        public bool IsPayrollEligible { get; set; }
        public bool IsBenefitEligible { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class EmploymentTypeDetailResponse : EmploymentTypeResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class EmploymentTypeOptionResponse
    {
        public Guid Id { get; set; }
        public string EmploymentTypeCode { get; set; } = string.Empty;
        public string EmploymentTypeName { get; set; } = string.Empty;
        public bool IsPermanent { get; set; }
        public bool IsContractBased { get; set; }
    }

    public class EmploymentTypeOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<EmploymentTypeOptionResponse> Items { get; set; } = new();
    }

    public class EmploymentTypeFilterMetadataResponse
    {
        public EmploymentTypeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<EmploymentTypeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<EmploymentTypeSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class EmploymentTypeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsPermanent { get; set; }
        public bool? IsContractBased { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "employmentTypeName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class EmploymentTypeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class EmploymentTypeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateEmploymentTypeRequest
    {
        [Required, MaxLength(150)]
        public string EmploymentTypeName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsPermanent { get; set; }
        public bool IsContractBased { get; set; }
        public bool RequiresContractEndDate { get; set; }
        public bool IsPayrollEligible { get; set; } = true;
        public bool IsBenefitEligible { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int? SortOrder { get; set; }
    }

    public class UpdateEmploymentTypeRequest : CreateEmploymentTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmploymentTypeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

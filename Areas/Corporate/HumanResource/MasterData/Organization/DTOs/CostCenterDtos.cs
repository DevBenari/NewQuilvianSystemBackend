using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs
{
    public class CostCenterSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int WithoutHospitalSiteData { get; set; }
    }

    public class CostCenterResponse
    {
        public Guid Id { get; set; }
        public Guid LegalEntityId { get; set; }
        public string LegalEntityCode { get; set; } = string.Empty;
        public string LegalEntityName { get; set; } = string.Empty;
        public Guid? HospitalSiteId { get; set; }
        public string? HospitalSiteCode { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string? OrganizationUnitCode { get; set; }
        public string? OrganizationUnitName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }
        public string CostCenterCode { get; set; } = string.Empty;
        public string CostCenterName { get; set; } = string.Empty;
        public string? AccountingCode { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
    }

    public class CostCenterDetailResponse : CostCenterResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class CostCenterOptionResponse
    {
        public Guid Id { get; set; }
        public Guid LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string CostCenterCode { get; set; } = string.Empty;
        public string CostCenterName { get; set; } = string.Empty;
        public string? AccountingCode { get; set; }
    }

    public class CostCenterOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<CostCenterOptionResponse> Items { get; set; } = new();
    }

    public class CostCenterFilterMetadataResponse
    {
        public CostCenterDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<CostCenterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<CostCenterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class CostCenterDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "costCenterName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CostCenterCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CostCenterSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateCostCenterRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required, MaxLength(200)]
        public string CostCenterName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? AccountingCode { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateCostCenterRequest : CreateCostCenterRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCostCenterStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

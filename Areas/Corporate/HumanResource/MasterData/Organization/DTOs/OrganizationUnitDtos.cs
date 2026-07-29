using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs
{
    public class OrganizationUnitSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int OperationalData { get; set; }
        public int RootUnitData { get; set; }
        public int ChildUnitData { get; set; }
    }

    public class OrganizationUnitResponse
    {
        public Guid Id { get; set; }
        public Guid LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? ParentOrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
        public bool IsOperationalUnit { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int ChildOrganizationUnitCount { get; set; }
        public int CostCenterCount { get; set; }
        public int WorkLocationCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
    }

    public class OrganizationUnitDetailResponse : OrganizationUnitResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class OrganizationUnitOptionResponse
    {
        public Guid Id { get; set; }
        public Guid LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? ParentOrganizationUnitId { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public int LevelNumber { get; set; }
    }

    public class OrganizationUnitOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<OrganizationUnitOptionResponse> Items { get; set; } = new();
    }

    public class OrganizationUnitFilterMetadataResponse
    {
        public OrganizationUnitDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<OrganizationUnitStringOptionResponse> UnitTypeOptions { get; set; } = new();
        public List<OrganizationUnitSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class OrganizationUnitDefaultFilterResponse
    {
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? ParentOrganizationUnitId { get; set; }
        public string? UnitType { get; set; }
        public bool? IsOperationalUnit { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "unitName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class OrganizationUnitStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class OrganizationUnitSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateOrganizationUnitRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        public Guid? HospitalSiteId { get; set; }
        public Guid? ParentOrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string UnitName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UnitType { get; set; } = "Unit";

        [Range(1, int.MaxValue)]
        public int LevelNumber { get; set; } = 1;

        public bool IsOperationalUnit { get; set; } = true;
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateOrganizationUnitRequest : CreateOrganizationUnitRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateOrganizationUnitStatusRequest
    {
        public bool IsActive { get; set; }
    }
}

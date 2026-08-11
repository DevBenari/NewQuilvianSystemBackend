using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs
{
    public class WorkLocationSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int PrimaryData { get; set; }
        public int RemoteData { get; set; }
        public int ClinicalAreaData { get; set; }
    }

    public class WorkLocationResponse
    {
        public Guid Id { get; set; }
        public Guid LegalEntityId { get; set; }
        public Guid HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty;
        public string? BuildingName { get; set; }
        public string? FloorName { get; set; }
        public string? RoomName { get; set; }
        public string? Address { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
    }

    public class WorkLocationDetailResponse : WorkLocationResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
    }

    public class WorkLocationOptionResponse
    {
        public Guid Id { get; set; }
        public Guid HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationName { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class WorkLocationOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkLocationOptionResponse> Items { get; set; } = new();
    }

    public class WorkLocationFilterMetadataResponse
    {
        public WorkLocationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkLocationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkLocationStringOptionResponse> LocationTypeOptions { get; set; } = new();
        public List<WorkLocationSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WorkLocationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? LocationType { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "locationName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkLocationCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkLocationStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkLocationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWorkLocationRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        public Guid HospitalSiteId { get; set; }

        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string LocationName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LocationType { get; set; } = "WorkArea";

        [MaxLength(150)]
        public string? BuildingName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        [MaxLength(100)]
        public string? RoomName { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public bool IsPrimary { get; set; }
    }

    public class UpdateWorkLocationRequest : CreateWorkLocationRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkLocationStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsPrimary { get; set; }
    }
}

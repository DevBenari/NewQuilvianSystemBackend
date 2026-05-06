using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs
{
    public class OrganizationSummaryResponse
    {
        public int TotalDepartment { get; set; }

        public int ActiveDepartment { get; set; }

        public int InactiveDepartment { get; set; }

        public int TotalPosition { get; set; }

        public int ActivePosition { get; set; }

        public int InactivePosition { get; set; }
    }

    public class OrganizationDepartmentResponse
    {
        public Guid Id { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public int PositionCount { get; set; }

        public int ActivePositionCount { get; set; }

        public List<OrganizationPositionCompactResponse> Positions { get; set; } = new();
    }

    public class OrganizationDepartmentOptionResponse
    {
        public Guid Id { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;
    }

    public class OrganizationPositionResponse
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class OrganizationPositionCompactResponse
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    public class OrganizationPositionOptionResponse
    {
        public Guid Id { get; set; }

        public Guid DepartmentId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;
    }

    public class OrganizationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan. Jika customPeriod kosong atau custom, frontend boleh mengirim startDate dan endDate.";

        public OrganizationDefaultFilterResponse DepartmentDefaultFilter { get; set; } = new();

        public OrganizationDefaultFilterResponse PositionDefaultFilter { get; set; } = new();

        public List<OrganizationCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<OrganizationSortOptionResponse> DepartmentSortOptions { get; set; } = new();

        public List<OrganizationSortOptionResponse> PositionSortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<OrganizationQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }

    public class OrganizationDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CustomPeriod { get; set; }

        public string? Search { get; set; }

        public bool? IsActive { get; set; }

        public Guid? DepartmentId { get; set; }

        public bool IncludePositions { get; set; } = false;

        public string SortBy { get; set; } = "createDateTime";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class OrganizationCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool UsesStartDate { get; set; }

        public bool UsesEndDate { get; set; }
    }

    public class OrganizationSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class OrganizationQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Required { get; set; } = "No";

        public string Description { get; set; } = string.Empty;

        public string? Example { get; set; }
    }

    public class CreateOrganizationDepartmentRequest
    {
        [Required]
        [MaxLength(150)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateOrganizationDepartmentRequest
    {
        [Required]
        [MaxLength(150)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool CascadeToPositions { get; set; } = true;
    }

    public class CreateOrganizationPositionRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string PositionName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateOrganizationPositionRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        [MaxLength(150)]
        public string PositionName { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateOrganizationStatusRequest
    {
        public bool IsActive { get; set; }

        public bool CascadeToPositions { get; set; } = true;
    }
}
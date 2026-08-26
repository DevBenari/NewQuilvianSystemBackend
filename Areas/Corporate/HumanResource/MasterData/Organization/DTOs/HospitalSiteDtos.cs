using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs
{
    public class HospitalSiteSummaryResponse
    {
        public int TotalData { get; set; }
        public int ActiveData { get; set; }
        public int InactiveData { get; set; }
        public int MainSiteData { get; set; }
        public int HospitalData { get; set; }
        public int ClinicData { get; set; }
    }

    public class HospitalSiteResponse
    {
        public Guid Id { get; set; }
        public Guid LegalEntityId { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public string SiteName { get; set; } = string.Empty;
        public string SiteType { get; set; } = string.Empty;
        public string? AccreditationNumber { get; set; }
        public string? TimeZoneId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? PostalCodeId { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsMainSite { get; set; }
        public bool IsActive { get; set; }
        public int OrganizationUnitCount { get; set; }
        public int CostCenterCount { get; set; }
        public int WorkLocationCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class HospitalSiteDetailResponse : HospitalSiteResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class HospitalSiteOptionResponse
    {
        public Guid Id { get; set; }
        public Guid LegalEntityId { get; set; }
        public string SiteCode { get; set; } = string.Empty;
        public string SiteName { get; set; } = string.Empty;
        public string SiteType { get; set; } = string.Empty;
        public bool IsMainSite { get; set; }
    }

    public class HospitalSiteOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<HospitalSiteOptionResponse> Items { get; set; } = new();
    }

    public class HospitalSiteFilterMetadataResponse
    {
        public HospitalSiteDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<HospitalSiteCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<HospitalSiteStringOptionResponse> SiteTypeOptions { get; set; } = new();
        public List<HospitalSiteSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class HospitalSiteDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? LegalEntityId { get; set; }
        public string? SiteType { get; set; }
        public bool? IsMainSite { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "siteName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class HospitalSiteCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class HospitalSiteStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class HospitalSiteSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateHospitalSiteRequest
    {
        [Required]
        public Guid LegalEntityId { get; set; }

        [Required]
        [MaxLength(200)]
        public string SiteName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SiteType { get; set; } = "Hospital";

        [MaxLength(100)]
        public string? AccreditationNumber { get; set; }

        [MaxLength(100)]
        public string? TimeZoneId { get; set; } = "Asia/Jakarta";

        [EmailAddress]
        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? PostalCodeId { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsMainSite { get; set; }
    }

    public class UpdateHospitalSiteRequest : CreateHospitalSiteRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateHospitalSiteStatusRequest
    {
        public bool IsActive { get; set; }
        public bool? IsMainSite { get; set; }
    }
}

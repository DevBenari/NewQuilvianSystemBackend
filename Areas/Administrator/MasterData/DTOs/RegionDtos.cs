using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class RegionSummaryResponse
    {
        public int TotalCountry { get; set; }
        public int ActiveCountry { get; set; }

        public int TotalProvince { get; set; }
        public int ActiveProvince { get; set; }

        public int TotalCity { get; set; }
        public int ActiveCity { get; set; }

        public int TotalDistrict { get; set; }
        public int ActiveDistrict { get; set; }

        public int TotalPostalCode { get; set; }
        public int ActivePostalCode { get; set; }
    }

    public class CountryResponse
    {
        public Guid Id { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string? PhoneCode { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class ProvinceResponse
    {
        public Guid Id { get; set; }
        public Guid CountryId { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string ProvinceCode { get; set; } = string.Empty;
        public string ProvinceName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class CityResponse
    {
        public Guid Id { get; set; }
        public Guid ProvinceId { get; set; }
        public string ProvinceCode { get; set; } = string.Empty;
        public string ProvinceName { get; set; } = string.Empty;
        public string CityCode { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string? CityType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DistrictResponse
    {
        public Guid Id { get; set; }
        public Guid CityId { get; set; }
        public string CityCode { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string DistrictCode { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PostalCodeResponse
    {
        public Guid Id { get; set; }
        public Guid DistrictId { get; set; }
        public string DistrictCode { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string? VillageName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class RegionOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    public class RegionFilterMetadataResponse
    {
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RegionSortOptionResponse> CountrySortOptions { get; set; } = new();
        public List<RegionSortOptionResponse> ProvinceSortOptions { get; set; } = new();
        public List<RegionSortOptionResponse> CitySortOptions { get; set; } = new();
        public List<RegionSortOptionResponse> DistrictSortOptions { get; set; } = new();
        public List<RegionSortOptionResponse> PostalCodeSortOptions { get; set; } = new();
    }

    public class RegionSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateCountryRequest
    {
        [Required]
        [MaxLength(20)]
        public string CountryCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CountryName { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? PhoneCode { get; set; }

        public bool IsDefault { get; set; } = false;
    }

    public class UpdateCountryRequest : CreateCountryRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class CreateProvinceRequest
    {
        [Required]
        public Guid CountryId { get; set; }

        [Required]
        [MaxLength(20)]
        public string ProvinceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ProvinceName { get; set; } = string.Empty;
    }

    public class UpdateProvinceRequest : CreateProvinceRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class CreateCityRequest
    {
        [Required]
        public Guid ProvinceId { get; set; }

        [Required]
        [MaxLength(20)]
        public string CityCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CityName { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? CityType { get; set; }
    }

    public class UpdateCityRequest : CreateCityRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class CreateDistrictRequest
    {
        [Required]
        public Guid CityId { get; set; }

        [Required]
        [MaxLength(20)]
        public string DistrictCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string DistrictName { get; set; } = string.Empty;
    }

    public class UpdateDistrictRequest : CreateDistrictRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class CreatePostalCodeRequest
    {
        [Required]
        public Guid DistrictId { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? VillageName { get; set; }
    }

    public class UpdatePostalCodeRequest : CreatePostalCodeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateRegionStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
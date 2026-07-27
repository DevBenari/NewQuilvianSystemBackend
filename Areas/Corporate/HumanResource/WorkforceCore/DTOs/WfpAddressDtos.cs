using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpAddressSummaryResponse
    {
        public int TotalAddress { get; set; }
        public int ActiveAddress { get; set; }
        public int InactiveAddress { get; set; }
        public int PrimaryAddress { get; set; }
        public int VerifiedAddress { get; set; }
        public int UnverifiedAddress { get; set; }
    }

    public class WfpAddressResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public string AddressType { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public Guid? CountryId { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; }
        public Guid? ProvinceId { get; set; }
        public string? ProvinceName { get; set; }
        public Guid? CityId { get; set; }
        public string? CityName { get; set; }
        public Guid? DistrictId { get; set; }
        public string? DistrictName { get; set; }
        public Guid? PostalCodeId { get; set; }
        public string? PostalCode { get; set; }
        public string? VillageName { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpAddressDetailResponse : WfpAddressResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpAddressFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpAddressDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpAddressStringOptionResponse> AddressTypeOptions { get; set; } = new();
        public List<WfpAddressSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpAddressDefaultFilterResponse
    {
        public string? AddressType { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "isPrimary";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpAddressStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpAddressSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpAddressRequest
    {
        [Required]
        [MaxLength(50)]
        public string AddressType { get; set; } = "Current";

        [Required]
        [MaxLength(500)]
        public string AddressLine { get; set; } = string.Empty;

        public Guid? CountryId { get; set; }
        public Guid? ProvinceId { get; set; }
        public Guid? CityId { get; set; }
        public Guid? DistrictId { get; set; }
        public Guid? PostalCodeId { get; set; }

        [MaxLength(150)]
        public string? VillageName { get; set; }

        [MaxLength(30)]
        public string? Latitude { get; set; }

        [MaxLength(30)]
        public string? Longitude { get; set; }

        public bool IsPrimary { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateWfpAddressRequest : CreateWfpAddressRequest
    {
    }

    public class UpdateWfpAddressStatusRequest
    {
        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class SetWfpAddressPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class VerifyWfpAddressRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}

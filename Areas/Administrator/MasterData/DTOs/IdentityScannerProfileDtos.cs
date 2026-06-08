using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class IdentityScannerProfileSummaryResponse
    {
        public int TotalProfile { get; set; }
        public int ActiveProfile { get; set; }
        public int InactiveProfile { get; set; }
        public int IdentityCardProfile { get; set; }
        public int PatientCardProfile { get; set; }
        public int MembershipCardProfile { get; set; }
        public int InsuranceCardProfile { get; set; }
        public int OcrEnabledProfile { get; set; }
        public int BarcodeEnabledProfile { get; set; }
        public int QrEnabledProfile { get; set; }
    }

    public class IdentityScannerProfileResponse
    {
        public Guid Id { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
        public IdentityScannerProfileType ProfileType { get; set; }

        public string? ScannerVendorName { get; set; }
        public string? ScannerModel { get; set; }
        public string? InputFormat { get; set; }
        public string? OutputFormat { get; set; }

        public bool IsForIdentityCard { get; set; }
        public bool IsForPatientCard { get; set; }
        public bool IsForMembershipCard { get; set; }
        public bool IsForInsuranceCard { get; set; }
        public bool IsOcrEnabled { get; set; }
        public bool IsBarcodeEnabled { get; set; }
        public bool IsQrEnabled { get; set; }
        public bool IsManualInputAllowed { get; set; }
        public bool IsAutoCreatePatientAllowed { get; set; }
        public bool IsVerificationRequired { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class IdentityScannerProfileDetailResponse : IdentityScannerProfileResponse
    {
        public string? IdentityNumberRegex { get; set; }
        public string? MemberNumberRegex { get; set; }
        public string? CardNumberRegex { get; set; }

        public string? IdentityNumberFieldName { get; set; }
        public string? FullNameFieldName { get; set; }
        public string? BirthDateFieldName { get; set; }
        public string? GenderFieldName { get; set; }
        public string? AddressFieldName { get; set; }

        public string? ConfigurationJson { get; set; }
        public string? Description { get; set; }
    }

    public class IdentityScannerProfileOptionResponse
    {
        public Guid Id { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
        public IdentityScannerProfileType ProfileType { get; set; }
        public bool IsForIdentityCard { get; set; }
        public bool IsForPatientCard { get; set; }
        public bool IsForMembershipCard { get; set; }
        public bool IsForInsuranceCard { get; set; }
        public bool IsOcrEnabled { get; set; }
        public bool IsBarcodeEnabled { get; set; }
        public bool IsQrEnabled { get; set; }
        public bool IsManualInputAllowed { get; set; }
    }

    public class IdentityScannerProfileOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<IdentityScannerProfileOptionResponse> Items { get; set; } = new();
    }

    public class IdentityScannerProfileEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class IdentityScannerProfileFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public IdentityScannerProfileDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<IdentityScannerProfileSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<IdentityScannerProfileEnumOptionResponse> ProfileTypeOptions { get; set; } = new();
    }

    public class IdentityScannerProfileDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public IdentityScannerProfileType? ProfileType { get; set; }
        public bool? IsForIdentityCard { get; set; }
        public bool? IsForPatientCard { get; set; }
        public bool? IsForMembershipCard { get; set; }
        public bool? IsForInsuranceCard { get; set; }
        public bool? IsOcrEnabled { get; set; }
        public bool? IsBarcodeEnabled { get; set; }
        public bool? IsQrEnabled { get; set; }
        public bool? IsManualInputAllowed { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class IdentityScannerProfileSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateIdentityScannerProfileRequest
    {
        [Required]
        [MaxLength(50)]
        public string ProfileCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string ProfileName { get; set; } = string.Empty;

        public IdentityScannerProfileType ProfileType { get; set; } = IdentityScannerProfileType.Unknown;

        [MaxLength(100)]
        public string? ScannerVendorName { get; set; }

        [MaxLength(100)]
        public string? ScannerModel { get; set; }

        [MaxLength(100)]
        public string? InputFormat { get; set; }

        [MaxLength(100)]
        public string? OutputFormat { get; set; }

        [MaxLength(250)]
        public string? IdentityNumberRegex { get; set; }

        [MaxLength(250)]
        public string? MemberNumberRegex { get; set; }

        [MaxLength(250)]
        public string? CardNumberRegex { get; set; }

        [MaxLength(100)]
        public string? IdentityNumberFieldName { get; set; }

        [MaxLength(100)]
        public string? FullNameFieldName { get; set; }

        [MaxLength(100)]
        public string? BirthDateFieldName { get; set; }

        [MaxLength(100)]
        public string? GenderFieldName { get; set; }

        [MaxLength(100)]
        public string? AddressFieldName { get; set; }

        public bool IsForIdentityCard { get; set; } = true;
        public bool IsForPatientCard { get; set; } = true;
        public bool IsForMembershipCard { get; set; } = true;
        public bool IsForInsuranceCard { get; set; } = false;
        public bool IsOcrEnabled { get; set; } = false;
        public bool IsBarcodeEnabled { get; set; } = false;
        public bool IsQrEnabled { get; set; } = false;
        public bool IsManualInputAllowed { get; set; } = true;
        public bool IsAutoCreatePatientAllowed { get; set; } = false;
        public bool IsVerificationRequired { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? ConfigurationJson { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateIdentityScannerProfileRequest : CreateIdentityScannerProfileRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class IdentityScannerProfileCreateResponse
    {
        public Guid Id { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
        public IdentityScannerProfileType ProfileType { get; set; }
        public bool IsForIdentityCard { get; set; }
        public bool IsForPatientCard { get; set; }
        public bool IsForMembershipCard { get; set; }
        public bool IsForInsuranceCard { get; set; }
        public bool IsActive { get; set; }
    }
}
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs
{
    public class ExternalUserSummaryResponse
    {
        public int TotalExternalUser { get; set; }

        public int ActiveExternalUser { get; set; }

        public int InactiveExternalUser { get; set; }

        public int PendingApprovalExternalUser { get; set; }

        public int SuspendedExternalUser { get; set; }

        public int AccessExpiredExternalUser { get; set; }

        public int ExternalUserWithLoginAccount { get; set; }
    }

    public class ExternalUserResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ExternalCode { get; set; } = string.Empty;

        public ExternalUserType ExternalUserType { get; set; }

        public ExternalUserStatus ExternalUserStatus { get; set; }

        public ExternalEngagementType EngagementType { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? ProfilePhotoPath { get; set; }

        public string? ProfilePhotoUrl { get; set; }

        public string? ExternalUserPhotoPath { get; set; }

        public string? ExternalUserPhotoUrl { get; set; }

        public string? CompanyName { get; set; }

        public string? CompanyCode { get; set; }

        public string? JobTitle { get; set; }

        public string? ContactPersonName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public Guid? CountryId { get; set; }

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

        public Guid? PrimaryDepartmentId { get; set; }

        public string PrimaryDepartmentCode { get; set; } = string.Empty;

        public string PrimaryDepartmentName { get; set; } = string.Empty;

        public Guid? PrimaryPositionId { get; set; }

        public string PrimaryPositionCode { get; set; } = string.Empty;

        public string PrimaryPositionName { get; set; } = string.Empty;

        public string? WorkLocation { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? AccessStartDate { get; set; }

        public DateTime? AccessEndDate { get; set; }

        public string? AccessPurpose { get; set; }

        public bool HasUserAccount { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid? CreateBy { get; set; }

        public string? CreateByName { get; set; }
    }

    public class ExternalUserDetailResponse : ExternalUserResponse
    {
        public string? Address { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? TaxNumber { get; set; }

        public string? BusinessLicenseNumber { get; set; }

        public string? Description { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }

        public string? UpdateByName { get; set; }

        public ExternalUserAccountCompactResponse? UserAccount { get; set; }

        public ExternalUserChildSummaryResponse ChildSummary { get; set; } = new();
    }

    public class ExternalUserAccountCompactResponse
    {
        public bool IsAvailable { get; set; }

        public Guid? UserId { get; set; }

        public string? UserCode { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? DisplayName { get; set; }

        public UserType? UserType { get; set; }

        public bool IsActive { get; set; }

        public bool MustChangePassword { get; set; }

        public string? ProfilePhotoPath { get; set; }

        public string? ProfilePhotoUrl { get; set; }

        public bool IsFingerprintRegistrationEnabled { get; set; }

        public string? FingerprintRegistrationReason { get; set; }

        public DateTime? FingerprintRegistrationEnabledAt { get; set; }

        public bool IsGeolocationBypassEnabled { get; set; }

        public string? GeolocationBypassReason { get; set; }

        public DateTime? GeolocationBypassUntil { get; set; }

        public bool IsGeolocationBypassActive { get; set; }
    }

    public class UpdateExternalUserAccountGeolocationBypassRequest
    {
        public bool IsGeolocationBypassEnabled { get; set; }

        public DateTime? GeolocationBypassUntil { get; set; }

        [MaxLength(250)]
        public string? GeolocationBypassReason { get; set; }
    }

    public class ExternalUserChildSummaryResponse
    {
        public int OrganizationCount { get; set; }

        public int ActiveOrganizationCount { get; set; }

        public int PrimaryOrganizationCount { get; set; }

        public int BankAccountCount { get; set; }

        public int DocumentCount { get; set; }

        public int CertificationCount { get; set; }

        public int CredentialLicenseCount { get; set; }

        public int ScheduleAssignmentCount { get; set; }

        public int AttendanceCount { get; set; }
    }

    public class ExternalUserOptionResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ExternalCode { get; set; } = string.Empty;

        public ExternalUserType ExternalUserType { get; set; }

        public ExternalUserStatus ExternalUserStatus { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? ProfilePhotoPath { get; set; }

        public string? ProfilePhotoUrl { get; set; }

        public string? ExternalUserPhotoPath { get; set; }

        public string? ExternalUserPhotoUrl { get; set; }

        public string? CompanyName { get; set; }

        public string? JobTitle { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public string PrimaryDepartmentName { get; set; } = string.Empty;

        public Guid? PrimaryPositionId { get; set; }

        public string PrimaryPositionName { get; set; } = string.Empty;
    }

    public class ExternalUserEnumOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class ExternalUserFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public string ExternalCodeInfo { get; set; } =
            "ExternalCode dibuat otomatis oleh backend dengan format EXT-RSMMC-00001.";

        public string InitialPasswordFormatInfo { get; set; } =
            "Jika akun user dibuat dari external user, password awal digenerate backend dan wajib diganti saat login pertama.";

        public string PeriodPriorityInfo { get; set; } =
            "Jika period diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public bool ShowResetButton { get; set; } = true;

        public string ResetButtonLabel { get; set; } = "Reset";

        public ExternalUserDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<ExternalUserCustomPeriodOptionResponse> Periods { get; set; } = new();

        public List<ExternalUserSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<ExternalUserEnumOptionResponse> ExternalUserTypeOptions { get; set; } = new();

        public List<ExternalUserEnumOptionResponse> ExternalUserStatusOptions { get; set; } = new();

        public List<ExternalUserEnumOptionResponse> EngagementTypeOptions { get; set; } = new();

        public List<ExternalUserQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public List<ExternalUserFormFieldMetadataResponse> CreateFields { get; set; } = new();

        public List<ExternalUserFormFieldMetadataResponse> UpdateFields { get; set; } = new();

        public List<ExternalUserDetailTabMetadataResponse> DetailTabs { get; set; } = new();
    }

    public class ExternalUserDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Period { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public bool? IsActive { get; set; }

        public string? Search { get; set; }

        public string SortBy { get; set; } = "createDateTime";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class ExternalUserCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool UsesStartDate { get; set; }

        public bool UsesEndDate { get; set; }
    }

    public class ExternalUserSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class ExternalUserQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Required { get; set; } = "No";

        public string Description { get; set; } = string.Empty;

        public string? Example { get; set; }
    }

    public class ExternalUserFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Section { get; set; } = string.Empty;

        public string InputType { get; set; } = string.Empty;

        public bool IsRequiredOnCreate { get; set; }

        public bool IsRequiredOnUpdate { get; set; }

        public string RequiredType { get; set; } = "Optional";

        public int? MaxLength { get; set; }

        public string? DependsOn { get; set; }

        public string? OptionsSource { get; set; }

        public string? ValidationRule { get; set; }

        public string? Description { get; set; }

        public string? Example { get; set; }

        public int SortOrder { get; set; }
    }

    public class ExternalUserDetailTabMetadataResponse
    {
        public string Key { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public string Endpoint { get; set; } = string.Empty;

        public bool IsVisibleInDetail { get; set; } = true;

        public bool IsVisibleInUpdate { get; set; } = true;

        public bool CanCreate { get; set; }

        public bool CanUpdate { get; set; }

        public bool CanDelete { get; set; }

        public int SortOrder { get; set; }
    }

    public class CreateExternalUserRequest
    {
        public bool CreateLoginAccount { get; set; } = false;

        public bool IsFingerprintRegistrationEnabled { get; set; } = false;

        [MaxLength(250)]
        public string? FingerprintRegistrationReason { get; set; }

        [Required]
        public ExternalUserType ExternalUserType { get; set; } = ExternalUserType.Unknown;

        public ExternalUserStatus ExternalUserStatus { get; set; } = ExternalUserStatus.Active;

        public ExternalEngagementType EngagementType { get; set; } = ExternalEngagementType.ContractBased;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        public string? CompanyCode { get; set; }

        [MaxLength(100)]
        public string? JobTitle { get; set; }

        [MaxLength(200)]
        public string? ContactPersonName { get; set; }

        [MaxLength(30)]
        [RegularExpression(@"^\d{1,30}$", ErrorMessage = "Nomor telepon maksimal 30 digit dan hanya boleh angka.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(30)]
        [RegularExpression(@"^\d{1,30}$", ErrorMessage = "Nomor WhatsApp maksimal 30 digit dan hanya boleh angka.")]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public Guid? CountryId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? PostalCodeId { get; set; }

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(100)]
        public string? TaxNumber { get; set; }

        [MaxLength(100)]
        public string? BusinessLicenseNumber { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? AccessStartDate { get; set; }

        public DateTime? AccessEndDate { get; set; }

        [MaxLength(250)]
        public string? AccessPurpose { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateExternalUserRequest
    {
        [Required]
        public ExternalUserType ExternalUserType { get; set; } = ExternalUserType.Unknown;

        public ExternalUserStatus ExternalUserStatus { get; set; } = ExternalUserStatus.Active;

        public ExternalEngagementType EngagementType { get; set; } = ExternalEngagementType.ContractBased;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CompanyName { get; set; }

        [MaxLength(100)]
        public string? CompanyCode { get; set; }

        [MaxLength(100)]
        public string? JobTitle { get; set; }

        [MaxLength(200)]
        public string? ContactPersonName { get; set; }

        [MaxLength(30)]
        [RegularExpression(@"^\d{1,30}$", ErrorMessage = "Nomor telepon maksimal 30 digit dan hanya boleh angka.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(30)]
        [RegularExpression(@"^\d{1,30}$", ErrorMessage = "Nomor WhatsApp maksimal 30 digit dan hanya boleh angka.")]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public Guid? CountryId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? PostalCodeId { get; set; }

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(100)]
        public string? IdentityNumber { get; set; }

        [MaxLength(100)]
        public string? TaxNumber { get; set; }

        [MaxLength(100)]
        public string? BusinessLicenseNumber { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? AccessStartDate { get; set; }

        public DateTime? AccessEndDate { get; set; }

        [MaxLength(250)]
        public string? AccessPurpose { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateExternalUserStatusRequest
    {
        public bool IsActive { get; set; }

        public ExternalUserStatus ExternalUserStatus { get; set; } = ExternalUserStatus.Active;

        public DateTime? AccessEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class ExternalUserCreateResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ExternalCode { get; set; } = string.Empty;

        public ExternalUserType ExternalUserType { get; set; }

        public ExternalUserStatus ExternalUserStatus { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? CompanyName { get; set; }

        public bool IsActive { get; set; }

        public ExternalUserLoginAccountResponse? LoginAccount { get; set; }
    }

    public class ExternalUserLoginAccountResponse
    {
        public bool IsCreated { get; set; }

        public Guid? UserId { get; set; }

        public string? UserCode { get; set; }

        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? DisplayName { get; set; }

        public UserType? UserType { get; set; }

        public string? InitialPassword { get; set; }

        public bool MustChangePassword { get; set; }

        public string? ProfilePhotoPath { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsFingerprintRegistrationEnabled { get; set; }

        public string? FingerprintRegistrationReason { get; set; }

        public DateTime? FingerprintRegistrationEnabledAt { get; set; }
    }
}
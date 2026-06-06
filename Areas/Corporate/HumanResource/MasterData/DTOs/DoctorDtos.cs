using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs
{
    public class DoctorSummaryResponse
    {
        public int TotalDoctor { get; set; }

        public int ActiveDoctor { get; set; }

        public int InactiveDoctor { get; set; }

        public int AvailableForAppointmentDoctor { get; set; }

        public int PermanentDoctor { get; set; }

        public int GuestDoctor { get; set; }

        public int DoctorWithUserAccount { get; set; }

        public int CredentialingPendingDoctor { get; set; }

        public int PrivilegeSuspendedDoctor { get; set; }
    }

    public class DoctorResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public UserType WorkforceUserType { get; set; }

        public string DoctorCode { get; set; } = string.Empty;

        public string DoctorNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? NickName { get; set; }

        public Gender? Gender { get; set; }

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

        public DoctorStatus DoctorStatus { get; set; }

        public DoctorType DoctorType { get; set; }

        public DoctorPracticeType PracticeType { get; set; }

        public EmploymentType EmploymentType { get; set; }

        public string? SpecialistName { get; set; }

        public string? SubSpecialistName { get; set; }

        public string? MedicalStaffGroup { get; set; }

        public string? GradeLevel { get; set; }

        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ResignDate { get; set; }

        public DateTime? CredentialingDate { get; set; }

        public bool IsAvailableForAppointment { get; set; }

        public bool HasUserAccount { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class DoctorDetailResponse : DoctorResponse
    {
        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public Religion Religion { get; set; }

        public MaritalStatus MaritalStatus { get; set; }

        public BloodType BloodType { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public string? ResignReason { get; set; }

        public DoctorUserAccountCompactResponse? UserAccount { get; set; }

        public DoctorChildSummaryResponse ChildSummary { get; set; } = new();
    }

    public class DoctorUserAccountCompactResponse
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

        public bool IsFingerprintRegistrationEnabled { get; set; }

        public string? FingerprintRegistrationReason { get; set; }

        public DateTime? FingerprintRegistrationEnabledAt { get; set; }

        public bool IsGeolocationBypassEnabled { get; set; }

        public string? GeolocationBypassReason { get; set; }

        public DateTime? GeolocationBypassUntil { get; set; }

        public bool IsGeolocationBypassActive { get; set; }
    }

    public class UpdateDoctorUserGeolocationBypassRequest
    {
        public bool IsGeolocationBypassEnabled { get; set; }

        public DateTime? GeolocationBypassUntil { get; set; }

        [MaxLength(250)]
        public string? GeolocationBypassReason { get; set; }
    }

    public class DoctorChildSummaryResponse
    {
        public int OrganizationCount { get; set; }

        public int ActiveOrganizationCount { get; set; }

        public int PrimaryOrganizationCount { get; set; }

        public int BankAccountCount { get; set; }

        public int DocumentCount { get; set; }

        public int EducationCount { get; set; }

        public int TrainingCount { get; set; }

        public int CertificationCount { get; set; }

        public int CredentialLicenseCount { get; set; }

        public int ClinicalPrivilegeCount { get; set; }

        public int HealthRecordCount { get; set; }

        public int ScheduleAssignmentCount { get; set; }

        public int AttendanceCount { get; set; }
    }

    public class DoctorOptionResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public UserType WorkforceUserType { get; set; }

        public string DoctorCode { get; set; } = string.Empty;

        public string DoctorNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? SpecialistName { get; set; }

        public string? SubSpecialistName { get; set; }

        public Guid? PrimaryDepartmentId { get; set; }

        public string PrimaryDepartmentName { get; set; } = string.Empty;

        public Guid? PrimaryPositionId { get; set; }

        public string PrimaryPositionName { get; set; } = string.Empty;

        public bool IsAvailableForAppointment { get; set; }
    }

    public class DoctorEnumOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class DoctorFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public string DoctorCodeInfo { get; set; } =
            "DoctorCode dibuat otomatis oleh backend dengan format DOC-RSMMC-00001.";

        public string DoctorNumberInfo { get; set; } =
            "DoctorNumber dibuat otomatis oleh backend saat create doctor.";

        public string InitialPasswordFormatInfo { get; set; } =
            "Jika akun user dibuat dari doctor, password awal dapat digenerate dari BirthDate dengan format ddMMMyyyy.";

        public string DoctorUserTypeInfo { get; set; } =
            "Doctor hanya boleh menggunakan UserType PermanentDoctor atau GuestDoctor.";

        public string PeriodPriorityInfo { get; set; } =
            "Jika period diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public bool ShowResetButton { get; set; } = true;

        public string ResetButtonLabel { get; set; } = "Reset";

        public DoctorDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<DoctorCustomPeriodOptionResponse> Periods { get; set; } = new();

        public List<DoctorSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<DoctorEnumOptionResponse> WorkforceUserTypeOptions { get; set; } = new();

        public List<DoctorEnumOptionResponse> GenderOptions { get; set; } = new();

        public List<DoctorEnumOptionResponse> ReligionOptions { get; set; } = new();

        public List<DoctorEnumOptionResponse> MaritalStatusOptions { get; set; } = new();

        public List<DoctorEnumOptionResponse> BloodTypeOptions { get; set; } = new();

        public List<DoctorEnumOptionResponse> DoctorStatusOptions { get; set; } = new();

        public List<DoctorEnumOptionResponse> DoctorTypeOptions { get; set; } = new();

        public List<DoctorEnumOptionResponse> PracticeTypeOptions { get; set; } = new();

        public List<DoctorEnumOptionResponse> EmploymentTypeOptions { get; set; } = new();

        public List<DoctorQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public List<DoctorFormFieldMetadataResponse> CreateFields { get; set; } = new();

        public List<DoctorFormFieldMetadataResponse> UpdateFields { get; set; } = new();

        public List<DoctorDetailTabMetadataResponse> DetailTabs { get; set; } = new();
    }

    public class DoctorDefaultFilterResponse
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

    public class DoctorCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool UsesStartDate { get; set; }

        public bool UsesEndDate { get; set; }
    }

    public class DoctorSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class DoctorQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Required { get; set; } = "No";

        public string Description { get; set; } = string.Empty;

        public string? Example { get; set; }
    }

    public class DoctorFormFieldMetadataResponse
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

    public class DoctorDetailTabMetadataResponse
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

    public class CreateDoctorRequest
    {
        public bool CreateLoginAccount { get; set; } = true;

        public bool IsFingerprintRegistrationEnabled { get; set; } = false;

        [MaxLength(250)]
        public string? FingerprintRegistrationReason { get; set; }

        public UserType DoctorUserType { get; set; } = UserType.PermanentDoctor;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NickName { get; set; }

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public Gender? Gender { get; set; }

        public Religion Religion { get; set; } = Religion.Unknown;

        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Unknown;

        public BloodType BloodType { get; set; } = BloodType.Unknown;

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(50)]
        public string? IdentityNumber { get; set; }

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

        public Guid? PrimaryDepartmentId { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public DoctorStatus DoctorStatus { get; set; } = DoctorStatus.Active;

        public DoctorType DoctorType { get; set; } = DoctorType.GeneralPractitioner;

        public DoctorPracticeType PracticeType { get; set; } = DoctorPracticeType.FullTime;

        public EmploymentType EmploymentType { get; set; } = EmploymentType.Contract;

        [MaxLength(100)]
        public string? SpecialistName { get; set; }

        [MaxLength(100)]
        public string? SubSpecialistName { get; set; }

        [MaxLength(100)]
        public string? MedicalStaffGroup { get; set; }

        [MaxLength(50)]
        public string? GradeLevel { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? CredentialingDate { get; set; }

        public bool IsAvailableForAppointment { get; set; } = true;        
    }

    public class UpdateDoctorUserFingerprintRegistrationRequest
    {
        public bool IsFingerprintRegistrationEnabled { get; set; }
        public string? FingerprintRegistrationReason { get; set; }
    }

    public class UpdateDoctorRequest
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NickName { get; set; }

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public Gender? Gender { get; set; }

        public Religion Religion { get; set; } = Religion.Unknown;

        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Unknown;

        public BloodType BloodType { get; set; } = BloodType.Unknown;

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(50)]
        public string? IdentityNumber { get; set; }

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

        public Guid? PrimaryDepartmentId { get; set; }

        public Guid? PrimaryPositionId { get; set; }

        public DoctorStatus DoctorStatus { get; set; } = DoctorStatus.Active;

        public DoctorType DoctorType { get; set; } = DoctorType.GeneralPractitioner;

        public DoctorPracticeType PracticeType { get; set; } = DoctorPracticeType.FullTime;

        public EmploymentType EmploymentType { get; set; } = EmploymentType.Contract;

        [MaxLength(100)]
        public string? SpecialistName { get; set; }

        [MaxLength(100)]
        public string? SubSpecialistName { get; set; }

        [MaxLength(100)]
        public string? MedicalStaffGroup { get; set; }

        [MaxLength(50)]
        public string? GradeLevel { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ResignDate { get; set; }

        [MaxLength(250)]
        public string? ResignReason { get; set; }

        public DateTime? CredentialingDate { get; set; }

        public bool IsAvailableForAppointment { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateDoctorStatusRequest
    {
        public bool IsActive { get; set; }

        public DoctorStatus DoctorStatus { get; set; } = DoctorStatus.Active;

        public bool IsAvailableForAppointment { get; set; } = true;

        public DateTime? ResignDate { get; set; }

        [MaxLength(250)]
        public string? ResignReason { get; set; }
    }

    public class DoctorCreateResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public UserType WorkforceUserType { get; set; }

        public string DoctorCode { get; set; } = string.Empty;

        public string DoctorNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public bool IsAvailableForAppointment { get; set; }

        public bool IsActive { get; set; }

        public DoctorLoginAccountResponse? LoginAccount { get; set; }
    }

    public class DoctorLoginAccountResponse
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
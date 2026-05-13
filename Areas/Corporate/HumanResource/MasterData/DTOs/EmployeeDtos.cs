using QuilvianSystemBackend.Enum;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs
{
    public class EmployeeSummaryResponse
    {
        public int TotalEmployee { get; set; }

        public int ActiveEmployee { get; set; }

        public int InactiveEmployee { get; set; }

        public int EmployeeWithTransportAllowanceProfile { get; set; }

        public int TransportEligibleEmployee { get; set; }

        public int NightTransportEligibleEmployee { get; set; }
    }

    public class EmployeeResponse
    {
        public Guid Id { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? NickName { get; set; }

        public Gender? Gender { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string Email { get; set; } = string.Empty;

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

        public Guid PrimaryDepartmentId { get; set; }

        public string PrimaryDepartmentCode { get; set; } = string.Empty;

        public string PrimaryDepartmentName { get; set; } = string.Empty;

        public Guid PrimaryPositionId { get; set; }

        public string PrimaryPositionCode { get; set; } = string.Empty;

        public string PrimaryPositionName { get; set; } = string.Empty;

        public EmployeeStatus EmployeeStatus { get; set; }

        public EmployeeProfessionType ProfessionType { get; set; }

        public string EmploymentType { get; set; } = string.Empty;

        public string? GradeLevel { get; set; }

        public string? WorkLocation { get; set; }

        public DateTime JoinDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ResignDate { get; set; }

        public bool HasUserAccount { get; set; }

        public bool HasTransportAllowanceProfile { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class EmployeeDetailResponse : EmployeeResponse
    {
        public string? BirthPlace { get; set; }

        public DateTime BirthDate { get; set; }

        public Religion Religion { get; set; }

        public MaritalStatus MaritalStatus { get; set; }

        public BloodType BloodType { get; set; }

        public string IdentityType { get; set; } = string.Empty;

        public string IdentityNumber { get; set; } = string.Empty;

        public string? Address { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public string? ResignReason { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactRelation { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? EmergencyContactAddress { get; set; }

        public EmployeeUserAccountCompactResponse? UserAccount { get; set; }

        public EmployeeTransportAllowanceProfileResponse? TransportAllowanceProfile { get; set; }

        public EmployeeChildSummaryResponse ChildSummary { get; set; } = new();
    }

    public class EmployeeUserAccountCompactResponse
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
    }

    public class EmployeeChildSummaryResponse
    {
        public int OrganizationCount { get; set; }

        public int ActiveOrganizationCount { get; set; }

        public int PrimaryOrganizationCount { get; set; }

        public int BankAccountCount { get; set; }

        public int DocumentCount { get; set; }

        public int PayrollProfileCount { get; set; }

        public int TaxProfileCount { get; set; }

        public int InsuranceProfileCount { get; set; }

        public int TransportAllowanceProfileCount { get; set; }

        public int TransportAllowanceTransactionCount { get; set; }

        public int LeaveCount { get; set; }

        public int ShiftAssignmentCount { get; set; }

        public int AttendanceCount { get; set; }

        public int SalaryCount { get; set; }

        public int TrainingCount { get; set; }

        public int WarningLetterCount { get; set; }

        public int PerformanceReviewCount { get; set; }
    }

    public class EmployeeOptionResponse
    {
        public Guid Id { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public Guid PrimaryDepartmentId { get; set; }

        public string PrimaryDepartmentName { get; set; } = string.Empty;

        public Guid PrimaryPositionId { get; set; }

        public string PrimaryPositionName { get; set; } = string.Empty;
    }

    public class EmployeeEnumOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class EmployeeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public string EmployeeNumberInfo { get; set; } =
            "EmployeeNumber dibuat otomatis oleh backend saat create employee.";

        public string InitialPasswordFormatInfo { get; set; } =
            "Jika akun user dibuat dari employee, password awal digenerate dari BirthDate dengan format ddMMMyyyy, contoh 01Jan2025.";

        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan. Jika customPeriod kosong atau custom, frontend boleh mengirim startDate dan endDate.";

        public EmployeeDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<EmployeeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<EmployeeSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<EmployeeEnumOptionResponse> GenderOptions { get; set; } = new();

        public List<EmployeeEnumOptionResponse> ReligionOptions { get; set; } = new();

        public List<EmployeeEnumOptionResponse> MaritalStatusOptions { get; set; } = new();

        public List<EmployeeEnumOptionResponse> BloodTypeOptions { get; set; } = new();

        public List<EmployeeEnumOptionResponse> EmployeeStatusOptions { get; set; } = new();

        public List<EmployeeEnumOptionResponse> ProfessionTypeOptions { get; set; } = new();

        public List<string> TransportAllowanceModes { get; set; } = new();

        public List<string> TransportTransactionStatuses { get; set; } = new();

        public List<string> TransportAllowanceTypes { get; set; } = new();

        public List<EmployeeQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public List<EmployeeFormFieldMetadataResponse> CreateFields { get; set; } = new();

        public List<EmployeeFormFieldMetadataResponse> UpdateFields { get; set; } = new();

        public List<EmployeeDetailTabMetadataResponse> DetailTabs { get; set; } = new();
    }

    public class EmployeeDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CustomPeriod { get; set; }

        public string? Search { get; set; }

        public bool? IsActive { get; set; }

        public Guid? DepartmentId { get; set; }

        public Guid? PositionId { get; set; }

        public Guid? CountryId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? PostalCodeId { get; set; }

        public EmployeeStatus? EmployeeStatus { get; set; }

        public EmployeeProfessionType? ProfessionType { get; set; }

        public Religion? Religion { get; set; }

        public MaritalStatus? MaritalStatus { get; set; }

        public BloodType? BloodType { get; set; }

        public bool? HasUserAccount { get; set; }

        public bool? HasTransportAllowanceProfile { get; set; }

        public string SortBy { get; set; } = "createDateTime";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class EmployeeCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool UsesStartDate { get; set; }

        public bool UsesEndDate { get; set; }
    }

    public class EmployeeSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class EmployeeQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Required { get; set; } = "No";

        public string Description { get; set; } = string.Empty;

        public string? Example { get; set; }
    }

    public class EmployeeFormFieldMetadataResponse
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

    public class EmployeeDetailTabMetadataResponse
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

    public class CreateEmployeeRequest
    {
        public bool CreateLoginAccount { get; set; } = true;

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NickName { get; set; }

        public Gender? Gender { get; set; }

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        public Religion Religion { get; set; } = Religion.Unknown;

        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Unknown;

        public BloodType BloodType { get; set; } = BloodType.Unknown;

        [Required]
        [MaxLength(50)]
        public string IdentityType { get; set; } = string.Empty;

        [Required]
        [MaxLength(16)]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Nomor identitas harus 16 digit.")]
        public string IdentityNumber { get; set; } = string.Empty;

        [MaxLength(13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "Nomor telepon harus 13 digit.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "Nomor WhatsApp harus 13 digit.")]
        public string? WhatsAppNumber { get; set; }

        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        public Guid? CountryId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? PostalCodeId { get; set; }

        [Required]
        public Guid PrimaryDepartmentId { get; set; }

        [Required]
        public Guid PrimaryPositionId { get; set; }

        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Contract;

        public EmployeeProfessionType ProfessionType { get; set; } = EmployeeProfessionType.GeneralStaff;

        [Required]
        [MaxLength(50)]
        public string EmploymentType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? GradeLevel { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        [Required]
        public DateTime JoinDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        [MaxLength(200)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(50)]
        public string? EmergencyContactRelation { get; set; }

        [MaxLength(13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "Nomor telepon kontak darurat harus 13 digit.")]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(500)]
        public string? EmergencyContactAddress { get; set; }
    }

    public class UpdateEmployeeRequest
    {
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NickName { get; set; }

        public Gender? Gender { get; set; }

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        public Religion Religion { get; set; } = Religion.Unknown;

        public MaritalStatus MaritalStatus { get; set; } = MaritalStatus.Unknown;

        public BloodType BloodType { get; set; } = BloodType.Unknown;

        [Required]
        [MaxLength(50)]
        public string IdentityType { get; set; } = string.Empty;

        [Required]
        [MaxLength(16)]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Nomor identitas harus 16 digit.")]
        public string IdentityNumber { get; set; } = string.Empty;

        [MaxLength(13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "Nomor telepon harus 13 digit.")]
        public string? PhoneNumber { get; set; }

        [MaxLength(13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "Nomor WhatsApp harus 13 digit.")]
        public string? WhatsAppNumber { get; set; }

        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        public Guid? CountryId { get; set; }

        public Guid? ProvinceId { get; set; }

        public Guid? CityId { get; set; }

        public Guid? DistrictId { get; set; }

        public Guid? PostalCodeId { get; set; }

        [Required]
        public Guid PrimaryDepartmentId { get; set; }

        [Required]
        public Guid PrimaryPositionId { get; set; }

        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Contract;

        public EmployeeProfessionType ProfessionType { get; set; } = EmployeeProfessionType.GeneralStaff;

        [Required]
        [MaxLength(50)]
        public string EmploymentType { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? GradeLevel { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        [Required]
        public DateTime JoinDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ResignDate { get; set; }

        [MaxLength(250)]
        public string? ResignReason { get; set; }

        [MaxLength(200)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(50)]
        public string? EmergencyContactRelation { get; set; }

        [MaxLength(13)]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "Nomor telepon kontak darurat harus 13 digit.")]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(500)]
        public string? EmergencyContactAddress { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? ResignDate { get; set; }

        [MaxLength(250)]
        public string? ResignReason { get; set; }
    }

    public class EmployeeCreateResponse
    {
        public Guid Id { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public EmployeeLoginAccountResponse? LoginAccount { get; set; }
    }

    public class EmployeeLoginAccountResponse
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
    }

    public class EmployeeOrganizationAssignmentResponse
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public Guid DepartmentId { get; set; }

        public string DepartmentCode { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public Guid PositionId { get; set; }

        public string PositionCode { get; set; } = string.Empty;

        public string PositionName { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class EmployeeOrganizationAssignmentListResponse
    {
        public Guid EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PrimaryData { get; set; }

        public List<EmployeeOrganizationAssignmentResponse> Items { get; set; } = new();
    }

    public class CreateEmployeeOrganizationAssignmentRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateEmployeeOrganizationAssignmentRequest
    {
        [Required]
        public Guid DepartmentId { get; set; }

        [Required]
        public Guid PositionId { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateEmployeeOrganizationAssignmentStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class SetEmployeeOrganizationAssignmentPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class EmployeeBankAccountResponse
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string BankName { get; set; } = string.Empty;

        public string AccountNumber { get; set; } = string.Empty;

        public string AccountHolderName { get; set; } = string.Empty;

        public string? BankBranch { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class EmployeeBankAccountListResponse
    {
        public Guid EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PrimaryData { get; set; }

        public List<EmployeeBankAccountResponse> Items { get; set; } = new();
    }

    public class CreateEmployeeBankAccountRequest
    {
        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BankBranch { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeBankAccountRequest
    {
        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BankBranch { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeBankAccountStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetEmployeeBankAccountPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class EmployeeDocumentResponse
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;

        public string DocumentName { get; set; } = string.Empty;

        public string? DocumentNumber { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class EmployeeDocumentListResponse
    {
        public Guid EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int VerifiedData { get; set; }

        public int DocumentWithFileData { get; set; }

        public List<EmployeeDocumentResponse> Items { get; set; } = new();
    }

    public class CreateEmployeeDocumentRequest
    {
        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeDocumentRequest
    {
        [Required]
        [MaxLength(100)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string DocumentName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DocumentNumber { get; set; }

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeDocumentStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyEmployeeDocumentRequest
    {
        public bool IsVerified { get; set; } = true;
    }

    public class EmployeeTransportAllowanceProfileResponse
    {
        public bool IsConfigured { get; set; }

        public Guid? Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public bool IsEligible { get; set; }

        public bool IsNightTransportEligible { get; set; }

        public string AllowanceMode { get; set; } = "None";

        public decimal MonthlyAmount { get; set; }

        public decimal DailyAmount { get; set; }

        public decimal NightAmount { get; set; }

        public bool IsProrated { get; set; }

        public bool IsTaxable { get; set; }

        public bool IsPayrollComponent { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }

    public class UpsertEmployeeTransportAllowanceProfileRequest
    {
        public bool IsEligible { get; set; }

        public bool IsNightTransportEligible { get; set; }

        [Required]
        [MaxLength(50)]
        public string AllowanceMode { get; set; } = "None";

        public decimal MonthlyAmount { get; set; }

        public decimal DailyAmount { get; set; }

        public decimal NightAmount { get; set; }

        public bool IsProrated { get; set; } = true;

        public bool IsTaxable { get; set; } = true;

        public bool IsPayrollComponent { get; set; } = true;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class EmployeeTransportAllowanceTransactionResponse
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public DateTime TransactionDate { get; set; }

        public string PeriodYearMonth { get; set; } = string.Empty;

        public string AllowanceType { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public bool IsGeneratedFromAttendance { get; set; }

        public bool IsNightShift { get; set; }

        public string TransactionStatus { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }
}
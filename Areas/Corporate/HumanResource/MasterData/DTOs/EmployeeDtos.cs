using global::QuilvianSystemBackend.Areas.Administrator.UserManagement.Enum;
using global::QuilvianSystemBackend.Enum;
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

        public string? EmployeeNumber { get; set; }

        public string? AttendanceNumber { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? NickName { get; set; }

        public Gender? Gender { get; set; }

        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        public string? Email { get; set; }

        public Guid PrimaryDepartmentId { get; set; }

        public string PrimaryDepartmentCode { get; set; } = string.Empty;

        public string PrimaryDepartmentName { get; set; } = string.Empty;

        public Guid PrimaryPositionId { get; set; }

        public string PrimaryPositionCode { get; set; } = string.Empty;

        public string PrimaryPositionName { get; set; } = string.Empty;

        public EmployeeStatus EmployeeStatus { get; set; }

        public EmployeeProfessionType ProfessionType { get; set; }

        public string? EmploymentType { get; set; }

        public string? GradeLevel { get; set; }

        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public DateTime? ResignDate { get; set; }

        public bool HasTransportAllowanceProfile { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class EmployeeDetailResponse : EmployeeResponse
    {
        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Religion { get; set; }

        public string? MaritalStatus { get; set; }

        public string? BloodType { get; set; }

        public string? IdentityType { get; set; }

        public string? IdentityNumber { get; set; }

        public string? Address { get; set; }

        public string? Province { get; set; }

        public string? City { get; set; }

        public string? District { get; set; }

        public string? Village { get; set; }

        public string? PostalCode { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public string? ResignReason { get; set; }

        public string? EmergencyContactName { get; set; }

        public string? EmergencyContactRelation { get; set; }

        public string? EmergencyContactPhone { get; set; }

        public string? EmergencyContactAddress { get; set; }

        public EmployeeTransportAllowanceProfileResponse? TransportAllowanceProfile { get; set; }
    }

    public class EmployeeOptionResponse
    {
        public Guid Id { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public Guid PrimaryDepartmentId { get; set; }

        public string PrimaryDepartmentName { get; set; } = string.Empty;

        public Guid PrimaryPositionId { get; set; }

        public string PrimaryPositionName { get; set; } = string.Empty;
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

    public class EmployeeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan. Jika customPeriod kosong atau custom, frontend boleh mengirim startDate dan endDate.";

        public EmployeeDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<EmployeeCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<EmployeeSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<string> TransportAllowanceModes { get; set; } = new();

        public List<string> TransportTransactionStatuses { get; set; } = new();

        public List<string> TransportAllowanceTypes { get; set; } = new();

        public List<EmployeeQueryParameterInfoResponse> QueryParameters { get; set; } = new();
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

        public EmployeeStatus? EmployeeStatus { get; set; }

        public EmployeeProfessionType? ProfessionType { get; set; }

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

    public class CreateEmployeeRequest
    {
        [MaxLength(50)]
        public string? EmployeeNumber { get; set; }

        [MaxLength(50)]
        public string? AttendanceNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NickName { get; set; }

        public Gender? Gender { get; set; }

        [MaxLength(100)]
        public string? BirthPlace { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(50)]
        public string? Religion { get; set; }

        [MaxLength(50)]
        public string? MaritalStatus { get; set; }

        [MaxLength(50)]
        public string? BloodType { get; set; }

        [MaxLength(50)]
        public string? IdentityType { get; set; }

        [MaxLength(50)]
        public string? IdentityNumber { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        [MaxLength(30)]
        public string? WhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(100)]
        public string? Village { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [Required]
        public Guid PrimaryDepartmentId { get; set; }

        [Required]
        public Guid PrimaryPositionId { get; set; }

        public EmployeeStatus EmployeeStatus { get; set; } = EmployeeStatus.Contract;

        public EmployeeProfessionType ProfessionType { get; set; } = EmployeeProfessionType.GeneralStaff;

        [MaxLength(50)]
        public string? EmploymentType { get; set; }

        [MaxLength(50)]
        public string? GradeLevel { get; set; }

        [MaxLength(50)]
        public string? WorkLocation { get; set; }

        public DateTime? JoinDate { get; set; }

        public DateTime? ProbationEndDate { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        [MaxLength(200)]
        public string? EmergencyContactName { get; set; }

        [MaxLength(50)]
        public string? EmergencyContactRelation { get; set; }

        [MaxLength(30)]
        public string? EmergencyContactPhone { get; set; }

        [MaxLength(500)]
        public string? EmergencyContactAddress { get; set; }
    }

    public class UpdateEmployeeRequest : CreateEmployeeRequest
    {
        public DateTime? ResignDate { get; set; }

        [MaxLength(250)]
        public string? ResignReason { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeStatusRequest
    {
        public bool IsActive { get; set; }

        public DateTime? ResignDate { get; set; }

        [MaxLength(250)]
        public string? ResignReason { get; set; }
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
}
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class DoctorServiceRuleSummaryResponse
    {
        public int TotalRule { get; set; }
        public int ActiveRule { get; set; }
        public int InactiveRule { get; set; }
        public int WalkInAllowedRule { get; set; }
        public int AppointmentAllowedRule { get; set; }
        public int KioskAllowedRule { get; set; }
        public int TelemedicineAllowedRule { get; set; }
        public int NeedReferralRule { get; set; }
        public int NeedApprovalRule { get; set; }
        public int PrimaryClinicRule { get; set; }
        public int DefaultClinicRule { get; set; }
        public int SuspendedRule { get; set; }
        public int ClosedRule { get; set; }
    }

    public class DoctorServiceRuleResponse
    {
        public Guid Id { get; set; }

        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public DoctorServiceRuleType RuleType { get; set; }
        public DoctorServiceRuleStatus RuleStatus { get; set; }

        public Guid DoctorId { get; set; }
        public string DoctorCode { get; set; } = string.Empty;
        public string DoctorNumber { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string? SpecialistName { get; set; }
        public string? SubSpecialistName { get; set; }
        public string? MedicalStaffGroup { get; set; }

        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitCode { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;

        public Guid? ClinicId { get; set; }
        public string? ClinicCode { get; set; }
        public string? ClinicName { get; set; }

        public Guid? TariffCategoryId { get; set; }
        public string? TariffCategoryCode { get; set; }
        public string? TariffCategoryName { get; set; }

        public Guid? TariffId { get; set; }
        public string? TariffCode { get; set; }
        public string? TariffName { get; set; }

        public Guid? ProcedureId { get; set; }
        public string? ProcedureCode { get; set; }
        public string? ProcedureName { get; set; }

        public Guid? PatientClassId { get; set; }
        public string? PatientClassCode { get; set; }
        public string? PatientClassName { get; set; }

        public bool IsAllowWalkIn { get; set; }
        public bool IsAllowAppointment { get; set; }
        public bool IsAllowKioskRegistration { get; set; }
        public bool IsAllowTelemedicine { get; set; }

        public bool IsNeedReferral { get; set; }
        public bool IsNeedApproval { get; set; }

        public bool IsPrimaryForClinic { get; set; }
        public bool IsDefaultForClinic { get; set; }

        public int DailyQuotaLimit { get; set; }
        public int PriorityLevel { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class DoctorServiceRuleDetailResponse : DoctorServiceRuleResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class DoctorServiceRuleOptionResponse
    {
        public Guid Id { get; set; }

        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public DoctorServiceRuleType RuleType { get; set; }
        public DoctorServiceRuleStatus RuleStatus { get; set; }

        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? SpecialistName { get; set; }

        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public Guid? TariffCategoryId { get; set; }
        public string? TariffCategoryName { get; set; }

        public Guid? TariffId { get; set; }
        public string? TariffName { get; set; }

        public Guid? ProcedureId { get; set; }
        public string? ProcedureName { get; set; }

        public Guid? PatientClassId { get; set; }
        public string? PatientClassName { get; set; }

        public bool IsAllowWalkIn { get; set; }
        public bool IsAllowAppointment { get; set; }
        public bool IsAllowKioskRegistration { get; set; }
        public bool IsAllowTelemedicine { get; set; }

        public bool IsNeedReferral { get; set; }
        public bool IsNeedApproval { get; set; }

        public bool IsPrimaryForClinic { get; set; }
        public bool IsDefaultForClinic { get; set; }

        public int DailyQuotaLimit { get; set; }
        public int PriorityLevel { get; set; }
    }

    public class DoctorServiceRuleOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<DoctorServiceRuleOptionResponse> Items { get; set; } = new();
    }

    public class DoctorServiceRuleEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DoctorServiceRuleFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public DoctorServiceRuleDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DoctorServiceRuleCustomPeriodResponse> CustomPeriods { get; set; } = new();
        public List<DoctorServiceRuleSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DoctorServiceRuleEnumOptionResponse> RuleTypeOptions { get; set; } = new();
        public List<DoctorServiceRuleEnumOptionResponse> RuleStatusOptions { get; set; } = new();
        public List<DoctorServiceRuleQueryParameterResponse> QueryParameters { get; set; } = new();
        public List<DoctorServiceRuleFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<DoctorServiceRuleFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class DoctorServiceRuleDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ClinicId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DoctorServiceRuleCustomPeriodResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DoctorServiceRuleSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DoctorServiceRuleQueryParameterResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class DoctorServiceRuleFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool Required { get; set; }
        public bool ReadOnly { get; set; }
        public bool IsVisible { get; set; } = true;
        public object? DefaultValue { get; set; }
        public string? OptionsSource { get; set; }
    }

    public class CreateDoctorServiceRuleRequest
    {
        [Required]
        [MaxLength(200)]
        public string RuleName { get; set; } = string.Empty;

        public DoctorServiceRuleType RuleType { get; set; } = DoctorServiceRuleType.GeneralService;

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? TariffCategoryId { get; set; }

        public Guid? TariffId { get; set; }

        public Guid? ProcedureId { get; set; }

        public Guid? PatientClassId { get; set; }

        public bool IsAllowWalkIn { get; set; } = true;

        public bool IsAllowAppointment { get; set; } = true;

        public bool IsAllowKioskRegistration { get; set; } = true;

        public bool IsAllowTelemedicine { get; set; } = false;

        public bool IsNeedReferral { get; set; } = false;

        public bool IsNeedApproval { get; set; } = false;

        public bool IsPrimaryForClinic { get; set; } = false;

        public bool IsDefaultForClinic { get; set; } = false;

        public int DailyQuotaLimit { get; set; } = 0;

        public int PriorityLevel { get; set; } = 0;

        public DoctorServiceRuleStatus RuleStatus { get; set; } = DoctorServiceRuleStatus.Active;

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateDoctorServiceRuleRequest : CreateDoctorServiceRuleRequest
    {
    }

    public class UpdateDoctorServiceRuleStatusRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class DeleteDoctorServiceRuleRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class DoctorServiceRuleCreateResponse
    {
        public Guid Id { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public DoctorServiceRuleType RuleType { get; set; }
        public DoctorServiceRuleStatus RuleStatus { get; set; }
        public Guid DoctorId { get; set; }
        public Guid ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? TariffCategoryId { get; set; }
        public Guid? TariffId { get; set; }
        public Guid? ProcedureId { get; set; }
        public Guid? PatientClassId { get; set; }
        public bool IsActive { get; set; }
    }

    public class DoctorServiceRuleUpdateResponse
    {
        public Guid Id { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class DoctorServiceRuleDeleteResponse
    {
        public Guid Id { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
        public DateTime? DeleteDateTime { get; set; }
    }
}

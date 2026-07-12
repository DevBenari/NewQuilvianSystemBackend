using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class InsuranceCoverageRuleSummaryResponse
    {
        public int TotalRule { get; set; }
        public int ActiveRule { get; set; }
        public int InactiveRule { get; set; }
        public int CoveredRule { get; set; }
        public int NotCoveredRule { get; set; }
        public int PartialCoveredRule { get; set; }
        public int NeedApprovalStatusRule { get; set; }
        public int ExcludedRule { get; set; }
        public int NeedApprovalRule { get; set; }
        public int NeedGuaranteeLetterRule { get; set; }
        public int AllowExcessPaymentByPatientRule { get; set; }
        public int EffectiveRule { get; set; }
        public int ExpiredRule { get; set; }
        public int TariffRule { get; set; }
        public int DrugRule { get; set; }
        public int DrugCategoryRule { get; set; }
        public int ProcedureRule { get; set; }
        public int ServiceCategoryRule { get; set; }
    }

    public class InsuranceCoverageRuleResponse
    {
        public Guid Id { get; set; }

        public Guid InsuranceProviderId { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public string? InsuranceGroupName { get; set; }

        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;

        public Guid? TariffId { get; set; }
        public string? TariffCode { get; set; }
        public string? TariffName { get; set; }

        public Guid? DrugId { get; set; }
        public string? DrugCode { get; set; }
        public string? DrugName { get; set; }

        public Guid? DrugCategoryId { get; set; }
        public string? DrugCategoryCode { get; set; }
        public string? DrugCategoryName { get; set; }

        public Guid? ProcedureId { get; set; }
        public string? ProcedureCode { get; set; }
        public string? ProcedureName { get; set; }

        public Guid? TariffCategoryId { get; set; }
        public string? TariffCategoryCode { get; set; }
        public string? TariffCategoryName { get; set; }

        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public Guid? PatientClassId { get; set; }
        public string? PatientClassCode { get; set; }
        public string? PatientClassName { get; set; }

        public string CoverageStatus { get; set; } = string.Empty;
        public decimal CoveragePercent { get; set; }
        public decimal? MaxCoverageAmount { get; set; }
        public decimal? CoPaymentPercent { get; set; }
        public decimal? CoPaymentAmount { get; set; }

        public bool IsNeedApproval { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }

        public int? MaxQuantityPerVisit { get; set; }
        public int? MaxQuantityPerMonth { get; set; }
        public decimal? MaxAmountPerVisit { get; set; }
        public decimal? MaxAmountPerMonth { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public int Priority { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class InsuranceCoverageRuleDetailResponse : InsuranceCoverageRuleResponse
    {
        public string? ApprovalInstruction { get; set; }
        public string? BillingInstruction { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class InsuranceCoverageRuleOptionResponse
    {
        public Guid Id { get; set; }

        public Guid InsuranceProviderId { get; set; }
        public string InsuranceProviderName { get; set; } = string.Empty;

        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;

        public Guid? TariffId { get; set; }
        public string? TariffName { get; set; }

        public Guid? DrugId { get; set; }
        public string? DrugName { get; set; }

        public Guid? DrugCategoryId { get; set; }
        public string? DrugCategoryName { get; set; }

        public Guid? ProcedureId { get; set; }
        public string? ProcedureName { get; set; }

        public Guid? TariffCategoryId { get; set; }
        public string? TariffCategoryName { get; set; }

        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public Guid? PatientClassId { get; set; }
        public string? PatientClassCode { get; set; }
        public string? PatientClassName { get; set; }

        public string CoverageStatus { get; set; } = string.Empty;
        public decimal CoveragePercent { get; set; }
        public decimal? MaxCoverageAmount { get; set; }
        public decimal? CoPaymentPercent { get; set; }
        public decimal? CoPaymentAmount { get; set; }

        public bool IsNeedApproval { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }
    }

    public class InsuranceCoverageRuleOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<InsuranceCoverageRuleOptionResponse> Items { get; set; } = new();
    }

    public class InsuranceCoverageRuleFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public InsuranceCoverageRuleDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<InsuranceCoverageRuleCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<InsuranceCoverageRuleSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<InsuranceCoverageRuleItemTypeOptionResponse> ItemTypeOptions { get; set; } = new();
        public List<InsuranceCoverageRuleCoverageStatusOptionResponse> CoverageStatusOptions { get; set; } = new();
    }

    public class InsuranceCoverageRuleDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? InsuranceProviderId { get; set; }
        public string? ItemType { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class InsuranceCoverageRuleCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class InsuranceCoverageRuleSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class InsuranceCoverageRuleItemTypeOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class InsuranceCoverageRuleCoverageStatusOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateInsuranceCoverageRuleRequest
    {
        [Required]
        public Guid InsuranceProviderId { get; set; }

        [Required]
        [MaxLength(150)]
        public string RuleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ItemType { get; set; } = string.Empty;

        public Guid? TariffId { get; set; }
        public Guid? DrugId { get; set; }
        public Guid? DrugCategoryId { get; set; }
        public Guid? ProcedureId { get; set; }
        public Guid? TariffCategoryId { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanName { get; set; }

        public Guid? PatientClassId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CoverageStatus { get; set; } = "Covered";

        [Range(0, 100)]
        public decimal CoveragePercent { get; set; } = 100;

        [Range(0, 999999999999)]
        public decimal? MaxCoverageAmount { get; set; }

        [Range(0, 100)]
        public decimal? CoPaymentPercent { get; set; }

        [Range(0, 999999999999)]
        public decimal? CoPaymentAmount { get; set; }

        public bool IsNeedApproval { get; set; } = false;
        public bool IsNeedGuaranteeLetter { get; set; } = false;
        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        [Range(0, int.MaxValue)]
        public int? MaxQuantityPerVisit { get; set; }

        [Range(0, int.MaxValue)]
        public int? MaxQuantityPerMonth { get; set; }

        [Range(0, 999999999999)]
        public decimal? MaxAmountPerVisit { get; set; }

        [Range(0, 999999999999)]
        public decimal? MaxAmountPerMonth { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? ApprovalInstruction { get; set; }

        [MaxLength(250)]
        public string? BillingInstruction { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public int Priority { get; set; } = 0;

        public int SortOrder { get; set; } = 0;
    }

    public class UpdateInsuranceCoverageRuleRequest : CreateInsuranceCoverageRuleRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateInsuranceCoverageRuleStatusRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class DeleteInsuranceCoverageRuleRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class InsuranceCoverageRuleCreateResponse
    {
        public Guid Id { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public Guid InsuranceProviderId { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class InsuranceCoverageRuleUpdateResponse
    {
        public Guid Id { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class InsuranceCoverageRuleStatusResponse
    {
        public Guid Id { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class InsuranceCoverageRuleDeleteResponse
    {
        public Guid Id { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
    }
}

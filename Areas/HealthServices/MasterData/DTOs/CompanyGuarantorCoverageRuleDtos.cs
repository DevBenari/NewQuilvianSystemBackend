using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class CompanyGuarantorCoverageRuleFilterMetadataResponse
    {
        public CompanyGuarantorCoverageRuleDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<CompanyGuarantorCoverageRuleCustomPeriodResponse> CustomPeriods { get; set; } = new();
        public List<CompanyGuarantorCoverageRuleSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
        public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
        public List<CompanyGuarantorCoverageRuleOptionItemResponse> ItemTypeOptions { get; set; } = new();
        public List<CompanyGuarantorCoverageRuleOptionItemResponse> CoverageStatusOptions { get; set; } = new();
        public List<CompanyGuarantorCoverageRuleQueryParameterResponse> QueryParameters { get; set; } = new();
        public List<CompanyGuarantorCoverageRuleFormFieldResponse> CreateFields { get; set; } = new();
        public List<CompanyGuarantorCoverageRuleFormFieldResponse> UpdateFields { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class CompanyGuarantorCoverageRuleDefaultFilterResponse
    {
        public string? Search { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public Guid? CompanyGuarantorId { get; set; }
        public string? ItemType { get; set; }
        public string? CoverageStatus { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? EmployeeGrade { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CompanyGuarantorCoverageRuleCustomPeriodResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class CompanyGuarantorCoverageRuleSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CompanyGuarantorCoverageRuleOptionItemResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CompanyGuarantorCoverageRuleQueryParameterResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class CompanyGuarantorCoverageRuleFormFieldResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = string.Empty;
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }

    public class CompanyGuarantorCoverageRuleSummaryResponse
    {
        public int TotalRule { get; set; }
        public int ActiveRule { get; set; }
        public int InactiveRule { get; set; }
        public int CoveredRule { get; set; }
        public int NotCoveredRule { get; set; }
        public int PartialCoveredRule { get; set; }
        public int NeedApprovalRule { get; set; }
    }

    public class CompanyGuarantorCoverageRuleResponse
    {
        public Guid Id { get; set; }
        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorCode { get; set; } = string.Empty;
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string? CompanyGroupName { get; set; }
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
        public string? EmployeeGrade { get; set; }
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
        public decimal? MaxQuantityPerVisit { get; set; }
        public decimal? MaxQuantityPerMonth { get; set; }
        public decimal? MaxAmountPerVisit { get; set; }
        public decimal? MaxAmountPerMonth { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int Priority { get; set; }
        public string? ApprovalInstruction { get; set; }
        public string? BillingInstruction { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class CompanyGuarantorCoverageRuleDetailResponse : CompanyGuarantorCoverageRuleResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class CompanyGuarantorCoverageRuleOptionResponse
    {
        public Guid Id { get; set; }
        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string RuleCode { get; set; } = string.Empty;
        public string RuleName { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string CoverageStatus { get; set; } = string.Empty;
        public decimal CoveragePercent { get; set; }
        public decimal? CoPaymentPercent { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? EmployeeGrade { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCompanyGuarantorCoverageRuleRequest
    {
        [Required(ErrorMessage = "Perusahaan penjamin wajib dipilih.")]
        public Guid CompanyGuarantorId { get; set; }

        [MaxLength(50, ErrorMessage = "Kode aturan maksimal 50 karakter.")]
        public string? RuleCode { get; set; }

        [Required(ErrorMessage = "Nama aturan wajib diisi.")]
        [MaxLength(200, ErrorMessage = "Nama aturan maksimal 200 karakter.")]
        public string RuleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipe item wajib diisi.")]
        [MaxLength(30, ErrorMessage = "Tipe item maksimal 30 karakter.")]
        public string ItemType { get; set; } = "Tariff";

        public Guid? TariffId { get; set; }
        public Guid? DrugId { get; set; }
        public Guid? DrugCategoryId { get; set; }
        public Guid? ProcedureId { get; set; }
        public Guid? TariffCategoryId { get; set; }
        public Guid? PatientClassId { get; set; }

        [MaxLength(100, ErrorMessage = "Kode benefit plan maksimal 100 karakter.")]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150, ErrorMessage = "Nama benefit plan maksimal 150 karakter.")]
        public string? BenefitPlanName { get; set; }

        [MaxLength(50, ErrorMessage = "Golongan karyawan maksimal 50 karakter.")]
        public string? EmployeeGrade { get; set; }

        [Required(ErrorMessage = "Status tanggungan wajib diisi.")]
        [MaxLength(30, ErrorMessage = "Status tanggungan maksimal 30 karakter.")]
        public string CoverageStatus { get; set; } = "Covered";

        public decimal CoveragePercent { get; set; } = 100m;

        public decimal? MaxCoverageAmount { get; set; }

        /// <summary>
        /// Masukan klien untuk CoPaymentPercent akan diabaikan; server menurunkan nilai ini otomatis (100 - CoveragePercent).
        /// </summary>
        public decimal? CoPaymentPercent { get; set; }

        public decimal? CoPaymentAmount { get; set; }

        public bool IsNeedApproval { get; set; } = false;

        public bool IsNeedGuaranteeLetter { get; set; } = false;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        public decimal? MaxQuantityPerVisit { get; set; }

        public decimal? MaxQuantityPerMonth { get; set; }

        public decimal? MaxAmountPerVisit { get; set; }

        public decimal? MaxAmountPerMonth { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public int Priority { get; set; } = 0;

        [MaxLength(500, ErrorMessage = "Instruksi persetujuan maksimal 500 karakter.")]
        public string? ApprovalInstruction { get; set; }

        [MaxLength(500, ErrorMessage = "Instruksi penagihan maksimal 500 karakter.")]
        public string? BillingInstruction { get; set; }

        [MaxLength(500, ErrorMessage = "Keterangan maksimal 500 karakter.")]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompanyGuarantorCoverageRuleRequest : CreateCompanyGuarantorCoverageRuleRequest
    {
    }

    public class UpdateCompanyGuarantorCoverageRuleStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteCompanyGuarantorCoverageRuleRequest
    {
        public string? DeleteReason { get; set; }
    }

    public class CompanyGuarantorCoverageRuleValidationException : Exception
    {
        public int StatusCode { get; }

        public CompanyGuarantorCoverageRuleValidationException(string message, int statusCode = 400)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}

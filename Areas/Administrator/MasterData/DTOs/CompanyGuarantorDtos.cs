using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class CompanyGuarantorSummaryResponse
    {
        public int TotalCompanyGuarantor { get; set; }
        public int ActiveCompanyGuarantor { get; set; }
        public int InactiveCompanyGuarantor { get; set; }
        public int CorporateGuarantor { get; set; }
        public int GovernmentGuarantor { get; set; }
        public int FoundationGuarantor { get; set; }
        public int SchoolGuarantor { get; set; }
        public int OtherGuarantor { get; set; }
        public int InvoiceBillingGuarantor { get; set; }
        public int DepositBillingGuarantor { get; set; }
        public int MixedBillingGuarantor { get; set; }
        public int UsingCompanyTariffBookGuarantor { get; set; }
        public int UsingHospitalTariffGuarantor { get; set; }
        public int NeedGuaranteeLetterGuarantor { get; set; }
        public int NeedEmployeeVerificationGuarantor { get; set; }
        public int NeedApprovalForProcedureGuarantor { get; set; }
        public int NeedApprovalForDrugGuarantor { get; set; }
        public int CoverageLimitedByEmployeeGradeGuarantor { get; set; }
        public int AllowExcessPaymentByPatientGuarantor { get; set; }
        public int ActiveContractGuarantor { get; set; }
        public int ExpiredContractGuarantor { get; set; }
    }

    public class CompanyGuarantorResponse
    {
        public Guid Id { get; set; }
        public string CompanyGuarantorCode { get; set; } = string.Empty;
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string? CompanyGroupName { get; set; }
        public string GuarantorType { get; set; } = string.Empty;
        public string BillingMethod { get; set; } = string.Empty;
        public string? ExternalCompanyCode { get; set; }
        public string? IntegrationCode { get; set; }
        public string? ContractNumber { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public bool IsUsingCompanyTariffBook { get; set; }
        public bool IsUsingHospitalTariff { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsNeedEmployeeVerification { get; set; }
        public bool IsNeedApprovalForProcedure { get; set; }
        public bool IsNeedApprovalForDrug { get; set; }
        public bool IsCoverageLimitedByEmployeeGrade { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }
        public decimal? CreditLimitAmount { get; set; }
        public decimal? CurrentOutstandingAmount { get; set; }
        public int PaymentDueDays { get; set; }
        public string? PicName { get; set; }
        public string? PicPhoneNumber { get; set; }
        public string? PicWhatsAppNumber { get; set; }
        public string? PicEmail { get; set; }
        public string? OfficeAddress { get; set; }
        public string? LogoPath { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class CompanyGuarantorDetailResponse : CompanyGuarantorResponse
    {
        public string? BillingInstruction { get; set; }
        public string? ClaimInstruction { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class CompanyGuarantorOptionResponse
    {
        public Guid Id { get; set; }
        public string CompanyGuarantorCode { get; set; } = string.Empty;
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string? CompanyGroupName { get; set; }
        public string GuarantorType { get; set; } = string.Empty;
        public string BillingMethod { get; set; } = string.Empty;
        public bool IsUsingCompanyTariffBook { get; set; }
        public bool IsUsingHospitalTariff { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsNeedEmployeeVerification { get; set; }
        public bool IsNeedApprovalForProcedure { get; set; }
        public bool IsNeedApprovalForDrug { get; set; }
        public bool IsCoverageLimitedByEmployeeGrade { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }
        public decimal? CreditLimitAmount { get; set; }
        public decimal? CurrentOutstandingAmount { get; set; }
        public int PaymentDueDays { get; set; }
    }

    public class CompanyGuarantorOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<CompanyGuarantorOptionResponse> Items { get; set; } = new();
    }

    public class CompanyGuarantorFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public CompanyGuarantorDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<CompanyGuarantorCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<CompanyGuarantorSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<CompanyGuarantorStringOptionResponse> GuarantorTypeOptions { get; set; } = new();
        public List<CompanyGuarantorStringOptionResponse> BillingMethodOptions { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class CompanyGuarantorDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CompanyGuarantorCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CompanyGuarantorSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CompanyGuarantorStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateCompanyGuarantorRequest
    {
        [Required]
        [MaxLength(200)]
        public string CompanyGuarantorName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CompanyGroupName { get; set; }

        [Required]
        [MaxLength(50)]
        public string GuarantorType { get; set; } = "Corporate";

        [Required]
        [MaxLength(50)]
        public string BillingMethod { get; set; } = "Invoice";

        [MaxLength(50)]
        public string? ExternalCompanyCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        [MaxLength(100)]
        public string? ContractNumber { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool IsUsingCompanyTariffBook { get; set; } = true;

        public bool IsUsingHospitalTariff { get; set; } = false;

        public bool IsNeedGuaranteeLetter { get; set; } = true;

        public bool IsNeedEmployeeVerification { get; set; } = true;

        public bool IsNeedApprovalForProcedure { get; set; } = true;

        public bool IsNeedApprovalForDrug { get; set; } = false;

        public bool IsCoverageLimitedByEmployeeGrade { get; set; } = true;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        [Range(0, 999999999999)]
        public decimal? CreditLimitAmount { get; set; }

        [Range(0, 999999999999)]
        public decimal? CurrentOutstandingAmount { get; set; }

        [Range(0, 3650)]
        public int PaymentDueDays { get; set; } = 30;

        [MaxLength(100)]
        public string? PicName { get; set; }

        [MaxLength(30)]
        public string? PicPhoneNumber { get; set; }

        [MaxLength(30)]
        public string? PicWhatsAppNumber { get; set; }

        [MaxLength(200)]
        public string? PicEmail { get; set; }

        [MaxLength(500)]
        public string? OfficeAddress { get; set; }

        [MaxLength(500)]
        public string? LogoPath { get; set; }

        [MaxLength(250)]
        public string? BillingInstruction { get; set; }

        [MaxLength(250)]
        public string? ClaimInstruction { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public int SortOrder { get; set; } = 0;
    }

    public class UpdateCompanyGuarantorRequest : CreateCompanyGuarantorRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCompanyGuarantorStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteCompanyGuarantorRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class CompanyGuarantorCreateResponse
    {
        public Guid Id { get; set; }
        public string CompanyGuarantorCode { get; set; } = string.Empty;
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string GuarantorType { get; set; } = string.Empty;
        public string BillingMethod { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CompanyGuarantorUpdateResponse
    {
        public Guid Id { get; set; }
        public string CompanyGuarantorCode { get; set; } = string.Empty;
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class CompanyGuarantorDeleteResponse
    {
        public Guid Id { get; set; }
        public string CompanyGuarantorCode { get; set; } = string.Empty;
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
    }
}

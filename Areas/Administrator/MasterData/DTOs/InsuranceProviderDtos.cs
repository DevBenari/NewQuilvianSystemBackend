using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class InsuranceProviderSummaryResponse
    {
        public int TotalInsuranceProvider { get; set; }
        public int ActiveInsuranceProvider { get; set; }
        public int InactiveInsuranceProvider { get; set; }
        public int PrivateInsuranceProvider { get; set; }
        public int TpaProvider { get; set; }
        public int GovernmentInsuranceProvider { get; set; }
        public int CorporateInsuranceProvider { get; set; }
        public int OtherInsuranceProvider { get; set; }
        public int CashlessProvider { get; set; }
        public int ReimbursementProvider { get; set; }
        public int GuaranteeLetterProvider { get; set; }
        public int MixedClaimProvider { get; set; }
        public int NeedEligibilityCheckProvider { get; set; }
        public int NeedGuaranteeLetterProvider { get; set; }
        public int NeedReferralLetterProvider { get; set; }
        public int NeedApprovalForProcedureProvider { get; set; }
        public int NeedApprovalForDrugProvider { get; set; }
        public int UsingInsuranceTariffBookProvider { get; set; }
        public int UsingHospitalTariffProvider { get; set; }
        public int CoverageLimitedByPlanProvider { get; set; }
        public int AllowExcessPaymentByPatientProvider { get; set; }
        public int ActiveContractProvider { get; set; }
        public int ExpiredContractProvider { get; set; }
        public int UpcomingContractProvider { get; set; }
        public int NoContractDateProvider { get; set; }
    }

    public class InsuranceProviderResponse
    {
        public Guid Id { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public string? InsuranceGroupName { get; set; }
        public string ProviderType { get; set; } = string.Empty;
        public string ProviderTypeName { get; set; } = string.Empty;
        public string ClaimMethod { get; set; } = string.Empty;
        public string ClaimMethodName { get; set; } = string.Empty;
        public string? ExternalProviderCode { get; set; }
        public string? IntegrationCode { get; set; }
        public string? ContractNumber { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public string ContractStatus { get; set; } = string.Empty;
        public string ContractStatusName { get; set; } = string.Empty;
        public bool IsUsingInsuranceTariffBook { get; set; }
        public bool IsUsingHospitalTariff { get; set; }
        public bool IsNeedEligibilityCheck { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsNeedReferralLetter { get; set; }
        public bool IsNeedApprovalForProcedure { get; set; }
        public bool IsNeedApprovalForDrug { get; set; }
        public bool IsCoverageLimitedByPlan { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }
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

    public class InsuranceProviderDetailResponse : InsuranceProviderResponse
    {
        public string? BillingInstruction { get; set; }
        public string? ClaimInstruction { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class InsuranceProviderOptionResponse
    {
        public Guid Id { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public string? InsuranceGroupName { get; set; }
        public string ProviderType { get; set; } = string.Empty;
        public string ProviderTypeName { get; set; } = string.Empty;
        public string ClaimMethod { get; set; } = string.Empty;
        public string ClaimMethodName { get; set; } = string.Empty;
        public string? ContractNumber { get; set; }
        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public string ContractStatus { get; set; } = string.Empty;
        public string ContractStatusName { get; set; } = string.Empty;
        public bool IsUsingInsuranceTariffBook { get; set; }
        public bool IsUsingHospitalTariff { get; set; }
        public bool IsNeedEligibilityCheck { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsNeedReferralLetter { get; set; }
        public bool IsNeedApprovalForProcedure { get; set; }
        public bool IsNeedApprovalForDrug { get; set; }
        public bool IsCoverageLimitedByPlan { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }
        public int SortOrder { get; set; }
    }

    public class InsuranceProviderOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<InsuranceProviderOptionResponse> Items { get; set; } = new();
    }

    public class InsuranceProviderFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset";

        public InsuranceProviderDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<InsuranceProviderCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<InsuranceProviderSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<InsuranceProviderStringOptionResponse> ProviderTypeOptions { get; set; } = new();
        public List<InsuranceProviderStringOptionResponse> ClaimMethodOptions { get; set; } = new();
        public List<InsuranceProviderStringOptionResponse> ContractStatusOptions { get; set; } = new();
        public List<InsuranceProviderQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<InsuranceProviderFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<InsuranceProviderFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class InsuranceProviderDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? ProviderType { get; set; }
        public string? ClaimMethod { get; set; }
        public string? ContractStatus { get; set; }
        public bool? IsUsingInsuranceTariffBook { get; set; }
        public bool? IsUsingHospitalTariff { get; set; }
        public bool? IsNeedEligibilityCheck { get; set; }
        public bool? IsNeedGuaranteeLetter { get; set; }
        public bool? IsNeedReferralLetter { get; set; }
        public bool? IsNeedApprovalForProcedure { get; set; }
        public bool? IsNeedApprovalForDrug { get; set; }
        public bool? IsCoverageLimitedByPlan { get; set; }
        public bool? IsAllowExcessPaymentByPatient { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class InsuranceProviderCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class InsuranceProviderSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class InsuranceProviderStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class InsuranceProviderQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class InsuranceProviderFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateInsuranceProviderRequest
    {
        [Required]
        [MaxLength(200)]
        public string InsuranceProviderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? InsuranceGroupName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProviderType { get; set; } = "PrivateInsurance";

        [Required]
        [MaxLength(50)]
        public string ClaimMethod { get; set; } = "Cashless";

        [MaxLength(50)]
        public string? ExternalProviderCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        [MaxLength(100)]
        public string? ContractNumber { get; set; }

        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
        public bool IsUsingInsuranceTariffBook { get; set; } = true;
        public bool IsUsingHospitalTariff { get; set; } = false;
        public bool IsNeedEligibilityCheck { get; set; } = true;
        public bool IsNeedGuaranteeLetter { get; set; } = true;
        public bool IsNeedReferralLetter { get; set; } = false;
        public bool IsNeedApprovalForProcedure { get; set; } = true;
        public bool IsNeedApprovalForDrug { get; set; } = false;
        public bool IsCoverageLimitedByPlan { get; set; } = true;
        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

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

    public class UpdateInsuranceProviderRequest : CreateInsuranceProviderRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateInsuranceProviderStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteInsuranceProviderRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class InsuranceProviderCreateResponse
    {
        public Guid Id { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public string ProviderType { get; set; } = string.Empty;
        public string ProviderTypeName { get; set; } = string.Empty;
        public string ClaimMethod { get; set; } = string.Empty;
        public string ClaimMethodName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class InsuranceProviderUpdateResponse
    {
        public Guid Id { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public string ProviderType { get; set; } = string.Empty;
        public string ProviderTypeName { get; set; } = string.Empty;
        public string ClaimMethod { get; set; } = string.Empty;
        public string ClaimMethodName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class InsuranceProviderDeleteResponse
    {
        public Guid Id { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}

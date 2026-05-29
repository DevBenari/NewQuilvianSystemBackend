using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
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
        public int CashlessProvider { get; set; }
        public int ReimbursementProvider { get; set; }
        public int GuaranteeLetterProvider { get; set; }
        public int MixedClaimProvider { get; set; }
        public int NeedEligibilityCheckProvider { get; set; }
        public int NeedGuaranteeLetterProvider { get; set; }
        public int NeedReferralLetterProvider { get; set; }
        public int NeedApprovalForProcedureProvider { get; set; }
        public int NeedApprovalForDrugProvider { get; set; }
        public int ActiveContractProvider { get; set; }
        public int ExpiredContractProvider { get; set; }
    }

    public class InsuranceProviderResponse
    {
        public Guid Id { get; set; }

        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public string? InsuranceGroupName { get; set; }

        public string ProviderType { get; set; } = string.Empty;
        public string ClaimMethod { get; set; } = string.Empty;

        public string? ExternalProviderCode { get; set; }
        public string? IntegrationCode { get; set; }
        public string? ContractNumber { get; set; }

        public DateTime? ContractStartDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

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
    }

    public class InsuranceProviderDetailResponse : InsuranceProviderResponse
    {
        public string? BillingInstruction { get; set; }
        public string? ClaimInstruction { get; set; }
        public string? Description { get; set; }
    }

    public class InsuranceProviderOptionResponse
    {
        public Guid Id { get; set; }

        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public string? InsuranceGroupName { get; set; }

        public string ProviderType { get; set; } = string.Empty;
        public string ClaimMethod { get; set; } = string.Empty;

        public bool IsNeedEligibilityCheck { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsNeedReferralLetter { get; set; }
        public bool IsNeedApprovalForProcedure { get; set; }
        public bool IsNeedApprovalForDrug { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }
    }

    public class InsuranceProviderFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public InsuranceProviderDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<InsuranceProviderSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> ProviderTypes { get; set; } = new();
        public List<string> ClaimMethods { get; set; } = new();
    }

    public class InsuranceProviderDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }

        public string? ProviderType { get; set; }
        public string? ClaimMethod { get; set; }
        public string? InsuranceGroupName { get; set; }

        public bool? IsUsingInsuranceTariffBook { get; set; }
        public bool? IsUsingHospitalTariff { get; set; }
        public bool? IsNeedEligibilityCheck { get; set; }
        public bool? IsNeedGuaranteeLetter { get; set; }
        public bool? IsNeedReferralLetter { get; set; }
        public bool? IsNeedApprovalForProcedure { get; set; }
        public bool? IsNeedApprovalForDrug { get; set; }
        public bool? IsCoverageLimitedByPlan { get; set; }
        public bool? IsAllowExcessPaymentByPatient { get; set; }

        public DateTime? ContractDate { get; set; }

        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class InsuranceProviderSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateInsuranceProviderRequest
    {
        [Required]
        [MaxLength(50)]
        public string InsuranceProviderCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InsuranceProviderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? InsuranceGroupName { get; set; }

        [MaxLength(50)]
        public string ProviderType { get; set; } = "PrivateInsurance";

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

        public bool IsActive { get; set; } = true;
    }

    public class UpdateInsuranceProviderRequest
    {
        [Required]
        [MaxLength(50)]
        public string InsuranceProviderCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string InsuranceProviderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? InsuranceGroupName { get; set; }

        [MaxLength(50)]
        public string ProviderType { get; set; } = "PrivateInsurance";

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

        public bool IsActive { get; set; } = true;
    }

    public class InsuranceProviderCreateResponse
    {
        public Guid Id { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class InsuranceProviderUpdateResponse
    {
        public Guid Id { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class InsuranceProviderDeleteResponse
    {
        public Guid Id { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
    }

    public class InsuranceProviderStatusResponse
    {
        public Guid Id { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}

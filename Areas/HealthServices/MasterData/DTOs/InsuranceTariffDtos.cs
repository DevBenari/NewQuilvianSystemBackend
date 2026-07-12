using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class InsuranceTariffSummaryResponse
    {
        public int TotalInsuranceTariff { get; set; }
        public int ActiveInsuranceTariff { get; set; }
        public int InactiveInsuranceTariff { get; set; }
        public int NeedApprovalTariff { get; set; }
        public int UsingContractPriceTariff { get; set; }
        public int EffectiveTariff { get; set; }
        public int ExpiredTariff { get; set; }
        public int FutureTariff { get; set; }
    }

    public class InsuranceTariffResponse
    {
        public Guid Id { get; set; }
        public Guid InsuranceProviderId { get; set; }
        public string InsuranceProviderCode { get; set; } = string.Empty;
        public string InsuranceProviderName { get; set; } = string.Empty;
        public Guid TariffId { get; set; }
        public string TariffCode { get; set; } = string.Empty;
        public string TariffName { get; set; } = string.Empty;
        public Guid TariffCategoryId { get; set; }
        public string TariffCategoryName { get; set; } = string.Empty;
        public Guid? DrugId { get; set; }
        public string? DrugName { get; set; }
        public Guid? ProcedureId { get; set; }
        public string? ProcedureName { get; set; }
        public Guid? PatientClassId { get; set; }
        public string? PatientClassCode { get; set; }
        public string? PatientClassName { get; set; }
        public string InsuranceTariffCode { get; set; } = string.Empty;
        public string InsuranceTariffName { get; set; } = string.Empty;
        public string? ExternalServiceCode { get; set; }
        public string? ExternalClassCode { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public decimal HospitalPrice { get; set; }
        public decimal ContractPrice { get; set; }
        public decimal? HospitalPriceSnapshot { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountPercent { get; set; }
        public bool IsUsingContractPrice { get; set; }
        public bool IsNeedApproval { get; set; }
        public int Priority { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsCurrentlyEffective { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class InsuranceTariffDetailResponse : InsuranceTariffResponse
    {
        public string? BillingInstruction { get; set; }
        public string? ClaimInstruction { get; set; }
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class InsuranceTariffOptionResponse
    {
        public Guid Id { get; set; }
        public Guid InsuranceProviderId { get; set; }
        public string InsuranceProviderName { get; set; } = string.Empty;
        public Guid TariffId { get; set; }
        public string TariffCode { get; set; } = string.Empty;
        public string TariffName { get; set; } = string.Empty;
        public Guid? DrugId { get; set; }
        public string? DrugName { get; set; }
        public Guid? ProcedureId { get; set; }
        public string? ProcedureName { get; set; }
        public Guid? PatientClassId { get; set; }
        public string? PatientClassName { get; set; }
        public string InsuranceTariffCode { get; set; } = string.Empty;
        public string InsuranceTariffName { get; set; } = string.Empty;
        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public decimal ContractPrice { get; set; }
        public bool IsNeedApproval { get; set; }
        public int Priority { get; set; }
    }

    public class InsuranceTariffOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<InsuranceTariffOptionResponse> Items { get; set; } = new();
    }

    public class InsuranceTariffFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public InsuranceTariffDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<InsuranceTariffCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<InsuranceTariffSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }


    public class InsuranceTariffDefaultFilterResponse
    {
        public Guid? InsuranceProviderId { get; set; }
        public Guid? TariffId { get; set; }
        public Guid? PatientClassId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class InsuranceTariffCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class InsuranceTariffSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateInsuranceTariffRequest
    {
        [Required]
        public Guid InsuranceProviderId { get; set; }

        [Required]
        public Guid TariffId { get; set; }

        public Guid? PatientClassId { get; set; }

        [Required, MaxLength(250)]
        public string InsuranceTariffName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ExternalServiceCode { get; set; }

        [MaxLength(50)]
        public string? ExternalClassCode { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanName { get; set; }

        [Range(typeof(decimal), "0", "999999999999")]
        public decimal ContractPrice { get; set; }

        [Range(typeof(decimal), "0", "999999999999")]
        public decimal? HospitalPriceSnapshot { get; set; }

        [Range(typeof(decimal), "0", "999999999999")]
        public decimal? DiscountAmount { get; set; }

        [Range(typeof(decimal), "0", "100")]
        public decimal? DiscountPercent { get; set; }

        public bool IsUsingContractPrice { get; set; } = true;
        public bool IsNeedApproval { get; set; }
        public int Priority { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public int SortOrder { get; set; }

        [MaxLength(250)] public string? BillingInstruction { get; set; }
        [MaxLength(250)] public string? ClaimInstruction { get; set; }
        [MaxLength(250)] public string? Description { get; set; }
    }

    public class UpdateInsuranceTariffRequest : CreateInsuranceTariffRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateInsuranceTariffStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteInsuranceTariffRequest
    {
        [MaxLength(250)] public string? DeleteReason { get; set; }
    }

    public class InsuranceTariffCreateResponse
    {
        public Guid Id { get; set; }
        public Guid InsuranceProviderId { get; set; }
        public Guid TariffId { get; set; }
        public Guid? PatientClassId { get; set; }
        public string InsuranceTariffCode { get; set; } = string.Empty;
        public string InsuranceTariffName { get; set; } = string.Empty;
        public decimal ContractPrice { get; set; }
        public bool IsActive { get; set; }
    }

    public class InsuranceTariffStatusResponse
    {
        public Guid Id { get; set; }
        public string InsuranceTariffCode { get; set; } = string.Empty;
        public string InsuranceTariffName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class InsuranceTariffDeleteResponse
    {
        public Guid Id { get; set; }
        public string InsuranceTariffCode { get; set; } = string.Empty;
        public string InsuranceTariffName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
    }
}

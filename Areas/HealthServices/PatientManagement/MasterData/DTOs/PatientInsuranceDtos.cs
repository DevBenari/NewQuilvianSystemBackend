using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    public class PatientInsuranceSummaryResponse
    {
        public int TotalPatientInsurance { get; set; }

        public int ActivePatientInsurance { get; set; }

        public int InactivePatientInsurance { get; set; }

        public int PrimaryPatientInsurance { get; set; }

        public int EligiblePatientInsurance { get; set; }

        public int IneligiblePatientInsurance { get; set; }

        public int NeedGuaranteeLetterPatientInsurance { get; set; }

        public int NeedReferralLetterPatientInsurance { get; set; }

        public int AllowExcessPaymentByPatientInsurance { get; set; }

        public int EffectivePatientInsurance { get; set; }

        public int ExpiredPatientInsurance { get; set; }

        public int WithAnnualLimitPatientInsurance { get; set; }

        public int WithRemainingLimitPatientInsurance { get; set; }

        public int WithCoPaymentPatientInsurance { get; set; }

        public int WithCardImagePatientInsurance { get; set; }
    }

    public class PatientInsuranceResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public Guid InsuranceProviderId { get; set; }

        public string InsuranceProviderCode { get; set; } = string.Empty;

        public string InsuranceProviderName { get; set; } = string.Empty;

        public string? InsuranceGroupName { get; set; }

        public string? ProviderType { get; set; }

        public string? ClaimMethod { get; set; }

        public string PolicyNumber { get; set; } = string.Empty;

        public string? CardNumber { get; set; }

        public string? MemberNumber { get; set; }

        public string? PlanName { get; set; }

        public string? ClassName { get; set; }

        public string? BenefitPlanCode { get; set; }

        public string? HolderName { get; set; }

        public string? HolderRelationship { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsEligible { get; set; }

        public DateTime? LastEligibilityCheckAt { get; set; }

        public string? LastEligibilityReferenceNumber { get; set; }

        public decimal? AnnualLimitAmount { get; set; }

        public decimal? RemainingLimitAmount { get; set; }

        public decimal? CoPaymentPercent { get; set; }

        public decimal? CoPaymentAmount { get; set; }

        public bool IsNeedGuaranteeLetter { get; set; }

        public bool IsNeedReferralLetter { get; set; }

        public bool IsAllowExcessPaymentByPatient { get; set; }

        public string? CardImagePath { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid? CreateBy { get; set; }

        public string? CreateByName { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }

        public string? UpdateByName { get; set; }
    }

    public class PatientInsuranceDetailResponse : PatientInsuranceResponse
    {
        public string? EligibilityNote { get; set; }

        public string? Notes { get; set; }
    }

    public class PatientInsuranceOptionResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string PatientCode { get; set; } = string.Empty;

        public string MedicalRecordNumber { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public Guid InsuranceProviderId { get; set; }

        public string InsuranceProviderCode { get; set; } = string.Empty;

        public string InsuranceProviderName { get; set; } = string.Empty;

        public string PolicyNumber { get; set; } = string.Empty;

        public string? CardNumber { get; set; }

        public string? MemberNumber { get; set; }

        public string? PlanName { get; set; }

        public string? ClassName { get; set; }

        public string? BenefitPlanCode { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsEligible { get; set; }

        public bool IsNeedGuaranteeLetter { get; set; }

        public bool IsNeedReferralLetter { get; set; }

        public bool IsAllowExcessPaymentByPatient { get; set; }

        public decimal? AnnualLimitAmount { get; set; }

        public decimal? RemainingLimitAmount { get; set; }

        public decimal? CoPaymentPercent { get; set; }

        public decimal? CoPaymentAmount { get; set; }
    }

    public class PatientInsuranceOptionPagedResponse
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalData { get; set; }

        public int TotalPage { get; set; }

        public List<PatientInsuranceOptionResponse> Items { get; set; } = new();
    }

    public class PatientInsuranceFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public PatientInsuranceDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<PatientInsuranceCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<PatientInsuranceRelationFilterResponse> RelationFilters { get; set; } = new();

        public List<PatientInsuranceSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<string> HolderRelationships { get; set; } = new();

        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class PatientInsuranceDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CustomPeriod { get; set; }

        public Guid? PatientId { get; set; }

        public Guid? InsuranceProviderId { get; set; }

        public bool? IsActive { get; set; }

        public string? Search { get; set; }

        public string SortBy { get; set; } = "createDateTime";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class PatientInsuranceRelationFilterResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Endpoint { get; set; } = string.Empty;
    }

    public class PatientInsuranceCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class PatientInsuranceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientInsuranceRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid InsuranceProviderId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PolicyNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CardNumber { get; set; }

        [MaxLength(100)]
        public string? MemberNumber { get; set; }

        [MaxLength(150)]
        public string? PlanName { get; set; }

        [MaxLength(100)]
        public string? ClassName { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(200)]
        public string? HolderName { get; set; }

        [MaxLength(50)]
        public string? HolderRelationship { get; set; }

        public DateTime? EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsEligible { get; set; } = true;

        public DateTime? LastEligibilityCheckAt { get; set; }

        [MaxLength(100)]
        public string? LastEligibilityReferenceNumber { get; set; }

        [MaxLength(250)]
        public string? EligibilityNote { get; set; }

        public decimal? AnnualLimitAmount { get; set; }

        public decimal? RemainingLimitAmount { get; set; }

        public decimal? CoPaymentPercent { get; set; }

        public decimal? CoPaymentAmount { get; set; }

        public bool IsNeedGuaranteeLetter { get; set; } = true;

        public bool IsNeedReferralLetter { get; set; } = false;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        [MaxLength(500)]
        public string? CardImagePath { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }
    }

    public class UpdatePatientInsuranceRequest : CreatePatientInsuranceRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePatientInsuranceStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeletePatientInsuranceRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class PatientInsuranceCreateResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public Guid InsuranceProviderId { get; set; }

        public string InsuranceProviderName { get; set; } = string.Empty;

        public string PolicyNumber { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public bool IsEligible { get; set; }

        public bool IsActive { get; set; }
    }
}

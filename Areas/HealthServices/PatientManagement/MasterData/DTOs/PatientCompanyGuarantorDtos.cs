using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    public class PatientCompanyGuarantorSummaryResponse
    {
        public int TotalPatientCompanyGuarantor { get; set; }
        public int ActivePatientCompanyGuarantor { get; set; }
        public int InactivePatientCompanyGuarantor { get; set; }
        public int PrimaryPatientCompanyGuarantor { get; set; }
        public int EligiblePatientCompanyGuarantor { get; set; }
        public int IneligiblePatientCompanyGuarantor { get; set; }
        public int NeedGuaranteeLetterPatientCompanyGuarantor { get; set; }
        public int NeedEmployeeVerificationPatientCompanyGuarantor { get; set; }
        public int AllowExcessPaymentByPatientCompanyGuarantor { get; set; }
        public int EffectivePatientCompanyGuarantor { get; set; }
        public int ExpiredPatientCompanyGuarantor { get; set; }
        public int WithAnnualLimitPatientCompanyGuarantor { get; set; }
        public int WithRemainingLimitPatientCompanyGuarantor { get; set; }
        public int WithCoPaymentPatientCompanyGuarantor { get; set; }
        public int RecentlyEligibilityCheckedPatientCompanyGuarantor { get; set; }
        public int WithGuaranteeDocumentPatientCompanyGuarantor { get; set; }
    }

    public class PatientCompanyGuarantorResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }
        public string PatientCode { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;

        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorCode { get; set; } = string.Empty;
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string? CompanyGroupName { get; set; }
        public string? GuarantorType { get; set; }
        public string? BillingMethod { get; set; }

        public string EmployeeNumber { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public string? GradeLevel { get; set; }

        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public string? ClassName { get; set; }

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
        public bool IsNeedEmployeeVerification { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }

        public string? GuaranteeDocumentPath { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientCompanyGuarantorDetailResponse : PatientCompanyGuarantorResponse
    {
        public string? EligibilityNote { get; set; }
        public string? Notes { get; set; }
    }

    public class PatientCompanyGuarantorOptionResponse
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorName { get; set; } = string.Empty;

        public string EmployeeNumber { get; set; } = string.Empty;
        public string? EmployeeName { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public string? GradeLevel { get; set; }

        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public string? ClassName { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        public bool IsPrimary { get; set; }
        public bool IsEligible { get; set; }
        public bool IsNeedGuaranteeLetter { get; set; }
        public bool IsNeedEmployeeVerification { get; set; }
        public bool IsAllowExcessPaymentByPatient { get; set; }

        public decimal? AnnualLimitAmount { get; set; }
        public decimal? RemainingLimitAmount { get; set; }
        public decimal? CoPaymentPercent { get; set; }
        public decimal? CoPaymentAmount { get; set; }
    }

    public class PatientCompanyGuarantorFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientCompanyGuarantorDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientCompanyGuarantorSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<string> GradeLevels { get; set; } = new();
    }

    public class PatientCompanyGuarantorDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }

        public Guid? PatientId { get; set; }
        public Guid? CompanyGuarantorId { get; set; }

        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public string? GradeLevel { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public string? ClassName { get; set; }

        public bool? IsPrimary { get; set; }
        public bool? IsEligible { get; set; }
        public bool? IsNeedGuaranteeLetter { get; set; }
        public bool? IsNeedEmployeeVerification { get; set; }
        public bool? IsAllowExcessPaymentByPatient { get; set; }

        public DateTime? EffectiveDate { get; set; }
        public DateTime? LastEligibilityCheckFrom { get; set; }
        public DateTime? LastEligibilityCheckTo { get; set; }

        public decimal? MinimumAnnualLimitAmount { get; set; }
        public decimal? MaximumAnnualLimitAmount { get; set; }
        public decimal? MinimumRemainingLimitAmount { get; set; }
        public decimal? MaximumRemainingLimitAmount { get; set; }

        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientCompanyGuarantorSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientCompanyGuarantorRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid CompanyGuarantorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? EmployeeName { get; set; }

        [MaxLength(100)]
        public string? DepartmentName { get; set; }

        [MaxLength(100)]
        public string? PositionName { get; set; }

        [MaxLength(100)]
        public string? GradeLevel { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanName { get; set; }

        [MaxLength(100)]
        public string? ClassName { get; set; }

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

        public bool IsNeedEmployeeVerification { get; set; } = true;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        [MaxLength(500)]
        public string? GuaranteeDocumentPath { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdatePatientCompanyGuarantorRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid CompanyGuarantorId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? EmployeeName { get; set; }

        [MaxLength(100)]
        public string? DepartmentName { get; set; }

        [MaxLength(100)]
        public string? PositionName { get; set; }

        [MaxLength(100)]
        public string? GradeLevel { get; set; }

        [MaxLength(100)]
        public string? BenefitPlanCode { get; set; }

        [MaxLength(150)]
        public string? BenefitPlanName { get; set; }

        [MaxLength(100)]
        public string? ClassName { get; set; }

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

        public bool IsNeedEmployeeVerification { get; set; } = true;

        public bool IsAllowExcessPaymentByPatient { get; set; } = true;

        [MaxLength(500)]
        public string? GuaranteeDocumentPath { get; set; }

        [MaxLength(250)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class PatientCompanyGuarantorEligibilityRequest
    {
        public bool IsEligible { get; set; } = true;

        [MaxLength(100)]
        public string? LastEligibilityReferenceNumber { get; set; }

        [MaxLength(250)]
        public string? EligibilityNote { get; set; }

        public decimal? RemainingLimitAmount { get; set; }
    }

    public class PatientCompanyGuarantorCreateResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsEligible { get; set; }
        public bool IsActive { get; set; }
    }

    public class PatientCompanyGuarantorUpdateResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public bool IsEligible { get; set; }
        public bool IsActive { get; set; }
    }

    public class PatientCompanyGuarantorDeleteResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsDelete { get; set; }
    }

    public class PatientCompanyGuarantorStatusResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PatientCompanyGuarantorPrimaryStatusResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class PatientCompanyGuarantorEligibilityResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public Guid CompanyGuarantorId { get; set; }
        public string CompanyGuarantorName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public DateTime? LastEligibilityCheckAt { get; set; }
        public string? LastEligibilityReferenceNumber { get; set; }
        public string? EligibilityNote { get; set; }
        public decimal? RemainingLimitAmount { get; set; }
    }
}

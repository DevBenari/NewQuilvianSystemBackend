using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs
{
    public class WfpPayrollSummaryResponse
    {
        public int TotalPayrollProfile { get; set; }
        public int ActivePayrollProfile { get; set; }
        public int InactivePayrollProfile { get; set; }
        public int EligiblePayrollProfile { get; set; }
        public int ConfidentialPayrollProfile { get; set; }
        public decimal TotalBaseSalary { get; set; }
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalNetSalary { get; set; }
    }

    public class WfpPayrollResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? SalaryAssignmentId { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? SalaryStructureId { get; set; }
        public string? SalaryStructureCode { get; set; }
        public string? SalaryStructureName { get; set; }
        public Guid? SalaryGradeId { get; set; }
        public string? SalaryGradeCode { get; set; }
        public string? SalaryGradeName { get; set; }
        public Guid? LastPayrollPeriodId { get; set; }
        public string? LastPayrollPeriodCode { get; set; }
        public string? LastPayrollPeriodName { get; set; }
        public string? PayrollNumber { get; set; }
        public string? PayrollGroupCode { get; set; }
        public string PayrollStatus { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public string PaymentFrequency { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public bool IsPayrollEligible { get; set; }
        public bool IsConfidential { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime? LastCalculatedAt { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpPayrollDetailResponse : WfpPayrollResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpPayrollFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpPayrollDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpPayrollStringOptionResponse> PayrollStatusOptions { get; set; } = new();
        public List<WfpPayrollStringOptionResponse> PaymentFrequencyOptions { get; set; } = new();
        public List<WfpPayrollStringOptionResponse> PaymentMethodOptions { get; set; } = new();
        public List<WfpPayrollSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpPayrollDefaultFilterResponse
    {
        public string? PayrollStatus { get; set; }
        public string? PaymentFrequency { get; set; }
        public string? PaymentMethod { get; set; }
        public bool? IsPayrollEligible { get; set; }
        public bool? IsConfidential { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpPayrollStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpPayrollSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpPayrollRequest
    {
        public Guid? EmployeeId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? SalaryAssignmentId { get; set; }
        public Guid? BankAccountId { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? SalaryStructureId { get; set; }
        public Guid? SalaryGradeId { get; set; }
        public Guid? LastPayrollPeriodId { get; set; }

        [MaxLength(50)]
        public string? PayrollNumber { get; set; }

        [MaxLength(50)]
        public string? PayrollGroupCode { get; set; }

        [Required, MaxLength(30)]
        public string PayrollStatus { get; set; } = "Active";

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "IDR";

        [Required, MaxLength(30)]
        public string PaymentFrequency { get; set; } = "Monthly";

        [Required, MaxLength(30)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        public bool IsPayrollEligible { get; set; } = true;
        public bool IsConfidential { get; set; } = true;
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal TotalAllowance { get; set; }
        public decimal TotalDeduction { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime? LastCalculatedAt { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpPayrollRequest : CreateWfpPayrollRequest
    {
    }

    public class UpdateWfpPayrollStatusRequest
    {
        [Required, MaxLength(30)]
        public string PayrollStatus { get; set; } = "Active";

        public bool IsPayrollEligible { get; set; }
        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}

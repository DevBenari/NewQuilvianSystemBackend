using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforcePayrollResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string PayrollGroup { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public Guid? PrimaryBankAccountId { get; set; }

        public string? PrimaryBankName { get; set; }

        public string? PrimaryBankAccountNumber { get; set; }

        public string? PrimaryBankAccountHolderName { get; set; }

        public decimal BasicSalary { get; set; }

        public decimal FixedAllowance { get; set; }

        public decimal FixedDeduction { get; set; }

        public decimal NetFixedAmount { get; set; }

        public bool IsOvertimeEligible { get; set; }

        public bool IsPayrollActive { get; set; }

        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforcePayrollListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PayrollActiveData { get; set; }

        public decimal TotalBasicSalary { get; set; }

        public decimal TotalFixedAllowance { get; set; }

        public decimal TotalFixedDeduction { get; set; }

        public List<WorkforcePayrollResponse> Items { get; set; } = new();
    }

    public class CreateWorkforcePayrollRequest
    {
        [MaxLength(50)]
        public string PayrollGroup { get; set; } = "Default";

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        public Guid? PrimaryBankAccountId { get; set; }

        public decimal BasicSalary { get; set; } = 0;

        public decimal FixedAllowance { get; set; } = 0;

        public decimal FixedDeduction { get; set; } = 0;

        public bool IsOvertimeEligible { get; set; } = false;

        public bool IsPayrollActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforcePayrollRequest
    {
        [MaxLength(50)]
        public string PayrollGroup { get; set; } = "Default";

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "BankTransfer";

        public Guid? PrimaryBankAccountId { get; set; }

        public decimal BasicSalary { get; set; } = 0;

        public decimal FixedAllowance { get; set; } = 0;

        public decimal FixedDeduction { get; set; } = 0;

        public bool IsOvertimeEligible { get; set; } = false;

        public bool IsPayrollActive { get; set; } = true;

        [Required]
        public DateTime EffectiveStartDate { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforcePayrollStatusRequest
    {
        public bool IsActive { get; set; }

        public bool IsPayrollActive { get; set; }

        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}

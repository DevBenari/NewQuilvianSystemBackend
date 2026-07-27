using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    public class WfpBankAccountSummaryResponse
    {
        public int TotalBankAccount { get; set; }
        public int ActiveBankAccount { get; set; }
        public int InactiveBankAccount { get; set; }
        public int PrimaryBankAccount { get; set; }
        public int PayrollBankAccount { get; set; }
        public int VerifiedBankAccount { get; set; }
        public int UnverifiedBankAccount { get; set; }
    }

    public class WfpBankAccountResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid? BankId { get; set; }
        public string? BankCode { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string? BankBranch { get; set; }
        public string AccountType { get; set; } = string.Empty;
        public string CurrencyCode { get; set; } = string.Empty;
        public bool IsPayrollAccount { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedByUserId { get; set; }
        public string? VerifiedByUserName { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WfpBankAccountDetailResponse : WfpBankAccountResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WfpBankAccountFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WfpBankAccountDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WfpBankOptionResponse> BankOptions { get; set; } = new();
        public List<WfpBankAccountStringOptionResponse> AccountTypeOptions { get; set; } = new();
        public List<WfpBankAccountStringOptionResponse> CurrencyOptions { get; set; } = new();
        public List<WfpBankAccountSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class WfpBankAccountDefaultFilterResponse
    {
        public Guid? BankId { get; set; }
        public string? AccountType { get; set; }
        public bool? IsPayrollAccount { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "isPrimary";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WfpBankAccountStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpBankAccountSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WfpBankOptionResponse
    {
        public Guid Id { get; set; }
        public string BankCode { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateWfpBankAccountRequest
    {
        public Guid? BankId { get; set; }

        [MaxLength(200)]
        public string? BankName { get; set; }

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? BankBranch { get; set; }

        [Required]
        [MaxLength(50)]
        public string AccountType { get; set; } = "Savings";

        [Required]
        [StringLength(3, MinimumLength = 3)]
        [RegularExpression(@"^[A-Za-z]{3}$", ErrorMessage = "CurrencyCode harus terdiri dari 3 huruf.")]
        public string CurrencyCode { get; set; } = "IDR";

        public bool IsPayrollAccount { get; set; } = true;
        public bool IsPrimary { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWfpBankAccountRequest : CreateWfpBankAccountRequest
    {
    }

    public class UpdateWfpBankAccountStatusRequest
    {
        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class SetWfpBankAccountPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }

    public class VerifyWfpBankAccountRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}

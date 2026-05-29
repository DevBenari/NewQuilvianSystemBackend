using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceBankAccountResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string BankName { get; set; } = string.Empty;

        public string AccountNumber { get; set; } = string.Empty;

        public string AccountHolderName { get; set; } = string.Empty;

        public string? BankBranch { get; set; }

        public bool IsPrimary { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceBankAccountListResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalData { get; set; }

        public int ActiveData { get; set; }

        public int PrimaryData { get; set; }

        public List<WorkforceBankAccountResponse> Items { get; set; } = new();
    }

    public class CreateWorkforceBankAccountRequest
    {
        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BankBranch { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceBankAccountRequest
    {
        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AccountNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string AccountHolderName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? BankBranch { get; set; }

        public bool IsPrimary { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceBankAccountStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class SetWorkforceBankAccountPrimaryRequest
    {
        public bool IsPrimary { get; set; } = true;
    }
}

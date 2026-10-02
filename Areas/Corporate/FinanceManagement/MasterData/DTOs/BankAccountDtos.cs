using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;

public sealed class BankAccountQuery
{
    public string? Search { get; set; }
    public Guid? BankId { get; set; }
    public string? AccountType { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "accountName";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class CreateBankAccountRequest
{
    [Required] public Guid BankId { get; set; }
    [Required, MaxLength(50)] public string AccountNumber { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string AccountName { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string AccountType { get; set; } = string.Empty;
    [MaxLength(3)] public string CurrencyCode { get; set; } = "IDR";
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateBankAccountRequest : CreateBankAccountRequest;

public sealed class UpdateBankAccountStatusRequest
{
    public bool IsActive { get; set; }
}

public sealed class BankAccountResponse
{
    public Guid Id { get; set; }
    public Guid BankId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    /// <summary>Sensitif — MUST NOT dicatat pada log aplikasi (data-dictionary.md §Sensitif).</summary>
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }
}

public sealed class BankAccountDeleteResponse
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public bool IsDelete { get; set; }
}

public sealed class BankAccountOptionResponse
{
    public Guid Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
}

public sealed class BankAccountSummaryResponse
{
    public int TotalAccount { get; set; }
    public int ActiveAccount { get; set; }
    public int InactiveAccount { get; set; }
}

public sealed class BankAccountDefaultFilterResponse
{
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "accountName";
    public string SortDirection { get; set; } = "asc";
}

public sealed class BankAccountFilterMetadataResponse
{
    public BankAccountDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<int> PageSizeOptions { get; set; } = new();
    public List<string> SortableFields { get; set; } = new();
    public List<string> AccountTypeOptions { get; set; } = new();
}

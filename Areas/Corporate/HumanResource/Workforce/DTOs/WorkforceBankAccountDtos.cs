using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceBankAccountSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalBankAccount { get; set; }
        public int ActiveBankAccount { get; set; }
        public int InactiveBankAccount { get; set; }
        public int PrimaryBankAccount { get; set; }
    }

    public class WorkforceBankAccountResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountNumberMasked { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string? BankBranch { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceBankAccountDetailResponse : WorkforceBankAccountResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class WorkforceBankAccountOptionResponse
    {
        public Guid Id { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumberMasked { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string? BankBranch { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }

    public class WorkforceBankAccountOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceBankAccountOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceBankAccountFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WorkforceBankAccountDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceBankAccountCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceBankAccountSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceBankAccountSimpleOptionResponse> BankNameExamples { get; set; } = new();
        public WorkforceBankAccountFrontendGuideResponse FrontendGuide { get; set; } = new();
    }

    public class WorkforceBankAccountDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsPrimary { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceBankAccountCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceBankAccountSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceBankAccountSimpleOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceBankAccountFrontendGuideResponse
    {
        public string BankNameInstruction { get; set; } =
            "BankName diisi nama bank, misalnya BCA, Mandiri, BRI, BNI, BSI. Field ini text agar tetap fleksibel jika ada bank baru.";

        public string AccountNumberInstruction { get; set; } =
            "AccountNumber diisi nomor rekening. Backend akan menormalisasi menjadi angka saja, contoh input 123-456 789 menjadi 123456789.";

        public string PrimaryInstruction { get; set; } =
            "Hanya boleh ada satu rekening primary aktif per workforce profile. Jika data pertama dibuat, backend otomatis menjadikannya primary.";

        public string SecurityInstruction { get; set; } =
            "Untuk tampilan list/dropdown, gunakan AccountNumberMasked. Gunakan AccountNumber penuh hanya pada halaman detail/edit yang berhak.";
    }

    public class CreateWorkforceBankAccountRequest
    {
        [Required]
        [MaxLength(100)]
        public string BankName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [RegularExpression(@"^[0-9\s\-\.]+$", ErrorMessage = "AccountNumber hanya boleh berisi angka, spasi, titik, atau tanda strip.")]
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
        [RegularExpression(@"^[0-9\s\-\.]+$", ErrorMessage = "AccountNumber hanya boleh berisi angka, spasi, titik, atau tanda strip.")]
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

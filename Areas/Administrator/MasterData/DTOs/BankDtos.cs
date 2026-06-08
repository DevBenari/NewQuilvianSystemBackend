using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class BankSummaryResponse
    {
        public int TotalBank { get; set; }
        public int ActiveBank { get; set; }
        public int InactiveBank { get; set; }
        public int DefaultBank { get; set; }
        public int CommercialBank { get; set; }
        public int SyariahBank { get; set; }
        public int DigitalBank { get; set; }
        public int RuralBank { get; set; }
        public int OtherBank { get; set; }
        public int WithClearingCodeBank { get; set; }
    }

    public class BankResponse
    {
        public Guid Id { get; set; }

        public string BankCode { get; set; } = string.Empty;

        public string BankName { get; set; } = string.Empty;

        public string? BankShortName { get; set; }

        public BankCategory BankCategory { get; set; } = BankCategory.Commercial;

        public string BankCategoryName { get; set; } = string.Empty;

        public string? ClearingCode { get; set; }        

        public bool IsDefault { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public Guid? CreateBy { get; set; }

        public string? CreateByName { get; set; }
    }

    public class BankDetailResponse : BankResponse
    {
        public string? Description { get; set; }

        public DateTime? UpdateDateTime { get; set; }

        public Guid? UpdateBy { get; set; }

        public string? UpdateByName { get; set; }
    }

    public class BankOptionResponse
    {
        public Guid Id { get; set; }

        public string BankCode { get; set; } = string.Empty;

        public string BankName { get; set; } = string.Empty;

        public string? BankShortName { get; set; }

        public BankCategory BankCategory { get; set; } = BankCategory.Commercial;

        public string BankCategoryName { get; set; } = string.Empty;

        public string? ClearingCode { get; set; }

        public bool IsDefault { get; set; }

        public int SortOrder { get; set; }
    }

    public class BankOptionPagedResponse
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalData { get; set; }

        public int TotalPage { get; set; }

        public List<BankOptionResponse> Items { get; set; } = new();
    }

    public class BankFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public BankDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<BankCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<BankSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<BankCategoryOptionResponse> BankCategoryOptions { get; set; } = new();

        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class BankDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class BankCategoryOptionResponse
    {
        public int Value { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class BankCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class BankSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class CreateBankRequest
    {
        [Required]
        [MaxLength(200)]
        public string BankName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? BankShortName { get; set; }

        public BankCategory BankCategory { get; set; } = BankCategory.Commercial;

        [MaxLength(50)]
        public string? ClearingCode { get; set; }

        public bool IsDefault { get; set; } = false;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateBankRequest : CreateBankRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateBankStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class DeleteBankRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class BankCreateResponse
    {
        public Guid Id { get; set; }

        public string BankCode { get; set; } = string.Empty;

        public string BankName { get; set; } = string.Empty;

        public string? BankShortName { get; set; }

        public bool IsActive { get; set; }
    }
}
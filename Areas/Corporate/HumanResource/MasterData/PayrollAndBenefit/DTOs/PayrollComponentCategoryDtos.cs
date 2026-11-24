using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs
{
    public class PayrollComponentCategorySummaryResponse
    {
        public int TotalCategory { get; set; }
        public int ActiveCategory { get; set; }
        public int InactiveCategory { get; set; }

        /// <summary>Kategori yang namanya sama dengan katalog dan kelompoknya sesuai (aktif maupun nonaktif).</summary>
        public int CoreCategory { get; set; }

        /// <summary>Kategori lain (legacy atau berkelompok tidak sesuai): tetap dibaca, tidak ditawarkan untuk komponen baru.</summary>
        public int LegacyCategory { get; set; }

        /// <summary>Kategori katalog yang belum ada di database sama sekali.</summary>
        public int MissingCoreCategory { get; set; }
    }

    public class PayrollComponentCategoryResponse
    {
        public Guid Id { get; set; }
        public string ComponentCategoryCode { get; set; } = string.Empty;
        public string ComponentCategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Kelompok teknis internal (Earning/Deduction/...), diturunkan backend dari kategori. Dipertahankan pada respons demi
        /// kompatibilitas; frontend baru tidak menampilkannya dan tidak mengirimnya.
        /// </summary>
        public string ComponentGroup { get; set; } = string.Empty;

        /// <summary>Kunci kategori katalog (BASIC_SALARY, ALLOWANCE, INCENTIVE, DEDUCTION, BONUS); null untuk kategori legacy.</summary>
        public string? CategoryKey { get; set; }

        /// <summary>True bila nama kategori sama dengan katalog dan karena itu dikunci (tidak dapat diubah).</summary>
        public bool IsNameLocked { get; set; }

        /// <summary>True bila kategori boleh dipilih untuk komponen baru (nama katalog dan kelompok sesuai).</summary>
        public bool IsOperational { get; set; }

        public bool AffectsGrossPay { get; set; }
        public bool AffectsTaxableIncome { get; set; }
        public bool AffectsTakeHomePay { get; set; }
        public bool IsEmployerCost { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int PayrollComponentCount { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class PayrollComponentCategoryDetailResponse : PayrollComponentCategoryResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class PayrollComponentCategoryOptionResponse
    {
        public Guid Id { get; set; }
        public string ComponentCategoryCode { get; set; } = string.Empty;
        public string ComponentCategoryName { get; set; } = string.Empty;
        public string ComponentGroup { get; set; } = string.Empty;
        public string? CategoryKey { get; set; }
        public bool IsOperational { get; set; }
    }

    public class PayrollComponentCategoryOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<PayrollComponentCategoryOptionResponse> Items { get; set; } = new();
    }

    public class PayrollComponentCategoryFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PayrollComponentCategoryDefaultFilterResponse DefaultFilter { get; set; } = new();

        /// <summary>
        /// Lima kategori katalog beserta keadaannya di database (Missing/Ready/Inactive/GroupMismatch). Menjadi sumber pilihan Nama
        /// Kategori pada formulir Tambah; frontend tidak mengarang kategori sendiri.
        /// </summary>
        public List<PayrollComponentCategoryCoreOptionResponse> CoreCategoryOptions { get; set; } = new();
        public List<PayrollMasterCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<PayrollMasterSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PayrollComponentCategoryDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public bool? AffectsGrossPay { get; set; }
        public bool? AffectsTaxableIncome { get; set; }
        public bool? AffectsTakeHomePay { get; set; }
        public bool? IsEmployerCost { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "componentCategoryName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreatePayrollComponentCategoryRequest
    {
        [Required]
        [MaxLength(150)]
        public string ComponentCategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Tidak lagi dipilih pengguna: backend menurunkan kelompok dari kategori. Boleh dikirim klien lama; bila terisi dan tidak sama
        /// dengan kelompok hasil turunan, permintaan ditolak (bukan diam-diam ditimpa).
        /// </summary>
        [MaxLength(50)]
        public string? ComponentGroup { get; set; }

        public bool AffectsGrossPay { get; set; } = true;
        public bool AffectsTaxableIncome { get; set; } = true;
        public bool AffectsTakeHomePay { get; set; } = true;
        public bool IsEmployerCost { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class UpdatePayrollComponentCategoryRequest : CreatePayrollComponentCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePayrollComponentCategoryStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class PayrollComponentCategoryCreateResponse
    {
        public Guid Id { get; set; }
        public string ComponentCategoryCode { get; set; } = string.Empty;
        public string ComponentCategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class PayrollComponentCategoryCoreOptionResponse
    {
        public string Key { get; set; } = string.Empty;

        /// <summary>Nama kanonik kategori; juga nilai yang dikirim saat membuat kategori.</summary>
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ComponentGroup { get; set; } = string.Empty;
        public bool DefaultAffectsGrossPay { get; set; }
        public bool DefaultAffectsTaxableIncome { get; set; }
        public bool DefaultAffectsTakeHomePay { get; set; }
        public int DefaultSortOrder { get; set; }
        public string? DefaultDescription { get; set; }

        /// <summary>Missing, Ready, Inactive, atau GroupMismatch.</summary>
        public string Status { get; set; } = "Missing";
        public bool AvailableForCreate { get; set; }
        public Guid? ExistingCategoryId { get; set; }
    }

    public class PayrollMasterStringOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PayrollMasterSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PayrollMasterCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}

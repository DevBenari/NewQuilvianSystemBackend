using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    /// <summary>
    /// Angka ringkasan daftar alasan terkendali untuk kartu statistik halaman index.
    /// </summary>
    /// <remarks>
    /// <c>CategoryWithoutActiveReasonCount</c> bukan angka hiasan. Sebuah kategori yang tidak
    /// punya satu pun alasan aktif membuat tindakan yang memerlukannya **tidak dapat
    /// diselesaikan sama sekali** — petugas membuka kotak pilihan alasan dan menemukannya
    /// kosong, sementara aturan menuntut alasan terkendali (<c>INV-BD-016</c>). Angka itu
    /// ditampilkan supaya keadaan tersebut terlihat sebelum petugas menemuinya di tengah proses.
    /// </remarks>
    public class BloodBankReasonSummaryResponse
    {
        public int TotalBloodBankReason { get; set; }
        public int ActiveBloodBankReason { get; set; }
        public int InactiveBloodBankReason { get; set; }

        /// <summary>Jumlah kategori sah yang belum punya satu pun alasan aktif.</summary>
        public int CategoryWithoutActiveReasonCount { get; set; }

        /// <summary>Nama kategori yang belum punya alasan aktif, supaya dapat langsung dilengkapi.</summary>
        public List<string> CategoryWithoutActiveReason { get; set; } = new();
    }

    /// <summary>Bentuk balasan satu alasan terkendali.</summary>
    public class BloodBankReasonResponse
    {
        public Guid Id { get; set; }

        /// <summary>Kode alasan, unik di seluruh tabel. Contoh <c>CANCEL-KLINIS-01</c>.</summary>
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>Teks yang ditampilkan kepada petugas saat memilih alasan.</summary>
        public string ReasonText { get; set; } = string.Empty;

        /// <summary>Kategori alasan, dari daftar tertutup kontrak.</summary>
        public string ReasonCategory { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>Bentuk ringan untuk kotak pilihan alasan pada layar tindakan.</summary>
    public class BloodBankReasonOptionResponse
    {
        public Guid Id { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public string ReasonText { get; set; } = string.Empty;
        public string ReasonCategory { get; set; } = string.Empty;
    }

    public class CreateBloodBankReasonRequest
    {
        [Required(ErrorMessage = "Kode alasan wajib diisi.")]
        [MaxLength(30, ErrorMessage = "Kode alasan terlalu panjang. Batasnya 30 huruf.")]
        public string ReasonCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Teks alasan wajib diisi.")]
        [MaxLength(200, ErrorMessage = "Teks alasan terlalu panjang. Batasnya 200 huruf.")]
        public string ReasonText { get; set; } = string.Empty;

        /// <summary>
        /// Kategori alasan. Wajib salah satu dari daftar tertutup; nilai di luar daftar ditolak
        /// supaya salah ketik tidak diam-diam menciptakan kategori yang tak pernah dibaca.
        /// </summary>
        [Required(ErrorMessage = "Kategori alasan wajib diisi.")]
        [MaxLength(40, ErrorMessage = "Kategori alasan terlalu panjang. Batasnya 40 huruf.")]
        public string ReasonCategory { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Bentuk permintaan mengubah alasan. Sengaja memuat <c>ReasonCode</c> dan
    /// <c>ReasonCategory</c>, karena keduanya harus dapat dibetulkan bila salah dipilih.
    /// </summary>
    public class UpdateBloodBankReasonRequest : CreateBloodBankReasonRequest
    {
    }

    public class UpdateBloodBankReasonStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class BloodBankReasonFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        public BloodBankReasonDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BloodBankReasonCategoryOptionResponse> ReasonCategoryOptions { get; set; } = new();
        public List<BloodBankReasonSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<BloodBankReasonQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<BloodBankReasonFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<BloodBankReasonFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class BloodBankReasonDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? ReasonCategory { get; set; }
        public string SortBy { get; set; } = "reasonCategory";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Pilihan kategori beserta label yang dibaca petugas.</summary>
    public class BloodBankReasonCategoryOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class BloodBankReasonSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class BloodBankReasonQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class BloodBankReasonFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }
}
